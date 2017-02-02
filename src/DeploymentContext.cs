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

namespace Zongsoft.Utilities
{
	public class DeploymentContext
	{
		#region 成员字段
		private Deployer _deployer;
		private DeploymentCounter _counter;
		private string _destinationDirectory;
		private Zongsoft.Options.Profiles.Profile _deploymentFile;
		#endregion

		#region 构造函数
		public DeploymentContext(Deployer deployer, Zongsoft.Options.Profiles.Profile deploymentFile, string destinationDirectory)
		{
			if(deployer == null)
				throw new ArgumentNullException(nameof(deployer));

			if(deploymentFile == null)
				throw new ArgumentNullException(nameof(deploymentFile));

			if(string.IsNullOrWhiteSpace(destinationDirectory))
				throw new ArgumentNullException(nameof(destinationDirectory));

			_deployer = deployer;
			_deploymentFile = deploymentFile;
			_destinationDirectory = destinationDirectory;
			_counter = new DeploymentCounter();
		}
		#endregion

		#region 公共属性
		public Deployer Deployer
		{
			get
			{
				return _deployer;
			}
		}

		public DeploymentCounter Counter
		{
			get
			{
				return _counter;
			}
		}

		public string DestinationDirectory
		{
			get
			{
				return _destinationDirectory;
			}
		}

		public Zongsoft.Options.Profiles.Profile DeploymentFile
		{
			get
			{
				return _deploymentFile;
			}
		}

		public string SourceDirectory
		{
			get
			{
				return Path.GetDirectoryName(this.DeploymentFile.FilePath);
			}
		}
		#endregion
	}
}
