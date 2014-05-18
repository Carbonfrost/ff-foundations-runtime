//
// - ConcreteClassProviderAttribute.cs -
//
// Copyright 2012, 2014 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    public sealed class ConcreteClassProviderAttribute : ConcreteClassProviderAttributeBase {

        private readonly LateBound<IConcreteClassProvider> concreteClassProvider;

        public Type ConcreteClassProviderType {
            get {
                return concreteClassProvider.TypeReference.Resolve();
            }
        }

        public IConcreteClassProvider Value {
            get {
                return this.concreteClassProvider.Value;
            }
        }

        public ConcreteClassProviderAttribute(Type concreteClassProviderType) {
            if (concreteClassProviderType == null)
                throw new ArgumentNullException("concreteClassProviderType");

            if (!typeof(IConcreteClassProvider).IsAssignableFrom(concreteClassProviderType))
                throw Failure.NotAssignableFrom("concreteClassProvider", concreteClassProviderType, typeof(IConcreteClassProvider));

            this.concreteClassProvider = new LateBound<IConcreteClassProvider>(TypeReference.FromType(concreteClassProviderType), ServiceProvider.Current);
        }

        protected override Type GetConcreteClassCore(Type sourceType, IServiceProvider serviceProvider) {
            return this.Value.GetConcreteClass(sourceType, serviceProvider);
        }
    }

}

