//
// - AdapterFactoryUsageAttribute.cs -
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

namespace Carbonfrost.Commons.Shared.Runtime  {

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class AdapterFactoryUsageAttribute : ProviderAttribute {

        private string[] rolesCache;

        public string Roles { get; set; }

        public AdapterFactoryUsageAttribute() : base(typeof(IAdapterFactory)) {
        }

        public IEnumerable<string> EnumerateRoles() {
            return Utility.SplitText(ref this.rolesCache, this.Roles);
        }

        protected override int MatchCriteriaCore(object criteria) {
            if (criteria == null)
                return 0;

            var pp = PropertyProvider.FromValue(criteria);
            return EnumerateRoles().Contains(pp.GetString("Role"), StringComparer.OrdinalIgnoreCase) ? 1 : 0;
        }
    }
}

