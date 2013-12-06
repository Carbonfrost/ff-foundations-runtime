//
// - ProviderAttribute.cs -
//
// Copyright 2012 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    [AttributeUsage(AttributeTargets.Class
                    | AttributeTargets.Struct
                    | AttributeTargets.Field
                    | AttributeTargets.Method
                    | AttributeTargets.Interface, AllowMultiple = true)]
    public class ProviderAttribute : Attribute, IProviderMetadata {

        public Type ProviderType { get; private set; }
        public string Name { get; set; }

        public ProviderAttribute(Type providerType) {
            if (providerType == null)
                throw new ArgumentNullException("providerType");

            this.ProviderType = providerType;
        }

        public virtual int MatchCriteria(object criteria) {
            return ProviderMetadataWrapper.MemberwiseEquals(this, criteria);
        }

        internal IEnumerable<QualifiedName> GetNames(Type type) {
            var qn = Adaptable.GetQualifiedName(type);
            if (string.IsNullOrEmpty(this.Name))
                return new[] { qn };

            return SelectNames(qn);
        }

        IEnumerable<QualifiedName> SelectNames(QualifiedName qn) {
            IEnumerable<string> names = this.Name.Split(
                new [] {
                    ' ', ',', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return names.Select(t => qn.ChangeLocalName(t));
        }

        internal IEnumerable<QualifiedName> GetNames(Type declaringType, string fieldOrProperty) {
            var qn = Adaptable.GetQualifiedName(declaringType);
            if (string.IsNullOrEmpty(this.Name))
                return new[] { qn.ChangeLocalName(Utility.Camel(fieldOrProperty)) };

            return SelectNames(qn);
        }
    }
}
