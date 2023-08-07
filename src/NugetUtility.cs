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

namespace Zongsoft.Tools.Deployer
{
	public static class NugetUtility
	{
		private const string USERPROFILE_ENVIRONMENT = "USERPROFILE";
		private const string NUGET_PACKAGES_ENVIRONMENT = "NUGET_PACKAGES";

		public static void Initialize(IDictionary<string, string> variables)
		{
			if(!variables.ContainsKey(USERPROFILE_ENVIRONMENT))
				variables[USERPROFILE_ENVIRONMENT] = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

			if(!variables.ContainsKey(NUGET_PACKAGES_ENVIRONMENT) && variables.TryGetValue(USERPROFILE_ENVIRONMENT, out var home))
				variables.TryAdd(NUGET_PACKAGES_ENVIRONMENT, Path.Combine(home, $".nuget{Path.DirectorySeparatorChar}packages"));
		}

		public static bool TryGetPackagesDirectory(IDictionary<string, string> variables, out string directory)
		{
			return variables.TryGetValue(NUGET_PACKAGES_ENVIRONMENT, out directory) && !string.IsNullOrEmpty(directory);
		}
	}
}