//
// - ReflectedAdapterFactory.cs -
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
using System.Reflection;

namespace Carbonfrost.Commons.Shared.Runtime {

    sealed class ReflectedAdapterFactory : AdapterFactory {

        private static readonly IDictionary<Assembly, ReflectedAdapterFactory> items
            = new Dictionary<Assembly, ReflectedAdapterFactory>();

        private readonly Assembly assembly;
        private IDictionary<string, IDictionary<Type, Type>> adapters; // role => (adaptee => adapter)
        private readonly ICollection<string> except;

        private ReflectedAdapterFactory(Assembly assembly, ICollection<string> except) {
            this.assembly = assembly;
            this.except = except;
        }

        public static ReflectedAdapterFactory Create(Assembly assembly, ICollection<string> except) {
            return items.GetValueOrCache(assembly,
                                         () => new ReflectedAdapterFactory(assembly, except));
        }

        private void InitAdapters() {
            if (this.adapters == null) {
                adapters = new Dictionary<string, IDictionary<Type, Type>>(StringComparer.OrdinalIgnoreCase);
                foreach (var adapteeType in assembly.GetTypesHelper()) {
                    var attrs = ((AdapterAttribute[]) adapteeType.GetCustomAttributes(typeof(AdapterAttribute), false));
                    if (attrs.Length == 0)
                        continue;

                    foreach (var attr in attrs) {
                        if (this.except.Contains(attr.Role))
                            continue;

                        var toute = adapters.GetValueOrCache(attr.Role, () => new Dictionary<Type, Type>());
                        toute[adapteeType] = attr.AdapterType;
                    }
                }
            }
        }

        public override Type GetAdapterType(Type adapteeType, string adapterRoleName) {
            if (adapteeType == null)
                throw new ArgumentNullException("adapteeType");
            if (adapterRoleName == null)
                throw new ArgumentNullException("adapterRoleName");
            if (adapterRoleName.Length == 0)
                throw Failure.EmptyString("adapterRoleName");

            if (adapteeType.Assembly != this.assembly)
                return null;

            if (this.except.Contains(adapterRoleName))
                return null;

            InitAdapters();

            IDictionary<Type, Type> result;
            if (adapters.TryGetValue(adapterRoleName, out result)) {
                return result.GetValueOrDefault(adapteeType);
            }

            return null;
        }

    }
}
