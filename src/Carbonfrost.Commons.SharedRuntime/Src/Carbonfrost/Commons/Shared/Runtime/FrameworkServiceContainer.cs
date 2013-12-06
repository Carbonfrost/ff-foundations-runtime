//
// - FrameworkServiceContainer.cs -
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
using System.Reflection;

using Carbonfrost.Commons.Shared.Runtime;

namespace Carbonfrost.Commons.Shared.Runtime {

    public partial class FrameworkServiceContainer : IServiceContainer, IServiceContainerExtension {

        protected IServiceProvider ParentProvider { get { return parentProvider; } }

        private IServiceContainer ParentContainer {
            get { return (IServiceContainer) this.ParentProvider.GetService(typeof(IServiceContainer)); }
        }

        public FrameworkServiceContainer() : this(null) {
        }

        public FrameworkServiceContainer(IServiceProvider parentProvider) : this(parentProvider, null) {
        }

        public FrameworkServiceContainer(IServiceProvider parentProvider, IActivationFactory activationFactory) {
            this.parentProvider = parentProvider ?? ServiceProvider.Null;
            this.activationFactory = activationFactory ?? ActivationFactory.Default;
        }

        protected virtual void Dispose(object instance) {
            if (instance != null) {
                IDisposable d = instance as IDisposable;
                if (d != null)
                    d.Dispose();
            }
        }

        public void Publish(object instance) {
            Publish(instance, false);
        }

        public void Publish(object instance, bool promote) {
            if (instance == null)
                throw new ArgumentNullException("instance"); // $NON-NLS-1

            IDictionary<Type, object> typeBindings = CreateTypeDictionary();
            IDictionary<string, object> nameBindings = new Dictionary<string, object>();
            PublishServices(instance, new HashSet<object>(), typeBindings, nameBindings);

            foreach (var t in typeBindings)
                this.services[t.Key] = t.Value;

            foreach (var t in nameBindings)
                this.servicesByName[t.Key] = t.Value;

            if (promote) {
                IServiceContainer parent = this.ParentContainer;
                foreach (var t in typeBindings)
                    parent.AddService(t.Key, t.Value, true);

                IServiceContainerExtension parentE = this.ParentContainer as IServiceContainerExtension;
                if (parentE != null) {
                    foreach (var t in nameBindings)
                        parentE.AddService(t.Key, t.Value, true);
                }
            }
        }

        void PublishServices(object instance,
                             HashSet<object> breadcrumbs,
                             IDictionary<Type, object> typeBindings,
                             IDictionary<string, object> nameBindings) {

            if (breadcrumbs.Contains(instance)) return;
            breadcrumbs.Add(instance);

            Type t = instance.GetType();
            foreach (var m in t.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                PublishAttribute[] attrs = (PublishAttribute[]) m.GetCustomAttributes(typeof(PublishAttribute), true);
                if (attrs == null || attrs.Length == 0) continue;

                object serviceValue = GetServiceValue(m, instance);
                if (serviceValue != null) {
                    PublishServices(serviceValue, breadcrumbs, typeBindings, nameBindings);

                    foreach (var a in attrs) {
                        Type serviceType = a.Type ?? serviceValue.GetType();
                        if (!serviceType.IsInstanceOfType(serviceValue))
                            throw RuntimeFailure.PublishAttributeTypeMismatch(m, serviceType);

                        typeBindings[serviceType] = serviceValue;
                        if (!string.IsNullOrEmpty(a.Name)) {
                            nameBindings[a.Name] = serviceValue;
                        }
                    }
                }
            }
        }

        static object GetServiceValue(MemberInfo m, object instance) {
            switch (m.MemberType) {
                case MemberTypes.Field:
                    return ((FieldInfo) m).GetValue(instance);
                case MemberTypes.Property:
                    return ((PropertyInfo) m).GetValue(instance, null);
                case MemberTypes.Method:
                    return ((MethodInfo) m).Invoke(instance, null);
                default:
                    return null;
            }
        }

    }
}
