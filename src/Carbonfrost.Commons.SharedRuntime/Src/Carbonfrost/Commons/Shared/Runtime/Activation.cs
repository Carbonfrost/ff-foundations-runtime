//
// - Activation.cs -
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Carbonfrost.Commons.ComponentModel;

namespace Carbonfrost.Commons.Shared.Runtime {

    public static class Activation {

        static readonly Expression<Func<string, object>> Parse
            = (a) => (default(object));

        static readonly Expression<Func<string, CultureInfo, object>> ParseWithCulture
            = (a, b) => (default(object));

        const string PARSE_METHOD_NAME = "Parse";

        public static readonly Attribute Reusable = new ReusableAttribute();
        public static readonly Attribute ProcessIsolated = new ProcessIsolatedAttribute();
        public static readonly Attribute AppDomainIsolated = new AppDomainIsolatedAttribute();

        public static T CreateInstance<T>(QualifiedName typeName,
                                          IEnumerable<KeyValuePair<string, object>> values = null,
                                          IPopulateComponentCallback callback = null,
                                          IServiceProvider serviceProvider = null) {
            if (typeName == null)
                throw new ArgumentNullException("typeName");

            Type type = AppDomain.CurrentDomain.GetTypeByQualifiedName(typeName);
            return CreateInstance<T>(type,
                                     values,
                                     callback,
                                     serviceProvider);
        }

        public static object CreateInstance(QualifiedName typeName,
                                            IEnumerable<KeyValuePair<string, object>> values = null,
                                            IPopulateComponentCallback callback = null,
                                            IServiceProvider serviceProvider = null) {
            if (typeName == null)
                throw new ArgumentNullException("typeName");

            return CreateInstance(AppDomain.CurrentDomain.GetTypeByQualifiedName(typeName),
                                  values,
                                  callback,
                                  serviceProvider);
        }

        public static T CreateInstance<T>() {
            return (T) CreateInstance(typeof(T), null, null);
        }

        public static T CreateInstance<T>(IServiceProvider serviceProvider) {
            return (T) CreateInstance(typeof(T), null, null, serviceProvider);
        }

        public static T CreateInstance<T>(Type type,
                                          IEnumerable<KeyValuePair<string, object>> values,
                                          IPopulateComponentCallback callback,
                                          IServiceProvider serviceProvider,
                                          params Attribute[] attributes) {
            return (T) CreateInstance(type, values, callback, serviceProvider, attributes);
        }

        public static T CreateInstance<T>(Type type,
                                          IServiceProvider serviceProvider) {
            return (T) CreateInstance(type, null, null, serviceProvider, null);
        }
        
        public static T CreateInstance<T>(Type type) {
            return (T) CreateInstance(type, null, null, null, null);
        }
        
        public static T CreateInstance<T>(IEnumerable<KeyValuePair<string, object>> values,
                                          IPopulateComponentCallback callback,
                                          IServiceProvider serviceProvider,
                                          params Attribute[] attributes) {
            return (T) CreateInstance(typeof(T), values, callback, serviceProvider, attributes);
        }
        
        public static T CreateInstance<T>(IEnumerable<KeyValuePair<string, object>> values,
                                          IPopulateComponentCallback callback) {
            return (T) CreateInstance(typeof(T), values, callback, null, null);
        }
        
        public static T CreateInstance<T>(IEnumerable<KeyValuePair<string, object>> values) {
            return (T) CreateInstance(typeof(T), values, null, null, null);
        }
        
        public static T CreateInstance<T>(this IActivationFactory factory) {
            return (T) CreateInstance(factory: factory, type: typeof(T));
        }

        public static T CreateInstance<T>(this IActivationFactory factory, IServiceProvider serviceProvider) {
            return (T) CreateInstance(factory: factory, type: typeof(T), serviceProvider: serviceProvider);
        }

        static IActivationFactory GetDefaultFactory(IActivationFactory factory, IServiceProvider serviceProvider) {
            serviceProvider = (serviceProvider ?? ServiceProvider.Null);
            return serviceProvider.TryGetService(factory ?? ActivationFactory.Default);
        }

        public static T CreateInstance<T>(this IActivationFactory factory,
                                          Type type,
                                          IEnumerable<KeyValuePair<string, object>> values = null,
                                          IPopulateComponentCallback callback = null,
                                          IServiceProvider serviceProvider = null,
                                          params Attribute[] attributes) {

            factory = GetDefaultFactory(factory, serviceProvider);
            return (T) factory.CreateInstance(type, values, callback, serviceProvider, attributes);
        }

        public static object CreateInstance(this IActivationFactory factory,
                                            Type type,
                                            IEnumerable<KeyValuePair<string, object>> values = null,
                                            IPopulateComponentCallback callback = null,
                                            IServiceProvider serviceProvider = null,
                                            params Attribute[] attributes) {
            factory = GetDefaultFactory(factory, serviceProvider);
            return factory.CreateInstance(type, values, callback, serviceProvider, attributes);
        }

        public static object Build(Type type,
                                   IEnumerable<KeyValuePair<string, object>> values = null,
                                   IPopulateComponentCallback callback = null,
                                   IServiceProvider serviceProvider = null,
                                   params Attribute[] attributes) {

            return ActivationFactory.Build.CreateInstance(type, values, callback, serviceProvider, attributes);
        }

        public static object CreateInstance(Type type,
                                            IEnumerable<KeyValuePair<string, object>> values = null,
                                            IPopulateComponentCallback callback = null,
                                            IServiceProvider serviceProvider = null,
                                            params Attribute[] attributes) {

            var factory = GetDefaultFactory(null, serviceProvider);
            return factory.CreateInstance(type, values, callback, serviceProvider, attributes);
        }

        public static void Initialize(object component,
                                      IEnumerable<KeyValuePair<string, object>> values,
                                      IPopulateComponentCallback callback = null,
                                      IServiceProvider serviceProvider = null) {

            ActivationFactory af = (ActivationFactory) ActivationFactory.Default;
            af.InitializeInternal(component, values, callback, serviceProvider);
        }

        // TODO Needs work -- we want there to be as much fallback as
        // possible with parsing and stream loading.  Also probably
        // get encoding from content type

        public static object FromFile(Type componentType, string fileName) {
            if (componentType == null)
                throw new ArgumentNullException("componentType");
            if (fileName == null)
                throw new ArgumentNullException("fileName");
            if (fileName.Length == 0)
                throw Failure.EmptyString("fileName");

            return FromStreamContext(componentType, StreamContext.FromFile(fileName));
        }

        public static T FromFile<T>(string fileName) {
            return (T) FromFile(typeof(T), fileName);
        }

        public static T FromStream<T>(Stream stream, Encoding encoding = null) {
            if (stream == null)
                throw new ArgumentNullException("stream");

            return (T) FromStream(typeof(T), stream, encoding);
        }

        public static object FromStream(Type instanceType, Stream stream, Encoding encoding = null) {
            if (instanceType == null)
                throw new ArgumentNullException("instanceType");
            if (stream == null)
                throw new ArgumentNullException("stream");

            return FromStreamContext(instanceType, StreamContext.FromStream(stream).ChangeEncoding(encoding));
        }

        public static T FromSource<T>(Uri uri) {
            return (T) FromSource(typeof(T), uri);
        }

        public static object FromSource(Type componentType, Uri uri) {
            if (componentType == null)
                throw new ArgumentNullException("componentType");
            if (uri == null)
                throw new ArgumentNullException("uri");
            throw new NotImplementedException();
        }

        public static T FromStreamContext<T>(StreamContext streamContext) {
            return (T) FromStreamContext(typeof(T), streamContext);
        }

        public static object FromStreamContext(Type componentType,
                                               StreamContext streamContext) {
            if (componentType == null)
                throw new ArgumentNullException("componentType");
            if (streamContext == null)
                throw new ArgumentNullException("streamContext");

            StreamingSource ss = StreamingSource.Create(componentType);
            if (ss == null)
                throw RuntimeFailure.NoAcceptableStreamingSource(componentType);

            return ss.Load(streamContext, componentType);
        }

        public static T FromText<T>(string text, CultureInfo culture = null) {
            return (T) FromText(typeof(T), text, culture);
        }

        public static object FromText(Type componentType, string text, CultureInfo culture = null) {
            if (componentType == null)
                throw new ArgumentNullException("componentType");

            throw new NotImplementedException();
        }

        static T CreateInstanceSafe<T>(ITemplate<T> temp, string name) {
            if (temp == null)
                throw RuntimeFailure.TemplateNotFound(name);
            return temp.CreateInstance();
        }

        public static T FromProvider<T>(string name,
                                        IEnumerable<KeyValuePair<string, object>> values = null,
                                        IPopulateComponentCallback callback = null,
                                        IServiceProvider serviceProvider = null) {
            if (name == null)
                throw new ArgumentNullException("name");
            if (name.Length == 0)
                throw Failure.EmptyString("name");

            AppDomain appDomain = AppDomain.CurrentDomain;
            return (T) ProviderData.GetProvidersByLocalName(appDomain, typeof(T), name,
                                                            t => t.Activate(values, callback, serviceProvider))
                .SingleOrThrow(RuntimeFailure.MultipleProviders);
        }

        public static T FromProvider<T>(QualifiedName name,
                                        IEnumerable<KeyValuePair<string, object>> values = null,
                                        IPopulateComponentCallback callback = null,
                                        IServiceProvider serviceProvider = null) {
            if (name == null)
                throw new ArgumentNullException("name");

            AppDomain appDomain = AppDomain.CurrentDomain;
            return (T) ProviderData.GetProvider(appDomain, typeof(T), name, t => t.Activate(values, callback, serviceProvider));
        }

        public static T FromTemplate<T>(string name) {
            ITemplate<T> temp = Template.FromName<T>(name);
            return CreateInstanceSafe(temp, name);
        }

        public static T FromTemplate<T>(QualifiedName name) {
            ITemplate<T> temp = Template.FromName<T>(name);
            return CreateInstanceSafe(temp, name.ToString());
        }

        public static object FromTemplate(Type componentType, string name) {
            ITemplate<object> temp = Template.FromName(componentType, name);
            return CreateInstanceSafe(temp, name);
        }

        public static object FromTemplate(Type componentType, QualifiedName name) {
            ITemplate<object> temp = Template.FromName(componentType, name);
            return CreateInstanceSafe(temp, name.ToString());
        }

        internal static object ConvertText(Type type,
                                           string text,
                                           IServiceProvider serviceProvider,
                                           CultureInfo culture) {
            TypeConverter tc = TypeDescriptor.GetConverter(type);
            if (tc.CanConvertFrom(typeof(string)))
                return tc.ConvertFromInvariantString(text);

            MethodInfo parse = type.GetMethod("Parse", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static);
            if (parse != null) {
                object[] parms = parse.GetParameters().Select<ParameterInfo, object>(
                    p => {
                        Type pt = p.ParameterType;
                        if (typeof(IServiceProvider).Equals(pt))
                            return serviceProvider;

                        if (typeof(CultureInfo).Equals(pt))
                            return culture;

                        if (typeof(string).Equals(pt))
                            return text;
                        else
                            return null;
                    }).ToArray();

                return parse.Invoke(null, parms);
            }

            Encoding e = Encoding.UTF8;
            return FromStream(type, new MemoryStream(e.GetBytes(text), false), e);
        }

    }
}
