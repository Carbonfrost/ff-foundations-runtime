//
// - Template.cs -
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

    public static class Template {

        public static readonly ITemplate Null = new NullTemplate();

        public static ITemplate<T> Compose<T>(params ITemplate<T>[] items) {
            if (items == null || items.Length == 0)
                return NullTemplate<T>.Instance;
            if (items.Length == 1)
                return items[0];
            else
                return new CompositeTemplate<T>(items.ToArray());
        }

        public static ITemplate<T> Compose<T>(IEnumerable<ITemplate<T>> items) {
            if (items == null)
                return NullTemplate<T>.Instance;

            return Compose<T>(items.ToArray());
        }

        public static ITemplate<T> Create<T>(Action<T> initializer) {
            if (initializer == null)
                return NullTemplate<T>.Instance;

            return new ThunkTemplate<T>(initializer);
        }

        public static Type GetTemplateValueType(ITemplate template) {
            if (template == null)
                throw new ArgumentNullException("template");

            return GetTemplateValueType(template.GetType());
        }

        public static Type GetTemplateValueType(Type templateType) {
            if (templateType == null)
                throw new ArgumentNullException("templateType");

            Type definition;
            if (templateType.IsInterface && templateType.IsGenericType
                && (definition = templateType.GetGenericTypeDefinition()) == typeof(ITemplate<>)) {
                return templateType.GetGenericArguments()[0];
            }

            var result = templateType.GetInterface(typeof(ITemplate<>).FullName);
			return result == null ? null : result.GetGenericArguments()[0];
        }

        public static Func<T> ToFactory<T>(ITemplate<T> template) {
            if (template == null)
                throw new ArgumentNullException("template");

            return () => {
                var result = Activation.CreateInstance<T>();
                template.Initialize(result);
                return result;
            };
        }

        public static ITemplate<T> Typed<T>(ITemplate template) {
            if (template == null)
                throw new ArgumentNullException("template");

            var t = template as ITemplate<T>;
            if (t != null)
                return t;

            return new TypedAdapter<T>(template);
        }

        public static object MemberwiseCopy(object source, object destination) {
            if (source == null)
                throw new ArgumentNullException("source");
            if (object.ReferenceEquals(destination, null))
                throw new ArgumentNullException("destination");
            var props = Properties.FromValue(source).ToArray();
            Activation.Initialize(destination, props);
            return destination;
        }

        public static object Copy(object source, object destination) {
            if (source == null)
                throw new ArgumentNullException("source");
            if (object.ReferenceEquals(destination, null))
                throw new ArgumentNullException("destination");
            var copyFromMethod = FindCopyFromMethod(source.GetType());
            if (copyFromMethod == null) {
                return MemberwiseCopy(source, destination);
            } else {
                copyFromMethod.Invoke(destination, new object[] {
                                          source
                                      });
                return destination;
            }
        }

        public static ITemplate<T> Create<T>(T initializer) {
            if (object.Equals(initializer, null))
                return NullTemplate<T>.Instance;

            var copyFromMethod = FindCopyFromMethod(typeof(T));
            if (copyFromMethod != null)
                return new InvokerTemplate<T>(copyFromMethod, initializer);

            var props = Properties.FromValue(initializer).ToArray();
            return Create<T>(t => Activation.Initialize(t, props));
        }

        public static ITemplate<T> Create<T>(IEnumerable<KeyValuePair<string, object>> initializer) {
            if (initializer == null)
                return NullTemplate<T>.Instance;

            var props = initializer.ToArray();
            return Create<T>(t => Activation.Initialize(t, props));
        }

        public static T CreateInstance<T>(this ITemplate<T> source) {
            return CreateInstance<T>(source, null);
        }

        public static T CreateInstance<T>(this ITemplate<T> source, IActivationFactory factory) {
            if (source == null)
                throw new ArgumentNullException("source");

            var result = (factory ?? ActivationFactory.Default).CreateInstance<T>();
            source.Initialize(result);
            return result;
        }

        public static QualifiedName GetTemplateName<T>(this ITemplate<T> source) {
            if (source == null)
                throw new ArgumentNullException("source");

            return Utility.LateBoundProperty<QualifiedName>(source, "QualifiedName");
        }

        public static ITemplate<object> FromName(Type templateType, string name) {
            if (templateType == null)
                throw new ArgumentNullException("templateType");
            if (name == null)
                throw new ArgumentNullException("name");
            if (name.Length == 0)
                throw Failure.EmptyString("name");
            return (ITemplate<object>) TemplateData.GetTemplatesByLocalName(AppDomain.CurrentDomain, templateType, name).SingleOrDefault();
        }

        public static ITemplate<object> FromName(Type templateType, QualifiedName name) {
            if (templateType == null)
                throw new ArgumentNullException("templateType");
            if (name == null)
                throw new ArgumentNullException("name");
            return (ITemplate<object>) TemplateData.GetTemplate(AppDomain.CurrentDomain, templateType, name);
        }

        public static ITemplate<T> FromName<T>(string name) {
            if (name == null)
                throw new ArgumentNullException("name");
            if (name.Length == 0)
                throw Failure.EmptyString("name");
            return (ITemplate<T>) TemplateData.GetTemplatesByLocalName(AppDomain.CurrentDomain, typeof(T), name).SingleOrDefault();
        }

        public static ITemplate<T> FromName<T>(QualifiedName name) {
            if (name == null)
                throw new ArgumentNullException("name");
            return (ITemplate<T>) TemplateData.GetTemplate(AppDomain.CurrentDomain, typeof(T), name);
        }

        static MethodInfo FindCopyFromMethod(Type type) {
            try {
                // Look for CopyFrom method
                return type.GetMethod(
                    "CopyFrom", BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { type }, null);
            } catch (AmbiguousMatchException) {
                return null;
            }
        }

        abstract class TemplateBase<T> : ITemplate<T> {

            public abstract void Initialize(T value);

            public virtual bool CanInitialize(object value) {
                return value is T;
            }

            public void Initialize(object value) {
                if (CanInitialize(value))
                    Initialize((T) value);
                else
                    throw RuntimeFailure.TemplateDoesNotSupportOperand("value");
            }
        }

        sealed class TypedAdapter<T> : TemplateBase<T> {

            readonly ITemplate template;

            public TypedAdapter(ITemplate template) {
                this.template = template;
            }

            public override void Initialize(T value) {
                template.Initialize(value);
            }

        }

        sealed class NullTemplate<T> : TemplateBase<T> {

            public static readonly ITemplate<T> Instance = new NullTemplate<T>();

            private NullTemplate() {}
            public override void Initialize(T value) {}
        }

        sealed class NullTemplate : ITemplate {
            bool ITemplate.CanInitialize(object value) { return true; }
            void ITemplate.Initialize(object value) {}
        }

        sealed class InvokerTemplate<T> : TemplateBase<T> {

            private readonly MethodInfo initializer;
            private readonly T copyFromValue;

            public InvokerTemplate(MethodInfo initializer, T copyFromValue) {
                this.initializer = initializer;
                this.copyFromValue = copyFromValue;
            }

            public override void Initialize(T value) {
                initializer.Invoke(value, new object[] { copyFromValue });
            }
        }

        sealed class CompositeTemplate<T> : TemplateBase<T> {

            readonly ITemplate<T>[] items;

            public CompositeTemplate(ITemplate<T>[] items) {
                this.items = items;
            }

            public override void Initialize(T value) {
                foreach (var t in items)
                    t.Initialize(value);
            }
        }

        sealed class ThunkTemplate<T> : TemplateBase<T> {

            private readonly Action<T> initializer;

            public ThunkTemplate(Action<T> initializer) {
                this.initializer = initializer;
            }

            public override void Initialize(T value) {
                initializer(value);
            }
        }
    }
}

