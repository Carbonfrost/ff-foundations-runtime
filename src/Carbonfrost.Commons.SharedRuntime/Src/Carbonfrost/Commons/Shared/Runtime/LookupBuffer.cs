//
// - LookupBuffer.cs -
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

    class LookupBuffer<TKey, TElement> : Buffer<TElement> {

        private readonly Func<TElement, TKey> keySelector;
        private readonly Dictionary<TKey, IEnumerable<TElement>> groupings = new Dictionary<TKey, IEnumerable<TElement>>();

        public LookupBuffer(Func<TElement, TKey> keySelector,
                            IEnumerable<TElement> e) : base(e) {

            this.keySelector = keySelector;
        }

        public IEnumerable<TElement> this[TKey key] {
            get {
                return GetGrouping(key);
            }
        }

        private IEnumerable<TElement> GetGrouping(TKey key) {
            return groupings.GetValueOrCache(key, () => NewGrouping(key));
        }

        Buffer<TElement> NewGrouping(TKey key) {
            IEnumerable<TElement> f = this;
            return new Buffer<TElement>(f.Where(e => object.Equals(key, keySelector(e))));
        }

    }
}
