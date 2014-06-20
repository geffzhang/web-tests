﻿//
// ParameterizedTestHost.cs
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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTests.Framework.Internal
{
	abstract class ParameterizedTestHost : TestHost
	{
		public TypeInfo ParameterType {
			get;
			private set;
		}

		protected ParameterizedTestHost (TypeInfo type)
		{
			ParameterType = type;
		}

		public bool CanReuseInstance (TestContext context)
		{
			if (!HasInstance)
				throw new InvalidOperationException ();
			if (context.Instance != CurrentInstance)
				throw new InvalidOperationException ();
			return CanReuse (context);
		}

		internal Task ReuseInstance (TestContext context, CancellationToken cancellationToken)
		{
			if (!HasInstance)
				throw new InvalidOperationException ();
			if (context.Instance != CurrentInstance)
				throw new InvalidOperationException ();
			return Reuse (context, cancellationToken);
		}

		protected sealed override Task Initialize (TestContext context, CancellationToken cancellationToken)
		{
			var instance = (ParameterizedTestInstance)context.Instance;
			return instance.Initialize (context, cancellationToken);
		}

		protected bool CanReuse (TestContext context)
		{
			var instance = (ParameterizedTestInstance)context.Instance;
			return instance.HasNext ();
		}

		protected Task Reuse (TestContext context, CancellationToken cancellationToken)
		{
			var instance = (ParameterizedTestInstance)context.Instance;
			return instance.MoveNext (context, cancellationToken);
		}

		protected sealed override Task Destroy (TestContext context, CancellationToken cancellationToken)
		{
			var instance = (ParameterizedTestInstance)context.Instance;
			return instance.Destroy (context, cancellationToken);
		}
	}
}
