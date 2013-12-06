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

    class ProviderData {

        private readonly LookupBuffer<Type, ProviderValueSource> all;
        private readonly IEnumerable<Type> providerRoots;

        private static readonly AppDomainLocal<ProviderData> Instance
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

        internal static IEnumerable<Type> GetAll(AppDomain appDomain) {
            return Instance.GetValue(appDomain).providerRoots;
        }

        internal static T GetProvider<T>(
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
                .Where(t => string.Equals(t.Key.LocalName, localName, StringComparison.OrdinalIgnoreCase))
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
            return source.Where(t => t.Key.EqualsIgnoreCase(name));
        }

        internal static IEnumerable<Type> GetProviderTypes(AppDomain appDomain, Type providerType) {
            return GetProviderData(appDomain, providerType).Select(t => t.ValueType);
        }

        internal static IEnumerable<QualifiedName> GetProviderNames(AppDomain appDomain, Type providerType) {
            return GetProviderData(appDomain, providerType).Select(t => t.Key);
        }

        internal static IEnumerable<object> GetProviders(AppDomain appDomain, Type providerType) {
            return GetProviders<object>(appDomain, providerType, t => t.GetValue());
        }

        internal static IEnumerable<T> GetProviders<T>(AppDomain appDomain, Type providerType, Func<ProviderValueSource, T> selector) {
            return GetProviderData(appDomain, providerType).Select(selector);
        }

        static IEnumerable<ProviderValueSource> GetProviderData(AppDomain appDomain, Type providerType) {
            var a = Instance.GetValue(appDomain);
            if (!a.providerRoots.Contains(providerType)) {
                return Enumerable.Empty<ProviderValueSource>();
            }

            return a.all[providerType];
        }

        public static T GetProviderUsingCriteria<T>(AppDomain appDomain,
                                                    Type providerType,
                                                    object criteria,
                                                    Func<ProviderValueSource, T> selector)
        {
            return GetProviderData(appDomain, providerType)
                .Select(t => new { result = t, criteria = t.Metadata == null ? 0 : t.Metadata.MatchCriteria(criteria) })
                .OrderByDescending(t => t.criteria)
                .Where(t => t.criteria > 0)
                .Select(t => selector(t.result))
                .FirstOrDefault();
        }


        public static T GetProviderMetadata<T>(AppDomain appDomain,
                                               Type providerType,
                                               Func<ProviderValueSource, bool> filter,
                                               Func<ProviderValueSource, T> selector)
        {
            return GetProviderData(appDomain, providerType).Where(filter).Select(selector).FirstOrDefault();
        }
    }
}
