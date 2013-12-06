//
// - ActivationFactory.cs -
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

using Carbonfrost.Commons.ComponentModel;
using Carbonfrost.Commons.ComponentModel.Annotations;

namespace Carbonfrost.Commons.Shared.Runtime {

    [Providers]
    public abstract class ActivationFactory : IActivationFactory {

        public static readonly IActivationFactory Default
            = new DefaultActivationFactory();

        public static readonly IActivationFactory Build
            = new BuildActivationFactory();

        public static IActivationFactory FromName(string name) {
            return AppDomain.CurrentDomain.GetProvider<IActivationFactory>(name);
        }

        public virtual object CreateInstance(Type type,
                                             IEnumerable<KeyValuePair<string, object>> values = null,
                                             IPopulateComponentCallback callback = null,
                                             IServiceProvider serviceProvider = null,
                                             params Attribute[] attributes)
        {

            if (type == null)
                throw new ArgumentNullException("type");

            try {
                ServiceProvider.PushCurrent(serviceProvider);
                serviceProvider = serviceProvider ?? ServiceProvider.Root;
                type = GetActivationType(type);
                object result = ActivateCoreHelper(type, ref values, serviceProvider);
                if (result == null)
                    throw RuntimeFailure.CannotActivateNoConstructor("type", type);

                InitializeCoreHelper(result, type, values, callback, serviceProvider);
                return result;

            } finally {
                ServiceProvider.PopCurrent();
            }
        }

        protected virtual Type GetActivationType(Type type) {
            if (type == null)
                throw new ArgumentNullException("type");

            return type.GetConcreteClass() ?? type;
        }

        internal void InitializeCoreHelper(object result,
                                           Type type,
                                           IEnumerable<KeyValuePair<string, object>> values,
                                           IPopulateComponentCallback callback,
                                           IServiceProvider serviceProvider) {

            Initialize(result, values, callback, serviceProvider);

            // Apply activation providers
            ExceptionHandler exceptionHandler = (ExceptionHandler) serviceProvider.GetService(typeof(ExceptionHandler));
            ApplyActivationProviders(result, exceptionHandler, type, serviceProvider);
            ApplyPropertyActivationProviders(result, exceptionHandler, serviceProvider);
        }

        protected virtual void ApplyPropertyActivationProviders(object result, ExceptionHandler exceptionHandler, IServiceProvider serviceProvider) {
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(result)) {
                foreach (var m in GetActivationProviders(property, result)) {
                    try {
                        m.ActivateProperty(serviceProvider, result, property);

                    } catch (Exception ex) {
                        if (Require.IsCriticalException(ex))
                            throw;

                        Exception ex2 = RuntimeFailure.CouldNotActivateProperty(property, ex);

                        if (exceptionHandler == null)
                            throw ex2;
                        else
                            exceptionHandler(m, ex2);
                    }
                }
            }
        }

        protected virtual void ApplyActivationProviders(object component,
                                                        ExceptionHandler exceptionHandler,
                                                        Type type,
                                                        IServiceProvider serviceProvider) {

            // TODO This activation provider should be supplied using reflection
            var pro = GetActivationProviders(type, component).Concat(new IActivationProvider[] { SubscribeActivationProvider.Instance });

            foreach (IActivationProvider m in pro) {
                try {

                    m.ActivateComponent(serviceProvider, component);
                } catch (Exception ex) {
                    if (exceptionHandler == null || Require.IsCriticalException(ex))
                        throw;
                    else
                        exceptionHandler(m, ex);
                }
            }
        }

        protected virtual IEnumerable<IActivationProvider> GetActivationProviders(PropertyDescriptor property, object component) {
            return property.GetActivationProviders();
        }

        protected virtual IEnumerable<IActivationProvider> GetActivationProviders(Type type, object component) {
            return type.GetActivationProviders();
        }

        protected virtual MethodBase GetActivationConstructor(Type type) {
            return type.GetActivationConstructor();
        }

        protected virtual void Initialize(object component,
                                          IEnumerable<KeyValuePair<string, object>> values,
                                          IPopulateComponentCallback callback = null,
                                          IServiceProvider serviceProvider = null) {

            if (component == null)
                throw new ArgumentNullException("component"); // $NON-NLS-1

            if (values == null)
                return;

            serviceProvider = serviceProvider ?? ServiceProvider.Null;
            callback = callback ?? PopulateComponentCallback.Default;

            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(component);
            foreach (var kvp in values) {
                PropertyDescriptor pd = properties.Find(kvp.Key, true);

                if (pd == null) {
                    callback.Missing(serviceProvider, kvp.Key);
                    continue;
                }

                object defaultValue = kvp.Value;
                try {
                    if (defaultValue == null || pd.PropertyType.IsInstanceOfType(defaultValue)) {
                    } else {
                        defaultValue = pd.Converter.ConvertFrom(defaultValue);
                    }
                    pd.SetValue(component, defaultValue);

                } catch (Exception ex) {
                    callback.ConversionError(serviceProvider, pd, defaultValue, ex);
                }
            }
        }

        object ActivateCoreHelper(Type type,
                                  ref IEnumerable<KeyValuePair<string, object>> values,
                                  IServiceProvider serviceProvider) {

            IEnumerable<KeyValuePair<string, object>> unboundValues;
            object result = InvokeActivationConstructor(type, values, out unboundValues, serviceProvider);
            values = unboundValues;
            return result;
        }

        protected virtual object InvokeActivationConstructor(
            Type type,
            IEnumerable<KeyValuePair<string, object>> values,
            out IEnumerable<KeyValuePair<string, object>> unboundValues,
            IServiceProvider serviceProvider) {

            MethodBase ac = GetActivationConstructor(type);
            if (ac == null) {
                unboundValues = null;
                return null;
            }

            object[] args = BindActivationConstructorArguments(
                ac, values, out unboundValues, serviceProvider);

            if (ac.MemberType == MemberTypes.Constructor) {
                return ((ConstructorInfo) ac).Invoke(args);

            } else {
                return ac.Invoke(null, args);
            }
        }

        protected virtual object[] BindActivationConstructorArguments(
            MethodBase activationConstructor,
            IEnumerable<KeyValuePair<string, object>> values,
            out IEnumerable<KeyValuePair<string, object>> unboundValues,
            IServiceProvider serviceProvider)
        {
            // We don't check for duplicates in the input enumerable;
            // the last one wins or we aggregate
            // TODO Perform aggregation, enforce types

            ParameterInfo[] parms = activationConstructor.GetParameters();
            IDictionary<string, int> namesToIndexes = parms.ToDictionary(p => p.Name, p => p.Position, StringComparer.OrdinalIgnoreCase);
            object[] args = new object[parms.Length];
            unboundValues = null;

            BitArray ba = new BitArray(parms.Length);
            if (values != null) {

                foreach (var kvp in values) {
                    if (string.IsNullOrWhiteSpace(kvp.Key))
                        continue;

                    int literal;
                    if (int.TryParse(kvp.Key, out literal) && literal >= 0 && literal < args.Length) {
                        args[literal] = kvp.Value;
                        ba[literal] = true;

                    } else {
                        int index;
                        if (namesToIndexes.TryGetValue(kvp.Key, out index)) {
                            args[index] = kvp.Value;
                            ba[index] = true;
                        }
                    }
                }

                // Remaining properties
                unboundValues = values.Where(t => !namesToIndexes.ContainsKey(t.Key));
            }

            for (int i = 0; i < args.Length; i++) {
                if (ba[i])
                    continue;

                object value = BindParameterSubscription(parms[i], serviceProvider);
                args[i] = value;
            }

            return args;
        }

        protected virtual object BindParameterSubscription(ParameterInfo parameter,
                                                           IServiceProvider serviceProvider) {
            object value = null;

            Type requiredType = parameter.ParameterType;
            if (requiredType.IsServiceType()) {
                value = serviceProvider.GetService(requiredType);
            }

            if (value == null && IsSubscriptionRequired(parameter))
                throw RuntimeFailure.RequiredSubscriptionConstructor(parameter.Member, parameter.Name);

            if (value != null)
                value = Adaptable.TryAdapt(value, requiredType, serviceProvider);

            return value;
        }

        static bool IsSubscriptionRequired(ParameterInfo pm) {
            SubscribeAttribute sa = (SubscribeAttribute) Attribute.GetCustomAttribute(pm, typeof(SubscribeAttribute));
			return (sa != null) && sa.Required;
        }

        internal void InitializeInternal(object component,
                                         IEnumerable<KeyValuePair<string, object>> values,
                                         IPopulateComponentCallback callback,
                                         IServiceProvider serviceProvider)
        {
            this.Initialize(component, values, callback, serviceProvider);
        }
    }
}
