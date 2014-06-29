﻿//
// PrePostRunTestInvoker.cs
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
	class PrePostRunTestInvoker : TestInvoker
	{
		public TestInvoker Inner {
			get;
			private set;
		}

		public PrePostRunTestInvoker (TestInvoker inner)
		{
			Inner = inner;
		}

		async Task<bool> PreRun (
			TestContext ctx, TestInstance instance, TestResult result, CancellationToken cancellationToken)
		{
			ctx.Debug (3, "PreRun({0}): {1}", ctx.GetCurrentTestName ().FullName, ctx.Print (instance));

			try {
				ctx.CurrentTestName.PushName ("PreRun");
				for (var current = instance; current != null; current = current.Parent) {
					cancellationToken.ThrowIfCancellationRequested ();
					await current.PreRun (ctx, cancellationToken);
				}
				return true;
			} catch (OperationCanceledException) {
				result.Status = TestStatus.Canceled;
				return false;
			} catch (Exception ex) {
				var error = ctx.CreateTestResult (ex);
				result.AddChild (error);
				return false;
			} finally {
				ctx.CurrentTestName.PopName ();
			}
		}

		async Task<bool> PostRun (
			TestContext ctx, TestInstance instance, TestResult result, CancellationToken cancellationToken)
		{
			ctx.Debug (3, "PostRun({0}): {1}", ctx.GetCurrentTestName ().FullName, ctx.Print (instance));

			try {
				ctx.CurrentTestName.PushName ("PostName");
				for (var current = instance; current != null; current = current.Parent) {
					cancellationToken.ThrowIfCancellationRequested ();
					await current.PostRun (ctx, cancellationToken);
				}
				return true;
			} catch (OperationCanceledException) {
				result.Status = TestStatus.Canceled;
				return false;
			} catch (Exception ex) {
				var error = ctx.CreateTestResult (ex);
				result.AddChild (error);
				return false;
			} finally {
				ctx.CurrentTestName.PopName ();
			}
		}

		public override async Task<bool> Invoke (
			TestContext ctx, TestInstance instance, TestResult result, CancellationToken cancellationToken)
		{
			if (!await PreRun (ctx, instance, result, cancellationToken))
				return false;

			bool success;
			try {
				success = await Inner.Invoke (ctx, instance, result, cancellationToken);
			} catch (Exception ex) {
				var error = ctx.CreateTestResult (ex);
				result.AddChild (error);
				success = false;
			}

			if (!await PostRun (ctx, instance, result, cancellationToken))
				success = false;

			return success;
		}
	}
}

