//
// - PropertyProviderBase.cs -
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

    abstract class PropertyProviderBase : IPropertyProvider {

        protected abstract ICustomAttributeProvider GetPropertyAttributeProviderCore(string property);
        protected abstract bool TryGetPropertyCore(string property, Type propertyType, out object value);

        // IPropertyProvider implementation.
        public Type GetPropertyType(string property) {
            Require.NotNullOrEmptyString("property", property); // $NON-NLS-1

            object objValue;
            if (TryGetProperty(property, typeof(object), out objValue)) {
                if (objValue == null)
                    return typeof(object);
                else
                    return objValue.GetType();
            } else
                throw RuntimeFailure.PropertyNotFound("property", property); // $NON-NLS-1
        }

        public bool TryGetProperty(string property, Type propertyType, out object value) {
            Require.NotNullOrEmptyString("property", property); // $NON-NLS-1, $NON-NLS-2
            object objValue;

            bool result = TryGetPropertyCore(property, propertyType, out objValue);
            value = objValue;
            return result;
        }

        public ICustomAttributeProvider GetPropertyAttributeProvider(string property) {
            if (this.HasProperty(property))
                return GetPropertyAttributeProviderCore(property);
            else
                throw RuntimeFailure.PropertyNotFound("property", property);
        }

    }
}
