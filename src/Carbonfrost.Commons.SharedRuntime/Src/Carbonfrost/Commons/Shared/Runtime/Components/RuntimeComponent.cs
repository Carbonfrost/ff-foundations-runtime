//
// - RuntimeComponent.cs -
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
using System.Reflection;

namespace Carbonfrost.Commons.Shared.Runtime.Components {

    public static class RuntimeComponent {

        public static Component AsComponent(this IRuntimeComponent source) {
            if (source == null)
                throw new ArgumentNullException("source");

            return new Component(source.ComponentName,
                                 source.ComponentType,
                                 source.Source);
        }

        public static Component AsComponent(this Assembly source) {
            if (source == null)
                throw new ArgumentNullException("source");

            return source.AsRuntimeComponent().AsComponent();
        }

        public static IRuntimeComponent AsRuntimeComponent(this Assembly source) {
            if (source == null)
                throw new ArgumentNullException("source");

            return AssemblyInfo.GetAssemblyInfo(source);
        }

        public static Uri ToComponentUri(this Assembly source) {
            if (source == null)
                throw new ArgumentNullException("source");

            return source.AsComponent().ToUri();
        }

        public static Uri ToComponentUri(this IRuntimeComponent source) {
            if (source == null)
                throw new ArgumentNullException("source");

            return source.AsComponent().ToUri();
        }

        public static IRuntimeComponent ReflectionOnlyLoad(string componentType, Uri source) {
            if (componentType == ComponentTypes.Anything)
                throw RuntimeFailure.ComponentTypeCannotBeAnything("componentType");

            var rcl = RuntimeComponentLoader.Create(componentType);
            return rcl.ReflectionOnlyLoad(componentType, source);
        }

        public static IRuntimeComponent Load(string componentType,
                                             Uri source) {
            if (source == null)
                throw new ArgumentNullException("source");

            if (componentType == ComponentTypes.Anything)
                throw RuntimeFailure.ComponentTypeCannotBeAnything("componentType");

            var rcl = RuntimeComponentLoader.Create(componentType);
            return rcl.Load(componentType, source);
        }

        public static Type GetRuntimeComponentType(string componentType) {
            if (componentType == null)
                throw new ArgumentNullException("componentType");

            if (componentType.Length == 0)
                throw Failure.EmptyString("componentType");

            if (componentType == ComponentTypes.Anything)
                throw RuntimeFailure.ComponentTypeCannotBeAnything("componentType");

            if (componentType == ComponentTypes.Assembly) {
                return typeof(AssemblyInfo);
            }

            return DescribeComponentTypes(AppDomain.CurrentDomain).GetValueOrDefault(componentType);
        }


        internal static IDictionary<string, Type> DescribeComponentTypes(AppDomain appDomain) {
            return appDomain.DescribeAssemblies(ComponentTypesSelector);
        }

        private static IEnumerable<KeyValuePair<string, Type>> ComponentTypesSelector(Assembly a) {
            foreach (var t in a.GetTypes()) {
                foreach (ComponentAttribute ca in Attribute.GetCustomAttributes(t, typeof(ComponentAttribute))) {
                    yield return new KeyValuePair<string, Type>(ca.ComponentType, t);
                }
            }
        }

    }
}
