//
// - LazyDescriptionDictionary.cs -
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

namespace Carbonfrost.Commons.Shared.Runtime {

    sealed class LazyDescriptionDictionary<TKey, TValue>
        : LazyDescriptionCollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue> {

        // TODO We overwrite duplicated keys (we probably shouldn't)

        IDictionary<TKey, TValue> Map {
            get {
                return (IDictionary<TKey, TValue>) base.Inner;
            }
        }

        private LazyDescriptionDictionary(Func<Assembly, IEnumerable<KeyValuePair<TKey, TValue>>> selector)
            : base(selector, new Dictionary<TKey, TValue>()) {

        }

        internal static IDictionary<TKey, TValue> Create(Func<Assembly, IEnumerable<KeyValuePair<TKey, TValue>>> selector) {
            var result = new LazyDescriptionDictionary<TKey, TValue>(selector);
            AssemblyInfo.AddStaticFilter(result);
            return result;
        }

        // IDictionary implementation
        public TValue this[TKey key] {
            get {
                TValue dummy;
                Continue(key, out dummy);
                return Map[key];
            }
            set {
                Map[key] = value;
            }
        }

        public ICollection<TKey> Keys {
            get {
                AssemblyInfo.CompleteAppDomain();
                return Map.Keys;
            }
        }

        public ICollection<TValue> Values {
            get {
                AssemblyInfo.CompleteAppDomain();
                return Map.Values;
            }
        }

        public bool ContainsKey(TKey key) {
            TValue dummy;
            return Continue(key, out dummy);
        }

        protected override void AddValue(KeyValuePair<TKey, TValue> value) {
            Map[value.Key] = value.Value;
        }

        public void Add(TKey key, TValue value) {
            Map.Add(key, value);
        }

        public bool Remove(TKey key) {
            throw new NotSupportedException();
        }

        public bool TryGetValue(TKey key, out TValue value) {
            return Continue(key, out value);
        }

        public override bool Contains(KeyValuePair<TKey, TValue> item) {
            TValue value;

            if (TryGetValue(item.Key, out value))
                return object.Equals(value, item.Value);
            else
                return false;
        }

        bool Continue(TKey key, out TValue value) {
            do {
                if (this.Map.TryGetValue(key, out value))
                    return true;

            } while (AssemblyInfo.ContinueAppDomain());

            return false;
        }

    }
}
