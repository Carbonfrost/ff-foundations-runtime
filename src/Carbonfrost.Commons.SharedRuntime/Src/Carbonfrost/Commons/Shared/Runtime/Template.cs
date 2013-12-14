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

namespace Carbonfrost.Commons.Shared.Runtime {

    public static class Template {

        public static ITemplate<T> Create<T>(Action<T> initializer) {
            if (initializer == null)
                return new NullTemplate<T>();

            return new ThunkTemplate<T>(initializer);
        }

        public static ITemplate<T> Create<T>(T initializer) {
			if (object.Equals(initializer, null))
				return new NullTemplate<T>();

            var props = Properties.FromValue(initializer).ToArray();
            return Create<T>(t => Activation.Initialize(t, props));
        }

        public static ITemplate<T> Create<T>(IEnumerable<KeyValuePair<string, object>> initializer) {
            if (initializer == null)
                return new NullTemplate<T>();

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

        sealed class NullTemplate<T> : ITemplate<T> {

            public void Initialize(T value) {
            }
        }

        sealed class ThunkTemplate<T> : ITemplate<T> {

            private readonly Action<T> initializer;

            public ThunkTemplate(Action<T> initializer) {
                this.initializer = initializer;
            }

            public void Initialize(T value) {
                initializer(value);
            }
        }
    }
}
