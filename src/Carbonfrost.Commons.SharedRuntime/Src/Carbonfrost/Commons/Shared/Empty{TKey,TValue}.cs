//
// - Empty{TKey,TValue}.cs -
//
// Copyright 2010 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

namespace Carbonfrost.Commons.Shared {

    static class Empty<TKey, TValue> {

        public static readonly IDictionary<TKey, TValue> Dictionary = new NullDictionary();

        sealed class NullDictionary : IDictionary<TKey, TValue> {

            TValue IDictionary<TKey, TValue>.this[TKey key] {
                get {
                    throw new KeyNotFoundException();
                }
                set {
                    throw Failure.ReadOnlyCollection();
                }
            }

            ICollection<TKey> IDictionary<TKey, TValue>.Keys {
                get {
                    return Empty<TKey>.Array;
                }
            }

            ICollection<TValue> IDictionary<TKey, TValue>.Values {
                get {
                    return Empty<TValue>.Array;
                }
            }

            int ICollection<KeyValuePair<TKey, TValue>>.Count {
                get { return 0; } }

            bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly {
                get { return true; } }

            bool IDictionary<TKey, TValue>.ContainsKey(TKey key) {
                return false;
            }

            void IDictionary<TKey, TValue>.Add(TKey key, TValue value) {
                throw Failure.ReadOnlyCollection();
            }

            bool IDictionary<TKey, TValue>.Remove(TKey key) {
                return false;
            }

            bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value) {
                value = default(TValue);
                return false;
            }

            void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) {
                throw Failure.ReadOnlyCollection();
            }

            void ICollection<KeyValuePair<TKey, TValue>>.Clear() {
                throw Failure.ReadOnlyCollection();
            }

            bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) {
                return false;
            }

            void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            }

            bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) {
                throw Failure.ReadOnlyCollection();
            }

            IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() {
                return Empty<KeyValuePair<TKey, TValue>>.Enumerator;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return Empty<KeyValuePair<TKey, TValue>>.Enumerator;
            }
        }
    }
}
