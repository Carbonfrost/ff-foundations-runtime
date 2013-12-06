//
// - PropertyProviderExtensions.cs -
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Carbonfrost.Commons.Shared.Runtime {

    public static class PropertyProviderExtensions {

        public static bool HasProperty(this IPropertyProvider source,
                                       string property) {
            if (source == null)
                throw new ArgumentNullException("source"); // $NON-NLS-1

            object dummy;
            return source.TryGetProperty(property, out dummy);
        }

        public static bool GetBoolean(this IPropertyProvider source, string property) {
            if (source == null)
                throw new ArgumentNullException("source");

            return Convert.ToBoolean(source.GetProperty(property));
        }

        public static byte GetByte(this IPropertyProvider source, string property) {
            if (source == null)
                throw new ArgumentNullException("source");
            return Convert.ToByte(source.GetProperty(property));
        }

        public static char GetChar(this IPropertyProvider source, string property) {
            if (source == null)
                throw new ArgumentNullException("source");
            return Convert.ToChar(source.GetProperty(property));
        }

        public static DateTime GetDateTime(this IPropertyProvider source, string property) {
            if (source == null)
                throw new ArgumentNullException("source");
            return Convert.ToDateTime(source.GetProperty(property));
        }

        public static decimal GetDecimal(this IPropertyProvider source, string property) {
            if (source == null)
                throw new ArgumentNullException("source");

            return Convert.ToDecimal(source.GetProperty(property));
        }

        public static double GetDouble(this IPropertyProvider source, string property) {
            if (source == null)
                throw new ArgumentNullException("source");

            return Convert.ToDouble(source.GetProperty(property));
        }

        public static short GetInt16(this IPropertyProvider source, string property) {
            if (source == null)
                throw new ArgumentNullException("source");

            return Convert.ToInt16(source.GetProperty(property));
        }

        public static int GetInt32(this IPropertyProvider source, string property) {
            if (source == null)
                throw new ArgumentNullException("source");
            return Convert.ToInt32(source.GetProperty(property));
        }

        public static long GetInt64(this IPropertyProvider source, string property) {
            if (source == null)
                throw new ArgumentNullException("source");

            return Convert.ToInt64(source.GetProperty(property));
        }

        [CLSCompliant(false)]
        public static sbyte GetSByte(this IPropertyProvider source, string property) {
            if (source == null)
                throw new ArgumentNullException("source");

            return Convert.ToSByte(source.GetProperty(property));
        }

        public static float GetSingle(this IPropertyProvider source, string property) {
            if (source == null)
                throw new ArgumentNullException("source");

            return Convert.ToSingle(source.GetProperty(property));
        }

        public static string GetString(this IPropertyProvider source, string property) {
            if (source == null)
                throw new ArgumentNullException("source");

            return Convert.ToString(source.GetProperty(property));
        }

        [CLSCompliant(false)]
        public static ushort GetUInt16(this IPropertyProvider source, string property) {
            if (source == null)
                throw new ArgumentNullException("source");

            return Convert.ToUInt16(source.GetProperty(property));
        }

        [CLSCompliant(false)]
        public static uint GetUInt32(this IPropertyProvider source, string property) {
            if (source == null)
                throw new ArgumentNullException("source");

            return Convert.ToUInt32(source.GetProperty(property));
        }

        [CLSCompliant(false)]
        public static ulong GetUInt64(this IPropertyProvider source, string property) {
            if (source == null)
                throw new ArgumentNullException("source");

            return Convert.ToUInt64(source.GetProperty(property));
        }

        // If no value is present, the value is simply set.
        // If a value is present, and it is a value, the value is converted to
        // an array containing that existing element plus <c>value</c>.
        // If the value located there is a collection of some sort, then
        // the value is added to it.  If the collection is read-only, its
        // contents are copied into a new.
        public static void Push(this IProperties source, string property, object value) {
            if (source == null)
                throw new ArgumentNullException("source");

            object current;
            if (source.TryGetProperty(property, out current)) {
                IEnumerable e = current as IEnumerable;
                if (e == null)
                    source.SetProperty(property, new object[] { current, value });
                else {
                    object[] objArray = current as object[];
                    if (objArray != null) {
                        Array.Resize(ref objArray, objArray.Length + 1);
                        objArray[objArray.Length - 1] = value;
                    } else {
                        List<object> items = new List<object>(e.Cast<object>());
                        items.Add(value);
                        source.SetProperty(property, items.ToArray());
                    }
                }

            } else {
                source.SetProperty(property, value);
            }
        }

        public static object GetProperty(this IPropertyProvider source, string property) {
            if (source == null)
                throw new ArgumentNullException("source"); // $NON-NLS-1

            return source.GetProperty<object>(property, null);
        }

        public static T GetProperty<T>(this IPropertyProvider source, string property) {
            if (source == null)
                throw new ArgumentNullException("source"); // $NON-NLS-1

            return source.GetProperty<T>(property, default(T));
        }

        public static T GetProperty<T>(this IPropertyProvider source, string property, T defaultValue) {
            if (source == null)
                throw new ArgumentNullException("source"); // $NON-NLS-1

            object result;
            if (source.TryGetProperty(property, out result))
                return (T) result;
            else
                return defaultValue;
        }

        public static bool TryGetProperty<T>(this IPropertyProvider source, string property, out T value) {
            if (source == null) { throw new ArgumentNullException("source"); } // $NON-NLS-1

            object objValue;
            bool result = source.TryGetProperty(property, typeof(T), out objValue);
            if (result && objValue is T) {
                value = (T) objValue;
                return true;
            } else {

                value = default(T);
                return false;
            }
        }

    }
}
