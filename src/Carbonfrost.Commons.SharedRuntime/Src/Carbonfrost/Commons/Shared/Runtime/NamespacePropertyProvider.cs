//
// - NamespacePropertyProvider.cs -
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

    sealed class NamespacePropertyProvider : PropertyProviderBase {

        private readonly IDictionary<string, CompositePropertyProvider> items
            = new Dictionary<string, CompositePropertyProvider>();

        public NamespacePropertyProvider(
            IEnumerable<KeyValuePair<string, IPropertyProvider>> elements) {

            foreach (var grouping in elements.GroupBy(l => l.Key, l => l.Value)) {
                items.Add(grouping.Key, new CompositePropertyProvider(grouping));
            }
        }

        protected override bool TryGetPropertyCore(string property, Type propertyType, out object value) {
            if (property == null)
                throw new ArgumentNullException("property");
            if (property.Length == 0)
                throw Failure.EmptyString("property");
            string internalKey;
            var result = GetCandidates(property, out internalKey);

            value = null;
            if (result == null)
                return false;
            else
                return result.TryGetProperty(internalKey, propertyType, out value);
        }

        protected override ICustomAttributeProvider GetPropertyAttributeProviderCore(string property) {
            if (property == null)
                throw new ArgumentNullException("property");
            if (property.Length == 0)
                throw Failure.EmptyString("property");
            string internalKey;
            var result = GetCandidates(property, out internalKey);

            if (result == null)
                return CustomAttributeProvider.Null;
            else
                return result.GetPropertyAttributeProvider(internalKey);
        }

        CompositePropertyProvider GetCandidates(string key, out string internalKey) {
            // Use the prefix and index method to pick the category
            string[] items = Utility.SplitInTwo(key, ':');
            string prefix = string.Empty;
            internalKey = string.Empty;

            if (items.Length == 1) {
                prefix = items[0];
            } else {
                internalKey = items[1];
            }

            // Look up the composite provider using the key
            CompositePropertyProvider pp;
            if (this.items.TryGetValue(prefix, out pp))
                return pp;
            else
                return null;
        }

    }
}
