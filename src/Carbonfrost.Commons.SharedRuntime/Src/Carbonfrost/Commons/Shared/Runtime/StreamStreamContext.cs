//
// - StreamStreamContext.cs -
//
// Copyright 2005, 2006, 2010 Carbonfrost Systems, Inc. (http://carbonfrost.com)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//


using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Carbonfrost.Commons.Shared.Runtime {

    internal sealed class StreamStreamContext : StreamContext {

        private readonly Stream stream;
        private readonly Encoding encoding;
        private readonly Uri uri;

        public StreamStreamContext(Stream stream, Encoding encoding)
            : this(new Uri("stream://"), stream, encoding) {
        }

        public StreamStreamContext(Uri uri, Stream stream, Encoding encoding) {
            if (stream == null)
                throw new ArgumentNullException("stream");

            this.uri = uri;
            this.stream = stream;
            this.encoding = encoding;
        }

        // 'StreamContext' overrides.

        public override bool IsLocal { get { return true; } }
        public override bool IsAnonymous { get { return true; } }

        public override Encoding Encoding {
            get { return this.encoding; }
        }

        public override CultureInfo Culture {
            get { return CultureInfo.InvariantCulture; }
        }

        public override Uri Uri {
            get { return uri; }
        }

        public override StreamContext ChangePath(string relativePath) {
            return null;
        }

        public override StreamContext ChangeCulture(CultureInfo resourceCulture) {
            return null;
        }

        public override StreamContext ChangeEncoding(Encoding encoding) {
            return new StreamStreamContext(this.stream, this.Encoding ?? encoding);
        }

        protected override Stream GetStreamCore(FileAccess access) {
            return Utility.ApplyAccess(this.stream, access);
        }

    }
}
