//
// - NamespaceUri.cs -
//
// Copyright 2005, 2006, 2010, 2012 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Carbonfrost.Commons.Shared.Runtime;

namespace Carbonfrost.Commons.Shared {

    [TypeConverter(typeof(NamespaceUriConverter))]
    public sealed class NamespaceUri : IEquatable<NamespaceUri>, IFormattable, IObjectReference {

        private static readonly Dictionary<string, NamespaceUri> namespaces
            = new Dictionary<string, NamespaceUri>();

        private readonly Dictionary<string, QualifiedName> names
            = new Dictionary<string, QualifiedName>();

        private readonly int hashCodeCache;
        private readonly string namespaceUri;

        static readonly Regex TAG_DATE_PATTERN = new Regex(
            @"^\d{4}(-\d{2}(-\d{2})?)?$",
            RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        private static readonly NamespaceUri xml = new NamespaceUri(XmlPrefixNamespace);
        private static readonly NamespaceUri xmlns = new NamespaceUri(XmlnsPrefixNamespace);
        private static readonly NamespaceUri defaultNamespace = new NamespaceUri();

        internal const string XmlnsPrefixNamespace = "http://www.w3.org/2000/xmlns/"; // $NON-NLS-1
        internal const string XmlPrefixNamespace = "http://www.w3.org/XML/1998/namespace"; // $NON-NLS-1

        private static readonly int xmlnsPrefixNamespaceLength = XmlnsPrefixNamespace.Length;
        private static readonly int xmlPrefixNamespaceLength = XmlPrefixNamespace.Length;

        // Properties.
        public static NamespaceUri Default { get { return defaultNamespace; } }
        public bool IsDefault { get { return object.ReferenceEquals(this, NamespaceUri.Default); } }
        public string NamespaceName { get { return this.namespaceUri; } }

        public static NamespaceUri Xml { get { return xml; } }
        public static NamespaceUri Xmlns { get { return xmlns; } }

        // Constructors.
        private NamespaceUri()
            : this(String.Empty)
        {
        }

        internal NamespaceUri(string namespaceName) {
            // N.B. Argument checking done upstream
            this.namespaceUri = string.Intern(namespaceName);
            unchecked {
                this.hashCodeCache = 1000000009 * namespaceName.GetHashCode();
            }
        }

        public static NamespaceUri Create(string namespaceName) {
            if (namespaceName == null)
                throw new ArgumentNullException("namespaceName");
            if (namespaceName.Length == 0)
                throw Failure.EmptyString("namespaceName");

            return Parse(namespaceName);
        }

        public static NamespaceUri Create(Uri uri) {
            if (uri == null)
                throw new ArgumentNullException("uri"); // $NON-NLS-1

            return Parse(uri.ToString());
        }

        public static bool TryParse(string text, out NamespaceUri value) {
            value = _TryParse(text, true);
            return value != null;
        }

        public static NamespaceUri Parse(string text) {
            return _TryParse(text, true);
        }

        internal static NamespaceUri _TryParse(string text, bool throwOnError) {
            if (text == null) {
                if (throwOnError)
                    throw new ArgumentNullException("text"); // $NON-NLS-1
                else
                    return null;
            }
            if (text.Length == 0)
                return NamespaceUri.Default;

            NamespaceUri result = null;

            if (namespaces.TryGetValue(text, out result))
                return result;

            else {
                // It may be the case that one of the static members is in use
                int count = text.Length;
                if ((count == NamespaceUri.xmlnsPrefixNamespaceLength)
                    && (string.CompareOrdinal(
                        text, XmlnsPrefixNamespace) == 0)) {
                    return Xmlns;
                }

                if ((count == NamespaceUri.xmlPrefixNamespaceLength)
                    && (string.CompareOrdinal(
                        text, XmlPrefixNamespace) == 0)) {
                    return Xml;
                }

                result = new NamespaceUri(text);
                namespaces.Add(text, result);
                return result;
            }
        }

        public QualifiedName GetName(string localName) {
            QualifiedName.VerifyLocalName("localName", localName);

            QualifiedName result = null;

            if (this.names.TryGetValue(localName, out result)) {
                return result;
            } else {
                result = new QualifiedName(this, localName);;
                this.names.Add(localName, result);
                return result;
            }
        }

        // Operators.
        public static QualifiedName operator +(NamespaceUri ns, string localName) {
            return (ns ?? NamespaceUri.Default).GetName(localName);
        }

        public static bool operator ==(NamespaceUri left, NamespaceUri right) {
            return object.ReferenceEquals(left, right);
        }

        [CLSCompliant(false)]
        public static implicit operator NamespaceUri(string namespaceName) {
            if (namespaceName == null) {
                return null;
            }
            return Parse(namespaceName);
        }

        public static bool operator !=(NamespaceUri left, NamespaceUri right) {
            return !object.ReferenceEquals(left, right);
        }

        // 'object' overrides.
        public override int GetHashCode() { return this.hashCodeCache; }

        public override bool Equals(object obj) {
            return Equals(obj as NamespaceUri);
        }

        public override string ToString() {
            return this.namespaceUri;
        }

        //  'IFormattable' implementation.
        public string ToString(string format, IFormatProvider formatProvider = null) {

            if (string.IsNullOrEmpty(format))
                format = "G";

            if (format.Length > 1)
                throw new FormatException();

            switch (char.ToLowerInvariant(format[0])) {
                case 'g':
                case 'f':
                    return this.NamespaceName;
                case 'b':
                    return string.Concat("{", this.NamespaceName, "}");
                case 't':
                    return CreateTagUri();

                default:
                    throw new FormatException();
            }
        }

        private string CreateTagUri() {
            Uri u;
            if (Uri.TryCreate(this.NamespaceName, UriKind.Absolute, out u)) {
                string hostName = u.Host;
                string[] split = u.PathAndQuery.Split(new char[] { '/' }, 3);
                string date = split[1];
                string rest = "/" + split.ElementAtOrDefault(2);

                if ((u.Scheme == "http" || u.Scheme == "https") && TAG_DATE_PATTERN.IsMatch(date))
                    return string.Format("tag:{0},{1}:{2}", hostName, date, rest);
            }

            throw RuntimeFailure.CannotBuildTagUri();
        }

        //  'IEquatable' implementation.
        public bool Equals(NamespaceUri other) {
            if (object.ReferenceEquals(this, other))
                return true;

            else
                return string.Compare(other.namespaceUri, this.namespaceUri, StringComparison.Ordinal) == 0;
        }

        // `IObjectReference' implementation
        object IObjectReference.GetRealObject(StreamingContext context) {
            return NamespaceUri.Create(this.NamespaceName);
        }
    }
}
