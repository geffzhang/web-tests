﻿//
// AuthenticatedPostHandler.cs
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
using Mono.Security.Protocol.Ntlm;

namespace Xamarin.WebTests.Handlers
{
	using Framework;
	using Server;

	public class AuthenticationHandler : AbstractRedirectHandler
	{
		public AuthenticationHandler (AuthenticationType type, Handler target)
			: base (target)
		{
			manager = new HttpAuthManager (type, target);
		}

		AuthenticationHandler (HttpAuthManager manager)
			: base (manager.Target)
		{
			this.manager = manager;
		}

		class HttpAuthManager : AuthenticationManager
		{
			public readonly Handler Target;

			public HttpAuthManager (AuthenticationType type, Handler target)
				: base (type)
			{
				Target = target;
			}

			protected override HttpResponse OnError (string message)
			{
				return HttpResponse.CreateError (message);
			}

			protected override HttpResponse OnUnauthenticated (HttpRequest request, string token, bool omitBody)
			{
				var handler = new AuthenticationHandler (this);
				if (omitBody)
					handler.Flags |= RequestFlags.NoBody;
				handler.Flags |= RequestFlags.Redirected;
				((HttpConnection)request.Connection).Server.RegisterHandler (request.Path, handler);

				var response = new HttpResponse (HttpStatusCode.Unauthorized);
				response.AddHeader ("WWW-Authenticate", token);
				return response;
			}
		}

		readonly HttpAuthManager manager;

		protected internal override HttpResponse HandleRequest (HttpConnection connection, HttpRequest request, RequestFlags effectiveFlags)
		{
			string authHeader;
			if (!request.Headers.TryGetValue ("Authorization", out authHeader))
				authHeader = null;

			var response = manager.HandleAuthentication (request, authHeader);
			if (response != null)
				return response;

			return Target.HandleRequest (connection, request, effectiveFlags);
		}

		public override HttpWebRequest CreateRequest (Uri uri)
		{
			var request = base.CreateRequest (uri);
			request.Credentials = new NetworkCredential ("xamarin", "monkey");
			return request;
		}
	}
}

