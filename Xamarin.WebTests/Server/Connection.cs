﻿//
// Connection.cs
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
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.WebTests.Server
{
	using Framework;

	public class Connection
	{
		StreamReader reader;
		StreamWriter writer;

		public Connection (StreamReader reader, StreamWriter writer)
		{
			this.reader = reader;
			this.writer = writer;
		}

		public int? ReadChunkSize {
			get; set;
		}

		public int? ReadChunkMinDelay {
			get; set;
		}

		public int? ReadChunkMaxDelay {
			get; set;
		}

		protected StreamReader RequestReader {
			get { return reader; }
		}

		protected StreamWriter ResponseWriter {
			get { return writer; }
		}

		public HttpRequest ReadRequest ()
		{
			return new HttpRequest (this, reader);
		}

		protected HttpResponse ReadResponse ()
		{
			return new HttpResponse (this, reader);
		}

		protected void WriteRequest (HttpRequest request)
		{
			request.Write (writer);
		}

		public void WriteResponse (HttpResponse response)
		{
			response.Write (writer);
		}

		public void Close ()
		{
			writer.Flush ();
		}
	}
}

