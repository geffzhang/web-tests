﻿//
// CustomHostInstance.cs
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
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.AsyncTests.Framework
{
	class CustomTestInstance<T> : HeavyTestInstance
		where T : ITestInstance
	{
		ITestHost<T> customHost;
		T instance;

		public Type HostType {
			get;
			private set;
		}

		public bool UseFixtureInstance {
			get;
			private set;
		}

		public CustomTestInstance (CustomTestHost<T> host, TestInstance parent, Type hostType, bool useFixtureInstance)
			: base (host, parent)
		{
			HostType = hostType;
			UseFixtureInstance = useFixtureInstance;
		}

		public override object Current {
			get { return instance; }
		}

		public override async Task Initialize (InvocationContext ctx, CancellationToken cancellationToken)
		{
			if (UseFixtureInstance)
				customHost = (ITestHost<T>)GetFixtureInstance ().Instance;
			else if (HostType != null)
				customHost = (ITestHost<T>)Activator.CreateInstance (HostType);
			else
				throw new InvalidOperationException ();

			instance = customHost.CreateInstance (ctx);
			await instance.Initialize (ctx, cancellationToken);
		}

		public override async Task PreRun (InvocationContext ctx, CancellationToken cancellationToken)
		{
			await instance.PreRun (ctx, cancellationToken);
		}

		public override async Task PostRun (InvocationContext ctx, CancellationToken cancellationToken)
		{
			await instance.PostRun (ctx, cancellationToken);
		}

		public override async Task Destroy (InvocationContext ctx, CancellationToken cancellationToken)
		{
			await instance.Destroy (ctx, cancellationToken);
		}
	}
}

