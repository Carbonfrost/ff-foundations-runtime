//
// - DefaultActivationFactory.cs -
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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Carbonfrost.Commons.ComponentModel;

namespace Carbonfrost.Commons.Shared.Runtime {

    class DefaultActivationFactory : ActivationFactory {

        public override object CreateInstance(Type type,
                                              IEnumerable<KeyValuePair<string, object>> values,
                                              IPopulateComponentCallback callback,
                                              IServiceProvider serviceProvider,
                                              params Attribute[] attributes) {
            if (type == null)
                throw new ArgumentNullException("type");
            attributes = attributes ?? Empty<Attribute>.Array;

            // TODO Implement process and appdomain isolation, reusable
            if (type.IsProcessIsolated() || attributes.Any(t => t is ProcessIsolatedAttribute)) {

            } else if (type.IsAppDomainIsolated() || attributes.Any(t => t is AppDomainIsolatedAttribute)) {

            }

            if (type.IsReusable() || attributes.Any(t => t is ReusableAttribute)) {
            }

            return base.CreateInstance(type, values, callback, serviceProvider, attributes);
        }
    }

}
