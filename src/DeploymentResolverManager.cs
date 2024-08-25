﻿/*
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
 * Copyright (C) 2015-2024 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Threading;
using System.Threading.Tasks;

namespace Zongsoft.Tools.Deployer
{
	public static class DeploymentResolverManager
	{
		public static IDeploymentResolver GetResolver(string name)
		{
			if(string.IsNullOrEmpty(name))
				return DefaultResolver.Instance;

			return name.ToLowerInvariant() switch
			{
				"nuget" => NugetResolver.Instance,
				"delete" or "remove" => DeleteResolver.Instance,
				_ => null,
			};
		}

		public class DeleteResolver : IDeploymentResolver
		{
			#region 单例字段
			public static readonly DeleteResolver Instance = new();
			#endregion

			#region 私有构造
			private DeleteResolver() { }
			#endregion

			#region 公共属性
			public string Name => "Delete";
			#endregion

			#region 公共方法
			public Task ResolveAsync(DeploymentContext context, DeploymentEntry deployment, CancellationToken cancellation)
			{
				var filePath = Path.Combine(deployment.Destination.Path, deployment.Source.Name);

				if(DeleteFile(filePath))
				{
					if(context.IsVerbosity(Verbosity.Detail))
						context.Deployer.Terminal.FileDeletedSucceed(filePath);
				}
				else
				{
					if(!context.IsVerbosity(Verbosity.Quiet))
						context.Deployer.Terminal.FileDeletedFailed(filePath);
				}

				return Task.CompletedTask;
			}
			#endregion

			#region 私有方法
			private static bool DeleteFile(string filePath)
			{
				try
				{
					if(!string.IsNullOrEmpty(filePath))
						File.Delete(filePath);

					return true;
				}
				catch
				{
					return false;
				}
			}
			#endregion
		}

		public class DefaultResolver : DeploymentResolverBase
		{
			#region 单例字段
			public static readonly DefaultResolver Instance = new();
			#endregion

			#region 私有构造
			private DefaultResolver() : base(string.Empty) { }
			#endregion
		}
	}
}