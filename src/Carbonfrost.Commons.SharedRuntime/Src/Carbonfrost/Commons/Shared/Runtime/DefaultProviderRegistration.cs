//
// - DefaultProviderRegistration.cs -
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
using System.Reflection;

namespace Carbonfrost.Commons.Shared.Runtime {

    sealed class DefaultProviderRegistration : ProviderRegistration {

        public override void RegisterProviderTypes(ProviderRegistrationContext context) {
            foreach (Type type in context.Assembly.GetTypesHelper()) {

                if (type.IsDefined(typeof(ProvidersAttribute), false)) {
                    ExtractFromFields(context, type);
                    ExtractFromMethods(context, type);
                }

                ExtractFromType(context, type);
            }

        }

        public override void RegisterRootProviderTypes(ProviderRegistrationContext context) {
            var attrs = (ProvidesAttribute[]) context.Assembly.GetCustomAttributes(typeof(ProvidesAttribute), false);

            foreach (var attr in attrs)
                context.DefineRootProvider(attr.ProviderType);
        }

        private void ExtractFromMethods(ProviderRegistrationContext context, Type type) {
            foreach (MethodInfo method in type.GetMethods()) {
                if (!method.IsStatic)
                    continue;

                ProviderAttribute[] attrs = (ProviderAttribute[]) method.GetCustomAttributes(typeof(ProviderAttribute), false);
                foreach (var pa in attrs) {
                    foreach (var qn in pa.GetNames(type)) {
                        context.DefineProvider(qn, pa.ProviderType, method, pa);
                    }
                }
            }
        }

        private void ExtractFromFields(ProviderRegistrationContext context, Type type) {
            foreach (FieldInfo field in type.GetFields()) {
                if (!field.IsStatic)
                    continue;

                ProviderAttribute[] attrs = (ProviderAttribute[]) field.GetCustomAttributes(typeof(ProviderAttribute), false);
                if (attrs.Length == 0) {
                    context.DefineProvider(null, field.FieldType, field, null);

                } else {
                    foreach (var pa in attrs) {
                        foreach (var qn in pa.GetNames(field.DeclaringType, field.Name)) {
                            try {
                                context.DefineProvider(qn, pa.ProviderType, field, pa);

                            } catch (Exception ex) {
                                var fullName = field.DeclaringType.FullName + "." + field.Name;
                                RuntimeWarning.InvalidProviderDeclared(fullName, ex);
                            }
                        }
                    }
                }
            }
        }

        private void ExtractFromType(ProviderRegistrationContext context, Type type) {
            if (type.IsPublic) {
                ProviderAttribute[] attrs = (ProviderAttribute[]) type.GetCustomAttributes(typeof(ProviderAttribute), false);

                if (attrs.Length > 0) {
                    foreach (var pa in attrs) {
                        foreach (var qn in pa.GetNames(type)) {
                            try {
                                context.DefineProvider(qn, pa.ProviderType, type, pa);

                            } catch (Exception ex) {
                                RuntimeWarning.InvalidProviderDeclared(type.FullName, ex);
                            }
                        }
                    }
                }
            }
        }

    }
}
