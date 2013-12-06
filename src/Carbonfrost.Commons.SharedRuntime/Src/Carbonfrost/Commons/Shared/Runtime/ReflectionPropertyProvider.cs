//
// - ReflectionPropertyProvider.cs -
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
using System.ComponentModel;

namespace Carbonfrost.Commons.Shared.Runtime {

    internal class ReflectionPropertyProvider : IPropertyProvider {

        protected readonly object objectContext;
        private PropertyDescriptorCollection properties;

        public ReflectionPropertyProvider(object component) {
            if (component == null)
                throw new ArgumentNullException("component");  // $NON-NLS-1

            this.objectContext = component;
        }

        public override string ToString() {
            return this.objectContext.ToString();
        }

        public Type GetPropertyType(string property) {
            PropertyDescriptor descriptor = _GetProperty(property);
            if (descriptor == null)
                return null;

            else return descriptor.PropertyType;
        }

        public bool TryGetProperty(string property, Type propertyType, out object value) {
            PropertyDescriptor pd = _GetProperty(property);
            if (pd == null) {
                value = null;
                return false;
            }

            value = pd.GetValue(objectContext);
            return propertyType.IsInstanceOfType(value);
        }

        private PropertyDescriptor _GetProperty(string property) {
            return _EnsureProperties().Find(property, false);
        }

        protected PropertyDescriptorCollection _EnsureProperties() {
            if (properties == null) {
                properties = TypeDescriptor.GetProperties(this.objectContext);
            }
            return properties;
        }

        private void _ResetProperty(PropertyDescriptor pd) {
            if (pd.CanResetValue(this.objectContext)) {
                pd.ResetValue(this.objectContext);
            }
        }

    }
}
