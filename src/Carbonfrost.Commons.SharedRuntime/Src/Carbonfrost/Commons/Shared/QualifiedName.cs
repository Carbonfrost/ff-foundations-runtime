//
// - QualifiedName.cs -
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
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Xml;

using Carbonfrost.Commons.Shared.Runtime;

namespace Carbonfrost.Commons.Shared {

    [TypeConverter(typeof(QualifiedNameConverter))]
    public sealed class QualifiedName : IEquatable<QualifiedName>, IComparable<QualifiedName>, IFormattable, IObjectReference {

        [NonSerialized] private readonly int hashCodeCache;
        private readonly string localName;
        private readonly NamespaceUri ns;

        public string LocalName { get { return this.localName; } }
        public NamespaceUri Namespace { get { return this.ns; } }
        public string NamespaceName { get { return this.ns.NamespaceName; } }

        // Constructors
        internal QualifiedName(NamespaceUri ns, string localName) {
            // N.B. Argument checking is done upstream
            this.ns = ns;
            this.localName = localName;

            unchecked {
                this.hashCodeCache += 1000000009 * localName.GetHashCode();
                this.hashCodeCache += 1000000021 * ns.GetHashCode();
            }
        }

        public QualifiedName ChangeLocalName(string value) {
            VerifyLocalName("value", value);
            return this.Namespace + value;
        }

        public QualifiedName ChangeNamespace(NamespaceUri value) {
            if (value == null)
                throw new ArgumentNullException("value");
            return value + this.LocalName;
        }

        public static QualifiedName Create(NamespaceUri namespaceUri, string name) {
            return (namespaceUri ?? NamespaceUri.Default) + name;
        }

        public static QualifiedName Create(Uri namespaceUri, string name) {
            NamespaceUri nu = NamespaceUri.Default;
            if (namespaceUri != null)
                nu = NamespaceUri.Create(namespaceUri);

            return nu + name;
        }

        public static QualifiedName Expand(string text, IXmlNamespaceResolver resolver) {
            if (text == null)
                throw new ArgumentNullException("text"); // $NON-NLS-1

            text = text.Trim();
            if (text.Length == 0)
                throw Failure.EmptyString("expandedName"); // $NON-NLS-1

            resolver = resolver ?? Utility.NullNamespaceResolver;
            return _Expand(text, t => resolver.LookupNamespace(t));
        }

        public static QualifiedName Expand(string text, IDictionary<string, string> prefixMap) {
            if (text == null)
                throw new ArgumentNullException("text"); // $NON-NLS-1

            text = text.Trim();
            if (text.Length == 0)
                Failure.EmptyString("expandedName"); // $NON-NLS-1

            if (prefixMap == null)
                throw new ArgumentNullException("prefixMap"); // $NON-NLS-1

            return _Expand(text, t => (prefixMap.GetValueOrDefault(t)));
        }

        public static bool TryParse(string text, IServiceProvider serviceProvider, out QualifiedName value) {
            value = _TryParse(text, false, serviceProvider);
            return value != null;
        }

        public static bool TryParse(string text, out QualifiedName value) {
            value = _TryParse(text, false, null);
            return value != null;
        }

        public static QualifiedName Parse(string text) {
            return _TryParse(text, true, ServiceProvider.Null);
        }

        public static QualifiedName Parse(string text, IServiceProvider serviceProvider) {
            return _TryParse(text, true, serviceProvider);
        }

        static QualifiedName _TryParse(string text, bool throwOnError, IServiceProvider serviceProvider) {
            serviceProvider = serviceProvider ?? ServiceProvider.Null;

            if (text == null) {
                if (throwOnError)
                    throw new ArgumentNullException("text"); // $NON-NLS-1
                else
                    return null;
            }

            if (text.Length == 0) {
                if (throwOnError)
                    throw Failure.EmptyString("text"); // $NON-NLS-1
                else
                    return null;
            }

            if (text[0] != '{') {
                if (text.Contains(":"))
                    return Expand(text, (IXmlNamespaceResolver) serviceProvider.GetService(typeof(IXmlNamespaceResolver)));
                else
                    return NamespaceUri.Default.GetName(text.Trim());
            }

            int num = text.LastIndexOf('}');

            if ((num <= 1) || (num == (text.Length - 1))) {
                if (throwOnError)
                    throw Failure.NotParsable("text", typeof(QualifiedName));
                else
                    return null;
            }

            if (num - 1 == 0) {
                // The default namespace is used (as in '{} expandedName')
                return NamespaceUri.Default.GetName(text.Trim());

            } else {
                // Some other namespace is used
                string ns = text.Substring(1, num - 1);
                string localName = text.Substring(num + 1).Trim();

                NamespaceUri nu = NamespaceUri._TryParse(ns, false);
                if (nu == null) {
                    if (throwOnError)
                        throw Failure.NotParsable("text", typeof(QualifiedName));
                    else
                        return null;
                }
                return nu.GetName(localName);
            }
        }

        public static QualifiedName Create(string namespaceName, string localName) {
            return NamespaceUri.Parse(namespaceName).GetName(localName);
        }

        // Operators.
        public static bool operator ==(QualifiedName left, QualifiedName right) {
            return StaticEquals(left, right);
        }

        public static bool operator !=(QualifiedName left, QualifiedName right) {
            return !StaticEquals(left, right);
        }

        // 'Object' overrides.
        public override bool Equals(object obj) {
            return Equals(obj as QualifiedName);
        }

        public override int GetHashCode() {
            return hashCodeCache;
        }

        public override string ToString() {
            return FullName();
        }

        string FullName() {
            if (this.ns.IsDefault) {
                return this.localName;
            } else {
                return string.Concat("{", this.ns.NamespaceName, "} ", this.localName); // $NON-NLS-1, $NON-NLS-2
            }
        }

        public string ToString(string format) {
            return ToString(format, CultureInfo.InvariantCulture);
        }

        // IFormattable
        public string ToString(string format, IFormatProvider formatProvider) {
            if (string.IsNullOrEmpty(format))
                return FullName();

            if (format.Length > 1)
                throw new FormatException();

            switch (char.ToUpperInvariant(format[0])) {
                case 'G':
                case 'F':
                    return FullName();
                case 'S':
                    return this.localName;
                case 'N':
                    return this.ns.NamespaceName;
                case 'M':
                    return string.Concat("{", this.ns.NamespaceName, "}");
                default:
                    throw new FormatException();
            }
        }

        // 'IEquatable' implementation.
        static bool StaticEquals(QualifiedName a, QualifiedName b) {
            if (object.ReferenceEquals(a, b)) {
                return true;
            } else if (object.ReferenceEquals(a, null)
                       || object.ReferenceEquals(b, null)) {
                return false;
            } else {
                return a.localName == b.localName
                    && object.Equals(a.ns, b.ns);
            }
        }

        public bool Equals(QualifiedName other) {
            return StaticEquals(this, other);
        }

        internal static void VerifyLocalName(string argName, string value) {
            if (value == null)
                throw new ArgumentNullException(argName);
            if (value.Length == 0)
                throw Failure.EmptyString(argName);

            foreach (char c in value) {
                if (!IsValidChar(c))
                    throw RuntimeFailure.NotValidLocalName(argName);
            }
        }

        static bool IsValidChar(char c) {
            return ('A' <= c && c <= 'Z')
                || ('a' <= c && c <= 'z')
                || ('0' <= c && c <= '9')
                || (c == '-' || c == '_' || c == '.');
        }

        static QualifiedName _Expand(string text, Func<string, string> d) {
            if (text[0] == '{')
                return Parse(text);

            int index = text.IndexOf(':');
            if (index < 0)
                return NamespaceUri.Default.GetName(text);

            string prefix = text.Substring(0, index);
            string name = text.Substring(index + 1);
            string fullNs = d(prefix);
            if (fullNs == null)
                throw RuntimeFailure.CannotExpandPrefixNotFound(prefix);
            else
                return QualifiedName.Create(fullNs, name);
        }

        // `IComparable' implementation
        public int CompareTo(QualifiedName other) {
            if (other == null)
                return 1;
            else
                return string.Compare(this.FullName(), other.FullName(), StringComparison.Ordinal);
        }

        // `IObjectReference' implementation
        object IObjectReference.GetRealObject(StreamingContext context) {
            return QualifiedName.Create(this.Namespace, this.LocalName);
        }

        internal bool EqualsIgnoreCase(QualifiedName name) {
            return this == name
                || (Namespace == name.Namespace && string.Equals(LocalName, name.LocalName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
