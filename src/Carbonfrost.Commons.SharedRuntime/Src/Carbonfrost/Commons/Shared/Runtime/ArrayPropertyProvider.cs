//
// - ArrayPropertyProvider.cs -
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

    sealed class ArrayPropertyProvider : IPropertyProvider {

        private readonly object[] items;

        public ArrayPropertyProvider(object[] items) {
            this.items = items;
        }

        public Type GetPropertyType(string property) {
            object result;
            if (TryGetProperty(property, typeof(object), out result))
                return (result == null) ? items.GetType().GetElementType() : result.GetType();
            else
                return null;
        }

        public bool TryGetProperty(string property, Type propertyType, out object value) {
            value = null;
            int index;
            if (!int.TryParse(property, out index) || index < 0 || index >= items.Length)
                return false;

            value = items[index];
            return propertyType.IsInstanceOfType(value);
        }

    }
}
