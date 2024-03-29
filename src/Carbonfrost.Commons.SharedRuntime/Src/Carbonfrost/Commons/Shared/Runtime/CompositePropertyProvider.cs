//
// - CompositePropertyProvider.cs -
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

    sealed class CompositePropertyProvider : PropertyProviderBase, IPropertyProviderExtension {

        private readonly IReadOnlyCollection<IPropertyProviderExtension> items;

        public CompositePropertyProvider(IReadOnlyCollection<IPropertyProviderExtension> items) {
            this.items = items;
        }

        protected override ICustomAttributeProvider GetPropertyAttributeProviderCore(string property) {
            Require.NotNullOrEmptyString("property", property);

            foreach (var pp in items) {
                ICustomAttributeProvider result = pp.GetPropertyAttributeProvider(property);
                if (result != null && result != CustomAttributeProvider.Null)
                    return result;
            }

            return CustomAttributeProvider.Null;
        }

        protected override bool TryGetPropertyCore(string property, Type propertyType, out object value) {
            Require.NotNullOrEmptyString("property", property);
            value = null;

            foreach (var pp in items) {
                if (pp.TryGetProperty(property, propertyType, out value))
                    return true;
            }

            return false;
        }
    }

}
