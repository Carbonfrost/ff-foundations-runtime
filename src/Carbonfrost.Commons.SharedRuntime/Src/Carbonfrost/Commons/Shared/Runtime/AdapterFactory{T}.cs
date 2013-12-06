//
// - AdapterFactory{T}.cs -
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
using System.Linq;

namespace Carbonfrost.Commons.Shared.Runtime {

    public class AdapterFactory<TAdapter> : IAdapterFactory {

        private readonly IAdapterFactory implementation;
        private readonly string role;

        protected IAdapterFactory Implementation {
            get { return implementation; }
        }

        public AdapterFactory(string role)
            : this(role, null)
        {
        }

        public AdapterFactory(
            string role, IAdapterFactory implementation) {

            if (role == null)
                throw new ArgumentNullException("role");
            if (role.Length == 0)
                throw Failure.EmptyString("role");

            this.role = role;
            this.implementation = implementation ?? AdapterFactory.Default;
        }

        public TAdapter Create(object adaptee) {
            return CreateCore(adaptee);
        }

        protected TAdapter CreateCore(object adaptee) {
            return (TAdapter) implementation.GetAdapter(adaptee, this.role);
        }

        protected Type GetAdapterType(object adaptee) {
            if (adaptee == null)
                throw new ArgumentNullException("adaptee");

            return implementation.GetAdapterType(adaptee, this.role);
        }

        protected Type GetAdapterType(Type adapteeType) {
            if (adapteeType == null)
                throw new ArgumentNullException("adapteeType");

            return implementation.GetAdapterType(adapteeType, this.role);
        }

        private void CheckArgs(object adaptee, string adapterRoleName) {
            if (adaptee == null)
                throw new ArgumentNullException("adaptee");
            if (adapterRoleName == null)
                throw new ArgumentNullException("adapterRoleName");
            if (adapterRoleName.Length == 0)
                throw Failure.EmptyString("adapterRoleName");
        }

        object IAdapterFactory.GetAdapter(object adaptee, string adapterRoleName) {
            CheckArgs(adaptee, adapterRoleName);

            if (CompareRoles(adapterRoleName))
                return CreateCore(adaptee);
            else
                return default(TAdapter);
        }

        Type IAdapterFactory.GetAdapterType(object adaptee, string adapterRoleName) {
            CheckArgs(adaptee, adapterRoleName);

            if (CompareRoles(adapterRoleName))
                return GetAdapterType(adaptee);
            else
                return null;
        }

        Type IAdapterFactory.GetAdapterType(Type adapteeType, string adapterRoleName) {
            if (adapteeType == null)
                throw new ArgumentNullException("adapteeType");
            if (adapterRoleName == null)
                throw new ArgumentNullException("adapterRoleName");
            if (adapterRoleName.Length == 0)
                throw Failure.EmptyString("adapterRoleName");

            if (CompareRoles(adapterRoleName))
                return GetAdapterType(adapteeType);
            else
                return null;
        }

        private bool CompareRoles(string a) {
            return string.Equals(this.role, a, StringComparison.OrdinalIgnoreCase);
        }

    }
}
