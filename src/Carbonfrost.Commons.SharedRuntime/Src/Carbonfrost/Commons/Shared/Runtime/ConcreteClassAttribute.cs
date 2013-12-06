//
// - ConcreteClassAttribute.cs -
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

namespace Carbonfrost.Commons.Shared.Runtime {

    [AttributeUsage(AttributeTargets.Property
                    | AttributeTargets.Interface
                    | AttributeTargets.Class,
                    AllowMultiple = false, Inherited = false)]
    public sealed class ConcreteClassAttribute : ConcreteClassProviderAttribute {

        private readonly Type type;

        public Type ConcreteType { get { return type; } }

        public ConcreteClassAttribute(string type) {
            this.type = Type.GetType(type, true, false);
        }

        public ConcreteClassAttribute(Type type) {
            this.type = type;
        }

        protected override Type GetConcreteClassCore(Type sourceType, IServiceProvider serviceProvider) {
            return ConcreteType;
        }
    }

}

