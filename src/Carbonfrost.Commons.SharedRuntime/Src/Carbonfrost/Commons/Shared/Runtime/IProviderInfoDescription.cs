//
// - IProviderInfoDescription.cs -
//
// Copyright 2014 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    public interface IProviderInfoDescription {

        IProviderInfo GetProviderInfo(Type providerType, QualifiedName name);
        IEnumerable<IProviderInfo> GetProviderInfos(Type providerType);

        IEnumerable<MemberInfo> GetProviderMembers(Type providerType);
        IEnumerable<object> GetProviders(Type providerType);
        IEnumerable<object> GetProviders(Type providerType, object criteria);
        IEnumerable<QualifiedName> GetProviderNames(Type providerType);
        IEnumerable<QualifiedName> GetTemplateNames(Type templateType);
        IEnumerable<T> GetProviders<T>();
        IEnumerable<T> GetProviders<T>(object criteria);
        IEnumerable<Type> GetProviderTypes();
        IEnumerable<Type> GetProviderTypes(Type providerType);
        MemberInfo GetProviderMember(Type providerType, QualifiedName name);
        MemberInfo GetProviderMember(Type providerType, string name);
        MemberInfo GetProviderMember(Type providerType, object criteria);
        object GetProvider(Type providerType, object criteria);
        object GetProvider(Type providerType, QualifiedName name);
        object GetProvider(Type providerType, string name);
        object GetProviderMetadata(Type providerType, object instance);
        QualifiedName GetProviderName(Type providerType, object instance);
        T GetProvider<T>(object criteria);
        T GetProvider<T>(QualifiedName name);
        Type GetProviderType(Type providerType, object criteria);
        Type GetProviderType(Type providerType, QualifiedName name);
        Type GetProviderType(Type providerType, string name);
        T GetProvider<T>(string name);
    }
}
