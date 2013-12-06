//
// - Component.Static.cs -
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
using System.Reflection;

namespace Carbonfrost.Commons.Shared.Runtime.Components {

    partial class Component {

        public static readonly Component Unknown = Anything(ComponentName.Unknown);


        public static Component Assembly(AssemblyName name, Uri source = null) {
            return Component.Assembly(ComponentName.FromAssemblyName(name),
                                      source);
        }

        public static Component Assembly(ComponentName name, Uri source = null) {
            return new Component(name, ComponentTypes.Assembly, source);
        }

        public static Component Anything(ComponentName name, Uri source = null) {
            return new Component(name, ComponentTypes.Anything, source);
        }

        public static Component FromUri(Uri uri) {
            if (uri == null)
                throw new ArgumentNullException("uri");

            Component result;
            Exception ex = _TryFromUri(uri, out result);
            if (ex == null)
                return result;
            else
                throw ex;
        }

    }
}
