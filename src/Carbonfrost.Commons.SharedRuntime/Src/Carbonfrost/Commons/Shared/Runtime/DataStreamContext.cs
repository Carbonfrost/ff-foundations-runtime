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

namespace Carbonfrost.Commons.Shared.Runtime {

    sealed class DataStreamContext : StreamContext {

        private readonly string baseUri;
        private readonly MemoryStream data;
        private readonly ContentType contentType;

        // "data:application/octet-stream;base64,"

        public DataStreamContext(DataStreamContext source, ContentType contentType) {
            this.contentType = contentType;
            this.baseUri = string.Format("data:{0};base64,", this.contentType);
            this.data = new MemoryStream();
            source.data.CopyTo(this.data);
        }

        public DataStreamContext(MemoryStream data, ContentType contentType) {
            this.contentType = contentType;
            this.baseUri = string.Format("data:{0};base64,", this.contentType);
            this.data = data;
        }

        public DataStreamContext(Uri u) {
            string[] parts = Utility.SplitInTwo(u.PathAndQuery, ';');
            if (parts.Length != 2)
                throw RuntimeFailure.NotValidDataUri();

            this.contentType = ContentType.Parse(parts[0]);
            string[] dataParts = Utility.SplitInTwo(parts[1], ',');

            byte[] buffer;
            string encoding = "base64";

            if (dataParts.Length == 1) {
                buffer = Convert.FromBase64String(dataParts[0]);

            } else {
                encoding = dataParts[0];

                // Only base64 is supported by RFC
                if (encoding == "base64")
                    buffer = Convert.FromBase64String(dataParts[1]);
                else
                    throw RuntimeFailure.NotValidDataUri();
            }

            this.baseUri = string.Format("data:{0};{1},", this.contentType, encoding);
            this.data = new MemoryStream(buffer.Length);
            this.data.Write(buffer, 0, buffer.Length);
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
            return Convert.ToBase64String(this.data.ToArray());
        }
    }
}
