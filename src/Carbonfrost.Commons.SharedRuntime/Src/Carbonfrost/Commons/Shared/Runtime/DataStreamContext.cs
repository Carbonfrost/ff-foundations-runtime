//
// - DataStreamContext.cs -
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
using System.Net;
using System.Text.RegularExpressions;

namespace Carbonfrost.Commons.Shared.Runtime {

    sealed class DataStreamContext : StreamContext {

        private readonly string baseUri;
        private readonly MemoryStream data;
        private readonly ContentType contentType;
        private readonly bool isBase64;

        // "data:application/octet-stream;base64,"
        // data:[<MIME-type>][;charset=<encoding>][;base64],<data>

        public DataStreamContext(DataStreamContext source, ContentType contentType) {
            this.contentType = contentType;
            this.baseUri = string.Format("data:{0};base64,", this.contentType);
            this.data = new MemoryStream();
            this.isBase64 = true;
            source.data.CopyTo(this.data);
        }

        public DataStreamContext(MemoryStream data, ContentType contentType) {
            this.contentType = contentType;
            this.baseUri = string.Format("data:{0};base64,", this.contentType);
            this.data = data;
            this.isBase64 = true;
        }

        public DataStreamContext(Uri u) {
            string[] parts = Utility.SplitInTwo(u.PathAndQuery, ',');
            if (parts.Length != 2)
                throw RuntimeFailure.NotValidDataUri();

            var ct = Regex.Replace(parts[0], ";base64", string.Empty);
            if (ct.Length == 0)
                this.contentType = new ContentType("text", "plain");
            else
                this.contentType = ContentType.Parse(ct);

            byte[] buffer;

            this.isBase64 = ct.Length < parts[0].Length; // implied by replacement
            if (this.isBase64)
                buffer = Convert.FromBase64String(parts[1]);
            else
                buffer = System.Text.Encoding.ASCII.GetBytes(WebUtility.UrlDecode(parts[1]));

            this.baseUri = string.Concat("data:",
                                         this.contentType,
                                         this.isBase64 ? ";base64" : string.Empty,
                                         ",");
            this.data = new MemoryStream(buffer.Length);
            this.data.Write(buffer, 0, buffer.Length);
        }

        private static ContentType ParseContentType(string text) {
            return ContentType.Parse(text);
        }

        public override ContentType ContentType {
            get { return this.contentType; }
        }

        public override Uri Uri {
            get { return new Uri(this.baseUri + EncodedBytes()); } }

        protected override Stream GetStreamCore(FileAccess access) {
            if (access != FileAccess.Write) {
                data.Position = 0;
            }

            return Utility.ApplyAccess(data, access);
        }

        public override CultureInfo Culture {
            get { return CultureInfo.InvariantCulture; } }

        public override StreamContext ChangePath(string relativePath) {
            throw new NotSupportedException();
        }

        public override StreamContext ChangeCulture(CultureInfo resourceCulture) {
            throw new NotSupportedException();
        }

        public override StreamContext ChangeContentType(ContentType contentType) {
            return new DataStreamContext(this, contentType);
        }

        string EncodedBytes() {
            if (this.isBase64)
                return Convert.ToBase64String(this.data.ToArray());
            else {
                string text = System.Text.Encoding.ASCII.GetString(this.data.ToArray());
                return WebUtility.UrlEncode(text).Replace("+", "%20");
            }
        }
    }
}
