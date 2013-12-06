//
// - UriContextActivationProvider.cs -
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
using System.ComponentModel;
using Carbonfrost.Commons.ComponentModel;

namespace Carbonfrost.Commons.Shared.Runtime {

    [ReusableAttribute]
    sealed class UriContextActivationProvider : IActivationProvider {

        // IActivationProvider implementation
        public object ActivateComponent(IServiceProvider serviceProvider, object component) {
            IUriContext uc = component as IUriContext;
            if (uc != null) {
                IUriContext sourceContext = Utility.FindServiceOrInheritanceAncestor<IUriContext>(component, serviceProvider);

                if (sourceContext != null) {
                    Uri u = sourceContext.BaseUri;
                    if (u != null)
                        uc.BaseUri = u;
                }
            }

            return component;
        }

        public void ActivateProperty(IServiceProvider serviceProvider, object component, PropertyDescriptor property) {

        }

    }
}
