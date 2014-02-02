//
// - ProviderValueSource.cs -
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Carbonfrost.Commons.ComponentModel;

namespace Carbonfrost.Commons.Shared.Runtime {

    internal abstract class ProviderValueSource : IProviderInfo {

        public abstract object GetValue();
        public abstract object Activate(IEnumerable<KeyValuePair<string, object>> arguments,
                                        IPopulateComponentCallback callback,
                                        IServiceProvider services);

        public abstract Type ValueType { get; }
        public abstract MemberInfo Member { get; }

        public virtual Assembly Assembly {
            get {
                return this.Member.DeclaringType.Assembly;
            }
        }

        public Type ProviderType { get; private set; }
        public IProviderMetadata Metadata { get; set; }

        protected ProviderValueSource(Type providerType, QualifiedName key) {
            this.ProviderType = providerType;
            this.Name = key;
        }

        // Gets whether the specified instance corresponds to this (if it is
        // a singleton provider value source, not a factory one)
        public abstract bool IsValue(object instance);

        public QualifiedName Name { get; private set; }

        public Type Type { get { return this.ValueType; } }

        object IProviderInfo.Metadata {
            get {
                return this.Metadata.Value;
            }
        }
    }
}
