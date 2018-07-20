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

using Zongsoft.Services;
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

			try
			{
				//使用当前命令行参数构造一个命令表达式
				var expression = CommandExpression.Parse("deployer " + string.Join(" ", args));

				//创建一个部署文件路径的列表
				var paths = new List<string>(expression.Arguments.Length);

				//校验所有指定的文件路径是否都存在，并将处理后的路径加入到待处理的列表中
				foreach(var argument in expression.Arguments)
				{
					var path = Path.IsPathRooted(argument) ? argument : Zongsoft.IO.Path.Combine(Environment.CurrentDirectory, argument);

					if(File.Exists(path))
						paths.Add(path);
					else
						throw new FileNotFoundException(path);
				}

				//创建部署器类的实例
				var deployer = new Deployer(Zongsoft.Terminals.ConsoleTerminal.Instance);

				//将命令行选项添加到部署器的环境变量中
				if(expression.Options.Count > 0)
				{
					foreach(var option in expression.Options)
					{
						deployer.EnvironmentVariables[option.Key] = option.Value;
					}
				}

				//依次部署指定的部署文件
				foreach(var path in paths)
				{
					//部署指定的文件
					var counter = deployer.Deploy(path);

					//打印部署的结果信息
					deployer.Terminal.WriteLine(CommandOutletColor.DarkGreen, ResourceUtility.GetString("Text.Deploy.CompleteInfo", path, counter.Total, counter.Successes, counter.Failures));
				}
			}
			catch(Exception ex)
			{
				//设置控制台前景色为“红色”
				var foregroundColor = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;

				//打印异常消息
				Console.Error.WriteLine(ex.Message);

				//重置控制台的前景色
				Console.ForegroundColor = foregroundColor;
			}
		}
	}
}
