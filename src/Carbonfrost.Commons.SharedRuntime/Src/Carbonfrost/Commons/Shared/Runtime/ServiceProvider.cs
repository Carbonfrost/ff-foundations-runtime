//
// - ServiceProvider.cs -
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
using System.Linq;
using System.Threading;
using Carbonfrost.Commons.Shared.Runtime;

namespace Carbonfrost.Commons.Shared {

    public static class ServiceProvider {

        static readonly IServiceProvider _null = new NullServiceProvider();
        static readonly object _rootInit = new object();
        static readonly string rootType;
        static IServiceProvider _root;

        public static IServiceProvider Null { get { return _null; } }
        public static IServiceProvider Root {
            get {
                EnsureRootInit();
                return _root;
            }
        }

        static readonly ThreadLocal<Stack<IServiceProvider>> _current
            = new ThreadLocal<Stack<IServiceProvider>>(() => new Stack<IServiceProvider>());

        public static IServiceProvider Current {
            get {
                if (_current.Value.Count == 0)
                    return ServiceProvider.Root;
                else
                    return _current.Value.Peek();
            }
        }

        static void EnsureRootInit() {
            if (Monitor.TryEnter(_rootInit)) {
                try {
                    if (rootType.Length > 0) {
                        _root = Activation.CreateInstance<IServiceProvider>(
                            Type.GetType(rootType));
                    }

                } catch (Exception ex) {
                    Traceables.RootServiceProviderInitFailure(rootType, ex);

                } finally {
                    Monitor.Exit(_rootInit);
                }

                if (_root == null) {
                    _root = new FrameworkServiceContainer();
                }
            }
        }

        static ServiceProvider() {
            ServiceProvider.rootType = Convert.ToString(AppDomain.CurrentDomain.GetData(AppDomainProperty.RootServiceProvider)) ?? string.Empty;
        }

        public static T GetService<T>(this IServiceProvider serviceProvider) {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider"); // $NON-NLS-1

            T t = (T) serviceProvider.GetService(typeof(T));
            if (object.Equals(t, default(T)))
                throw RuntimeFailure.ServiceNotFound(typeof(T));

            return t;
        }

        public static T TryGetService<T>(this IServiceProvider serviceProvider, T defaultService = default(T)) {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider"); // $NON-NLS-1

            T t = (T) serviceProvider.GetService(typeof(T));
            if (object.Equals(t, default(T)))
                return defaultService;
            else
                return t;
        }

        public static IEnumerable<T> GetServices<T>(this IServiceProviderExtension serviceProvider) {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider"); // $NON-NLS-1

            var result = serviceProvider.GetServices(typeof(T));
            if (result == null)
                return Empty<T>.Array;
            else
                return result.Cast<T>();
        }

        public static IServiceProviderExtension Extend(this IServiceProvider serviceProvider) {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            IServiceProviderExtension e = serviceProvider as IServiceProviderExtension;
            if (e != null)
                return e;

            return new ServiceProviderExtension(serviceProvider);
        }

        public static IServiceProvider FromValue(object value) {
            if (value == null)
                throw new ArgumentNullException("value");

            IServiceProvider s = value as IServiceProvider;
            if (s == null)
                return new ServiceProviderValue(value);
            else
                return s;
        }

        public static IServiceProvider Filtered(IServiceProvider serviceProvider, Func<Type, bool> typeFilter) {
            if (serviceProvider == null || serviceProvider == Null)
                return Null;
            if (typeFilter == null)
                return serviceProvider;

            return new FilteredServiceProvider(typeFilter, serviceProvider);
        }

        public static IServiceProvider Filtered(IServiceProvider serviceProvider, params Type[] types) {
            if (serviceProvider == null || serviceProvider == Null)
                return Null;
            if (types == null || types.Length == 0)
                return serviceProvider;

            ICollection<Type> myTypes;
            if (types.Length > 8)
                myTypes = new HashSet<Type>(types);
            else
                myTypes = new List<Type>(types);

            return Filtered(serviceProvider, t => myTypes.Contains(t));
        }

        public static IServiceProvider FromValue(object value, params Type[] types) {
            if (value == null)
                throw new ArgumentNullException("value");

            return Filtered(FromValue(value), types);
        }

        public static IServiceProvider Compose(IEnumerable<IServiceProvider> items) {
            return Utility.OptimalComposite(items,
                                            i => new CompositeServiceProvider(i),
                                            Null);
        }

        public static IServiceProvider Compose(IEnumerable<IServiceProviderExtension> items) {
            return Utility.OptimalComposite(items,
                                            i => new CompositeServiceProviderExtension(i),
                                            (IServiceProviderExtension) Null);
        }

        public static IServiceProvider Compose(params IServiceProvider[] items) {
            return Utility.OptimalComposite(items,
                                            i => new CompositeServiceProvider(i),
                                            Null);
        }

        public static IServiceProviderExtension Compose(params IServiceProviderExtension[] items) {
            return Utility.OptimalComposite(items,
                                            i => new CompositeServiceProviderExtension(i),
                                            (IServiceProviderExtension) Null);
        }

        // Nested type definitions
        class NullServiceProvider : IServiceProviderExtension {

            public object GetService(Type serviceType) {
                if (serviceType == null)
                    throw new ArgumentNullException("serviceType"); // $NON-NLS-1
                return null;
            }

            public object GetService(string serviceName) {
                Require.NotNullOrEmptyString("serviceName", serviceName); // $NON-NLS-1
                return null;
            }

            public IEnumerable<object> GetServices(Type serviceType) {
                if (serviceType == null)
                    throw new ArgumentNullException("serviceType");

                return Empty<object>.Array;
            }
        }

        class CompositeServiceProvider : IServiceProvider {

            private readonly IReadOnlyCollection<IServiceProvider> items;

            public CompositeServiceProvider(IReadOnlyCollection<IServiceProvider> items) {
                this.items = items;
            }

            public object GetService(Type serviceType) {
                if (serviceType == null)
                    throw new ArgumentNullException("serviceType");

                foreach (var s in items) {
                    object result = s.GetService(serviceType);
                    if (result != null)
                        return result;
                }
                return null;
            }
        }

        class ServiceProviderExtension : IServiceProviderExtension {

            private readonly IServiceProvider item;

            public ServiceProviderExtension(IServiceProvider item) {
                this.item = item;
            }

            public object GetService(string serviceName) {
                return null;
            }

            public IEnumerable<object> GetServices(Type serviceType) {
                if (serviceType == null)
                    throw new ArgumentNullException("serviceType");

                yield return item.GetService(serviceType);
            }

            public object GetService(Type serviceType) {
                if (serviceType == null)
                    throw new ArgumentNullException("serviceType");

                return item.GetService(serviceType);
            }
        }

        sealed class ServiceProviderValue : IServiceProvider {

            private readonly object value;

            public ServiceProviderValue(object value) {
                this.value = value;
            }

            public object GetService(Type serviceType) {
                if (serviceType == null)
                    throw new ArgumentNullException("serviceType");

                return value.TryAdapt(serviceType, null);
            }
        }

        sealed class FilteredServiceProvider : IServiceProvider {

            private readonly Func<Type, bool> types;
            private readonly IServiceProvider sp;

            public FilteredServiceProvider(Func<Type, bool> types, IServiceProvider sp) {
                this.sp = sp;
                this.types = types;
            }

            public object GetService(Type serviceType) {
                if (serviceType == null)
                    throw new ArgumentNullException("serviceType");
                if (types(serviceType))
                    return sp.GetService(serviceType);
                else
                    return null;
            }
        }

        class CompositeServiceProviderExtension : CompositeServiceProvider, IServiceProviderExtension {

            private readonly IReadOnlyCollection<IServiceProviderExtension> items;

            public CompositeServiceProviderExtension(IReadOnlyCollection<IServiceProviderExtension> items) : base(items) {
                this.items = items;
            }

            public object GetService(string serviceName) {
                if (serviceName == null)
                    throw new ArgumentNullException("serviceName");
                if (serviceName.Length == 0)
                    throw Failure.EmptyString("serviceName");

                return items.FirstNonNull(i => i.GetService(serviceName));
            }

            public IEnumerable<object> GetServices(Type serviceType) {
                if (serviceType == null)
                    throw new ArgumentNullException("serviceType");

                return items.SelectMany(t => t.GetServices(serviceType));
            }

        }

        internal static void PushCurrent(IServiceProvider serviceProvider) {
            _current.Value.Push(serviceProvider);
        }

        internal static void PopCurrent() {
            _current.Value.Pop();
        }
    }
}
