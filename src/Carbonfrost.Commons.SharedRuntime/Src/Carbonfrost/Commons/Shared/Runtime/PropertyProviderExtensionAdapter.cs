//
// - PropertyProviderExtensionAdapter.cs -
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
using System.Reflection;

namespace Carbonfrost.Commons.Shared.Runtime {

    sealed class PropertyProviderExtensionAdapter : IPropertyProviderExtension {

        private readonly IPropertyProvider source;

        public PropertyProviderExtensionAdapter(IPropertyProvider source) {
            this.source = source;
        }

        public ICustomAttributeProvider GetPropertyAttributeProvider(string key) {
            return CustomAttributeProvider.Null;
        }

        public Type GetPropertyType(string key) {
            return source.GetPropertyType(key);
        }

        public bool TryGetProperty(string key, Type propertyType, out object value) {
            return source.TryGetProperty(key, propertyType, out value);
        }
    }
}
