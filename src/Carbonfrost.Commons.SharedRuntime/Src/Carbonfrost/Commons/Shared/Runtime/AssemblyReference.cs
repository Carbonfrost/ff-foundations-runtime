//
// - AssemblyReference.cs -
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

namespace Carbonfrost.Commons.Shared.Runtime {

    [Builder(typeof(AssemblyReferenceBuilder))]
    public sealed class AssemblyReference {

        public Uri Source { get; private set; }
        public AssemblyName AssemblyName { get; private set; }
        public bool Defer { get; private set; }

        public AssemblyReference(AssemblyName assemblyName,
                                 Uri source,
                                 bool defer) {
            if (assemblyName == null)
                throw new ArgumentNullException("assemblyName");

            this.Source = source;
            this.AssemblyName = assemblyName;
            this.Defer = defer;
        }

        public AssemblyReference(AssemblyName assemblyName, Uri source) : this(assemblyName, source, false) {
        }
    }
}
