//
// - Mixins.cs -
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
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Shared {

    static class Mixins {

        public static IEnumerable<Type> GetTypesHelper(this Assembly a) {
            // Sometimes we don't care about type load exceptions
            // because we can't recover anyway:
            try {
                return a.GetTypes();

            } catch (ReflectionTypeLoadException e) {
                return e.Types.WhereNotNull();
            }
        }

        public static IEnumerable<T> Sorted<T>(this IEnumerable<T> source) {
            return source.OrderBy(t => t);
        }

        public static IEnumerable<TValue> Values<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source) {
            return source.Select(t => t.Value);
        }

        public static T SingleOrThrow<T>(this IEnumerable<T> source, Func<Exception> error) where T: class {
            T result = null;

            foreach (T t in source) {
                if (result == null) {
                    result = t;
                } else
                    throw error();
            }
            return result;
        }

        public static TValue GetValueOrCache<TKey, TValue>(this IDictionary<TKey, TValue> source,
                                                           TKey key,
                                                           TValue defaultValue) {
            TValue v;
            if (!source.TryGetValue(key, out v)) {
                source.Add(key, v = defaultValue);
            }
            return v;
        }

        public static TValue GetValueOrCache<TKey, TValue>(this IDictionary<TKey, TValue> source,
                                                           TKey key,
                                                           Func<TValue> cacheFunction = null) {
            TValue v;
            if (!source.TryGetValue(key, out v)) {
                source.Add(key, v = cacheFunction());
            }
            return v;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> source,
                                                             TKey key,
                                                             TValue defaultValue = default(TValue)) {
            TValue v;
            if (!source.TryGetValue(key, out v)) {
                return defaultValue;
            }
            return v;
        }

        public static void AddIfNotNull<T>(this ICollection<T> source, T item) where T : class {
            if (item == null) return;
            source.Add(item);
        }

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> source) where T : class {
            foreach (var t in source)
                if (t != null) yield return t;
        }

        public static T FirstNonNull<T>(this IEnumerable<T> source) where T : class {
            foreach (var t in source)
                if (t != null) return t;

            return default(T);
        }

        public static TResult FirstNonNull<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector) where TResult : class {
            return source.Select(t => selector(t)).FirstNonNull();
        }

        public static string RequireNext(this CharEnumerator e, int count) {
            char[] result = new char[count];
            int index = 0;
            while (count-- > 0 && e.MoveNext()) {
                result[index++] = e.Current;
            }

            if (count != 0)
                return null;
            else
                return new String(result);
        }

        public static char? RequireNext(this CharEnumerator e) {
            if (e.MoveNext())
                return e.Current;
            else
                return null;
        }
    }
}
