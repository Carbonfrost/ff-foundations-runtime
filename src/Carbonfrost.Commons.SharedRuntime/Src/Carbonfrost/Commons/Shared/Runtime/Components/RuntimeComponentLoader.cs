//
// - RuntimeComponentLoader.cs -
//
// Copyright 2013 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

namespace Carbonfrost.Commons.Shared.Runtime.Components {

    [Providers]
    public abstract class RuntimeComponentLoader {

        public static readonly RuntimeComponentLoader Any
            = new AnyRuntimeComponentLoader();

        [RuntimeComponentLoaderUsage(ComponentTypes = "assembly")]
        public static readonly RuntimeComponentLoader Assembly
            = new AssemblyComponentLoader();

        public virtual IRuntimeComponent ReflectionOnlyLoad(string componentType,
                                                            Uri source) {
            return Load(componentType, source);
        }

        public abstract IRuntimeComponent Load(string componentType, Uri source);

        public static RuntimeComponentLoader Create(string componentType) {
            if (componentType == null)
                throw new ArgumentNullException("componentType");
            if (componentType.Length == 0)
                throw Failure.EmptyString("componentType");

            return AppDomain.CurrentDomain.GetProvider<RuntimeComponentLoader>(new { ComponentType = componentType });
        }

        public static RuntimeComponentLoader FromName(string name) {
            if (name == null)
                throw new ArgumentNullException("name");
            if (name.Length == 0)
                throw Failure.EmptyString("name");

            return AppDomain.CurrentDomain.GetProvider<RuntimeComponentLoader>(name);
        }
    }
}
