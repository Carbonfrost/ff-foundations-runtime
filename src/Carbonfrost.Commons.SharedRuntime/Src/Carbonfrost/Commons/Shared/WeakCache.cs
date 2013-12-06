//
// - WeakCache.cs -
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

namespace Carbonfrost.Commons.Shared {

    sealed class WeakCache<T> {

        readonly IDictionary<Type, WeakReference> map = new Dictionary<Type, WeakReference>();
        readonly Func<Type, T> factory;

        public WeakCache(Func<Type, T> factory) {
            this.factory = factory;
        }

        public T Get(Type type) {
            WeakReference wr;
            if (map.TryGetValue(type, out wr) && wr.IsAlive) {
                return (T) wr.Target;
            } else {
                T t = factory(type);
                map[type] = new WeakReference(t);
                return t;
            }
        }

        public IEnumerable<T> GetAll(IEnumerable<Type> types) {
            foreach (var type in types)
                yield return Get(type);
        }
    }
}
