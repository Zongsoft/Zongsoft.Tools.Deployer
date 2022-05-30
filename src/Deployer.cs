/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@qq.com>
 *
 * The MIT License (MIT)
 * 
 * Copyright (C) 2015-2017 Zongsoft Corporation <http://www.zongsoft.com>
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Zongsoft.Services;
using Zongsoft.Resources;
using Zongsoft.Terminals;
using Zongsoft.Configuration;
using Zongsoft.Configuration.Profiles;

namespace Zongsoft.Utilities
{
	public class Deployer
	{
		#region 常量定义
		private const string DEPLOYMENTDIRECTORY_PARAMETER = "deploymentDirectory";
		private const string IGNORERESOLVEDEPLOYMENTFILE_PARAMETER = "ignoreResolve";

		private const string USERPROFILE_ENVIRONMENT = "USERPROFILE";
		private const string NUGET_PACKAGES_ENVIRONMENT = "NUGET_PACKAGES";

		private const string REGEX_VALUE_GROUP = "value";
		private static readonly Regex _REGEX_ = new(@"(?<opt>\$\((?<value>\w+)\))|(?<env>\%(?<value>\w+)\%)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
		#endregion

		#region 成员字段
		private ITerminal _terminal;
		private readonly IDictionary<string, string> _environmentVariables;
		#endregion

		#region 构造函数
		public Deployer(ITerminal terminal)
		{
			_terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
			_environmentVariables = Zongsoft.Collections.DictionaryExtension.ToDictionary<string, string>(Environment.GetEnvironmentVariables(), StringComparer.OrdinalIgnoreCase);

			if(!_environmentVariables.ContainsKey(NUGET_PACKAGES_ENVIRONMENT) && _environmentVariables.TryGetValue(USERPROFILE_ENVIRONMENT, out var home))
				_environmentVariables.TryAdd(NUGET_PACKAGES_ENVIRONMENT, Path.Combine(home, ".nuget/packages"));
		}
		#endregion

		#region 公共属性
		public ITerminal Terminal
		{
			get => _terminal;
			set => _terminal = value ?? throw new ArgumentNullException();
		}

		public IDictionary<string, string> EnvironmentVariables
		{
			get => _environmentVariables;
		}
		#endregion

		#region 公共方法
		public DeploymentCounter Deploy(string deploymentFilePath, string destinationDirectory = null)
		{
			if(string.IsNullOrWhiteSpace(deploymentFilePath))
				throw new ArgumentNullException(nameof(deploymentFilePath));

			if(!Path.IsPathRooted(deploymentFilePath))
				deploymentFilePath = Zongsoft.IO.Path.Combine(Environment.CurrentDirectory, deploymentFilePath);

			if(!File.Exists(deploymentFilePath))
				throw new FileNotFoundException(ResourceUtility.GetResourceString(typeof(Deployer).Assembly, "Text.DeploymentFileNotExists", deploymentFilePath));

			if(string.IsNullOrWhiteSpace(destinationDirectory))
			{
				if(_environmentVariables.TryGetValue(DEPLOYMENTDIRECTORY_PARAMETER, out destinationDirectory))
				{
					if(!Path.IsPathRooted(destinationDirectory))
						destinationDirectory = Zongsoft.IO.Path.Combine(Environment.CurrentDirectory, destinationDirectory);
				}
				else
				{
					destinationDirectory = Environment.CurrentDirectory;
				}
			}

			if(!Directory.Exists(destinationDirectory))
				throw new DirectoryNotFoundException(destinationDirectory);

			//创建部署上下文对象
			var context = this.CreateContext(deploymentFilePath, destinationDirectory);

			foreach(var item in context.DeploymentProfile.Items)
			{
				this.DeployItem(item, context);
			}

			return context.Counter;
		}
		#endregion

		#region 虚拟方法
		protected virtual DeploymentContext CreateContext(string deploymentFilePath, string destinationDirectory)
		{
			if(string.IsNullOrWhiteSpace(deploymentFilePath))
				throw new ArgumentNullException(nameof(deploymentFilePath));

			return new DeploymentContext(this, Profile.Load(deploymentFilePath), destinationDirectory);
		}
		#endregion

		#region 私有方法
		private void DeployItem(ProfileItem item, DeploymentContext context)
		{
			switch(item.ItemType)
			{
				case ProfileItemType.Section:
					var section = (ProfileSection)item;

					//确保部署的目标目录已经存在，如不存在则创建它
					Utility.EnsureDirectory(context.DestinationDirectory, Normalize(section.FullName.Replace(' ', '/'), _environmentVariables));

					foreach(var child in section.Items)
						this.DeployItem(child, context);

					break;
				case ProfileItemType.Entry:
					this.DeployEntry((ProfileEntry)item, context);
					break;
			}
		}

		private void DeployEntry(ProfileEntry entry, DeploymentContext context)
		{
			var sourcePath = Normalize(entry.Name, _environmentVariables);

			if(!Path.IsPathRooted(sourcePath))
				sourcePath = Zongsoft.IO.Path.Combine(context.SourceDirectory, sourcePath);

			var destinationName = string.IsNullOrWhiteSpace(entry.Value) ? string.Empty : Normalize(entry.Value, _environmentVariables);
			var destinationDirectory = context.DestinationDirectory;

			if(entry.Section != null)
				destinationDirectory = Zongsoft.IO.Path.Combine(context.DestinationDirectory, Normalize(entry.Section.FullName.Replace(' ', '/'), _environmentVariables));

			//由于源路径中可能含有通配符，因此必须查找匹配的文件集
			foreach(var sourceFile in GetFiles(sourcePath))
			{
				if(!File.Exists(sourceFile))
				{
					//累加文件复制失败计数器
					context.Counter.Fail();

					_terminal.Write(CommandOutletColor.Magenta, ResourceUtility.GetResourceString(typeof(Deployer).Assembly, "Text.Warn"));
					_terminal.WriteLine(CommandOutletColor.DarkYellow, string.Format(ResourceUtility.GetResourceString(typeof(Deployer).Assembly, "Text.FileNotExists"), sourceFile));

					continue;
				}

				//如果指定要拷贝的源文件是一个部署文件
				if(IsDeploymentFile(sourceFile))
				{
					//如果没有指定忽略处理子部署文件，则进行子部署文件的递归处理
					if(!_environmentVariables.ContainsKey(IGNORERESOLVEDEPLOYMENTFILE_PARAMETER))
					{
						var counter = this.Deploy(sourceFile, destinationDirectory);

						context.Counter.Fail(counter.Failures);
						context.Counter.Success(counter.Successes);

						continue;
					}
				}

				//执行文件复制
				if(CopyFile(sourceFile, Path.Combine(destinationDirectory, string.IsNullOrEmpty(destinationName) ? Path.GetFileName(sourceFile) : destinationName)))
					context.Counter.Success();
				else
					context.Counter.Fail();
			}
		}

		private static string Normalize(string text, IDictionary<string, string> variables)
		{
			if(string.IsNullOrWhiteSpace(text))
				return string.Empty;

			return _REGEX_.Replace(text, match =>
			{
				if(match.Success && match.Groups.TryGetValue(REGEX_VALUE_GROUP, out var group))
					return variables.TryGetValue(group.Value, out var value) ? value : null;

				return null;
			});
		}

		public static IEnumerable<string> GetFiles(string filePath)
		{
			if(string.IsNullOrEmpty(filePath))
				yield break;

			var directoryName = Path.GetDirectoryName(filePath);
			var fileName = Path.GetFileName(filePath);

			if(string.IsNullOrEmpty(fileName))
				yield break;

			foreach(var directory in GetDirectories(directoryName))
			{
				if(fileName.Contains("*") || fileName.Contains("?"))
				{
					foreach(var file in Directory.GetFiles(directory, fileName))
						yield return file;
				}
				else
				{
					yield return Path.Combine(directory, fileName);
				}
			}
		}

		private static IEnumerable<string> GetDirectories(string directory)
		{
			if(string.IsNullOrEmpty(directory))
				return Array.Empty<string>();

			directory = Path.GetFullPath(directory);
			var parts = Common.StringExtension.Slice(directory, new[] { '/', '\\' }).ToArray();
			List<string> directories = null;

			for(int i = 0; i < parts.Length; i++)
			{
				if(parts[i] == "**")
				{
					if(directories == null)
						directories = new List<string>();

					directories.AddRange(Directory.GetDirectories(Path.Combine(parts.Take(i).ToArray()), "*", SearchOption.AllDirectories));
				}
				else if(parts[i].Contains("*") || parts.Contains("?"))
				{
					if(directories == null)
						directories = new List<string>();

					directories.AddRange(Directory.GetDirectories(Path.Combine(parts.Take(i).ToArray()), parts[i], SearchOption.TopDirectoryOnly));
				}
				else
				{
					if(directories != null && directories.Count > 0)
					{
						for(int j = 0; j < directories.Count; j++)
							directories[j] = Path.Combine(directories[j], parts[i]);
					}
				}
			}

			if(directories == null || directories.Count == 0)
				return new[] { directory };

			return directories;
		}

		private static bool CopyFile(string source, string destination)
		{
			if(string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(destination))
				return false;

			bool requiredCope = true;

			if(File.Exists(destination))
				requiredCope = File.GetLastWriteTime(source) > File.GetLastWriteTime(destination);

			if(requiredCope)
				File.Copy(source, destination, true);

			return requiredCope;
		}

		private static bool IsDeploymentFile(string filePath)
		{
			if(string.IsNullOrWhiteSpace(filePath))
				return false;

			//如果指定的文件的扩展名为.deploy，则判断为部署文件
			return string.Equals(Path.GetExtension(filePath), ".deploy", StringComparison.OrdinalIgnoreCase);
		}
		#endregion

		private readonly struct DirectoryToken
		{
			public DirectoryToken(string path, params string[] wildcards)
			{
				this.Path = path;
				this.Wildcards = wildcards;
			}

			public readonly string Path;
			public readonly string[] Wildcards;
		}
	}
}
