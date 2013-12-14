//
// - CustomAttributeProvider.cs -
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

    public class CustomAttributeProvider : ICustomAttributeProvider {

        private readonly IEnumerable<Attribute> attributeValues;
        private IEnumerable<object> attributeValuesCache;
        private static readonly ICustomAttributeProvider _null = new NullCustomAttributeProvider();

        protected IEnumerable<Attribute> AttributeValues { get { return attributeValues; } }
        public ICustomAttributeProvider InheritanceContext { get; set; }
        public static ICustomAttributeProvider Null { get { return _null; } }

        public CustomAttributeProvider(IEnumerable<Attribute> attributeValues) {
            if (attributeValues == null)
                throw new ArgumentNullException("attributeValues"); // $NON-NLS-1
            this.attributeValues = attributeValues;
        }

        internal static TAttribute GetCustomAttribute<TAttribute>(ICustomAttributeProvider pro, bool inherit) {
            var attrs = pro.GetCustomAttributes(typeof(TAttribute), inherit);
			return (TAttribute) attrs.FirstOrDefault();
        }

        // `ICustomAttributeProvider' implementation.

        public virtual object[] GetCustomAttributes(bool inherit) {
            return GetCustomAttributesCore(typeof(object), inherit).ToArray();
        }

        public virtual object[] GetCustomAttributes(Type attributeType, bool inherit) {
            if (attributeType == null)
                throw new ArgumentNullException("attributeType"); // $NON-NLS-1

            return GetCustomAttributesCore(attributeType, inherit).ToArray();
        }

        public virtual bool IsDefined(Type attributeType, bool inherit) {
            if (attributeType == null)
                throw new ArgumentNullException("attributeType"); // $NON-NLS-1

            Func<Attribute, bool> predicate = (o) => (attributeType.IsAssignableFrom(o.GetType()));
            return this.AttributeValues.Any<Attribute>(predicate)
                || (inherit && this.InheritanceContext != null && this.InheritanceContext.IsDefined(attributeType, true));
        }

        private IEnumerable<object> GetCustomAttributesCore(Type attributeType, bool inherit) {
            if (attributeValuesCache == null) {
                attributeValuesCache = Enumerable.Cast<object>(this.attributeValues);
            }

            IEnumerable<object> source =
                (this.InheritanceContext == null || !inherit) ? attributeValuesCache
                : Enumerable.Concat(attributeValuesCache,
                                    this.InheritanceContext.GetCustomAttributes(true));

            if (attributeType.Equals(typeof(object)))
                return source;
            else {
                Func<object, bool> predicate = (o) => (attributeType.IsAssignableFrom(o.GetType()));
                return source.Where(predicate).ToArray();
            }
        }

        // Nested types.

        private sealed class NullCustomAttributeProvider : ICustomAttributeProvider {

            object[] ICustomAttributeProvider.GetCustomAttributes(bool inherit) {
                return new object[0];
            }

            object[] ICustomAttributeProvider.GetCustomAttributes(Type attributeType, bool inherit) {
                return new object[0];
            }

            bool ICustomAttributeProvider.IsDefined(Type attributeType, bool inherit) {
                return false;
            }

        }
    }
}
