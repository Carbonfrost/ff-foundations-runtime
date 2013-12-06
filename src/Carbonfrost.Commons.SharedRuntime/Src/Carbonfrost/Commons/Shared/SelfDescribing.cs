//
// - SelfDescribing.cs -
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
using System.Reflection;
using System.Text;

namespace Carbonfrost.Commons.Shared {

    public static class SelfDescribing {

        public static string ToString(object instance) {
            return ToString(instance, PriorityLevel.Default);
        }

        public static string ToString(object instance,
                                      PriorityLevel verbosity) {
            return ToString(instance, (int) verbosity);
        }

        public static string ToString(object instance,
                                      int verbosity) {

            if (instance == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            _ToString(instance, verbosity, 0, sb);

            return sb.ToString();
        }

        private static void _ToString(object instance,
                                      int verbosity,
                                      int depth,
                                      StringBuilder sb) {

            if (instance == null)
                return;

            Type type = instance.GetType();
            // Don't print properties on an escalated depth or non-self describing
            if (depth >= 3) { return; }

            // Check for an implementation of ToString
            var mi = instance.GetType().GetMethod(
                "ToString", BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public,
                null, Type.EmptyTypes, null);

            if (mi == null) {
                sb.Append(instance);
                return;

            } else {
                // Continue below
                sb.Append(Utility.GetCanonicalTypeName(type));
                sb.Append(' ');
            }

            bool bNeedComma = false;
            sb.Append('(');

            // Print each property one by one:
            foreach (PropertyInfo member in type.GetProperties()) {
                SelfDescribingPriorityAttribute attr =
                    Attribute.GetCustomAttribute(member, typeof(SelfDescribingPriorityAttribute)) as SelfDescribingPriorityAttribute
                    ?? SelfDescribingPriorityAttribute.Default;
                if (attr.PriorityLevel < verbosity) { continue; }

                // Inherit the verbosity settings:
                try {
                    object o = member.GetValue(instance, null);
                    if (o != null) {
                        if (bNeedComma) { sb.Append(", "); } // $NON-NLS-1

                        sb.Append(member.Name);
                        sb.Append('=');

                        // Process the child (with an increased depth)
                        _ToString(o, verbosity + 50, depth + 1, sb);
                        bNeedComma = true;
                    }
                } catch (Exception ex) {
                    if (Require.IsCriticalException(ex))
                        throw;
                }
            } // next

            sb.Append(')');
        }

    }
}
