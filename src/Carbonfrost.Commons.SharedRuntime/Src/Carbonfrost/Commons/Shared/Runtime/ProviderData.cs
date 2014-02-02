//
// - ProviderData.cs -
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

    class ProviderData : IProviderInfoDescription {

        private readonly LookupBuffer<Type, ProviderValueSource> all;
        private readonly IEnumerable<Type> providerRoots;

        internal static readonly AppDomainLocal<ProviderData> Instance
            = new AppDomainLocal<ProviderData>(() => new ProviderData());

        private ProviderData() {
            IEnumerable<ProviderValueSource> providers;

            var appDomain = AppDomain.CurrentDomain;
            providers = appDomain.DescribeAssemblies(
                t => ExtractFromTypes(t));

            providerRoots = appDomain.DescribeAssemblies(
                t => GetRootProviderTypes(t));

            all = new LookupBuffer<Type, ProviderValueSource>(
                t => t.ProviderType, providers);
        }

        private static IEnumerable<ProviderValueSource> ExtractFromTypes(Assembly a) {
            IProviderRegistration registration = AssemblyInfo.GetAssemblyInfo(a).GetProviderRegistration();
            ProviderRegistrationContext context = new ProviderRegistrationContext(a);
            registration.RegisterProviderTypes(context);

            return context.EnumerateValueSources();
        }

        static IEnumerable<Type> GetRootProviderTypes(Assembly a) {
            IProviderRegistration registration = AssemblyInfo.GetAssemblyInfo(a).GetProviderRegistration();
            ProviderRegistrationContext context = new ProviderRegistrationContext(a);
            registration.RegisterRootProviderTypes(context);

            return context.EnumerateRoots();
        }

        private static IEnumerable<Type> GetAll(AppDomain appDomain) {
            return Instance.GetValue(appDomain).providerRoots;
        }

        private static T GetProvider<T>(
            AppDomain appDomain,
            Type providerType,
            QualifiedName name,
            Func<ProviderValueSource, T> selector) {

            return WhereByName(GetProviderData(appDomain, providerType), name)
                .Select(selector)
                .FirstOrDefault();
        }

        internal static IEnumerable<T> GetProvidersByLocalName<T>(
            AppDomain appDomain,
            Type providerType,
            string localName,
            Func<ProviderValueSource, T> selector) {

            // TODO Inheritance -- Get by IAdapterFactory should return all derived ones

            return GetProviderData(appDomain, providerType)
                .Where(t => string.Equals(t.Name.LocalName, localName, StringComparison.OrdinalIgnoreCase))
                .Select(selector);
        }

        internal static Type GetProviderType(
            AppDomain appDomain,
            Type providerType,
            QualifiedName name) {

            // TODO Convert from linear time to O(1) if possible
            var r = WhereByName(GetProviderData(appDomain, providerType), name).FirstOrDefault();
            if (r == null)
                return null;

            return r.ValueType;
        }

        static IEnumerable<ProviderValueSource> WhereByName(IEnumerable<ProviderValueSource> source, QualifiedName name) {
            return source.Where(t => t.Name.EqualsIgnoreCase(name));
        }

        internal static IEnumerable<Type> GetProviderTypes(AppDomain appDomain, Type providerType) {
            return GetProviderData(appDomain, providerType).Select(t => t.ValueType);
        }

        internal static IEnumerable<QualifiedName> GetProviderNames(AppDomain appDomain, Type providerType) {
            return GetProviderData(appDomain, providerType).Select(t => t.Name);
        }

        private static IEnumerable<object> GetProviders(AppDomain appDomain, Type providerType) {
            return GetProviders<object>(appDomain, providerType, t => t.GetValue());
        }

        private static IEnumerable<T> GetProviders<T>(AppDomain appDomain, Type providerType, Func<ProviderValueSource, T> selector) {
            return GetProviderData(appDomain, providerType).Select(selector);
        }

        static IEnumerable<ProviderValueSource> GetProviderData(AppDomain appDomain, Type providerType) {
            var a = Instance.GetValue(appDomain);
            if (!a.providerRoots.Contains(providerType)) {
                return Enumerable.Empty<ProviderValueSource>();
            }

            return a.all[providerType];
        }

        private static IEnumerable<T> GetProvidersUsingCriteria<T>(AppDomain appDomain,
                                                                   Type providerType,
                                                                   object criteria,
                                                                   Func<ProviderValueSource, T> selector)
        {
            return GetProviderData(appDomain, providerType)
                .Select(t => new { result = t, criteria = t.Metadata == null ? 0 : t.Metadata.MatchCriteria(criteria) })
                .OrderByDescending(t => t.criteria)
                .Where(t => t.criteria > 0)
                .Select(t => selector(t.result));
        }

        private static T GetProviderMetadata<T>(AppDomain appDomain,
                                                Type providerType,
                                                Func<ProviderValueSource, bool> filter,
                                                Func<ProviderValueSource, T> selector)
        {
            return GetProviderData(appDomain, providerType).Where(filter).Select(selector).FirstOrDefault();
        }

        // IProviderInfoDescription implementation

        IEnumerable<MemberInfo> IProviderInfoDescription.GetProviderMembers(Type providerType) {
            return ProviderData.GetProviders(AppDomain.CurrentDomain, providerType, t => t.Member);
        }

        IEnumerable<object> IProviderInfoDescription.GetProviders(Type providerType) {
            if (providerType == null)
                throw new ArgumentNullException("providerType");

            return ProviderData.GetProviders(AppDomain.CurrentDomain, providerType);
        }

        IEnumerable<object> IProviderInfoDescription.GetProviders(Type providerType, object criteria) {
            if (providerType == null)
                throw new ArgumentNullException("providerType");

            return ProviderData.GetProvidersUsingCriteria(AppDomain.CurrentDomain, providerType, criteria, t => t.GetValue());
        }

        IEnumerable<QualifiedName> IProviderInfoDescription.GetProviderNames(Type providerType) {
            if (providerType == null)
                throw new ArgumentNullException("providerType");

            return ProviderData.GetProviderNames(AppDomain.CurrentDomain, providerType);
        }

        IEnumerable<QualifiedName> IProviderInfoDescription.GetTemplateNames(Type templateType) {
            if (templateType == null)
                throw new ArgumentNullException("templateType");

            return TemplateData.GetTemplateNames(AppDomain.CurrentDomain, templateType);
        }

        IEnumerable<T> IProviderInfoDescription.GetProviders<T>() {
            return ProviderData.GetProviders(AppDomain.CurrentDomain, typeof(T)).Cast<T>();
        }

        IEnumerable<T> IProviderInfoDescription.GetProviders<T>(object criteria) {
            return ((IProviderInfoDescription) this).GetProviders(typeof(T), criteria).Cast<T>();
        }

        IEnumerable<Type> IProviderInfoDescription.GetProviderTypes() {
            return ProviderData.GetAll(AppDomain.CurrentDomain);
        }

        IEnumerable<Type> IProviderInfoDescription.GetProviderTypes(Type providerType) {
            return ProviderData.GetProviderTypes(AppDomain.CurrentDomain, providerType);
        }

        MemberInfo IProviderInfoDescription.GetProviderMember(Type providerType, QualifiedName name) {
            return ProviderData.GetProvider(AppDomain.CurrentDomain, providerType, name, t => t.Member);
        }

        MemberInfo IProviderInfoDescription.GetProviderMember(Type providerType, string name) {
            return ProviderData.GetProvidersByLocalName(AppDomain.CurrentDomain, providerType, name, t => t.Member).SingleOrThrow(RuntimeFailure.MultipleProviders);
        }

        object IProviderInfoDescription.GetProvider(Type providerType, object criteria) {
            var appDomain = AppDomain.CurrentDomain;
            if (providerType == null)
                throw new ArgumentNullException("providerType");
            if (criteria == null)
                throw new ArgumentNullException("criteria");

            string s = criteria as string;
            if (s != null)
                return ((IProviderInfoDescription) this).GetProvider(providerType, s);

            QualifiedName t = criteria as QualifiedName;
            if (t != null)
                return ((IProviderInfoDescription) this).GetProvider(providerType, t);

            return ProviderData.GetProvidersUsingCriteria(appDomain, providerType, criteria, u => u.GetValue()).FirstOrDefault();
        }

        object IProviderInfoDescription.GetProvider(Type providerType, QualifiedName name) {
            if (providerType == null)
                throw new ArgumentNullException("providerType");

            return ProviderData.GetProvider(AppDomain.CurrentDomain, providerType, name, t => t.GetValue());
        }

        object IProviderInfoDescription.GetProvider(Type providerType, string name) {
            if (providerType == null)
                throw new ArgumentNullException("providerType");

            return ProviderData.GetProvidersByLocalName(AppDomain.CurrentDomain, providerType, name, t => t.GetValue()).SingleOrThrow(RuntimeFailure.MultipleProviders);
        }

        object IProviderInfoDescription.GetProviderMetadata(Type providerType, object instance) {
            if (providerType == null)
                throw new ArgumentNullException("providerType");
            if (instance == null)
                throw new ArgumentNullException("instance");

            return ProviderData.GetProviderMetadata(AppDomain.CurrentDomain, providerType, t => object.ReferenceEquals(t.GetValue(), instance), t => t.Metadata);
        }

        QualifiedName IProviderInfoDescription.GetProviderName(Type providerType, object instance) {
            if (providerType == null)
                throw new ArgumentNullException("providerType");
            if (instance == null)
                throw new ArgumentNullException("instance");

            return ProviderData.GetProviderMetadata(AppDomain.CurrentDomain, providerType, t => object.ReferenceEquals(t.Member, instance) || t.IsValue(instance), t => t.Name);
        }

        T IProviderInfoDescription.GetProvider<T>(object criteria) {
            return (T) AppDomain.CurrentDomain.GetProvider(typeof(T), criteria);
        }

        T IProviderInfoDescription.GetProvider<T>(QualifiedName name) {
            return (T) ProviderData.GetProvider(AppDomain.CurrentDomain, typeof(T), name, t => t.GetValue());
        }

        Type IProviderInfoDescription.GetProviderType(Type providerType, object criteria) {
            if (criteria == null)
                throw new ArgumentNullException("criteria");

            var appDomain = AppDomain.CurrentDomain;
            string s = criteria as string;
            if (s != null)
                return ((IProviderInfoDescription) this).GetProviderType(providerType, s);

            QualifiedName t = criteria as QualifiedName;
            if (t != null)
                return ((IProviderInfoDescription) this).GetProviderType(providerType, t);

            return ProviderData.GetProvidersUsingCriteria(appDomain, providerType, criteria, u => u.ValueType).FirstOrDefault();
        }

        Type IProviderInfoDescription.GetProviderType(Type providerType, QualifiedName name) {
            return ProviderData.GetProviderType(AppDomain.CurrentDomain, providerType, name);
        }

        Type IProviderInfoDescription.GetProviderType(Type providerType, string name) {
            return ProviderData.GetProvidersByLocalName(AppDomain.CurrentDomain, providerType, name, t => t.ValueType).SingleOrThrow(RuntimeFailure.MultipleProviders);
        }

        T IProviderInfoDescription.GetProvider<T>(string name) {
            return (T) ProviderData.GetProvidersByLocalName(
                AppDomain.CurrentDomain, typeof(T), name, t => t.GetValue()).SingleOrThrow(RuntimeFailure.MultipleProviders);
        }

        IProviderInfo IProviderInfoDescription.GetProviderInfo(Type type, QualifiedName name) {
            return ProviderData.GetProvider(AppDomain.CurrentDomain, type, name, t => t);
        }

        IEnumerable<IProviderInfo> IProviderInfoDescription.GetProviderInfos(Type providerType) {
            return ProviderData.GetProviders(AppDomain.CurrentDomain, providerType, t => t);
        }
    }
}
