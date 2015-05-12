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

using Zongsoft.Options;
using Zongsoft.Options.Profiles;
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
			var parameters = new Dictionary<string, string>();
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
	}
}
