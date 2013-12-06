//
// - Utility.cs -
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Carbonfrost.Commons.Shared.Runtime;

namespace Carbonfrost.Commons.Shared {

    static class Utility {

        const string BASE_64_PREFIX = "base64:"; // $NON-NLS-1
        const int BUFFER_CAPACITY = 2048;
        static readonly Regex VALID_ID = new Regex("^[a-zA-Z0-9_.-]+$");
        static readonly Assembly THIS_ASSEMBLY = typeof(Utility).Assembly;
        static readonly Assembly CORLIB = typeof(object).Assembly;

        public static readonly IXmlNamespaceResolver NullNamespaceResolver = new NullNamespaceResolverImpl();
        public static readonly IEqualityComparer<Type> EquivalentComparer = new ExtendedTypeComparer();

        private class ExtendedTypeComparer : IEqualityComparer<Type> {

            // If a forwarded type is used, we still want it to be treated
            // the same in the dictionary in .Net 4

            public bool Equals(Type x, Type y) {
                #if NET_4_0
                return x.IsEquivalentTo(y);
                #else
                return x.Equals(y);
                #endif
            }

            public int GetHashCode(Type obj) {
                return obj.FullName.GetHashCode();
            }

        }

        class NullNamespaceResolverImpl : IXmlNamespaceResolver {

            public IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope) {
                return new Dictionary<string, string>();
            }

            public string LookupNamespace(string prefix) {
                return null;
            }

            public string LookupPrefix(string namespaceName) {
                return null;
            }
        }

        public static string Display(object value) {
            if (object.ReferenceEquals(value, null))
                return "<null>";
            else
                return string.Concat("`", value, "'");
        }

        public static string[] SplitInTwo(string text, char c) {
            int i = text.IndexOf(c);
            if (i < 0)
                return new string[] { text };
            else
                return new string[] { text.Substring(0, i), text.Substring(i + 1) };
        }

        public static Regex SplitExtensionList(string extensions) {
            return new Regex(TransformPattern(extensions));
        }

        private static string TransformPattern(string text) {
            // Makes a wildcard pattern into a regex pattern

            StringBuilder sb = new StringBuilder();
            sb.Append("^(");
            CharEnumerator c = text.GetEnumerator();

            while (c.MoveNext()) {
                if (c.Current == '\\') {
                    c.MoveNext();
                    sb.Append('\\');
                    sb.Append(c.Current);
                }

                string str = MapChar(c.Current);
                if (str == null)
                    sb.Append(c.Current);
                else
                    sb.Append(str);
            }
            sb.Append(")$");
            return sb.ToString();
        }

        private static string MapChar(char c) {
            switch (c) {
                case '$':
                case '(':
                case ')':
                case '+':
                case '.':
                case '[':
                case ']':
                case '^':
                    return (@"\" + c);

                case ';':
                    return "|";

                case '*':
                    return @"[^\s]*";

                case '?':
                    return @"[^\s]";
            }
            return null;
        }

        public static byte[] BufferedCopyToBytes(this Stream source) {
            int capacity = BUFFER_CAPACITY;
            try {
                capacity = (int) source.Length;
            } catch (NotSupportedException) {
            }

            MemoryStream memory = new MemoryStream(capacity);
            source.CopyTo(memory, BUFFER_CAPACITY);
            return memory.ToArray();
        }

        public static string BytesToHex(byte[] value, bool lowercase = false) {
            string result = BitConverter.ToString(value).Replace("-", string.Empty); // $NON-NLS-1
            if (lowercase)
                return result.ToLowerInvariant();
            else
                return result;
        }

        public static StreamReader MakeStreamReader(Stream stream, Encoding encoding) {
            if (encoding == null)
                return new StreamReader(stream);
            else
                return new StreamReader(stream, encoding);
        }

        public static byte[] ConvertHexToBytes(string hex, bool allowBase64 = true) {
            if (allowBase64 && hex.StartsWith(BASE_64_PREFIX, StringComparison.OrdinalIgnoreCase))
                return Convert.FromBase64String(hex.Substring(BASE_64_PREFIX.Length));

            if ((hex.Length % 2) == 1)
                throw RuntimeFailure.NotValidHexString();

            byte[] result = new byte[hex.Length / 2];

            for (int i = 0; i < result.Length; i++) {
                result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return result;
        }

        public static string GetCanonicalTypeName(Type type) {
            if (type == null)
                throw new ArgumentNullException("type");

            QualifiedName qn = type.GetQualifiedName();
            if (qn == null)
                return type.Name;

            string prefix = AssemblyInfo.GetAssemblyInfo(type.Assembly)
                .GetXmlNamespacePrefix(qn.Namespace);
            if (string.IsNullOrEmpty(prefix))
                return type.Name;
            else
                return string.Concat(prefix, ':', type.Name);
        }

        public static bool IsAnonymousName(string name) {
            Guid dummy;
            return Guid.TryParse(name, out dummy);
        }

        public static Stream ApplyAccess(Stream stream, FileAccess access) {
            bool canRead = stream.CanRead;
            bool canWrite = stream.CanWrite;

            if ((access == FileAccess.ReadWrite) && canRead && canWrite) {
                return stream;

            } else if (access == FileAccess.Read && canRead) {
                return new DirectedStream(StreamDirection.Read, stream);

            } else if (access == FileAccess.Write && canWrite) {
                return new DirectedStream(StreamDirection.Write, stream);

            } else {
                return null;
            }
        }

        public static string SubstringAfter(string source, char value) {
            if (string.IsNullOrEmpty(source))
                return source;

            CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
            int index = compareInfo.IndexOf(source, value, CompareOptions.Ordinal);
            if (index < 0)
                return string.Empty;

            return source.Substring(index + 1);
        }

        public static PropertyInfo FindIndexerProperty(Type type) {
            foreach (PropertyInfo pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                var parameters = pi.GetIndexParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                    return pi;
            }
            return null;
        }

        public static string Unescape(string text) {
            StringBuilder result = new StringBuilder();

            using (StringReader reader = new StringReader(text)) {
                int i;

                while ((i = reader.Read()) != -1) {
                    char c = (char) i;

                    if (c == '\\') {
                        i = reader.Read();
                        if (i == -1) {
                            result.Append('\\');
                            break;
                        }

                        char ch = (char) i;
                        switch (ch) {
                            case 'b':
                                ch = '\b';
                                break;

                            case 't':
                                ch = '\t';
                                break;

                            case 'n':
                                ch = '\n';
                                break;

                            case 'f':
                                ch = '\f';
                                break;

                            case 'r':
                                ch = '\r';
                                break;

                            case 'u':
                                string unicodeValue = new String(_ReadOrThrow(reader, 4));
                                result.Append(UnescapeUnicode(unicodeValue));
                                break;

                            case 'x':
                                string hexValue = new String(_ReadOrThrow(reader, 2));
                                result.Append(UnescapeHex(hexValue));
                                break;

                            default:
                                break;
                        }

                        result.Append(ch);
                    } else {
                        result.Append(c);
                    }

                } // end while

            } // end using

            return result.ToString();
        }

        public static char UnescapeUnicode(string chars) {
            if (chars == null)
                throw RuntimeFailure.IncompleteEscapeSequence();
            int unicodeValue = Convert.ToInt32(chars, 16);
            return Convert.ToChar(unicodeValue);
        }

        public static char UnescapeHex(string chars) {
            if (chars == null)
                throw RuntimeFailure.IncompleteEscapeSequence();

            int hexValue = Convert.ToInt32(chars, 16);
            return Convert.ToChar(hexValue);
        }

        private static char[] _ReadOrThrow(TextReader reader, int count) {
            char[] result = new char[count];
            for (int i = 0; i < count; i++) {
                int character = reader.Read();
                if (character == -1)
                    throw RuntimeFailure.IncompleteEscapeSequence();

                result[i] = (char) character;
            }

            return result;
        }

        public static string GetImpliedName(Type type, string name) {
            string tname = type.Name;
			if (tname.EndsWith(name, StringComparison.Ordinal))
				return tname.Substring(0, tname.Length - name.Length);
			else
				return tname;
        }

        public static StreamWriter MakeStreamWriter(Stream s, Encoding encoding) {
            if (encoding == null)
                return new StreamWriter(s);
            else
                return new StreamWriter(s, encoding);
        }

        public static IEnumerable<string> GetNamespaces(this Assembly a) {
            return a.GetTypes().Select(t => t.Namespace).Distinct();
        }

        public static IEnumerable<string> FilterNamespaces(IEnumerable<string> all,
                                                           string pattern) {
            if (string.IsNullOrWhiteSpace(pattern) || pattern == "*")
                return all;

            Regex r = GetNamespaceFilterRegex(pattern);
            return all.Where(t => r.IsMatch(t ?? string.Empty));
        }

        public static Encoding GetEncodingFromContentType(string contentType) {
            Match m = Regex.Match(contentType, @"charset\s*=\s*(?'Encoding'[^;]+)");
            return m.Success ? Encoding.GetEncoding(m.Groups["Encoding"].Value) : null;
        }

        public static void ValidateComponentId(string argName, string id) {
            if (!VALID_ID.IsMatch(id))
                throw Failure.NotCompliantIdentifier("id", id);
        }

        public static Regex GetNamespaceFilterRegex(string pattern) {
            StringBuilder sb = new StringBuilder();

            foreach (string p in pattern.Split(',')) {
                if (sb.Length > 0)
                    sb.Append("|");

                sb.Append(GetNamespaceFilterRegexInternal(p));
            }

            Regex r = new Regex(sb.ToString());
            return r;
        }

        static string GetNamespaceFilterRegexInternal(string pattern) {
            // Last one is special (allow .* to be used at end)
            pattern = pattern.Trim();
			if (pattern.EndsWith(".*", StringComparison.Ordinal))
				pattern = pattern.Substring(0, pattern.Length - 2) + (@"(\..+)?");

            return string.Concat("(^", pattern.Replace("*", ".+?"), "$)");
        }

        public static T FindServiceOrInheritanceAncestor<T>(object component, IServiceProvider serviceProvider)
            where T : class
        {
            T t = (T) serviceProvider.GetService(typeof(T));
            if (t != null)
                return t;

            // TODO Allow invoking the context provider (IInheritanceContextProvider service)
            return component.GetInheritanceAncestor<T>();
        }

        public static bool IsScannableAssembly(Assembly a) {
            if (a == THIS_ASSEMBLY)
                return true;

            bool isCorlib = a.GetName().GetPublicKey()
                .SequenceEqual(CORLIB.GetName().GetPublicKey());

            if (isCorlib)
                return false;

            return a.GetReferencedAssemblies().Any(t => t.Name == THIS_ASSEMBLY.GetName().Name);
        }

        public static string Camel(string name) {
            return char.ToLowerInvariant(name[0]) + name.Substring(1);
        }

        public static T LateBoundProperty<T>(object source,
                                             string property) {

            PropertyInfo pi = source.GetType().GetProperty(property);
            if (pi == null)
                return default(T);
            try {
                object value = pi.GetValue(source, null);
                if (pi.PropertyType == typeof(T))
                    return (T) value;
                else
                    return default(T);

            } catch (Exception ex) {
                if (Require.IsCriticalException(ex))
                    throw;

                return default(T);
            }
        }

        internal static string[] SplitText(ref string[] cache, string text, char[] chars = null) {
            chars = chars ?? new [] { '\t', '\r', '\n', ' ', ';', ',' };
            if (cache == null)
                return cache = (text ?? string.Empty).Split(chars, StringSplitOptions.RemoveEmptyEntries);
            else
                return cache;
        }
    }

}
