//
// - ReflectOnlyAssemblyAttributeProvider.cs -
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
using Carbonfrost.Commons.ComponentModel.Annotations;
using Carbonfrost.Commons.Shared;

namespace Carbonfrost.Commons.Shared {

    class ReflectOnlyAssemblyAttributeProvider : ICustomAttributeProvider {

        private readonly Assembly assembly;

        private static readonly Assembly THIS_ASSEMBLY = typeof(ReflectOnlyAssemblyAttributeProvider).Assembly;

        public ReflectOnlyAssemblyAttributeProvider(Assembly assembly) {
            this.assembly = assembly;
        }

        public IEnumerable<object> GetCustomAttributes() {
            foreach (var ca in assembly.GetCustomAttributesData()) {
                var decl = ca.Constructor.DeclaringType;
                if (decl.Assembly == THIS_ASSEMBLY) {

                    switch (decl.Name) {
                        case "ActivationProviderAttribute":
                        case "AdapterAttribute":
                        case "AdapterFactoryAttribute":
                            break;

                        case "BaseAttribute":
                            yield return new BaseAttribute((string) ca.ConstructorArguments[0].Value);
                            break;

                        case "DevelopmentAttribute":
                            yield return new DevelopmentAttribute((string) ca.ConstructorArguments[0].Value);
                            break;

                        case "DocumentationAttribute":
                            yield return new DocumentationAttribute((string) ca.ConstructorArguments[0].Value);
                            break;

                        case "ExceptionCatalogAttribute":
                            yield return new ExceptionCatalogAttribute((string) ca.ConstructorArguments[0].Value);
                            break;

                        case "GeneratorAttribute":
                            yield return new GeneratorAttribute((string) ca.ConstructorArguments[0].Value);
                            break;

                        case "LicenseAttribute":
                            yield return new LicenseAttribute((string) ca.ConstructorArguments[0].Value);
                            break;

                        case "PrivateKeyAttribute":
                            yield return new PrivateKeyAttribute((string) ca.ConstructorArguments[0].Value);
                            break;

                        case "RelationshipAttribute":
                            break;

                        case "SelfAttribute":
                            yield return new SelfAttribute((string) ca.ConstructorArguments[0].Value);
                            break;

                        case "SharedRuntimeOptionsAttribute":
                            break;

                        case "SourceAttachmentAttribute":
                            yield return new SourceAttachmentAttribute((string) ca.ConstructorArguments[0].Value);
                            break;

                        case "StandardsCompliantAttribute":
                            yield return new StandardsCompliantAttribute((string) ca.ConstructorArguments[0].Value);
                            break;

                        case "TranslationsAttribute":
                            yield return new TranslationsAttribute((string) ca.ConstructorArguments[0].Value);
                            break;

                        case "UrlAttribute":
                            yield return new UrlAttribute((string) ca.ConstructorArguments[0].Value);
                            break;

                        case "XmlnsAttribute":
                            yield return new XmlnsAttribute((string) ca.ConstructorArguments[0].Value);
                            break;
                    }
                }
            }
        }

        public object[] GetCustomAttributes(bool inherit) {
            return GetCustomAttributes().ToArray();
        }

        public object[] GetCustomAttributes(Type attributeType, bool inherit) {
            return GetCustomAttributes().Where(attributeType.IsInstanceOfType).ToArray();
        }

        public bool IsDefined(Type attributeType, bool inherit) {
            return GetCustomAttributes().Any(attributeType.IsInstanceOfType);
        }
    }
}

