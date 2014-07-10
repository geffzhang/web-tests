﻿//
// Command.cs
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
using System.Windows.Input;
using Xamarin.AsyncTests;
using Xamarin.AsyncTests.Framework;
using Xamarin.Forms;

namespace Xamarin.AsyncTests.UI
{
	public abstract class CommandProvider : BindableObject
	{
		public TestApp App {
			get;
			private set;
		}

		public CommandProvider (TestApp app)
		{
			App = app;

			stopCommand = new StopCommand (this);
			CanStart = true;
		}

		public ICommand Stop {
			get { return stopCommand; }
		}

		readonly StopCommand stopCommand;

		public static readonly BindableProperty CanStartProperty =
			BindableProperty.Create ("CanStart", typeof(bool), typeof(CommandProvider), false,
				propertyChanged: (bo, o, n) => ((CommandProvider)bo).OnCanStartChanged ((bool)n));

		public bool CanStart {
			get { return (bool)GetValue (CanStartProperty); }
			set { SetValue (CanStartProperty, value); }
		}

		protected void OnCanStartChanged (bool canStart)
		{
			if (CanStartChanged != null)
				CanStartChanged (this, canStart);
		}

		public event EventHandler<bool> CanStartChanged;

		public static readonly BindableProperty CanStopProperty =
			BindableProperty.Create ("CanStop", typeof(bool), typeof(CommandProvider), false,
				propertyChanged: (bo, o, n) => ((CommandProvider)bo).OnCanStopChanged ((bool)n));

		public bool CanStop {
			get { return (bool)GetValue (CanStopProperty); }
			set { SetValue (CanStopProperty, value); }
		}

		protected void OnCanStopChanged (bool canStop)
		{
			if (CanStopChanged != null)
				CanStopChanged (this, canStop);
		}

		public event EventHandler<bool> CanStopChanged;

		public static readonly BindableProperty HasInstanceProperty =
			BindableProperty.Create ("HasInstance", typeof(bool), typeof(CommandProvider), false,
				propertyChanged: (bo, o, n) => ((CommandProvider)bo).OnHasInstanceChanged ((bool)n));

		public bool HasInstance {
			get { return (bool)GetValue (HasInstanceProperty); }
			set { SetValue (HasInstanceProperty, value); }
		}

		protected void OnHasInstanceChanged (bool hasInstance)
		{
			if (HasInstanceChanged != null)
				HasInstanceChanged (this, hasInstance);
		}

		public event EventHandler<bool> HasInstanceChanged;

		string statusMessage;
		public string StatusMessage {
			get { return statusMessage; }
			set {
				statusMessage = value;
				App.Context.Debug (0, "{0}: {1}", GetType ().Name, value);
				OnPropertyChanged ("StatusMessage");
			}
		}

		protected void SetStatusMessage (string message, params object[] args)
		{
			StatusMessage = string.Format (message, args);
		}

		internal abstract Task ExecuteStop ();

		class StopCommand : Command
		{
			public StopCommand (CommandProvider command)
				: base (command)
			{
				CanExecute = command.CanStop;
				command.CanStopChanged += (sender, e) => CanExecute = e;
			}

			public override Task Execute ()
			{
				return Provider.ExecuteStop ();
			}
		}
	}

	public abstract class CommandProvider<T> : CommandProvider
		where T : class
	{
		public CommandProvider (TestApp app)
			: base (app)
		{
		}

		public T Instance {
			get { return instance; }
			set {
				instance = value;
				HasInstance = instance != null;
				OnPropertyChanged ("HasInstance");
			}
		}

		T instance;
		Command<T> currentCommand;
		TaskCompletionSource<T> startTcs;
		CancellationTokenSource cts;

		internal async Task ExecuteStart (Command<T> command)
		{
			lock (this) {
				if (startTcs != null || !CanStart || !command.CanExecute)
					return;
				CanStart = false;
				CanStop = true;
				currentCommand = command;
				startTcs = new TaskCompletionSource<T> ();
				cts = new CancellationTokenSource ();
			}

			try {
				Instance = await command.Start (cts.Token);
				startTcs.SetResult (Instance);
			} catch (OperationCanceledException) {
				startTcs.SetCanceled ();
			} catch (Exception ex) {
				SetStatusMessage ("Command failed: {0}", ex.Message);
				startTcs.SetException (ex);
			}
		}

		internal async override Task ExecuteStop ()
		{
			lock (this) {
				if (startTcs == null)
					return;
				CanStop = false;
				Instance = null;
				cts.Cancel ();
			}

			try {
				await startTcs.Task;
			} catch {
				;
			}

			try {
				await currentCommand.Stop (CancellationToken.None);
			} catch {
				;
			}

			lock (this) {
				startTcs = null;
				cts.Dispose ();
				cts = null;
				currentCommand = null;
				CanStart = true;
			}
		}
	}
}
