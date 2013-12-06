//
// - MapBuffer.cs -
//
// Copyright 2013 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

namespace Carbonfrost.Commons.Shared.Runtime {

    class MapBuffer<TKey, TElement> : Buffer<TElement> {

        private readonly Func<TElement, TKey> keySelector;
        private readonly Dictionary<TKey, TElement> items = new Dictionary<TKey, TElement>();

        public MapBuffer(Func<TElement, TKey> keySelector,
                         IEnumerable<TElement> e) : base(e) {

            this.keySelector = keySelector;
        }

        public TElement this[TKey key] {
            get {
                TElement result;
                if (items.TryGetValue(key, out result))
                    return result;

                return this.FirstOrDefault(t => object.Equals(key, keySelector(t)));
            }
        }

        protected override void OnCacheValue(TElement current) {
            this.items.Add(keySelector(current), current);
        }

    }
}
