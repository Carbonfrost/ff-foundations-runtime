//
// - ProviderRegistrationContext.cs -
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

    public class ProviderRegistrationContext {

        readonly List<ProviderValueSource> result = new List<ProviderValueSource>();
        readonly List<Type> roots = new List<Type>();

        public Assembly Assembly {
            get; private set;
        }

        internal ProviderRegistrationContext(Assembly assembly) {
            this.Assembly = assembly;
        }

        // TODO Validate members - should be static, public,
        // other constraints

        // TODO Validate root providers

        // TODO Custom client implementations could register providers and root providers out of order (uncommon)

        internal IEnumerable<Type> EnumerateRoots() {
            return roots;
        }

        public void DefineRootProvider(Type providerType) {
            if (providerType == null)
                throw new ArgumentNullException("providerType");

            roots.Add(providerType);
        }

        public void DefineProvider(QualifiedName name,
                                   Type providerType,
                                   Type providerInstanceType,
                                   object metadata = null)
        {
            if (providerType == null)
                throw new ArgumentNullException("providerType");
            if (providerInstanceType == null)
                throw new ArgumentNullException("providerInstanceType");

            var qn = GetName(name, providerInstanceType, providerInstanceType.Name);

            var tr = new ProviderType(providerInstanceType, providerType, qn);
            tr.Metadata = ProviderMetadataWrapper.Create(metadata);
            result.Add(tr);
        }

        public void DefineProvider(QualifiedName name,
                                   Type providerType,
                                   FieldInfo field,
                                   object metadata = null)
        {
            if (providerType == null)
                throw new ArgumentNullException("providerType");
            if (field == null)
                throw new ArgumentNullException("field");

            var qn = GetName(name, field.DeclaringType, field.Name);

            var fieldResult = new ProviderField(field, providerType, qn);
            fieldResult.Metadata = ProviderMetadataWrapper.Create(metadata);
            result.Add(fieldResult);
        }

        public void DefineProvider(QualifiedName name,
                                   Type providerType,
                                   MethodInfo factoryMethod,
                                   object metadata = null)
        {

            if (providerType == null)
                throw new ArgumentNullException("providerType");
            if (factoryMethod == null)
                throw new ArgumentNullException("factoryMethod");

            var qn = GetName(name, factoryMethod.DeclaringType, factoryMethod.Name);
            var methodResult = new ProviderMethod(factoryMethod,
                                                  providerType,
                                                  name);
            methodResult.Metadata = ProviderMetadataWrapper.Create(metadata);
            result.Add(methodResult);
        }

        static QualifiedName GetName(QualifiedName userName,
                                     Type declaringType,
                                     string memberName) {
            return userName
                ?? Adaptable.GetQualifiedName(declaringType)
                .ChangeLocalName(Utility.Camel(memberName));
        }

        internal IEnumerable<ProviderValueSource> EnumerateValueSources() {
            return result;
        }

        sealed class ProviderField : ProviderValueSource {

            private readonly FieldInfo field;

            public ProviderField(FieldInfo field,
                                 Type providerType,
                                 QualifiedName key)
                : base(providerType, key) {
                this.field = field;
            }

            public override MemberInfo Member {
                get {
                    return this.field;
                }
            }

            public override Type ValueType {
                get {
                    return GetValue().GetType();
                }
            }

            public override object GetValue() {
                return field.GetValue(null);
            }

            public override bool IsValue(object instance) {
                return field.IsInitOnly && object.ReferenceEquals(instance, GetValue());
            }

            public override object Activate(IEnumerable<KeyValuePair<string, object>> arguments,
                                            IPopulateComponentCallback callback,
                                            IServiceProvider services) {
                object result = GetValue();
                Activation.Initialize(result, arguments, callback, services);
                return result;
            }
        }

        sealed class ProviderMethod : ProviderValueSource {

            private readonly MethodInfo method;
            private ActivationHelper helper;

            public ProviderMethod(MethodInfo method,
                                  Type providerType,
                                  QualifiedName key)
                : base(providerType, key) {
                this.method = method;
            }

            public override MemberInfo Member {
                get {
                    return this.method;
                }
            }

            public override Type ValueType {
                get {
                    return method.ReturnType;
                }
            }

            public override bool IsValue(object instance) {
                // Non-singletons
                return false;
            }

            public override object GetValue() {
                return Activate(null, null, null);
            }

            public override object Activate(IEnumerable<KeyValuePair<string, object>> arguments,
                                            IPopulateComponentCallback callback,
                                            IServiceProvider services) {
                if (helper == null)
                    helper = new ActivationHelper(method);

                var instance = helper.CreateInstance(method.ReturnType, arguments, callback, services);
                return instance;
            }

            sealed class ActivationHelper : DefaultActivationFactory {

                // We have a method, so we can't use pure Activation
                private readonly MethodInfo method;

                public ActivationHelper(MethodInfo method) {
                    this.method = method;
                }

                protected override MethodBase GetActivationConstructor(Type type) {
                    return method;
                }
            }
        }

        sealed class ProviderType : ProviderValueSource {

            private readonly Type type;

            public ProviderType(Type type,
                                Type providerType,
                                QualifiedName key)
                : base(providerType, key) {
                this.type = type;
            }

            public override Type ValueType {
                get {
                    return type;
                }
            }

            public override MemberInfo Member {
                get {
                    return this.type;
                }
            }

            public override bool IsValue(object instance) {
                return ValueType == instance.GetType();
            }

            public override object GetValue() {
                return Activation.CreateInstance(type);
            }

            public override object Activate(IEnumerable<KeyValuePair<string, object>> arguments,
                                            IPopulateComponentCallback callback,
                                            IServiceProvider services) {
                return Activation.CreateInstance(type, arguments, callback, services);
            }
        }

    }
}
