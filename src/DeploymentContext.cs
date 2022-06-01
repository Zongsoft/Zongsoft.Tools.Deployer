/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@qq.com>
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

namespace Zongsoft.Tools.Deployer
{
	public class DeploymentContext
	{
		#region 构造函数
		public DeploymentContext(Deployer deployer, Zongsoft.Configuration.Profiles.Profile deploymentProfile, string destinationDirectory)
		{
			if(string.IsNullOrWhiteSpace(destinationDirectory))
				throw new ArgumentNullException(nameof(destinationDirectory));

			this.Deployer = deployer ?? throw new ArgumentNullException(nameof(deployer));
			this.DeploymentProfile = deploymentProfile ?? throw new ArgumentNullException(nameof(deploymentProfile));
			this.DestinationDirectory = destinationDirectory;
			this.Counter = new DeploymentCounter();
		}
		#endregion

		#region 公共属性
		public Deployer Deployer { get; init; }
		public DeploymentCounter Counter { get; init; }
		public string DestinationDirectory { get; init; }
		public Configuration.Profiles.Profile DeploymentProfile { get; init; }
		public string SourceDirectory => Path.GetDirectoryName(this.DeploymentProfile.FilePath);
		#endregion

		#region 公共方法
		public void Count(DeploymentCounter counter)
		{
			this.Counter.Fail(counter.Failures);
			this.Counter.Success(counter.Successes);
		}
		#endregion
	}
}
