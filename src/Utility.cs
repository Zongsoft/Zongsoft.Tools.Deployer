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

using Zongsoft.Services;
using Zongsoft.Terminals;

namespace Zongsoft.Tools.Deployer
{
	internal static class Utility
	{
		private const string FRAMEWORK_VARIABLE = "Framework";

		public static readonly char[] TARGET_SEPARATORS = new[] { ',', ';' };
		public static readonly char[] PATH_SEPARATORS = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

		public static bool IsDirectory(string path) => !string.IsNullOrEmpty(path) && IsDirectorySeparator(path[^1]);
		public static bool IsDirectorySeparator(char chr) => chr == Path.DirectorySeparatorChar || chr == Path.AltDirectorySeparatorChar;

		public static string GetTargetFramework(IDictionary<string, string> variables) => TryGetTargetFramework(variables, out var value) ? value : null;
		public static bool TryGetTargetFramework(IDictionary<string, string> variables, out string value)
		{
			if(variables == null || variables.Count == 0)
			{
				value = null;
				return false;
			}

			return variables.TryGetValue(FRAMEWORK_VARIABLE, out value) && !string.IsNullOrEmpty(value);
		}

		public static bool IsTargetFramework(IDictionary<string, string> variables, string targets)
		{
			if(string.IsNullOrEmpty(targets))
				return true;

			return IsTargetFramework(GetTargetFramework(variables), string.IsNullOrEmpty(targets) ? Array.Empty<string>() : targets.Split(TARGET_SEPARATORS, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
		}

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
			terminal.Write(CommandOutletColor.Magenta, Properties.Resources.Tips_Prompt);
			terminal.WriteLine(CommandOutletColor.DarkCyan, string.Format(Properties.Resources.FileDeleteSucceed_Message, filePath));
		}

		public static void FileDeletedFailed(this ITerminal terminal, string filePath)
		{
			terminal.Write(CommandOutletColor.Magenta, Properties.Resources.Tips_Prompt);
			terminal.WriteLine(CommandOutletColor.DarkCyan, string.Format(Properties.Resources.FileDeleteFailed_Message, filePath));
		}

		public static void FileNotExists(this ITerminal terminal, string filePath)
		{
			if(string.IsNullOrEmpty(filePath))
				return;

			terminal.Write(CommandOutletColor.Magenta, Properties.Resources.Warn_Prompt);

			if(Deployer.IsDeploymentFile(filePath))
				terminal.WriteLine(CommandOutletColor.DarkMagenta, string.Format(Properties.Resources.DeploymentFileNotExists_Message, filePath));
			else
				terminal.WriteLine(CommandOutletColor.DarkYellow, string.Format(Properties.Resources.FileNotExists_Message, filePath));
		}

		public static void UndefinedVariable(this ITerminal terminal, string variable, string expression) => UndefinedVariable(terminal, variable, expression, null, -1);
		public static void UndefinedVariable(this ITerminal terminal, string variable, string expression, string filePath, int lineNumber = -1)
		{
			if(lineNumber >= 0)
				filePath = $"{filePath} (#{lineNumber})";

			terminal.Write(CommandOutletColor.Red, Properties.Resources.Error_Prompt);

			if(string.IsNullOrEmpty(filePath))
				terminal.WriteLine(CommandOutletColor.DarkRed, string.Format(Properties.Resources.VariableUndefined_Message, variable, expression));
			else
				terminal.WriteLine(CommandOutletColor.DarkRed, string.Format(Properties.Resources.VariableUndefinedInFile_Message, variable, expression, filePath));
		}

		public static void CompleteDeployment(this ITerminal terminal, string filePath, DeploymentCounter counter, bool final)
		{
			var content = CommandOutletContent
				.Create(CommandOutletColor.DarkGreen, string.Format(Properties.Resources.DeploymentComplete_Message, filePath, counter.Total))
				.Append(Properties.Resources.DeploymentComplete_CountBegin)
				.Append(CommandOutletColor.Green, string.Format(Properties.Resources.DeploymentComplete_SucceedCount, counter.Successes))
				.Append(Properties.Resources.DeploymentComplete_CountSparator)
				.Append(CommandOutletColor.DarkRed, string.Format(Properties.Resources.DeploymentComplete_FailedCount, counter.Failures))
				.Append(Properties.Resources.DeploymentComplete_CountEnd);

			var count = 0;
			var message = CommandOutletContent.GetFullText(content);

			for(int i = 0; i < message.Length; i++)
				count += char.IsAscii(message[i]) ? 1 : 2;

			terminal.WriteLine(new string('-', count));
			terminal.WriteLine(content);
			terminal.WriteLine(new string('-', count));

			if(!final)
				terminal.WriteLine();
		}

		/// <summary>
		/// 提供部署项必须条件处理的工具类。
		/// </summary>
		public static class Requisition
		{
			public static (string name, string value) GetRequisites(Configuration.Profiles.ProfileEntry entry, out string requisites)
			{
				var entryName = GetRequisites(entry.Name, out var requisitesName).ToString();
				var entryValue = GetRequisites(entry.Value, out var requisitesValue).ToString();

				if(requisitesName.IsEmpty)
					requisites = requisitesValue.IsEmpty ? null : requisitesValue.ToString();
				else
					requisites = requisitesValue.IsEmpty ? requisitesName.ToString() : $"{requisitesName} & {requisitesValue}";

				return (entryName, entryValue);
			}

			public static ReadOnlySpan<char> GetRequisites(ReadOnlySpan<char> text, out ReadOnlySpan<char> requisites)
			{
				requisites = default;

				if(text.IsEmpty)
					return text;

				var index = text.IndexOf("<");
				if(index > 0)
				{
					requisites = text[(index + 1)..].Trim();
					text = text[..index].Trim();

					index = requisites.IndexOf('>');
					if(index > 0)
						requisites = requisites[..index].Trim();
				}

				return text.Trim();
			}

			public static bool IsRequisites(IDictionary<string, string> variables, ReadOnlySpan<char> requisites)
			{
				if(requisites.IsEmpty)
					return true;

				var combiner = '|';
				var position = 0;
				bool? result = null;

				for(int i = 1; i < requisites.Length; i++)
				{
					if(requisites[i] == '|' || requisites[i] == '&')
					{
						var requisite = requisites[position..i].Trim();
						var matched = IsRequisite(variables, requisite);
						result = GetResult(result, matched, combiner);

						combiner = requisites[i];
						position = i + 1;
					}
				}

				if(position < requisites.Length - 1)
				{
					var matched = IsRequisite(variables, requisites[position..].Trim());
					return GetResult(result, matched, combiner);
				}

				return result ?? true;

				static bool GetResult(bool? result, bool value, char combiner)
				{
					if(result == null)
						return value;

					if(combiner == '|')
						return result.Value || value;
					else
						return result.Value && value;
				}
			}

			private static bool IsRequisite(IDictionary<string, string> variables, ReadOnlySpan<char> requisite)
			{
				if(requisite.IsEmpty)
					return true;

				bool result;
				ReadOnlySpan<char> name, value;
				var index = requisite.IndexOf(':');

				switch(index)
				{
					case 0:
						return false;
					case < 0:
						name = requisite[0] == '!' ? requisite[1..].Trim() : requisite.Trim();
						result = variables.ContainsKey(name.ToString());
						return requisite[0] == '!' ? !result : result;
					default:
						name = requisite[0] == '!' ? requisite[1..index].Trim() : requisite[0..index].Trim();
						value = requisite[(index + 1)..].Trim();

						if(value.IsEmpty)
						{
							result = variables.ContainsKey(name.ToString());
							return requisite[0] == '!' ? !result : result;
						}

						if(name.Equals(FRAMEWORK_VARIABLE, StringComparison.OrdinalIgnoreCase))
						{
							result = IsTargetFramework(variables, value.ToString());
							return requisite[0] == '!' ? !result : result;
						}

						if(variables.TryGetValue(name.ToString(), out var variable))
						{
							var parts = value.ToString().Split(',', StringSplitOptions.TrimEntries);
							result = parts.Contains(variable.Trim(), StringComparer.OrdinalIgnoreCase);
							return requisite[0] == '!' ? !result : result;
						}

						return requisite[0] == '!';
				}
			}
		}
	}
}
