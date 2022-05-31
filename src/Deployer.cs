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
		private const string DEPLOYMENTDIRECTORY_OPTION  = "deploymentDirectory";
		private const string IGNOREDEPLOYMENTFILE_OPTION = "ignoreDeploymentFile";
		private const string EXPANSION_OPTION = "expansion";

		private const string USERPROFILE_ENVIRONMENT = "USERPROFILE";
		private const string NUGET_PACKAGES_ENVIRONMENT = "NUGET_PACKAGES";

		private const string REGEX_VALUE_GROUP = "value";
		private static readonly Regex _REGEX_ = new(@"(?<opt>\$\((?<value>\w+)\))|(?<env>\%(?<value>\w+)\%)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
		#endregion

		#region 成员字段
		private ITerminal _terminal;
		private readonly IDictionary<string, string> _variables;
		#endregion

		#region 构造函数
		public Deployer(ITerminal terminal)
		{
			_terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
			_variables = Zongsoft.Collections.DictionaryExtension.ToDictionary<string, string>(Environment.GetEnvironmentVariables(), StringComparer.OrdinalIgnoreCase);

			if(!_variables.ContainsKey(NUGET_PACKAGES_ENVIRONMENT) && _variables.TryGetValue(USERPROFILE_ENVIRONMENT, out var home))
				_variables.TryAdd(NUGET_PACKAGES_ENVIRONMENT, Path.Combine(home, ".nuget/packages"));
		}
		#endregion

		#region 公共属性
		public ITerminal Terminal
		{
			get => _terminal;
			set => _terminal = value ?? throw new ArgumentNullException();
		}

		public IDictionary<string, string> Variables => _variables;
		#endregion

		#region 公共方法
		public DeploymentCounter Deploy(string deploymentFilePath, string destinationDirectory = null)
		{
			if(string.IsNullOrWhiteSpace(deploymentFilePath))
				throw new ArgumentNullException(nameof(deploymentFilePath));

			if(!Path.IsPathRooted(deploymentFilePath))
				deploymentFilePath = Path.Combine(Environment.CurrentDirectory, deploymentFilePath);

			if(!File.Exists(deploymentFilePath))
				throw new FileNotFoundException(ResourceUtility.GetResourceString(typeof(Deployer).Assembly, "Text.DeploymentFileNotExists", deploymentFilePath));

			if(string.IsNullOrWhiteSpace(destinationDirectory))
			{
				if(_variables.TryGetValue(DEPLOYMENTDIRECTORY_OPTION, out destinationDirectory))
				{
					if(!Path.IsPathRooted(destinationDirectory))
						destinationDirectory = Path.Combine(Environment.CurrentDirectory, destinationDirectory);
				}
				else
				{
					destinationDirectory = Environment.CurrentDirectory;
				}
			}

			if(!Directory.Exists(destinationDirectory))
				Directory.CreateDirectory(destinationDirectory);

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
					Utility.EnsureDirectory(context.DestinationDirectory, Normalize(section.FullName.Replace(' ', '/'), _variables));

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
			var sourcePath = Normalize(entry.Name, _variables);

			if(!Path.IsPathRooted(sourcePath))
				sourcePath = Path.Combine(context.SourceDirectory, sourcePath);

			var destinationName = string.IsNullOrWhiteSpace(entry.Value) ? string.Empty : Normalize(entry.Value, _variables);
			var destinationDirectory = context.DestinationDirectory;

			if(entry.Section != null)
				destinationDirectory = Path.Combine(context.DestinationDirectory, Normalize(entry.Section.FullName.Replace(' ', Path.DirectorySeparatorChar), _variables));

			//由于源路径中可能含有通配符，因此必须查找匹配的文件集
			foreach(var sourceFile in GetFiles(sourcePath, _variables.ContainsKey(EXPANSION_OPTION)))
			{
				if(!sourceFile.Exists())
				{
					//累加文件复制失败计数器
					context.Counter.Fail();

					_terminal.Write(CommandOutletColor.Magenta, ResourceUtility.GetResourceString(typeof(Deployer).Assembly, "Text.Warn"));
					_terminal.WriteLine(CommandOutletColor.DarkYellow, string.Format(ResourceUtility.GetResourceString(typeof(Deployer).Assembly, "Text.FileNotExists"), sourceFile));

					continue;
				}

				//如果指定要拷贝的源文件是一个部署文件
				if(IsDeploymentFile(sourceFile.Path))
				{
					//如果没有指定忽略处理子部署文件，则进行子部署文件的递归处理
					if(!_variables.ContainsKey(IGNOREDEPLOYMENTFILE_OPTION))
					{
						var counter = this.Deploy(sourceFile.Path, GetDestinationDirectory(destinationDirectory, sourceFile.Suffix));
						context.Count(counter);
						continue;
					}
				}

				//执行文件复制
				if(CopyFile(sourceFile.Path, Path.Combine(GetDestinationDirectory(destinationDirectory, sourceFile.Suffix), string.IsNullOrEmpty(destinationName) ? Path.GetFileName(sourceFile.Path) : destinationName)))
					context.Counter.Success();
				else
					context.Counter.Fail();
			}

			static string GetDestinationDirectory(string root, string suffix) => string.IsNullOrEmpty(suffix) ? root : Path.Combine(root, suffix);
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

		private static IEnumerable<PathToken> GetFiles(string filePath, bool expansion)
		{
			if(string.IsNullOrEmpty(filePath))
				yield break;

			var directoryName = Path.GetDirectoryName(filePath);
			var fileName = Path.GetFileName(filePath);

			if(string.IsNullOrEmpty(fileName))
				yield break;

			foreach(var directory in GetDirectories(directoryName, expansion))
			{
				if(fileName.Contains("*") || fileName.Contains("?"))
				{
					foreach(var file in Directory.GetFiles(directory.Path, fileName))
						yield return new PathToken(file, directory.Suffix);
				}
				else
				{
					directory.Combine(fileName);
					yield return directory;
				}
			}
		}

		private static IEnumerable<PathToken> GetDirectories(string directory, bool expansion)
		{
			const int Asterisk1 = 1;
			const int Asterisk2 = 2;

			if(string.IsNullOrEmpty(directory))
				return Array.Empty<PathToken>();

			directory = Path.GetFullPath(directory);
			var parts = Common.StringExtension.Slice(directory, new[] { '/', '\\' }).ToArray();
			List<PathToken> directories = null;
			var flags = 0;

			for(int i = 0; i < parts.Length; i++)
			{
				if(parts[i] == "**")
				{
					if(directories == null)
						directories = new List<PathToken>();

					flags |= Asterisk2;
					var origin = Path.Combine(parts.Take(i).ToArray());
					directories.AddRange(Directory.GetDirectories(origin, "*", SearchOption.AllDirectories).Select(p => PathToken.Create(p, origin)));
				}
				else if(parts[i].Contains("*") || parts.Contains("?"))
				{
					if(directories == null)
						directories = new List<PathToken>();

					flags |= Asterisk1;
					var origin = Path.Combine(parts.Take(i).ToArray());
					directories.AddRange(Directory.GetDirectories(origin, parts[i], SearchOption.TopDirectoryOnly).Select(p => PathToken.Create(p, origin)));
				}
				else if(directories != null && directories.Count > 0)
				{
					if((flags & Asterisk2) == Asterisk2)
					{
						for(int j = 0; j < directories.Count; j++)
						{
							if(!directories[j].Suffix.EndsWith("/" + parts[i]))
								directories[j].Deprecate();
						}
					}
					else
					{
						for(int j = 0; j < directories.Count; j++)
						{
							directories[j].Combine(parts[i]);

							if(expansion)
								directories[j].AppendSuffix(parts[i]);
						}
					}
				}
			}

			if(directories == null || directories.Count == 0)
				return new[] { new PathToken(directory) };

			return directories.Where(token => !string.IsNullOrEmpty(token.Path));
		}

		private static bool CopyFile(string source, string destination)
		{
			if(string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(destination))
				return false;

			bool requiredCope = true;

			if(File.Exists(destination))
				requiredCope = File.GetLastWriteTime(source) > File.GetLastWriteTime(destination);

			if(requiredCope)
			{
				var directory = Path.GetDirectoryName(destination);
				if(!Directory.Exists(directory))
					Directory.CreateDirectory(directory);

				File.Copy(source, destination, true);
			}

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

		#region 嵌套子类
		private class PathToken
		{
			public PathToken(string path, string suffix = null)
			{
				this.Path = path;
				this.Suffix = string.IsNullOrEmpty(suffix) ? null : suffix.Trim('/', '\\');
			}

			public string Path;
			public string Suffix;

			public bool Exists() => !string.IsNullOrEmpty(this.Path) && File.Exists(this.Path);
			public void Deprecate() => this.Path = null;
			public void Combine(string path) => this.Path = System.IO.Path.Combine(this.Path, path);
			public void AppendSuffix(string value)
			{
				if(string.IsNullOrEmpty(value) || value == "/" || value == "\\")
					return;

				if(string.IsNullOrEmpty(this.Suffix))
					this.Suffix = value;
				else
					this.Suffix = $"{this.Suffix}/{value}";
			}

			public static PathToken Create(string fullPath, string prefix)
			{
				if(prefix != null && prefix.Length > 0 && fullPath.StartsWith(prefix))
					return new PathToken(fullPath, fullPath.Substring(prefix.Length));

				return new PathToken(fullPath);
			}

			public override string ToString() => string.IsNullOrEmpty(this.Suffix) ? this.Path : $"{this.Path}?{this.Suffix}";
		}
		#endregion
	}
}
