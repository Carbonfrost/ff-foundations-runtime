//
// - TemplateData.cs -
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

namespace Carbonfrost.Commons.Shared.Runtime {

    class TemplateData {

        private readonly MapBuffer<Tuple<Type, QualifiedName>, ProviderField> providers;

        // TODO Optimize by memoizing lookups on local names

        private static readonly AppDomainLocal<TemplateData> Instance
            = new AppDomainLocal<TemplateData>(() => new TemplateData());

        private TemplateData() {
            var appDomain = AppDomain.CurrentDomain;
            providers = new MapBuffer<Tuple<Type, QualifiedName>, TemplateData.ProviderField>(
                t => t.Key, appDomain.DescribeAssemblies(t => ExtractFromTypes(t)));
        }

        private IEnumerable<ProviderField> ExtractFromTypes(Assembly a) {
            if (!AssemblyInfo.GetAssemblyInfo(a).ScanForTemplates)
                yield break;

            foreach (Type type in a.GetTypesHelper()) {
                if (type.IsDefined(typeof(TemplatesAttribute), false)) {

                    foreach (FieldInfo field in type.GetFields()) {
                        if (!field.IsStatic)
                            continue;

                        if (field.FieldType.IsGenericType && typeof(ITemplate<>).Equals(field.FieldType.GetGenericTypeDefinition())) {
                            var fieldResult = new ProviderField(field);
                            yield return fieldResult;
                        }
                    }
                }
            }
        }

        internal class ProviderField {

            private readonly FieldInfo field;
            private object templateCache;

            public ProviderField(FieldInfo field) {
                this.field = field;
            }

            public Tuple<Type, QualifiedName> Key {
                get {
                    return Tuple.Create(TemplateType, QualifiedName);
                }
            }

            public QualifiedName QualifiedName {
                get {
                    return Adaptable.GetQualifiedName(field.DeclaringType).ChangeLocalName(field.Name);
                }
            }

            public string Name {
                get {
                    return field.Name;
                }
            }

            public Type TemplateType {
                get {
                    return field.FieldType.GetGenericArguments()[0];
                }
            }

            public object GetValue() {
                if (this.templateCache == null) {
                    this.templateCache = Activator.CreateInstance(
                        typeof(ReflectedTemplate<>).MakeGenericType(this.TemplateType), field.GetValue(null), QualifiedName);
                }
                return this.templateCache;
            }
        }

        class ReflectedTemplate<T> : ITemplate<T>, IAdaptable {

            private readonly ITemplate<T> template;
            private readonly QualifiedName name;

            public ReflectedTemplate(ITemplate<T> value, QualifiedName name) {
                this.template = value;
                this.name = name;
            }

            public void Initialize(T value) {
                this.template.Initialize(value);
            }

            public QualifiedName QualifiedName { get { return name; } }

            public object GetAdapter(Type targetType) {
                return Adaptable.TryAdapt(template, targetType, null);
            }
        }

        internal static object GetTemplate(AppDomain appDomain, Type templateType, QualifiedName name) {
            var s = Instance.GetValue(appDomain);
            ProviderField f = s.providers[Tuple.Create(templateType, name)];
            if (f == null)
                return null;
            else
                return f.GetValue();
        }

        internal static IEnumerable<object> GetTemplatesByLocalName(AppDomain appDomain, Type templateType, string localName) {
            var s = Instance.GetValue(appDomain);
            return s.providers.Where(t => t.Key.Item1 == templateType && t.Key.Item2.LocalName == localName)
                .Select(t => t.GetValue());
        }

        internal static IEnumerable<QualifiedName> GetTemplateNames(AppDomain appDomain, Type templateType) {
            var s = Instance.GetValue(appDomain);
            return s.providers.Where(t => t.Key.Item1 == templateType)
                .Select(t => t.Key.Item2);
        }

    }
}
