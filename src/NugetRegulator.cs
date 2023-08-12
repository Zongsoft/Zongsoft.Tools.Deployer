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
 * Copyright (C) 2015-2023 Zongsoft Corporation <http://www.zongsoft.com>
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

namespace Zongsoft.Tools.Deployer
{
	public class NugetRegulator : IDirectoryRegulator
	{
		#region 单例字段
		public static readonly NugetRegulator Instance = new NugetRegulator();
		#endregion

		#region 私有构造
		private NugetRegulator() { }
		#endregion

		#region 公共方法
		public bool Regulate(string directory, IDictionary<string, string> variables, out string result)
		{
			result = null;

			if(variables == null || variables.Count == 0 || string.IsNullOrEmpty(directory))
				return false;

			//如果获取Nuget包路径失败则返回
			if(!NugetUtility.TryGetPackagesDirectory(variables, out var nugetDirectory))
				return false;

			//如果指定的路径是不是Nuget包路径的子路径则返回
			if(!directory.StartsWith(nugetDirectory, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
				return false;

			//如果没有指定“目标框架”参数则无需进行路径修整
			if(!Utility.TryGetTargetFramework(variables, out var framework))
				return false;

			//解析当前Nuget包路径中的前导部分和“目标框架”名部分
			if(ResolveNugetDirectory(directory, out var precursor, out var frameworkInPath))
			{
				//如果待替换的目录名是“目标框架”名则不需要替换
				if(frameworkInPath == framework.AsSpan())
					return false;

				//将Nuget包目录的最后的目录名替换成“目标框架”参数值
				var regulatedPath = Path.Combine(precursor.ToString(), framework);

				//如果替换成“目标框架”版本的Nuget包目录是存在的，则表示可替换成该“目标框架”包目录
				if(Directory.Exists(regulatedPath))
				{
					result = regulatedPath;
					return true;
				}
			}

			return false;
		}
		#endregion

		#region 私有方法
		/*
		 * 譬如 Nuget 包路径如下：
		 *	C:\Users\Administrator\.nuget\packages\mysql.data\8.1.0\lib\netstandard2.0
		 *	
		 * 前导路径 precursor 参数的返回值为：C:\Users\Administrator\.nuget\packages\mysql.data\8.1.0\lib
		 * 路径目标 framework 参数的返回值为：netstandard2.0
		 */
		private static bool ResolveNugetDirectory(string directory, out ReadOnlySpan<char> precursor, out ReadOnlySpan<char> framework)
		{
			int count = 0;
			int position;

			do
			{
				position = directory.LastIndexOfAny(new[] { '/', '\\' }, directory.Length - ++count);
			} while(position == directory.Length - count);

			if(position > 0)
			{
				var span = directory.AsSpan();
				precursor = span[..position];
				framework = span.Slice(position + 1, directory.Length - position - count);
				return true;
			}

			framework = ReadOnlySpan<char>.Empty;
			precursor = ReadOnlySpan<char>.Empty;
			return false;
		}
		#endregion
	}
}