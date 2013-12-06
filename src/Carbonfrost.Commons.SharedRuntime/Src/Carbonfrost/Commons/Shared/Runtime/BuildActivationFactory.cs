//
// - BuildActivationFactory.cs -
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
using System.Reflection;

using Carbonfrost.Commons.ComponentModel;

namespace Carbonfrost.Commons.Shared.Runtime {

    class BuildActivationFactory : ActivationFactory {

        public override object CreateInstance(Type type,
                                              IEnumerable<KeyValuePair<string, object>> values,
                                              IPopulateComponentCallback callback,
                                              IServiceProvider serviceProvider,
                                              params Attribute[] attributes)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            object result = null;
            Type builderType = Adaptable.GetBuilderType(type);

            if (builderType == null) {
                return Default.CreateInstance(type, values, callback, serviceProvider, attributes);

            } else {
                result = Default.CreateInstance(builderType, values, callback, serviceProvider, attributes);

                MethodInfo mi;
                result = Adaptable.InvokeBuilder(result, out mi, serviceProvider);
                InitializeCoreHelper(result, mi.ReturnType, values, callback, serviceProvider);
            }

            if (result == null)
                throw RuntimeFailure.CannotActivateNoConstructorOrBuilder("type", type);
            else
                return null;
        }

    }
}
