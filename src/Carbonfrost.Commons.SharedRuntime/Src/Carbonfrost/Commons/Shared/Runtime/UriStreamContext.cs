//
// - UriStreamContext.cs -
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
using System.Text;

namespace Carbonfrost.Commons.Shared.Runtime {

    internal sealed class UriStreamContext : StreamContext {

        private const string LANG_QUERY = "lang="; // $NON-NLS-1
        private const int LANG_SPEC_LENGTH = 5;

        private readonly Uri uri;
        private readonly Encoding encoding;
        private Encoding cacheEncoding;

        public UriStreamContext(Uri uri, Encoding encoding) {
            if (uri == null) { throw new ArgumentNullException("uri"); } // $NON-NLS-1
            this.uri = uri;
            this.encoding = encoding;
        }

        #region 'StreamContext' overrides.

        public override bool IsLocal { get { return this.uri.IsFile; } }
        public override bool IsAnonymous { get { return false; } }

        public override CultureInfo Culture {
            get {
                int index = this.Uri.Query.IndexOf(LANG_QUERY, StringComparison.Ordinal);

                if (index < 0) {
                    return CultureInfo.InvariantCulture;
                } else {
                    int length = this.Uri.Query.IndexOf('&', index);
                    string languageString = null;

                    if (length < 0) {
                        languageString = this.Uri.Query.Substring(LANG_SPEC_LENGTH + index);
                    } else {
                        languageString = this.Uri.Query.Substring(LANG_SPEC_LENGTH + index, length - index - 5);
                    }

                    return new CultureInfo(languageString);
                }
            }
        }

        public override Encoding Encoding {
            get { return cacheEncoding ?? encoding; }
        }

        public override Uri Uri { get { return this.uri; } }

        public override StreamContext ChangePath(string relativePath) {
            if (relativePath == null) { throw new ArgumentNullException("relativePath"); } // $NON-NLS-1

            // Add the relative path
            UriBuilder baseUri = new UriBuilder(new Uri(this.uri, relativePath));
            return StreamContext.FromSource(baseUri.Uri);
        }

        public override StreamContext ChangeCulture(CultureInfo resourceCulture) {
            if (resourceCulture == null) { throw new ArgumentNullException("resourceCulture"); } // $NON-NLS-1

            // Perform the language query
            UriBuilder baseUri = new UriBuilder(this.uri);
            string languageQuery = LANG_QUERY + resourceCulture.Name;

            if (baseUri.Query != null && baseUri.Query.Length > 1) {
                baseUri.Query = baseUri.Query.Substring(1) + "&" + languageQuery; // $NON-NLS-1
            } else {
                baseUri.Query = languageQuery;
            }

            return StreamContext.FromSource(baseUri.Uri);
        }

        public override StreamContext ChangeEncoding(Encoding encoding) {
            if (encoding == null)
                return this;

            return new UriStreamContext(this.Uri, this.Encoding);
        }

        protected override Stream GetStreamCore(FileAccess access) {
            using (WebClient client = new WebClient()) {

                WebRequest request = WebRequest.CreateDefault(this.Uri);
                WebResponse response = request.GetResponse();
                try {
                    this.cacheEncoding = Utility.GetEncodingFromContentType(response.ContentType);

                } catch (NotSupportedException) {
                    this.cacheEncoding = null;
                }

                switch (access) {
                    case FileAccess.Read:
                        return client.OpenRead(this.uri);

                    case FileAccess.Write:
                        return client.OpenWrite(this.uri);

                    case FileAccess.ReadWrite:
                    default:
                        return client.OpenWrite(this.uri);
                }
            }
        }

        #endregion

    }
}
