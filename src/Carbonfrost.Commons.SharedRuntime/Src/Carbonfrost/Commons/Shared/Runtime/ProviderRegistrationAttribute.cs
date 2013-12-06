//
// - ProviderRegistrationAttribute.cs -
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

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class ProviderRegistrationAttribute : Attribute {

        private readonly IProviderRegistration registration;

        internal static readonly ProviderRegistrationAttribute Default
            = new ProviderRegistrationAttribute(ProviderRegistrationType.Default);

        internal IProviderRegistration Registration {
            get {
                return this.registration;
            }
        }

        public Type Type {
            get {
                switch (KnownType) {
                    case ProviderRegistrationType.Custom:
                        return registration.GetType();

                    case ProviderRegistrationType.Default:
                    case ProviderRegistrationType.Explicit:
                    default:
                        return null;
                }
            }
        }

        public ProviderRegistrationType KnownType {
            get {
                if (registration == ProviderRegistration.Default)
                    return ProviderRegistrationType.Default;

                if (registration == ProviderRegistration.Explicit)
                    return ProviderRegistrationType.Explicit;

                else
                    return ProviderRegistrationType.Custom;
            }
        }

        public ProviderRegistrationAttribute(Type type) {
            if (type == null)
                throw new ArgumentNullException("type");
            if (!typeof(IProviderRegistration).IsAssignableFrom(type))
                throw Failure.NotAssignableFrom("type", typeof(IProviderRegistration), type);

            this.registration = (IProviderRegistration) Activator.CreateInstance(type);
        }

        public ProviderRegistrationAttribute(ProviderRegistrationType knownType) {
            if (knownType == ProviderRegistrationType.Custom)
                throw RuntimeFailure.UseProviderRegistrationAttributeOverload("knownType", knownType);

            this.registration = ProviderRegistration.FromKind(knownType);
        }

    }
}
