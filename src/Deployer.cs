/*
 * Authors:
 *   钟峰(Popeye Zhong) <9555843@qq.com>
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
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Zongsoft.Services;
using Zongsoft.Resources;
using Zongsoft.Terminals;
using Zongsoft.Options.Profiles;

namespace Zongsoft.Utilities
{
	public class Deployer
	{
		#region 常量定义
		private const string DEPLOYMENTDIRECTORY_PARAMETER = "deploymentDirectory";
		private const string IGNORERESOLVEDEPLOYMENTFILE_PARAMETER = "ignoreResolve";
		#endregion

		#region 成员字段
		private ITerminal _terminal;
		private IDictionary<string, string> _environmentVariables;
		#endregion

		#region 构造函数
		public Deployer(ITerminal terminal)
		{
			if(terminal == null)
				throw new ArgumentNullException(nameof(terminal));

			_terminal = terminal;
			_environmentVariables = Zongsoft.Collections.DictionaryExtension.ToDictionary<string, string>(Environment.GetEnvironmentVariables());
		}
		#endregion

		#region 公共属性
		public ITerminal Terminal
		{
			get
			{
				return _terminal;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_terminal = value;
			}
		}

		public IDictionary<string, string> EnvironmentVariables
		{
			get
			{
				return _environmentVariables;
			}
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
				throw new FileNotFoundException(ResourceUtility.GetString("Text.DeploymentFileNotExists", deploymentFilePath));

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

			foreach(var item in context.DeploymentFile.Items)
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
					Utility.EnsureDirectory(context.DestinationDirectory, this.Normalize(section.FullName.Replace(' ', '/')));

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
			var sourcePath = this.Normalize(entry.Name);

			if(!Path.IsPathRooted(sourcePath))
				sourcePath = Zongsoft.IO.Path.Combine(context.SourceDirectory, sourcePath);

			var sourceName = Path.GetFileName(sourcePath);
			var destinationName = string.IsNullOrWhiteSpace(entry.Value) ? sourceName : this.Normalize(entry.Value);
			var destinationDirectory = context.DestinationDirectory;

			if(entry.Section != null)
				destinationDirectory = Zongsoft.IO.Path.Combine(context.DestinationDirectory, this.Normalize(entry.Section.FullName.Replace(' ', '/')));

			if(sourceName.Contains("*") || sourceName.Contains("?"))
			{
				var directory = new DirectoryInfo(Path.GetDirectoryName(sourcePath));

				if(!directory.Exists)
				{
					_terminal.Write(CommandOutletColor.Magenta, ResourceUtility.GetString("Text.Warn"));
					_terminal.WriteLine(CommandOutletColor.Yellow, ResourceUtility.GetString("Text.DirectoryNotExists", directory.FullName));
					return;
				}

				var files = directory.EnumerateFiles(sourceName);

				foreach(var file in files)
				{
					//执行文件复制
					if(this.CopyFile(file.FullName, Path.Combine(destinationDirectory, file.Name)))
						context.Counter.IncrementSuccesses();
					else
						context.Counter.IncrementFailures();
				}

				return;
			}

			if(!File.Exists(sourcePath))
			{
				//累加文件复制失败计数器
				context.Counter.IncrementFailures();

				_terminal.Write(CommandOutletColor.Magenta, ResourceUtility.GetString("Text.Warn"));
				_terminal.WriteLine(CommandOutletColor.DarkYellow, ResourceUtility.GetString("Text.FileNotExists", sourcePath));

				return;
			}

			//如果指定要拷贝的源文件是一个部署文件
			if(this.IsDeploymentFile(sourceName))
			{
				//如果没有指定忽略处理子部署文件，则进行子部署文件的递归处理
				if(!_environmentVariables.ContainsKey(IGNORERESOLVEDEPLOYMENTFILE_PARAMETER))
				{
					var counter = this.Deploy(sourcePath, destinationDirectory);

					context.Counter.IncrementFailures(counter.Failures);
					context.Counter.IncrementSuccesses(counter.Successes);

					return;
				}
			}

			//执行文件复制
			if(this.CopyFile(sourcePath, Path.Combine(destinationDirectory, destinationName)))
				context.Counter.IncrementSuccesses();
			else
				context.Counter.IncrementFailures();
		}

		private string Normalize(string text)
		{
			if(string.IsNullOrWhiteSpace(text))
				return string.Empty;

			var result = text;

			foreach(var parameter in _environmentVariables)
			{
				if(string.IsNullOrWhiteSpace(parameter.Key))
					continue;

				result = Regex.Replace(result, @"\$\(" + Common.StringExtension.RemoveCharacters(parameter.Key, @"`~!@#$%^&*()+={}[]\|:;""'<>,.?/") + @"\)", parameter.Value, RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
			}

			return result;
		}

		private bool CopyFile(string source, string destination)
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

		private bool IsDeploymentFile(string filePath)
		{
			if(string.IsNullOrWhiteSpace(filePath))
				return false;

			//如果指定的文件的扩展名为.deploy，则判断为部署文件
			return string.Equals(Path.GetExtension(filePath), ".deploy", StringComparison.OrdinalIgnoreCase);
		}
		#endregion
	}
}
