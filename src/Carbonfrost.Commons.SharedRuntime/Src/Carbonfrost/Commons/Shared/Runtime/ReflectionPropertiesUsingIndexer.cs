//
// - ReflectionPropertiesUsingIndexer.cs -
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
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Shared.Runtime {

    internal sealed class ReflectionPropertiesUsingIndexer : PropertiesImpl {

        private readonly object objectContext;
        private readonly PropertyInfo indexer;

        public ReflectionPropertiesUsingIndexer(object objectContext, PropertyInfo indexer) : base(objectContext) {
            this.objectContext = objectContext;
            this.indexer = indexer;
        }

        // PropertiesImpl implementation
        protected override void SetPropertyCore(string key, object defaultValue, bool isImplicit) {
            this.indexer.SetValue(objectContext, defaultValue, new object[] { key });
        }

        protected override bool TryGetPropertyCore(string key, Type requiredType, out object value) {
            value = this.indexer.GetValue(objectContext, new object[] { key });
            return true;
        }

        public override IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
            return Enumerable.Empty<KeyValuePair<string, object>>().GetEnumerator();
        }

        public override void ClearProperties() {
        }
    }
}
