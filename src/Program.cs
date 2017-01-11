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
using System.Collections.Generic;

using Zongsoft.Resources;

namespace Zongsoft.Utilities
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			if(args == null || args.Length < 1)
			{
				Console.WriteLine(ResourceUtility.GetString("Text.MissingArguments"));
				return;
			}

			var filePaths = new List<string>();
			var parameters = new Dictionary<string, string>(ToDictionary<string, string>(Environment.GetEnvironmentVariables()), StringComparer.OrdinalIgnoreCase);
			var deployer = new Deployer(new Zongsoft.Terminals.ConsoleTerminal());

			for(int i = 0; i < args.Length;i++)
			{
				var arg = args[i].Trim();

				if(string.IsNullOrWhiteSpace(arg))
					continue;

				if(arg[0] == '/' || arg[0] == '-')
				{
					var parts = arg.Split(':', '=');

					if(parts.Length == 1)
						parameters[parts[0].Substring(1)] = null;
					else if(parts.Length == 2)
						parameters[parts[0].Substring(1)] = parts[1];
					else
					{
						Console.WriteLine(ResourceUtility.GetString("Text.InvalidArgumentFormat", arg));
						return;
					}
				}
				else
					filePaths.Add(arg);
			}

			foreach(var filePath in filePaths)
			{
				deployer.Deploy(filePath, parameters);
			}
		}

		private static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(System.Collections.IDictionary dictionary, Func<object, TKey> convertKey = null, Func<object, TValue> convertValue = null)
		{
			if(dictionary == null)
				return null;

			var result = new Dictionary<TKey, TValue>(dictionary.Count);

			TKey key;
			TValue value;

			foreach(System.Collections.DictionaryEntry entry in dictionary)
			{
				key = convertKey != null ? convertKey(entry.Key) : Zongsoft.Common.Convert.ConvertValue<TKey>(entry.Key);
				value = convertValue != null ? convertValue(entry.Value) : Zongsoft.Common.Convert.ConvertValue<TValue>(entry.Value);

				result.Add(key, value);
			}

			return result;
		}
	}
}
