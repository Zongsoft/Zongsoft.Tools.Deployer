/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * The MIT License (MIT)
 * 
 * Copyright (C) 2015 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using System.Text.RegularExpressions;

using Zongsoft.Options;
using Zongsoft.Options.Profiles;
using Zongsoft.Resources;
using Zongsoft.Terminals;

namespace Zongsoft.Utilities
{
	public class Deployer
	{
		#region 成员字段
		private ITerminal _terminal;
		private string _workingDirectory;
		private string _currentDirectory;

		private int _fileCountOfSucced;
		private int _fileCountOfFailed;
		#endregion

		#region 构造函数
		public Deployer(ITerminal terminal)
		{
			if(terminal == null)
				throw new ArgumentNullException("terminal");

			_terminal = terminal;
		}

		public Deployer(ITerminal terminal, string workingDirectory)
		{
			if(terminal == null)
				throw new ArgumentNullException("terminal");

			if(string.IsNullOrWhiteSpace(workingDirectory))
				throw new ArgumentNullException("workingDirectory");

			if(!File.Exists(workingDirectory))
				throw new DirectoryNotFoundException(workingDirectory);

			_workingDirectory = workingDirectory;
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

		public string CurrentDirectory
		{
			get
			{
				return string.IsNullOrWhiteSpace(_currentDirectory) ? this.WorkingDirectory : _currentDirectory;
			}
		}

		public string WorkingDirectory
		{
			get
			{
				return string.IsNullOrWhiteSpace(_workingDirectory) ? Environment.CurrentDirectory : _workingDirectory;
			}
		}
		#endregion

		#region 公共方法
		public void Deploy(string filePath, IDictionary<string, string> parameters = null)
		{
			if(string.IsNullOrWhiteSpace(filePath))
				return;

			if(Path.IsPathRooted(filePath))
				_workingDirectory = Path.GetDirectoryName(filePath);
			else
				filePath = Path.Combine(this.WorkingDirectory, filePath);

			if(!File.Exists(filePath))
			{
				_terminal.WriteLine(TerminalColor.Red, ResourceUtility.GetString("Text.DeploymentFileNotExists", filePath));
				return;
			}

			_fileCountOfFailed = 0;
			_fileCountOfSucced = 0;

			var profile = Zongsoft.Options.Profiles.Profile.Load(filePath);

			foreach(var item in profile.Items)
			{
				DeployItem(item, parameters);
			}

			_terminal.WriteLine();
			_terminal.WriteLine(TerminalColor.DarkGreen, ResourceUtility.GetString("Text.Deploy.CompleteInfo", _fileCountOfSucced + _fileCountOfFailed, _fileCountOfSucced, _fileCountOfFailed));
		}
		#endregion

		#region 私有方法
		private void DeployItem(ProfileItem item, IDictionary<string, string> parameters)
		{
			switch(item.ItemType)
			{
				case ProfileItemType.Section:
					_currentDirectory = EnsureDirectory(((ProfileSection)item).FullName.Replace(' ', '/'));

					foreach(var child in ((ProfileSection)item).Items)
						DeployItem(child, parameters);

					break;
				case ProfileItemType.Entry:
					this.DeployEntry((ProfileEntry)item, parameters);
					break;
			}
		}

		private void DeployEntry(ProfileEntry entry, IDictionary<string, string> parameters)
		{
			if(entry == null)
				throw new ArgumentNullException("entry");

			var filePath = this.Format(entry.Name, parameters).Replace('/', Path.DirectorySeparatorChar).Trim();
			var fileName = Path.GetFileName(filePath);

			if(fileName.Contains("*") || fileName.Contains("?"))
			{
				var directory = new DirectoryInfo(Path.GetDirectoryName(filePath));

				if(!directory.Exists)
				{
					_terminal.Write(TerminalColor.Magenta, ResourceUtility.GetString("Text.Warn"));
					_terminal.WriteLine(TerminalColor.Yellow, ResourceUtility.GetString("Text.DirectoryNotExists", directory.FullName));
					return;
				}

				var files = directory.EnumerateFiles(fileName);

				foreach(var file in files)
				{
					//执行文件复制
					this.CopyFile(file.FullName, Path.Combine(this.CurrentDirectory, file.Name));

					//累加文件复制成功计数器
					Interlocked.Increment(ref _fileCountOfSucced);
				}

				return;
			}

			if(!File.Exists(filePath))
			{
				//累加文件复制失败计数器
				Interlocked.Increment(ref _fileCountOfFailed);

				_terminal.Write(TerminalColor.Magenta, ResourceUtility.GetString("Text.Warn"));
				_terminal.WriteLine(TerminalColor.DarkYellow, ResourceUtility.GetString("Text.FileNotExists", filePath));
				return;
			}

			//执行文件复制
			this.CopyFile(filePath, Path.Combine(this.CurrentDirectory, fileName));

			//累加文件复制成计数器
			Interlocked.Increment(ref _fileCountOfSucced);
		}

		private string Format(string text, IDictionary<string, string> parameters)
		{
			if(string.IsNullOrWhiteSpace(text))
				return string.Empty;

			if(parameters == null || parameters.Count < 1)
				return text;

			var result = text;

			foreach(var parameter in parameters)
			{
				if(string.IsNullOrWhiteSpace(parameter.Key))
					continue;

				result = Regex.Replace(result, @"\$\(" + parameter.Key + @"\)", parameter.Value, RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
			}

			return result;
		}

		private string EnsureDirectory(string directoryName)
		{
			if(string.IsNullOrWhiteSpace(directoryName))
				throw new ArgumentNullException("directoryName");

			var fullPath = Path.Combine(this.WorkingDirectory, directoryName.Replace('/', Path.DirectorySeparatorChar));

			if(!Directory.Exists(fullPath))
				Directory.CreateDirectory(fullPath);

			return fullPath;
		}

		private void CopyFile(string source, string destination)
		{
			if(string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(destination))
				return;

			bool isCope = true;

			if(File.Exists(destination))
				isCope = File.GetLastWriteTime(source) > File.GetLastWriteTime(destination);

			if(isCope)
				File.Copy(source, destination, true);
		}
		#endregion
	}
}
