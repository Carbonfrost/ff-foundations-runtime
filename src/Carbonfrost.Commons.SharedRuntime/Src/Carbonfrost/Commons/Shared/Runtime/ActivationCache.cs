//
// - ActivationCache.cs -
//
// Copyright 2012 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using Carbonfrost.Commons.ComponentModel;

namespace Carbonfrost.Commons.Shared.Runtime {

    public class ActivationCache<TBase> {

        private readonly IDictionary<Type, TBase> items = new Dictionary<Type, TBase>();
        private readonly IActivationFactory activationFactory;
        private readonly IServiceProvider serviceProvider;

        public ActivationCache()
            : this(null, null) {}

        public ActivationCache(
            IActivationFactory activationFactory,
            IServiceProvider serviceProvider) {

            this.activationFactory = activationFactory ?? ActivationFactory.Build;
            this.serviceProvider = serviceProvider ?? ServiceProvider.Root;
        }

        public T Get<T>() where T : TBase {
            return (T) Get(type: typeof(T));
        }

        public T Get<T>(Type type,
                        IEnumerable<KeyValuePair<string, object>> values = null,
                        IPopulateComponentCallback callback = null,
                        params Attribute[] attributes)
            where T : TBase
        {
            return (T) this.activationFactory.CreateInstance(type, values, callback, serviceProvider, attributes);
        }

        public TBase Get(Type type,
                         IEnumerable<KeyValuePair<string, object>> values = null,
                         IPopulateComponentCallback callback = null,
                         params Attribute[] attributes)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            callback = callback ?? PopulateComponentCallback.Default;

            TBase result;
            if (!this.items.TryGetValue(type, out result)) {
                result = this.activationFactory.CreateInstance<TBase>(
                    type, values, callback, this.serviceProvider, attributes);

                this.items[type] = result;
            }

            return result;
        }

    }

}
