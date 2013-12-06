//
// - PropertyProvider.Static.cs -
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
using System.Linq;
using System.Text;
using Carbonfrost.Commons.Shared.Runtime;

namespace Carbonfrost.Commons.Shared.Runtime {

    partial class PropertyProvider {

        public static IPropertyProvider Null { get { return Properties.Null; } }

        public static IPropertyProvider Compose(IEnumerable<IPropertyProvider> providers) {
            if (providers == null)
                throw new ArgumentNullException("providers"); // $NON-NLS-1

            return ComposeCore(providers.ToArray());
        }

        public static IPropertyProvider Compose(IEnumerable<KeyValuePair<string, IPropertyProvider>> providers) {
            if (providers == null)
                throw new ArgumentNullException("providers"); // $NON-NLS-1

            return new NamespacePropertyProvider(providers);
        }

        public static IPropertyProvider Compose(
            IEnumerable<KeyValuePair<string, object>> propertyProviders) {
            if (propertyProviders == null)
                throw new ArgumentNullException("propertyProviders"); // $NON-NLS-1

            return Compose(propertyProviders.Select(
                s => new KeyValuePair<string, IPropertyProvider>(s.Key, PropertyProvider.FromValue(s.Value))));
        }

        public static IPropertyProvider Compose(params IPropertyProvider[] providers) {
            if (providers == null)
                throw new ArgumentNullException("providers"); // $NON-NLS-1

            return ComposeCore((IPropertyProvider[]) providers.Clone());
        }

        public static IPropertyProviderExtension Extend(IPropertyProvider provider) {
            if (provider == null)
                throw new ArgumentNullException("provider"); // $NON-NLS-1

            IPropertyProviderExtension ppe = provider as IPropertyProviderExtension;
            if (ppe == null)
                return new PropertyProviderExtensionAdapter(provider);
            else
                return ppe;
        }

        public static IPropertyProvider FromValue(object context) {
            if (context == null)
                throw new ArgumentNullException("context");

            IPropertyProvider pp = context as IPropertyProvider;
            if (pp != null)
                return pp;
            if (context == null)
                return Properties.Null;

            return new ReflectionPropertyProvider(context);
        }

        public static IPropertyProvider FromArray(params object[] values) {
            if (values == null || values.Length == 0)
                return PropertyProvider.Null;

            return new ArrayPropertyProvider(values);
        }

        public static string Format(string format, object args) {
            if (string.IsNullOrEmpty(format))
                return string.Empty;

            if (args == null)
                return format;

            return Format(format, (IPropertyProvider) Properties.FromValue(args));
        }

        public static string Format(string format,
                                    IPropertyProvider propertyProvider) {

            if (string.IsNullOrEmpty(format))
                return string.Empty;

            if (propertyProvider == null)
                return format;

            // Initialize the builder with the default length of the input
            StringReader sr = new StringReader(format);
            StringBuilder result = new StringBuilder(format.Length);
            int i;

            while ((i = sr.Read()) > 0) {
                char c = (char) i;

                if (c == '$') {
                    int nextChar = sr.Peek();
                    if (nextChar == '$') {
                        // $$ This is the dollar literal, take the second one off the stream
                        result.Append((char) sr.Read());

                    } else if (nextChar == '{') {
                        // Collect the token name ${...} and append the lookup result
                        string tokenName = CollectTokenName(sr);
                        object resolvedValue = Eval(propertyProvider, tokenName);
                        string output = Convert.ToString(resolvedValue);

                        if (output == null) {
                            result.Append('$');
                            result.Append('{');
                            result.Append(tokenName);
                            result.Append('}');
                        } else
                            result.Append(output);

                    } else
                        throw RuntimeFailure.ExpectedLeftBraceOrDollar();

                } else
                    result.Append(c);

            } // end while

            sr.Dispose();
            return result.ToString();
        }

        public static string Format(string format,
                                    IEnumerable<KeyValuePair<string, object>> propertyProvider) {
            if (string.IsNullOrEmpty(format))
                return string.Empty;

            if (propertyProvider == null)
                return format;

            return Format(format, PropertyProvider.Compose(propertyProvider));
        }

        static IPropertyProvider ComposeCore(IPropertyProvider[] providers) {
            if (providers.Length == 0)
                return Properties.Null;

            else if (providers.Length == 1)
                return providers[0];

            else
                return new CompositePropertyProvider(providers);
        }

        static object Eval(IPropertyProvider pp, string tokenName) {
            object result = pp.GetProperty(tokenName);

            Delegate d = result as Delegate;
            if (d == null)
                return result;
            else
                return d.DynamicInvoke(null);
        }

        private static string CollectTokenName(TextReader reader) {
            reader.Read();
            StringBuilder result = new StringBuilder();
            int i;

            while ((i = reader.Read()) > 0) {
                char c = (char) i;

                if (c == '}') {
                    if (result.Length == 0)
                        throw RuntimeFailure.ExpectedIdentifier();

                    string value = result.ToString();
                    return value;
                } else {
                    result.Append(c);
                }
            }

            throw RuntimeFailure.ExpectedRightBrace();
        }

    }
}
