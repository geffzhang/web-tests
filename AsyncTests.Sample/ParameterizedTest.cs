﻿//
// ParameterizedTest.cs
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
using System.Collections.Generic;

namespace AsyncTests.Sample
{
	using Framework;

	// [AsyncTestFixture]
	public class ParameterizedTest : ITestParameterSource<Foo>
	{
		public IEnumerable<Foo> GetParameters (TestContext context, string filter)
		{
			if (filter != null)
				yield return new Foo (filter);
			else
				yield return new Foo ("Chicago");
		}

		[AsyncTest]
		public void Hello (TestContext context, [TestParameter (typeof (HelloSource))] string hello)
		{
			context.Log ("HELLO: {0}", hello);
		}

		[AsyncTest]
		public void HelloIFoo (TestContext context, IFoo foo)
		{
			context.Log ("HELLO IFOO: {0}", foo);
		}

		[AsyncTest]
		public void HelloFoo (TestContext context, [TestParameter ("New York")] Foo foo, [TestParameter] Foo bar)
		{
			context.Log ("HELLO FOO: {0} {1}", foo, bar);
		}

		[AsyncTest]
		public void Repeat (TestContext context, [Repeat (10)] int index)
		{
			context.Log ("REPEAT: {0}", index);
		}

		[Repeat (5)]
		[AsyncTest]
		public void SimpleRepeat (TestContext context)
		{
			context.Log ("SIMPLE REPEAT");
		}
	}
}