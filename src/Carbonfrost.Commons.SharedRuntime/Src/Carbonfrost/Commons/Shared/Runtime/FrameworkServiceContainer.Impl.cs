//
// - FrameworkServiceContainer.Impl.cs -
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
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;

using Carbonfrost.Commons.ComponentModel;
using Carbonfrost.Commons.Shared.Runtime;

namespace Carbonfrost.Commons.Shared.Runtime {

    public partial class FrameworkServiceContainer : IServiceContainer, IServiceContainerExtension, IActivationFactory {

        private readonly Dictionary<Type, object> services = CreateTypeDictionary();
        private readonly Dictionary<string, object> servicesByName = new Dictionary<string, object>();
        private readonly IServiceProvider parentProvider;
        private readonly IActivationFactory activationFactory;

        protected object OnServiceResolve(ServiceResolveEventArgs args) {
            if (ServiceResolve == null)
                return null;
            else
                return ServiceResolve(this, args);
        }

        // IActivationFactory implementation
        public virtual object CreateInstance(
            Type type,
            IEnumerable<KeyValuePair<string, object>> values = null,
            IPopulateComponentCallback callback = null,
            IServiceProvider serviceProvider = null,
            params Attribute[] attributes)
        {
            return this.activationFactory.CreateInstance(type, values, callback, ServiceProvider.Compose(serviceProvider, this), attributes);
        }

        // IServiceContainerExtension implementation
        public event ServiceResolveEventHandler ServiceResolve;

        public void AddService(string serviceName, object serviceInstance) {
            AddService(serviceName, serviceInstance, false);
        }

        public void AddService(string serviceName, object serviceInstance, bool promote) {
            Require.NotNullOrEmptyString("serviceName", serviceName); // $NON-NLS-1

            if (this.servicesByName.ContainsKey(serviceName))
                throw RuntimeFailure.ServiceAlreadyExists("serviceName", serviceName);

            this.servicesByName.Add(serviceName, serviceInstance);

            if (promote) {
                IServiceContainerExtension ice = this.ParentContainer as IServiceContainerExtension;
                if (ice != null)
                    ice.AddService(serviceName, serviceInstance, true);
            }
        }

        public void AddService(string serviceName, ServiceCreatorCallback creatorCallback) {
            AddService(serviceName, creatorCallback, false);
        }

        public void AddService(string serviceName, ServiceCreatorCallback creatorCallback, bool promote) {
            Require.NotNullOrEmptyString("serviceName", serviceName); // $NON-NLS-1

            if (this.servicesByName.ContainsKey(serviceName))
                throw RuntimeFailure.ServiceAlreadyExists("serviceName", serviceName);

            this.servicesByName.Add(serviceName, creatorCallback);

            if (promote) {
                IServiceContainerExtension ice = this.ParentContainer as IServiceContainerExtension;
                if (ice != null)
                    ice.AddService(serviceName, creatorCallback, true);
            }
        }

        public void RemoveService(string serviceName, bool promote) {
            Require.NotNullOrEmptyString("serviceName", serviceName); // $NON-NLS-1

            this.servicesByName.Remove(serviceName);

            if (promote) {
                IServiceContainerExtension ice = this.ParentContainer as IServiceContainerExtension;
                if (ice != null)
                    ice.RemoveService(serviceName, true);
            }
        }

        public void RemoveService(string serviceName) {
            RemoveService(serviceName, false);
        }

        public object GetService(string serviceName) {
            Require.NotNullOrEmptyString("serviceName", serviceName); // $NON-NLS-1
            object result;
            if (this.servicesByName.TryGetValue(serviceName, out result))
                return result;
            else
                return null;
        }

        public IEnumerable<object> GetServices(Type serviceType) {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");

            var items = this.services.Values.Concat(this.servicesByName.Values);
            return items.Where(serviceType.IsInstanceOfType).Distinct();
        }

        // IServiceContainer implementation
        public void AddService(Type serviceType, object serviceInstance) {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType"); // $NON-NLS-1

            AddService(serviceType, serviceInstance, false);
        }

        public void AddService(Type serviceType, object serviceInstance, bool promote) {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType"); // $NON-NLS-1

            IService s = (serviceInstance as IService);
            if (s != null)
                s.StartService();

            if (promote) {
                IServiceContainer container = this.ParentContainer;
                if (container != null) {
                    container.AddService(serviceType, serviceInstance, promote);
                    return;
                }
            }

            if (this.services.ContainsKey(serviceType))
                throw RuntimeFailure.ServiceAlreadyExists("serviceType", serviceType);

            this.services[serviceType] = serviceInstance;
        }

        public void AddService(Type serviceType, ServiceCreatorCallback callback) {
            AddService(serviceType, callback, false);
        }

        public void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote) {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType"); // $NON-NLS-1
            if (callback == null)
                throw new ArgumentNullException("callback"); // $NON-NLS-1

            if (promote) {
                IServiceContainer container = this.ParentContainer;
                if (container != null) {
                    container.AddService(serviceType, callback, promote);
                    return;
                }
            }

            if (this.services.ContainsKey(serviceType))
                throw RuntimeFailure.ServiceAlreadyExists("serviceType", serviceType);

            this.services[serviceType] = callback;
        }

        public void RemoveService(Type serviceType) {
            RemoveService(serviceType, false);
        }

        public void RemoveService(Type serviceType, bool promote) {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType"); // $NON-NLS-1

            if (promote) {
                IServiceContainer container = this.ParentContainer;
                if (container != null) {
                    container.RemoveService(serviceType, promote);
                    return;
                }
            }

            this.services.Remove(serviceType);
        }

        public object GetService(Type serviceType) {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType"); // $NON-NLS-1

            // Process self
            if (serviceType.IsInstanceOfType(this))
                return this;

            object service = null;
            if (service == null)
                this.services.TryGetValue(serviceType, out service);

            ServiceCreatorCallback callback = service as ServiceCreatorCallback;
            if (callback != null) {
                service = callback(this, serviceType);
                if (service != null && !serviceType.IsAssignableFrom(service.GetType())) {
                    service = null;
                }

                this.services[serviceType] = service;
            }

            return service ?? this.ParentProvider.GetService(serviceType) ?? ResolveUsingEvent(null, serviceType);
        }

        private object ResolveUsingEvent(string serviceName, Type serviceType) {
            // prevent reentrancy
            return OnServiceResolve(new ServiceResolveEventArgs(serviceName, serviceType));
        }

        private static Dictionary<Type, object> CreateTypeDictionary() {
            return new Dictionary<Type, object>(Utility.EquivalentComparer);
        }

    }
}
