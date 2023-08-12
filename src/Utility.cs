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
using System.Collections.Generic;

using Zongsoft.Services;
using Zongsoft.Terminals;

namespace Zongsoft.Tools.Deployer
{
	internal static class Utility
	{
		public static readonly char[] TARGET_SEPARATORS = new[] { ',', ';' };
		public static readonly char[] PATH_SEPARATORS = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

		public static bool IsDirectory(string path) => !string.IsNullOrEmpty(path) && IsDirectorySeparator(path[^1]);
		public static bool IsDirectorySeparator(char chr) => chr == Path.DirectorySeparatorChar || chr == Path.AltDirectorySeparatorChar;

		public static string GetTargetFramework(IDictionary<string, string> variables) => TryGetTargetFramework(variables, out var value) ? value : null;
		public static bool TryGetTargetFramework(IDictionary<string, string> variables, out string value)
		{
			const string FRAMEWORK_VARIABLE = "Framework";

			if(variables == null || variables.Count == 0)
			{
				value = null;
				return false;
			}

			return variables.TryGetValue(FRAMEWORK_VARIABLE, out value) && !string.IsNullOrEmpty(value);
		}

		public static bool IsTargetFramework(IDictionary<string, string> variables, string targets) =>
			IsTargetFramework(GetTargetFramework(variables), string.IsNullOrEmpty(targets) ? Array.Empty<string>() : targets.Split(TARGET_SEPARATORS, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
		public static bool IsTargetFramework(string value, params string[] targets)
		{
			if(string.IsNullOrEmpty(value))
				return targets == null || targets.Length == 0;

			if(targets == null || targets.Length == 0)
				return true;

			var framework = TargetFramework.Parse(value);

			for(int i = 0; i < targets.Length; i++)
			{
				if(string.IsNullOrEmpty(targets[i]))
					continue;

				var uplook = targets[i][^1] == '^';
				var target = uplook ? TargetFramework.Parse(targets[i].AsSpan()[..^1]) : TargetFramework.Parse(targets[i]);

				if(framework.IsFramework(target.Framework) && framework.IsPlatform(target.Platform))
				{
					if(uplook ? framework.FrameworkVersion >= target.FrameworkVersion : framework.FrameworkVersion == target.FrameworkVersion)
						return true;
				}
			}

			return false;
		}

		public static string EnsureDirectory(params string[] paths)
		{
			if(paths == null)
				throw new ArgumentNullException(nameof(paths));

			if(paths.Length == 0)
				return string.Empty;

			var fullPath = Path.Combine(paths);

			if(!Directory.Exists(fullPath))
				Directory.CreateDirectory(fullPath);

			return fullPath;
		}

		public static void FileDeletedSucceed(this ITerminal terminal, string filePath)
		{
			terminal.Write(CommandOutletColor.Magenta, Properties.Resources.Text_Tips);
			terminal.WriteLine(CommandOutletColor.DarkCyan, string.Format(Properties.Resources.Text_FileDeleteSucceed, filePath));
		}

		public static void FileDeletedFailed(this ITerminal terminal, string filePath)
		{
			terminal.Write(CommandOutletColor.Magenta, Properties.Resources.Text_Tips);
			terminal.WriteLine(CommandOutletColor.DarkCyan, string.Format(Properties.Resources.Text_FileDeleteFailed, filePath));
		}

		public static void FileNotExists(this ITerminal terminal, string filePath)
		{
			terminal.Write(CommandOutletColor.Magenta, Properties.Resources.Text_Warn);
			terminal.WriteLine(CommandOutletColor.DarkYellow, string.Format(Properties.Resources.Text_FileNotExists, filePath));
		}

		public static void FileNotExists(this ITerminal terminal, CommandOutletColor color, string message)
		{
			terminal.Write(CommandOutletColor.Magenta, Properties.Resources.Text_Warn);
			terminal.WriteLine(color, message);
		}
	}
}
