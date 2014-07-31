﻿//
// TestSuite.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Xamarin.AsyncTests;

[assembly: AsyncTestSuite (typeof (Xamarin.WebTests.WebTestFeatures))]

namespace Xamarin.WebTests
{
	using Runners;

	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
	public class WorkAttribute : TestCategoryAttribute
	{
		public override TestCategory Category {
			get { return WebTestFeatures.WorkCategory; }
		}
	}

	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
	public class RecentlyFixedAttribute : TestCategoryAttribute
	{
		public override TestCategory Category {
			get { return WebTestFeatures.RecentlyFixedCategory; }
		}
	}

	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
	public class ProxyAttribute : TestFeatureAttribute
	{
		public override TestFeature Feature {
			get { return WebTestFeatures.Proxy; }
		}
	}

	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
	public class Mono38Attribute : TestFeatureAttribute
	{
		public override TestFeature Feature {
			get { return WebTestFeatures.Mono38; }
		}
	}

	public class WebTestFeatures : ITestConfiguration
	{
		public static readonly TestFeature NTLM = new TestFeature ("NTLM", "NTLM Authentication");
		public static readonly TestFeature SSL = new TestFeature ("SSL", "Use SSL", true);
		public static readonly TestFeature Redirect = new TestFeature ("Redirect", "Redirect Tests", true);
		public static readonly TestFeature Proxy = new TestFeature ("Proxy", "Proxy Tests", true);
		public static readonly TestFeature ProxyAuth = new TestFeature ("ProxyAuth", "Proxy Authentication", true);
		public static readonly TestFeature Experimental = new TestFeature ("Experimental", "Experimental Tests", false);

		public static readonly TestFeature NotWorking = new TestFeature ("NotWorking", "Not Working", false);

		public static readonly TestFeature ReuseConnection = new TestFeature ("ReuseConnection", "Reuse Connection", false);

		public static readonly TestFeature HasNetwork = new TestFeature (
			"Network", "HasNetwork", () => !IPAddress.IsLoopback (TestRunner.GetAddress ()));

		public static readonly TestFeature Mono38 = new TestFeature (
			"Mono38", "Mono 3.8.0", () => MonoVersion != null && MonoVersion >= new Version (3, 8, 0));

		public static readonly TestCategory WorkCategory = new TestCategory ("Work") { IsExplicit = true };
		public static readonly TestCategory RecentlyFixedCategory = new TestCategory ("RecentlyFixed") { IsExplicit = true };

		#region ITestSuite implementation
		public IEnumerable<TestFeature> Features {
			get {
				yield return NTLM;
				yield return SSL;
				yield return Redirect;
				yield return Proxy;
				yield return ProxyAuth;
				yield return Experimental;
				yield return ReuseConnection;
				yield return NotWorking;

				yield return HasNetwork;
				yield return Mono38;
			}
		}

		public IEnumerable<TestCategory> Categories {
			get {
				yield return WorkCategory;
				yield return RecentlyFixedCategory;
			}
		}
		#endregion

		public class SelectSSL : ITestParameterSource<bool>
		{
			#region ITestParameterSource implementation
			public IEnumerable<bool> GetParameters (TestContext context, string filter)
			{
				yield return false;
				if (context.IsEnabled (SSL))
					yield return true;
			}
			#endregion
		}

		public class SelectReuseConnection : ITestParameterSource<bool>
		{
			#region ITestParameterSource implementation
			public IEnumerable<bool> GetParameters (TestContext context, string filter)
			{
				yield return false;
				if (context.IsEnabled (ReuseConnection))
					yield return true;
			}
			#endregion
		}

		public static readonly Version MonoVersion;

		static WebTestFeatures ()
		{
			try {
				MonoVersion = GetRuntimeVersion ();
			} catch {
				;
			}
		}

		static Version GetRuntimeVersion ()
		{
			string version;
			#if __MOBILE__
			version = Mono.Runtime.GetDisplayName ();
			#else
			Type type = Type.GetType ("Mono.Runtime", false);
			if (type == null)
				return null;

			var method = type.GetMethod ("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
			if (method == null)
				return null;

			version = (string)method.Invoke (null, null);
			#endif

			var match = Regex.Match (version, @"^(\d+)\.(\d+)(?:\.(\d+))?\b");
			if (!match.Success)
				return null;

			var major = int.Parse (match.Groups [1].Value);
			var minor = int.Parse (match.Groups [2].Value);
			int build = 0;

			if (match.Groups.Count > 2 && match.Groups [3].Success)
				build = int.Parse (match.Groups [3].Value);

			return new Version (major, minor, build);
		}
	}
}

