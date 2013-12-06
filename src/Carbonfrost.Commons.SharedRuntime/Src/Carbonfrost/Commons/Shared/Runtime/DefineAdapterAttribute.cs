//
// - DefineAdapterAttribute.cs -
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

namespace Carbonfrost.Commons.Shared.Runtime {

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class DefineAdapterAttribute : Attribute {

        static IDictionary<string, DefineAdapterAttribute[]> cache;

        private readonly Type adapteeType;
        private readonly Type adapterType;
        private readonly string role;

        public Type AdapteeType {
            get { return this.adapteeType; } }

        public Type AdapterType {
            get { return this.adapterType; } }

        public string Role {
            get { return this.role; } }

        public DefineAdapterAttribute(
            string role, Type adapteeType, Type adapterType) {
            if (role == null)
                throw new ArgumentNullException("role");
            if (role.Length == 0)
                throw Failure.EmptyString("role");
            if (adapteeType == null)
                throw new ArgumentNullException("adapteeType");
            if (adapterType == null)
                throw new ArgumentNullException("adapterType");

            this.role = role;
            this.adapteeType = adapteeType;
            this.adapterType = adapterType;
        }

        public DefineAdapterAttribute(
            string role, Type adapteeType, string adapterType) {
            if (role == null)
                throw new ArgumentNullException("role");
            if (role.Length == 0)
                throw Failure.EmptyString("role");
            if (adapteeType == null)
                throw new ArgumentNullException("adapteeType");
            if (adapterType == null)
                throw new ArgumentNullException("adapterType");
            if (adapterType.Length == 0)
                throw Failure.EmptyString("adapterType");

            this.role = role;
            this.adapteeType = adapteeType;
            this.adapterType = Type.GetType(adapterType);
        }

        internal static IEnumerable<Type> GetAdapterTypes(
            Type adapteeType, string adapterRoleName, bool inherit)
        {
            EnsureCache();

            Func<DefineAdapterAttribute, bool> predicate;
            if (inherit) {
                predicate = t => (t.AdapteeType.IsAssignableFrom(adapteeType));
            } else {
                predicate = t => (adapteeType == t.AdapteeType);
            }

            return cache.GetValueOrDefault(adapterRoleName, Empty<DefineAdapterAttribute>.Array)
                .Where(predicate)
                .Select(t => t.AdapterType);
        }

        static void EnsureCache() {
            if (cache == null) {
                var allItems = AppDomain.CurrentDomain.DescribeAssemblies(
                    t => (DefineAdapterAttribute[]) t.GetCustomAttributes(typeof(DefineAdapterAttribute), false));

                cache = allItems.GroupBy(t => t.Role).ToDictionary(t => t.Key, t => t.ToArray());
            }
        }
    }
}
