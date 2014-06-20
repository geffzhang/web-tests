﻿//
// ReflectionTestFixture.cs
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
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTests.Framework.Internal.Reflection
{
	class ReflectionTestFixture : TestFixture
	{
		List<ReflectionTestCase> tests;
		IList<string> categories;
		IList<TestWarning> warnings;
		RepeatAttribute repeat;

		public override IList<string> Categories {
			get { return categories; }
		}

		public override IList<TestWarning> Warnings {
			get { return warnings; }
		}

		public override int CountTests {
			get { return tests.Count; }
		}

		public override TestCase[] Tests {
			get { return tests.ToArray (); }
		}

		public RepeatAttribute Repeat {
			get { return repeat; }
		}

		public ReflectionTestFixture (TestSuite suite, AsyncTestFixtureAttribute attr, TypeInfo type)
			: base (suite, attr, type)
		{
			Resolve (suite, null, type, out repeat, out categories, out warnings);
		}

		public override bool Resolve ()
		{
			tests = new List<ReflectionTestCase> ();

			foreach (var method in Type.DeclaredMethods) {
				if (method.IsStatic || !method.IsPublic)
					continue;
				var attr = method.GetCustomAttribute<AsyncTestAttribute> (true);
				if (attr == null)
					continue;

				tests.Add (new ReflectionTestCase (this, attr, method));
			}

			return true;
		}

		internal static void Resolve (
			TestSuite suite, TestFixture parent, MemberInfo member, out RepeatAttribute repeat,
			out IList<string> categories, out IList<TestWarning> warnings)
		{
			warnings = new List<TestWarning> ();
			categories = new List<string> ();

			if (parent != null) {
				foreach (var category in parent.Categories)
					categories.Add (category);
			}

			string fullName;
			if (member is TypeInfo)
				fullName = ((TypeInfo)member).FullName;
			else if (member is MethodInfo) {
				var method = (MethodInfo)member;
				fullName = method.DeclaringType.FullName + "." + method.Name;
			} else {
				fullName = member.ToString ();
			}

			repeat = member.GetCustomAttribute<RepeatAttribute> ();

			var attrs = member.GetCustomAttributes (typeof(TestCategoryAttribute), false);

			foreach (var obj in attrs) {
				var category = obj as TestCategoryAttribute;
				if (category == null)
					continue;

				if (categories.Contains (category.Name)) {
					suite.Log ("Duplicate [{0}] in {1}.", category.Name, fullName);
					continue;
				}

				categories.Add (category.Name);
			}

			var wattrs = member.GetCustomAttributes (typeof(TestWarningAttribute), false);

			foreach (var obj in wattrs) {
				var attr = obj as TestWarningAttribute;
				if (attr == null)
					continue;

				string message;
				if (member is MethodInfo)
					message = member.Name + ": " + attr.Message;
				else
					message = attr.Message;
				warnings.Add (new TestWarning (message));
			}
		}

		public override Task<TestResult> Run (TestContext context, CancellationToken cancellationToken)
		{
			var invoker = CreateInvoker ();
			return invoker.Invoke (context, cancellationToken);
		}

		TestInvoker CreateInvoker ()
		{
			var invoker = new TestFixtureInvoker (this);
			var selected = Filter ();
			invoker.Resolve (selected);

			if (Repeat != null) {
				var repeatHost = new RepeatedTestHost (Repeat);
				return repeatHost.CreateInvoker (invoker);
			}

			return invoker;
		}

		internal override async Task InitializeInstance (TestContext context, CancellationToken cancellationToken)
		{
			var instance = (TestFixtureInstance)context.Instance;
			var fixtureInstance = instance.Instance as IAsyncTestFixture;
			if (fixtureInstance != null)
				await fixtureInstance.SetUp (context, cancellationToken);
		}

		internal override async Task DestroyInstance (TestContext context, CancellationToken cancellationToken)
		{
			var instance = (TestFixtureInstance)context.Instance;
			var fixtureInstance = instance.Instance as IAsyncTestFixture;
			if (fixtureInstance != null)
				await fixtureInstance.TearDown (context, cancellationToken);
		}

		internal IEnumerable<ReflectionTestCase> Filter ()
		{
			return tests.Where (t => Suite.Filter (t));
		}
	}
}

