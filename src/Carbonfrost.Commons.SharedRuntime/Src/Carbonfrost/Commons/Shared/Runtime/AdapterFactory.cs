//
// - AdapterFactory.cs -
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
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Shared.Runtime {

    public abstract partial class AdapterFactory : IAdapterFactory {

        private static readonly IDictionary<Assembly, IAdapterFactory> cache
            = new Dictionary<Assembly, IAdapterFactory>();

        public static readonly IAdapterFactory Default = new DefaultAdapterFactoryImpl();

        protected AdapterFactory() {}

        public virtual object GetAdapter(object adaptee, string adapterRoleName) {
            if (adaptee == null)
                throw new ArgumentNullException("adaptee");
            if (adapterRoleName == null)
                throw new ArgumentNullException("adapterRoleName");
            if (adapterRoleName.Length == 0)
                throw Failure.EmptyString("adapterRoleName");

            var type = GetAdapterType(adaptee, adapterRoleName);
            if (type == null)
                return null;

            var props = Properties.FromArray(adaptee);
            return Activation.CreateInstance(type, props, null, ServiceProvider.Current);
        }

        public virtual Type GetAdapterType(object adaptee, string adapterRoleName) {
            if (adaptee == null)
                throw new ArgumentNullException("adaptee");
            if (adapterRoleName == null)
                throw new ArgumentNullException("adapterRoleName");
            if (adapterRoleName.Length == 0)
                throw Failure.EmptyString("adapterRoleName");

            return GetAdapterType(adaptee.GetType(), adapterRoleName);
        }

        public abstract Type GetAdapterType(Type adapteeType, string adapterRoleName);

        public static IAdapterFactory Compose(params IAdapterFactory[] items) {
            if (items == null)
                throw new ArgumentNullException("items");

            if (items.Length == 0)
                return Null;

            else if (items.Length == 1)
                return items[1];

            else
                return new CompositeAdapterFactoryImpl((IAdapterFactory[]) items.Clone());
        }

        public static IAdapterFactory Compose(IEnumerable<IAdapterFactory> items) {
            if (items == null)
                throw new ArgumentNullException("items");

            var all = items.ToArray();
            if (all.Length == 0)
                return Null;

            else if (all.Length == 1)
                return all[1];

            else
                return new CompositeAdapterFactoryImpl(all);
        }

        public static IAdapterFactory FromAssembly(Assembly assembly) {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            return cache.GetValueOrCache(assembly, () => FromAssemblyInternal(assembly));
        }

        static IAdapterFactory FromAssemblyInternal(Assembly assembly) {
            // This will contain all factories that are declared.  If any factory has an explicit role, then
            // all adapters in that role must be made available from it (otherwise, the optimization is pointless since we
            // have to fall back to a full scan).

            var all = (AdapterFactoryAttribute[]) Attribute.GetCustomAttributes(assembly, typeof(AdapterFactoryAttribute));
            var except = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var genericFactories = new List<IAdapterFactory>();  // [assembly: AdapterFactory(typeof(H))]
            var roleFactories = new List<IAdapterFactory>();  // [assembly: StreamingSourceFactory(typeof(H))]

            if (all.Length == 0)
                return ReflectedAdapterFactory.Create(assembly, Empty<string>.Array);

            foreach (var t in all) {
                var inst = (IAdapterFactory) Activator.CreateInstance(t.AdapterFactoryType);
                if (t.Role == null) {
                    genericFactories.Add(inst);
                } else {
                    except.Add(t.Role); // Don't consider role because it has a factory
                    roleFactories.Add(inst);
                }
            }

            // If no generic factories were defined, then fallback available
            if (genericFactories.Count == 0)
                genericFactories.Add(ReflectedAdapterFactory.Create(assembly, except));

            // Consider role factories before generic ones
            return Compose(roleFactories.Concat(genericFactories));
        }

    }
}
