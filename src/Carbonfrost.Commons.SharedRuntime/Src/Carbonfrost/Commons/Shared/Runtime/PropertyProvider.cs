//
// - PropertyProvider.cs -
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
using System.ComponentModel;
using System.Reflection;

using Carbonfrost.Commons.Shared.Runtime;

namespace Carbonfrost.Commons.Shared.Runtime {

    public abstract partial class PropertyProvider : IPropertyProvider, IPropertyProviderExtension {

        protected PropertyProvider() {}

        protected virtual ICustomAttributeProvider GetPropertyAttributeProviderCore(string property) {
            return GetType().GetProperty(property);
        }

        protected virtual bool TryGetPropertyCore(
            string property, Type requiredType, out object value) {
            requiredType = requiredType ?? typeof(object);

            PropertyDescriptor pd = _GetProperty(property);
            if (pd == null || !requiredType.IsAssignableFrom(pd.PropertyType)) {
                value = null;
                return false;
            }

            value = pd.GetValue(this);
            return requiredType.IsInstanceOfType(value);
        }

        // IPropertyProvider implementation.
        public virtual Type GetPropertyType(string property) {
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

            if (TryGetPropertyCore(property, propertyType, out objValue)) {
                value = objValue;
                return true;
            }

            value = null;
            return false;
        }

        public virtual ICustomAttributeProvider GetPropertyAttributeProvider(string property) {
            if (this.HasProperty(property))
                return GetPropertyAttributeProviderCore(property);
            else
                throw RuntimeFailure.PropertyNotFound("property", property);
        }

        private PropertyDescriptor _GetProperty(string property) {
            return _EnsureProperties().Find(property, false);
        }

        private PropertyDescriptorCollection _EnsureProperties() {
            return TypeDescriptor.GetProperties(this);
        }

    }

}
