//
// - DictionaryProperties.cs -
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

namespace Carbonfrost.Commons.Shared.Runtime {

    sealed class DictionaryProperties<T> : PropertiesImpl {

        private readonly IDictionary<string, T> items;

        public DictionaryProperties(IDictionary<string, T> items) : base(items) {
            this.items = items;
        }

        public override void ClearProperties() {
            items.Clear();
        }

        public override bool IsReadOnly(string key) {
            return this.items.IsReadOnly;
        }

        public override IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
            foreach (var m in items)
                yield return new KeyValuePair<string, object>(m.Key, m.Value);
        }

        protected override void SetPropertyCore(string key, object defaultValue, bool isImplicit) {
            this.items[key] = (T) defaultValue;
        }

        protected override bool TryGetPropertyCore(string property, Type requiredType, out object value) {
            T tvalue;
            value = null;

            if (typeof(T).IsAssignableFrom(requiredType)
                && this.items.TryGetValue(property, out tvalue)) {
                value = tvalue;
                return requiredType.IsInstanceOfType(tvalue);
            } else {
                return false;
            }
        }
    }
}
