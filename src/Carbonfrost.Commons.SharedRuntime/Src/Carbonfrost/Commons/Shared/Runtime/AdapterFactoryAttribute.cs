//
// - AdapterFactoryAttribute.cs -
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

namespace Carbonfrost.Commons.Shared.Runtime {

    [AttributeUsage(AttributeTargets.Assembly)]
    public class AdapterFactoryAttribute : Attribute {

        public Type AdapterFactoryType { get; private set; }
        public string Role { get; private set; }

        public AdapterFactoryAttribute(Type adapterFactoryType) {
            this.AdapterFactoryType = adapterFactoryType;
        }

        public AdapterFactoryAttribute(string role, Type adapterFactoryType) {
            if (role == null)
                throw new ArgumentNullException("role");
            if (role.Length == 0)
                throw Failure.EmptyString("role");
            if (adapterFactoryType == null)
                throw new ArgumentNullException("adapterFactoryType");

            this.AdapterFactoryType = adapterFactoryType;
            this.Role = role;
        }

    }

}

