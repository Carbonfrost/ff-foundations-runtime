//
// - Adaptable.Providers.cs -
//
// Copyright 2012, 2013 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    public static partial class Adaptable {

        public static T GetProvider<T>(this AppDomain appDomain, string name) {
            if (appDomain == null)
                throw new ArgumentNullException("appDomain");

            return (T) ProviderData.GetProvidersByLocalName(
                appDomain, typeof(T), name, t => t.GetValue()).SingleOrThrow(RuntimeFailure.MultipleProviders);
        }

        public static T GetProvider<T>(this AppDomain appDomain, QualifiedName name) {
            if (appDomain == null)
                throw new ArgumentNullException("appDomain");

            return (T) ProviderData.GetProvider(appDomain, typeof(T), name, t => t.GetValue());
        }

        public static T GetProvider<T>(this AppDomain appDomain, object criteria) {
            return (T) appDomain.GetProvider(typeof(T), criteria);
        }

        public static object GetProvider(this AppDomain appDomain, Type providerType, string name) {
            if (appDomain == null)
                throw new ArgumentNullException("appDomain");

            if (providerType == null)
                throw new ArgumentNullException("providerType");

            return ProviderData.GetProvidersByLocalName(appDomain, providerType, name, t => t.GetValue()).SingleOrThrow(RuntimeFailure.MultipleProviders);
        }

        public static object GetProvider(this AppDomain appDomain, Type providerType, QualifiedName name) {
            if (appDomain == null)
                throw new ArgumentNullException("appDomain");

            if (providerType == null)
                throw new ArgumentNullException("providerType");

            return ProviderData.GetProvider(appDomain, providerType, name, t => t.GetValue());
        }

        public static object GetProvider(this AppDomain appDomain, Type providerType, object criteria) {
            if (appDomain == null)
                throw new ArgumentNullException("appDomain");
            if (providerType == null)
                throw new ArgumentNullException("providerType");
            if (criteria == null)
                throw new ArgumentNullException("criteria");

            string s = criteria as string;
            if (s != null)
                return GetProvider(appDomain, providerType, s);

            QualifiedName t = criteria as QualifiedName;
            if (t != null)
                return GetProvider(appDomain, providerType, t);

            return ProviderData.GetProviderUsingCriteria(appDomain, providerType, criteria, u => u.GetValue());
        }

        public static IEnumerable<T> GetProviders<T>(this AppDomain appDomain) {
            return ProviderData.GetProviders(appDomain, typeof(T)).Cast<T>();
        }

        public static IEnumerable<object> GetProviders(this AppDomain appDomain, Type providerType) {
            if (providerType == null)
                throw new ArgumentNullException("providerType");

            return ProviderData.GetProviders(appDomain, providerType);
        }

        public static IEnumerable<Type> GetProviderTypes(this AppDomain appDomain) {
            return ProviderData.GetAll(appDomain);
        }

        public static MemberInfo GetProviderMember(this AppDomain appDomain, Type providerType, string name) {
            return ProviderData.GetProvidersByLocalName(appDomain, providerType, name, t => t.Member).SingleOrThrow(RuntimeFailure.MultipleProviders);
        }

        public static MemberInfo GetProviderMember(this AppDomain appDomain, Type providerType, QualifiedName name) {
            return ProviderData.GetProvider(appDomain, providerType, name, t => t.Member);
        }

        public static IEnumerable<MemberInfo> GetProviderMembers(this AppDomain appDomain, Type providerType) {
            return ProviderData.GetProviders(appDomain, providerType, t => t.Member);
        }

        public static Type GetProviderType(this AppDomain appDomain, Type providerType, string name) {
            return ProviderData.GetProvidersByLocalName(appDomain, providerType, name, t => t.ValueType).SingleOrThrow(RuntimeFailure.MultipleProviders);
        }

        public static Type GetProviderType(this AppDomain appDomain, Type providerType, QualifiedName name) {
            return ProviderData.GetProviderType(appDomain, providerType, name);
        }

        public static IEnumerable<Type> GetProviderTypes(this AppDomain appDomain, Type providerType) {
            if (providerType == null)
                throw new ArgumentNullException("providerType");

            return ProviderData.GetProviderTypes(appDomain, providerType);
        }

        public static bool IsProviderType(this Type providerType) {
            if (providerType == null)
                throw new ArgumentNullException("providerType");

            return AppDomain.CurrentDomain.GetProviderTypes().Contains(providerType);
        }

        public static QualifiedName GetProviderName(this AppDomain appDomain, Type providerType, object instance) {
            if (providerType == null)
                throw new ArgumentNullException("providerType");
            if (instance == null)
                throw new ArgumentNullException("instance");

            return ProviderData.GetProviderMetadata(appDomain, providerType, t => object.ReferenceEquals(t.Member, instance) || t.IsValue(instance), t => t.Key);
        }

        public static object GetProviderMetadata(this AppDomain appDomain, Type providerType, object instance) {
            if (providerType == null)
                throw new ArgumentNullException("providerType");
            if (instance == null)
                throw new ArgumentNullException("instance");

            return ProviderData.GetProviderMetadata(appDomain, providerType, t => object.ReferenceEquals(t.GetValue(), instance), t => t.Metadata);
        }

        public static IEnumerable<QualifiedName> GetProviderNames(this AppDomain appDomain, Type providerType) {
            if (providerType == null)
                throw new ArgumentNullException("providerType");

            return ProviderData.GetProviderNames(appDomain, providerType);
        }

        public static IEnumerable<QualifiedName> GetTemplateNames(this AppDomain appDomain, Type templateType) {
            if (templateType == null)
                throw new ArgumentNullException("templateType");

            return TemplateData.GetTemplateNames(appDomain, templateType);
        }

    }
}
