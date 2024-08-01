/*
 *   _____                                ______
 *  /_   /  ____  ____  ____  _________  / __/ /_
 *    / /  / __ \/ __ \/ __ \/ ___/ __ \/ /_/ __/
 *   / /__/ /_/ / / / / /_/ /\_ \/ /_/ / __/ /_
 *  /____/\____/_/ /_/\__  /____/\____/_/  \__/
 *                   /____/
 *
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
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

using Zongsoft.Terminals;
using Zongsoft.Configuration.Profiles;

namespace Zongsoft.Tools.Deployer
{
	public class Deployer
	{
		#region 常量定义
		internal const string IGNOREDEPLOYMENTFILE_OPTION = "ignoreDeploymentFile";
		internal const string DESTINATION_OPTION = "destination";
		internal const string EXPANSION_OPTION = "expansion";
		internal const string OVERWRITE_OPTION = "overwrite";
		internal const string VERBOSITY_OPTION = "verbosity";

		//变量解析的正则组名称
		private const string REGEX_VARIABLE_NAME = "name";
		//变量解析的正则表达式（变量包括两种语法：$(variable) 或 %variable%）
		private static readonly Regex _variableRegex = new(@"(?<opt>\$\((?<name>\w+)\))|(?<env>\%(?<name>\w+)\%)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
		#endregion

		#region 成员字段
		private ITerminal _terminal;
		private readonly IDictionary<string, string> _variables;
		#endregion

		#region 构造函数
		public Deployer(ITerminal terminal, IEnumerable<KeyValuePair<string, string>> options)
		{
			_terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
			_variables = Collections.DictionaryExtension.ToDictionary<string, string>(Environment.GetEnvironmentVariables(), StringComparer.OrdinalIgnoreCase);

			//将部署目录中的 appsettings.json 文件内容解析后加载到变量集
			AppSettingsUtility.Load(_variables);
			//初始化 Nuget 相关的变量
			NugetUtility.Initialize(_variables);

			if(options != null)
			{
				foreach(var option in options)
					_variables[option.Key] = Normalize(option.Value, _variables, variable => throw new TerminalCommandExecutor.ExitException(-1, string.Format(Properties.Resources.VariableUndefinedInOption_Message, variable, option.Value)));
			}
		}
		#endregion

		#region 公共属性
		public ITerminal Terminal => _terminal;
		public IDictionary<string, string> Variables => _variables;
		#endregion

		#region 公共方法
		public DeploymentCounter Deploy(string deploymentFilePath, string destinationDirectory = null)
		{
			if(string.IsNullOrWhiteSpace(deploymentFilePath))
				throw new ArgumentNullException(nameof(deploymentFilePath));

			if(!Path.IsPathRooted(deploymentFilePath))
				deploymentFilePath = Path.Combine(Environment.CurrentDirectory, deploymentFilePath);

			//对部署文件路径进行参数规整
			deploymentFilePath = Normalize(deploymentFilePath, _variables, variable => _terminal.UndefinedVariable(variable, deploymentFilePath));

			if(!File.Exists(deploymentFilePath))
			{
				//如果指定部署文件路径是个目录，并且该目录下有一个名为.deploy的文件，则将该部署文件路径指向它
				if(Directory.Exists(deploymentFilePath) && File.Exists(Path.Combine(deploymentFilePath, ".deploy")))
					deploymentFilePath = Path.Combine(deploymentFilePath, ".deploy");
				else
				{
					//打印部署文件不存在的消息
					_terminal.FileNotExists(deploymentFilePath);
					//返回部署计数器
					return new DeploymentCounter(1, 0);
				}
			}

			if(string.IsNullOrWhiteSpace(destinationDirectory))
			{
				if(_variables.TryGetValue(DESTINATION_OPTION, out destinationDirectory))
				{
					if(!Path.IsPathRooted(destinationDirectory))
						destinationDirectory = Path.Combine(Environment.CurrentDirectory, destinationDirectory);
				}
				else
				{
					destinationDirectory = Environment.CurrentDirectory;
				}
			}

			//对目标目录路径进行参数规整
			destinationDirectory = Normalize(destinationDirectory, _variables, variable => _terminal.UndefinedVariable(variable, destinationDirectory));

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
					Utility.EnsureDirectory(context.DestinationDirectory,
						Normalize(
							section.FullName.Replace(' ', '/'),
							_variables,
							variable => _terminal.UndefinedVariable(variable, $"[{section.FullName}]", section.Profile.FilePath, section.LineNumber)
						)
					);

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
			//获取部署条目的必须条件
			(var entryName, var entryValue) = Utility.Requisition.GetRequisites(entry, out var requisites);

			//如果不满足必须条件则忽略该部署条目
			if(!Utility.Requisition.IsRequisites(_variables, requisites))
				return;

			var destinationName = string.IsNullOrWhiteSpace(entryValue) ? string.Empty :
				Normalize(entryValue, _variables, variable => _terminal.UndefinedVariable(variable, entryValue, entry.Profile.FilePath, entry.LineNumber));
			var destinationDirectory = context.DestinationDirectory;

			if(entry.Section != null)
				destinationDirectory = Path.Combine(context.DestinationDirectory,
					Normalize(entry.Section.FullName.Replace(' ', Path.DirectorySeparatorChar), _variables, variable => _terminal.UndefinedVariable(variable, $"[{entry.Section.FullName}]", entry.Profile.FilePath, entry.LineNumber)));

			//以叹号打头的部署条目表示将其对应目标位置的匹配文件删除
			var deletabled = entryName[0] == '!';
			var sourcePath = deletabled ? entryName[1..] : entryName;

			sourcePath = Normalize(sourcePath, _variables, variable => _terminal.UndefinedVariable(variable, entryName, entry.Profile.FilePath, entry.LineNumber));

			if(!Path.IsPathRooted(sourcePath))
				sourcePath = Path.Combine(context.SourceDirectory, sourcePath);

			//由于源路径中可能含有通配符，因此必须查找匹配的文件集
			foreach(var sourceFile in GetFiles(sourcePath, _variables))
			{
				var destinationFile = Path.Combine(GetDestinationDirectory(destinationDirectory, sourceFile.Suffix), string.IsNullOrEmpty(destinationName) ? Path.GetFileName(sourceFile.Path) : destinationName);

				//如果是删除项
				if(deletabled)
				{
					if(DeleteFile(destinationFile))
					{
						if(!this.IsQuietMode)
							_terminal.FileDeletedSucceed(destinationFile);
					}
					else
					{
						if(!this.IsQuietMode)
							_terminal.FileDeletedFailed(destinationFile);
					}

					continue;
				}

				if(!sourceFile.Exists())
				{
					//累加文件复制失败计数器
					context.Counter.Fail();

					//打印文件不存在的消息（如果是静默模式则不打印提示消息）
					if(!this.IsQuietMode)
						_terminal.FileNotExists(sourceFile.Path);

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

				//获取覆盖选项
				_variables.TryGetValue(OVERWRITE_OPTION, out var overwrite);

				//执行文件复制
				if(CopyFile(sourceFile.Path, destinationFile, overwrite))
					context.Counter.Success();
				else
					context.Counter.Fail();
			}

			static string GetDestinationDirectory(string root, string suffix) => string.IsNullOrEmpty(suffix) ? root : Path.Combine(root, suffix);
		}

		private static string Normalize(string text, IDictionary<string, string> variables, Action<string> failure)
		{
			if(string.IsNullOrWhiteSpace(text))
				return string.Empty;

			return _variableRegex.Replace(text.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar), match =>
			{
				if(match.Success && match.Groups.TryGetValue(REGEX_VARIABLE_NAME, out var group))
				{
					if(variables.TryGetValue(group.Value, out var value))
						return value;

					failure?.Invoke(group.Value);
				}

				return null;
			});
		}

		private static IEnumerable<PathToken> GetFiles(string filePath, IDictionary<string, string> variables)
		{
			if(string.IsNullOrEmpty(filePath))
				yield break;

			var directoryName = Path.GetDirectoryName(filePath);
			var fileName = Path.GetFileName(filePath);

			if(string.IsNullOrEmpty(fileName))
				yield break;

			foreach(var directory in GetDirectories(directoryName, variables.ContainsKey(EXPANSION_OPTION)))
			{
				//对当前目录路径进行修正和调整
				if(DirectoryRegulator.Regulate(directory.Path, variables, out var result))
					directory.Path = result;

				if(fileName.Contains('*') || fileName.Contains('?'))
				{
					//如果指定目录不存在则跳过，否则后面的代码会引发系统IO异常
					if(!Directory.Exists(directory.Path))
						continue;

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
			var parts = Common.StringExtension.Slice(directory, Utility.PATH_SEPARATORS).ToArray();
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

					//如果指定目录不存在则跳过，否则后面的代码会引发系统IO异常
					if(!Directory.Exists(origin))
						continue;

					directories.AddRange(Directory.GetDirectories(origin, "*", SearchOption.AllDirectories).Select(p => PathToken.Create(p, origin)));
				}
				else if(parts[i].Contains('*') || parts.Contains("?"))
				{
					if(directories == null)
						directories = new List<PathToken>();

					flags |= Asterisk1;
					var origin = Path.Combine(parts.Take(i).ToArray());

					//如果指定目录不存在则跳过，否则后面的代码会引发系统IO异常
					if(!Directory.Exists(origin))
						continue;

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

		private static bool DeleteFile(string filePath)
		{
			try
			{
				File.Delete(filePath);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private static bool CopyFile(string source, string destination, string overwrite)
		{
			if(string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(destination))
				return false;

			var copyRequired = true;

			if(File.Exists(destination) && string.Equals(overwrite, "latest", StringComparison.OrdinalIgnoreCase))
				copyRequired = File.GetLastWriteTime(source) >= File.GetLastWriteTime(destination);

			if(copyRequired)
			{
				var directory = Path.GetDirectoryName(destination);
				if(!Directory.Exists(directory))
					Directory.CreateDirectory(directory);

				File.Copy(source, destination, true);
			}

			return copyRequired;
		}

		internal static bool IsDeploymentFile(string filePath)
		{
			if(string.IsNullOrWhiteSpace(filePath))
				return false;

			//如果指定的文件的扩展名为.deploy，则判断为部署文件
			return string.Equals(Path.GetExtension(filePath), ".deploy", StringComparison.OrdinalIgnoreCase);
		}

		private bool IsQuietMode => _variables.TryGetValue(VERBOSITY_OPTION, out var verbosity) && string.Equals(verbosity, "quiet", StringComparison.OrdinalIgnoreCase);
		#endregion

		#region 嵌套子类
		private class PathToken
		{
			public PathToken(string path, string suffix = null)
			{
				this.Path = path;
				this.Suffix = string.IsNullOrEmpty(suffix) ? null : suffix.Trim(Utility.PATH_SEPARATORS);
			}

			public string Path;
			public string Suffix;

			public bool Exists() => !string.IsNullOrEmpty(this.Path) && File.Exists(this.Path);
			public void Deprecate() => this.Path = null;
			public void Combine(string path) => this.Path = System.IO.Path.Combine(this.Path, path);
			public void AppendSuffix(string value)
			{
				if(string.IsNullOrEmpty(value) || Utility.IsDirectory(value))
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