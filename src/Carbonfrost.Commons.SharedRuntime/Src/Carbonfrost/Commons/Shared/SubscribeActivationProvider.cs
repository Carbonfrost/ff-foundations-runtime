//
// - SubscribeActivationProvider.cs -
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
using System.ComponentModel;
using System.Reflection;

using Carbonfrost.Commons.Shared.Runtime;

namespace Carbonfrost.Commons.Shared {

    [ReusableAttribute]
    sealed class SubscribeActivationProvider : IActivationProvider {

        internal static readonly SubscribeActivationProvider Instance
            = new SubscribeActivationProvider();

        // TODO Activation of subscribed properties

        public object ActivateComponent(IServiceProvider serviceProvider, object component) {
            if (component == null)
                return null;

            Type type = component.GetType();

            // Optimization - ignore builtin assemblies
            while (type != null
                   && AssemblyInfo.GetAssemblyInfo(type.Assembly).Scannable) {

                foreach (var field in type.GetFields(
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)) {

                    var sa = Attribute.GetCustomAttribute(field, typeof(SubscribeAttribute)) as SubscribeAttribute;

                    // TODO Named services
                    if (sa != null) {
                        field.SetValue(component, serviceProvider.GetService(sa.Type ?? field.FieldType));
                    }
                }

                type = type.BaseType;
            }

            return component;
        }

        public void ActivateProperty(IServiceProvider serviceProvider, object component, PropertyDescriptor property) {}

    }
}
