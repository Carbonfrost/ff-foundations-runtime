//
// - NamespaceBinding.cs -
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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Carbonfrost.Commons.Shared.Runtime.Components {

    [TypeConverter(typeof(NamespaceBindingConverter))]
    public sealed class NamespaceBinding : IEquatable<NamespaceBinding> {

        [SelfDescribingPriority(PriorityLevel.High)]
        public string Namespace { get; private set; }

        [SelfDescribingPriority(PriorityLevel.High)]
        public AssemblyName AssemblyName { get; private set; }

        public NamespaceBinding(string @namespace, AssemblyName assemblyName) {
            if (assemblyName == null)
                throw new ArgumentNullException("assemblyName");

            this.Namespace = @namespace;
            this.AssemblyName = assemblyName;
        }

        private Assembly LoadAssembly() {
            return Assembly.Load(this.AssemblyName);
        }

        public IEnumerable<string> EnumerateNamespaces() {
            var s = LoadAssembly().GetTypes().Select(t => t.Namespace).Distinct();
            return Utility.FilterNamespaces(s, this.Namespace);
        }

        public IEnumerable<Type> EnumerateTypes() {
            var types = LoadAssembly().GetTypes();
            if (string.IsNullOrEmpty(this.Namespace))
                return types;

            Regex r = Utility.GetNamespaceFilterRegex(this.Namespace);
            return types.Where(t => r.IsMatch(t.Namespace));
        }

        public static NamespaceBinding FromUri(Uri uri) {
            if (uri == null)
                throw new ArgumentNullException("uri");

            NamespaceBinding binding;
            Exception ex = _TryConvertUri(uri, out binding);
            if (ex == null)
                return binding;
            else
                throw ex;
        }

        public Uri ToUri() {
            return new Uri(string.Format("clr-namespace:{0};assembly:{1}",
                                         Namespace, AssemblyName));
        }

        public override string ToString() {
            return ToUri().ToString();
        }

        // `IEquatable' implementation
        public bool Equals(NamespaceBinding other) {
            return StaticEquals(this, other);
        }

        // `object' overrides
        public override bool Equals(object obj) {
            return StaticEquals(this, obj as NamespaceBinding);
        }

        public override int GetHashCode() {
            int hashCode = 0;
            unchecked {
                hashCode += 1000000007 * Namespace.GetHashCode();
                hashCode += 1000000009 * AssemblyName.GetHashCode();
            }
            return hashCode;
        }

        public static bool operator ==(NamespaceBinding lhs, NamespaceBinding rhs) {
            return StaticEquals(lhs, rhs);
        }

        public static bool operator !=(NamespaceBinding lhs, NamespaceBinding rhs) {
            return !StaticEquals(lhs, rhs);
        }

        public static NamespaceBinding Parse(string text) {
            NamespaceBinding result;
            Exception ex = _TryParse(text, out result);
            if (ex == null)
                return result;
            else
                throw ex;
        }

        public static bool TryParse(string text, out NamespaceBinding result) {
            return _TryParse(text, out result) == null;
        }

        static bool StaticEquals(NamespaceBinding lhs, NamespaceBinding rhs) {
            if (ReferenceEquals(lhs, rhs))
                return true;
            if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
                return false;

            return lhs.Namespace == rhs.Namespace
                && object.Equals(lhs.AssemblyName, rhs.AssemblyName);
        }

        static Exception _TryParse(string text, out NamespaceBinding result) {
            result = null;

            if (text == null)
                return new ArgumentNullException("text");
            if (text.Length == 0)
                return Failure.EmptyString("text");

            Uri uri;
            Exception ex = null;
            if (Uri.TryCreate(text, UriKind.RelativeOrAbsolute, out uri)) {
                ex = _TryConvertUri(uri, out result);
                if (ex == null)
                    return null;
            }

            return Failure.NotParsable("text", typeof(NamespaceBinding), ex);
        }


        static Exception _TryConvertUri(Uri uriString, out NamespaceBinding binding) {
            binding = null;

            if (uriString.Scheme != "clr-namespace")
                return RuntimeFailure.UnknownUriScheme(typeof(NamespaceBinding));

            var dict = ParseDictionary(uriString.ToString());
            if (dict == null || dict.Count != 2)
                return new UriFormatException();

            string assembly = dict.GetValueOrDefault("assembly");
            string ns = dict.GetValueOrDefault("clr-namespace");
            if (assembly == null || ns == null)
                return new UriFormatException();

            try {
                binding = new NamespaceBinding(
                    ns, new AssemblyName(assembly));
                return null;

            } catch (ArgumentException ex) {
                return ex;
            }
        }

        static IDictionary<string, string> ParseDictionary(string text) {
            string[] items = text.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            Dictionary<string, string> lookup = new Dictionary<string, string>();
            foreach (string s in items) {
                int index = s.IndexOf(':');
                if (index < 0)
                    return null;

                else {
                    string key = s.Substring(0, index).Trim();
                    string value = s.Substring(index + 1).Trim();
                    lookup.Add(key, value);
                }
            }

            return lookup;
        }

    }
}
