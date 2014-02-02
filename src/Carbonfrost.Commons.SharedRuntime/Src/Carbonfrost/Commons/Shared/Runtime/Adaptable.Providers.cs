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

        public static IProviderInfoDescription DescribeProviders(this AppDomain appDomain) {
            if (appDomain == null)
                throw new ArgumentNullException("appDomain");

            return ProviderData.Instance.GetValue(appDomain);
        }

        public static T GetProvider<T>(this AppDomain appDomain, string name) {
            return DescribeProviders(appDomain).GetProvider<T>(name);
        }

        public static T GetProvider<T>(this AppDomain appDomain, QualifiedName name) {
            return DescribeProviders(appDomain).GetProvider<T>(name);
        }

        public static T GetProvider<T>(this AppDomain appDomain, object criteria) {
            return DescribeProviders(appDomain).GetProvider<T>(criteria);
        }

        public static IEnumerable<T> GetProviders<T>(this AppDomain appDomain, object criteria) {
            return DescribeProviders(appDomain).GetProviders<T>(criteria);
        }

        public static object GetProvider(this AppDomain appDomain, Type providerType, string name) {
            return DescribeProviders(appDomain).GetProvider(providerType, name);
        }

        public static object GetProvider(this AppDomain appDomain, Type providerType, QualifiedName name) {
            return DescribeProviders(appDomain).GetProvider(providerType, name);
        }

        public static object GetProvider(this AppDomain appDomain, Type providerType, object criteria) {
            return DescribeProviders(appDomain).GetProvider(providerType, criteria);
        }

        public static IEnumerable<T> GetProviders<T>(this AppDomain appDomain) {
            return DescribeProviders(appDomain).GetProviders<T>();
        }

        public static IEnumerable<object> GetProviders(this AppDomain appDomain, Type providerType) {
            return DescribeProviders(appDomain).GetProviders(providerType);
        }

        public static IEnumerable<object> GetProviders(this AppDomain appDomain, Type providerType, object criteria) {
            return DescribeProviders(appDomain).GetProviders(providerType, criteria);
        }

        public static IEnumerable<Type> GetProviderTypes(this AppDomain appDomain) {
            return DescribeProviders(appDomain).GetProviderTypes();
        }

        public static MemberInfo GetProviderMember(this AppDomain appDomain, Type providerType, string name) {
            return DescribeProviders(appDomain).GetProviderMember(providerType, name);
        }

        public static MemberInfo GetProviderMember(this AppDomain appDomain, Type providerType, QualifiedName name) {
            return DescribeProviders(appDomain).GetProviderMember(providerType, name);
        }

        public static IEnumerable<MemberInfo> GetProviderMembers(this AppDomain appDomain, Type providerType) {
            return DescribeProviders(appDomain).GetProviderMembers(providerType);
        }

        public static Type GetProviderType(this AppDomain appDomain, Type providerType, string name) {
            return DescribeProviders(appDomain).GetProviderType(providerType, name);
        }

        public static Type GetProviderType(this AppDomain appDomain, Type providerType, QualifiedName name) {
            return DescribeProviders(appDomain).GetProviderType(providerType, name);
        }

        public static Type GetProviderType(this AppDomain appDomain, Type providerType, object criteria) {
            return DescribeProviders(appDomain).GetProviderType(providerType, criteria);
        }

        public static IEnumerable<Type> GetProviderTypes(this AppDomain appDomain, Type providerType) {
            return DescribeProviders(appDomain).GetProviderTypes(providerType);
        }

        public static bool IsProviderType(this Type providerType) {
            if (providerType == null)
                throw new ArgumentNullException("providerType");

            return AppDomain.CurrentDomain.GetProviderTypes().Contains(providerType);
        }

        public static QualifiedName GetProviderName(this AppDomain appDomain, Type providerType, object instance) {
            return DescribeProviders(appDomain).GetProviderName(providerType, instance);
        }

        public static object GetProviderMetadata(this AppDomain appDomain, Type providerType, object instance) {
            return DescribeProviders(appDomain).GetProviderMetadata(providerType, instance);
        }

        public static IEnumerable<QualifiedName> GetProviderNames(this AppDomain appDomain, Type providerType) {
            return DescribeProviders(appDomain).GetProviderNames(providerType);
        }

        public static IEnumerable<QualifiedName> GetTemplateNames(this AppDomain appDomain, Type templateType) {
            return DescribeProviders(appDomain).GetTemplateNames(templateType);
        }

    }
}
