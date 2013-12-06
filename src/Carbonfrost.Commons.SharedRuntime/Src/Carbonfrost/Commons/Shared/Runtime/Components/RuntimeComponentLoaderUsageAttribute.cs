//
// - RuntimeComponentLoaderUsageAttribute.cs -
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

namespace Carbonfrost.Commons.Shared.Runtime.Components {

    [AttributeUsage(AttributeTargets.Class
                    | AttributeTargets.Field
                    | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class RuntimeComponentLoaderUsageAttribute
        : ProviderAttribute {

        private string[] cache;

        public string ComponentTypes { get; set; }

        public IEnumerable<string> EnumerateComponentTypes() {
            return Utility.SplitText(ref this.cache, this.ComponentTypes, new [] { '\t', '\r', '\n', });
        }

        public RuntimeComponentLoaderUsageAttribute()
            : base(typeof(RuntimeComponentLoader)) {}

        public override int MatchCriteria(object criteria) {
            if (criteria == null)
                return 0;

            int result = 0;
            var pp = PropertyProvider.FromValue(criteria);
            result += EnumerateComponentTypes().Contains(pp.GetString("ComponentType")) ? 1 : 0;

            return result;
        }
    }
}
