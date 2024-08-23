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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace Zongsoft.Tools.Deployer
{
	public class NugetResolver : DeploymentResolverBase
	{
		#region 单例字段
		public static readonly NugetResolver Instance = new();
		#endregion

		#region 私有构造
		private NugetResolver() : base("Nuget") { }
		#endregion

		#region 重写方法
		protected override async Task<IEnumerable<DeploymentUtility.PathToken>> GetSourcesAsync(DeploymentContext context, DeploymentEntry deployment, CancellationToken cancellation)
		{
			var argument = Argument.Parse(deployment.Source.Name);
			if(argument.IsEmpty)
			{
				context.Deployer.Terminal.IllegalArgument(deployment.Source.Name, deployment.Profile.FilePath);
				return Array.Empty<DeploymentUtility.PathToken>();
			}

			var version = await GetPackageVersionAsync(context.Variables, argument.Name, argument.Version, cancellation);
			if(version == null)
			{
				context.Deployer.Terminal.NotFound(argument.Name, argument.Version);
				return Array.Empty<DeploymentUtility.PathToken>();
			}

			var path = await DownloadAsync(context.Variables, argument.Name, version, cancellation);
			if(string.IsNullOrEmpty(path))
			{
				context.Deployer.Terminal.DownloadFailed(argument.Name, argument.Version);
				return Array.Empty<DeploymentUtility.PathToken>();
			}

			path = string.IsNullOrEmpty(argument.Path) ?
				Path.Combine(path, Deployer.DEFAULT_DEPLOYMENT_FILENAME) :
				Path.Combine(path, argument.Path);

			return DeploymentUtility.GetFiles(path, context.Variables);
		}
		#endregion

		#region 私有方法
		private static async Task<NuGetVersion> GetPackageVersionAsync(IDictionary<string, string> variables, string name, string version, CancellationToken cancellation)
		{
			using var cache = new SourceCacheContext();
			var repository = GetRepository(variables);
			var resource = repository.GetResource<FindPackageByIdResource>();
			var versions = await resource.GetAllVersionsAsync(name, cache, NullLogger.Instance, cancellation);

			if(string.IsNullOrEmpty(version) || string.Equals(version, "latest", StringComparison.OrdinalIgnoreCase))
				return versions.OrderByDescending(ver => ver).FirstOrDefault();
			else
				return NuGetVersion.TryParse(version, out var nuGetVersion) ? versions.FirstOrDefault(ver => ver == nuGetVersion) : null;
		}

		private static async Task<string> DownloadAsync(IDictionary<string, string> variables, string name, NuGetVersion version, CancellationToken cancellation)
		{
			if(string.IsNullOrEmpty(name) || version == null)
				return null;

			using var cache = new SourceCacheContext();
			var context = new PackageDownloadContext(cache);
			var directory = NugetUtility.GetPackagesDirectory(variables);
			var resource = await GetRepository(variables).GetResourceAsync<DownloadResource>(cancellation);
			using var result = await resource.GetDownloadResourceResultAsync(new PackageIdentity(name, version), context, directory, NullLogger.Instance, cancellation);

			return result.Status == DownloadResourceResultStatus.Available || result.Status == DownloadResourceResultStatus.AvailableWithoutStream ? NugetUtility.GetFolderPath(directory, name, version) : null;
		}

		private static SourceRepository GetRepository(IDictionary<string, string> variables) => Repository.Factory.GetCoreV3(NugetUtility.GetNugetServer(variables));
		#endregion

		#region 嵌套结构
		public readonly struct Argument
		{
			#region 静态常量
			static readonly char[] SEPARATORS = new char[] { '/', '\\'};
			#endregion

			#region 构造函数
			private Argument(string name, string version = null, string path = null)
			{
				this.Name = name;
				this.Version = version?.Trim();
				this.Path = path?.Trim();
			}
			#endregion

			#region 公共字段
			public readonly string Name;
			public readonly string Version;
			public readonly string Path;
			#endregion

			#region 公共属性
			public bool IsEmpty => string.IsNullOrEmpty(this.Name);
			#endregion

			#region 重写方法
			public override string ToString()
			{
				if(string.IsNullOrEmpty(this.Version))
					return string.IsNullOrEmpty(this.Path) ? this.Name : $"{this.Name}/{this.Path}";
				else
					return string.IsNullOrEmpty(this.Path) ? $"{this.Name}@{this.Version}" : $"{this.Name}@{this.Version}/{this.Path}";
			}
			#endregion

			#region 解析方法
			public static Argument Parse(ReadOnlySpan<char> text)
			{
				if(text.IsEmpty)
					return default;

				var index = text.IndexOfAny(SEPARATORS);

				if(index == 0)
					return default;

				if(index < 0)
				{
					(var name, var version) = ParseIdentity(text);
					return string.IsNullOrEmpty(name) ? default : new Argument(name, version);
				}
				else
				{
					(var name, var version) = ParseIdentity(text[..index]);
					return string.IsNullOrEmpty(name) ? default : new Argument(name, version, text[(index + 1)..].ToString());
				}
			}

			private static (string name, string version) ParseIdentity(ReadOnlySpan<char> text)
			{
				if(text.IsEmpty)
					return default;

				var index = text.IndexOf('@');

				if(index == 0)
					return default;
				if(index < 0)
					return (text.ToString(), null);

				return (text[..index].ToString(), text[(index + 1)..].ToString());
			}
			#endregion
		}
		#endregion
	}
}