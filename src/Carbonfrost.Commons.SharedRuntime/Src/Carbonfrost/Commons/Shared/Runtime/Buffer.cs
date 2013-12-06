//
// - Buffer.cs -
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Carbonfrost.Commons.Shared.Runtime {

    class Buffer<TElement> : IEnumerable<TElement> {

        private IEnumerator<TElement> source;
        private readonly List<TElement> cache;

        private bool MoveNext() {
            if (source == null)
                return false;

            if (source.MoveNext()) {
                this.cache.Add(source.Current);
                OnCacheValue(source.Current);
                return true;
            }

            this.source = null;
            this.cache.TrimExcess();
            OnCacheComplete();
            return false;
        }

        protected virtual void OnCacheComplete() {}
        protected virtual void OnCacheValue(TElement current) {}

        public Buffer(IEnumerable<TElement> e) {
            this.source = e.GetEnumerator();
            this.cache = new List<TElement>();
        }

        public IEnumerator<TElement> GetEnumerator() {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        struct Enumerator : IEnumerator<TElement> {

            private readonly Buffer<TElement> source;
            private int index;

            public Enumerator(Buffer<TElement> source) {
                this.source = source;
                this.index = -1;
            }

            public TElement Current {
                get {
                    if (index < 0 || index >= source.cache.Count)
                        throw Failure.OutsideEnumeration();

                    return source.cache[index];
                }
            }

            object IEnumerator.Current {
                get { return Current; } }

            public void Dispose() {}

            public bool MoveNext() {
                this.index++;

                if (source.cache.Count == this.index) {
                    return source.MoveNext();
                }

                return true;
            }

            public void Reset() {}

        }
    }
}
