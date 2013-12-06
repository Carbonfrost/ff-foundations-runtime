//
// - PropertiesReader.cs -
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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Carbonfrost.Commons.Shared.Runtime {

    class PropertiesReader : DisposableObject {

        public const string AnnotationKey = "#annotation"; // $NON-NLS-1
        public const string CategoryKey = "#category"; // $NON-NLS-1
        static readonly char[] GetCommentChars = { ';', '#', '!' };

        private string key;
        private string value;
        private string category = string.Empty;
        private PropertyNodeKind nodeKind;
        private readonly StreamContext streamContext;

        public bool AllowUriDereferences { get; set; }

        private TextReader BaseReader { get; set; }

        public string Category { get { return category; } }
        public string Key { get { return key; } }
        public PropertyNodeKind NodeKind { get { return nodeKind; } }

        public string QualifiedKey {
            get {
                if (this.category.Length == 0) {
                    return key;
                } else {
                    return this.category + "." + key; // $NON-NLS-1
                }
            }
        }

        public string Value { get { return value; } }

        public PropertiesReader(TextReader source) {
            this.streamContext = StreamContext.Null;
            this.BaseReader = source;
        }

        public PropertiesReader(StreamContext source, Encoding encoding = null) {
            this.streamContext = source;
            this.BaseReader = source.OpenText(encoding);
        }

        public bool Read() {
            string line = this.BaseReader.ReadLine();
            if (line == null) { return false; }

            // This removes preceeding whitespace on continued lines
            line = line.Trim();

            // Skip blank lines
            while (line.Length == 0) {
                line = this.BaseReader.ReadLine();
                if (line == null) { return false; }
                line = line.Trim();
            }

            // This is a category line
            if (line.StartsWith("[", StringComparison.Ordinal)) { // $NON-NLS-1
                if (line.EndsWith("]", StringComparison.Ordinal)) { // $NON-NLS-1
                    EnterCategory(line);
                } else
                    throw RuntimeFailure.PropertiesCategoryMissingBrackets();


            } else {
                // Either pick this as a comment or a property
                if (line[0] == ';' || line[0] == '!' || line[0] == '#') {
                    EnterComment(line);

                } else {
                    StringBuilder buffer = new StringBuilder();
                    // Deal with line continuations \
                    // New to 1.3: assume that if a equals sign is missing, it is part
                    // of the previous line

                    while (line != null && line.EndsWith("\\", StringComparison.Ordinal)) {
                        buffer.Append(line, 0, line.Length - 1);
                        line = this.BaseReader.ReadLine();

                        if (line != null) {
                            line = line.Trim();
                        }
                    }
                    if (line != null) {
                        buffer.AppendLine(line);
                    }
                    EnterText(buffer.ToString());
                }
            }

            return true;
        }

        public bool MoveToProperty() {
            bool readResult = false;
            // Skip past comments and categories
            do {
                readResult = Read();
            } while (NodeKind != PropertyNodeKind.Property && readResult == true);

            return readResult;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadToEnd() {
            while (MoveToProperty()) {
                yield return new KeyValuePair<string, string>(this.QualifiedKey, this.Value);
            }
        }

        private void EnterCategory(string categoryDeclaration) {
            this.key = PropertiesReader.CategoryKey;
            this.value = this.category =
                categoryDeclaration.Substring(1, categoryDeclaration.Length - 2);
            this.nodeKind = PropertyNodeKind.Category;
        }

        private void EnterComment(string commentLine) {
            this.nodeKind = PropertyNodeKind.Annotation;
            this.key = PropertiesReader.AnnotationKey;
            if (commentLine.Length == 1) {
                this.value = string.Empty;

            } else {
                this.value = commentLine.Substring(1);
            }
        }

        private void EnterText(string line) {
            int equalsIndex = line.IndexOf('=');
            if (equalsIndex < 0) {
                throw RuntimeFailure.PropertyDeclarationMissingKey();
            }

            string newKey = null;
            string newValue = null;
            if (equalsIndex == line.Length - 1) {
                newKey = line.Substring(0, line.Length - 1);
                newValue = string.Empty;

            } else {
                newKey = line.Substring(0, equalsIndex);
                newValue = line.Substring(equalsIndex + 1).Trim();
            }

            this.key = newKey;
            string url;
            if (this.AllowUriDereferences && TryUrlSyntax(newValue, out url)) {
                this.value = this.streamContext.ChangePath(url).ReadAllText();

            } else {
                this.value = Utility.Unescape(newValue);
            }

            this.nodeKind = PropertyNodeKind.Property;
        }

        static readonly string URL_FUNC = "url(";

        private static bool TryUrlSyntax(string urlText, out string result) {
            int length = urlText.Length;

            if (length > (URL_FUNC.Length + 1)) {
                if (urlText.StartsWith(URL_FUNC, StringComparison.OrdinalIgnoreCase) && urlText[length - 1] == ')') { // $NON-NLS-1

                    string url;
                    if ((urlText[4] == '"' && urlText[length - 2] == '"')
                        || (urlText[4] == '\'' && urlText[length - 2] == '\'')) {

                        url = urlText.Substring(URL_FUNC.Length + 1, urlText.Length - URL_FUNC.Length - 2);

                    } else {
                        // url()
                        url = urlText.Substring(URL_FUNC.Length, urlText.Length - URL_FUNC.Length - 1);
                    }

                    result = null;

                    // TODO Try URL syntax
                    return true;
                }
            }
            result = null;
            return false;
        }
    }

}
