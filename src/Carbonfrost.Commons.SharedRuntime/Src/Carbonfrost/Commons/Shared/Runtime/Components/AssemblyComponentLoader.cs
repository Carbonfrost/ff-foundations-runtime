//
// - AssemblyComponentLoader.cs -
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
using ReflectionAssembly = System.Reflection.Assembly;

namespace Carbonfrost.Commons.Shared.Runtime.Components {

    class AssemblyComponentLoader : RuntimeComponentLoader {

        public override IRuntimeComponent ReflectionOnlyLoad(string componentType, Uri source) {
            return LoadCore(componentType, source, ReflectionAssembly.ReflectionOnlyLoadFrom);
        }

        public override IRuntimeComponent Load(string componentType, Uri source) {
            return LoadCore(componentType, source, ReflectionAssembly.LoadFrom);
        }

        static AssemblyInfo LoadCore(string componentType, Uri source, Func<string, ReflectionAssembly> func) {
            string path = source.IsAbsoluteUri ? source.LocalPath : source.ToString();

            if (componentType == null)
                throw new ArgumentNullException("componentType");
            if (componentType.Length == 0)
                throw Failure.EmptyString("componentType");
            if (source == null)
                throw new ArgumentNullException("source");

            if (componentType != ComponentTypes.Assembly)
                return null;

            if (source.IsAbsoluteUri && source.Scheme != "file")
                throw RuntimeFailure.AssemblyComponentCanOnlyLoadLocalFile("source");

            return AssemblyInfo.GetAssemblyInfo(func(path));
        }
    }
}
