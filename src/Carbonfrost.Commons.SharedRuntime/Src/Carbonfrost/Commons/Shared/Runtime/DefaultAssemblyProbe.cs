//
// - DefaultAssemblyProbe.cs -
//
// Copyright 2014 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.IO;
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Shared.Runtime {

    class DefaultAssemblyProbe : AssemblyProbe {

        // TODO Improve by looking at binding manifest

        public override IEnumerable<string> EnumerateAssemblyFiles() {
            try {
                string loc = Assembly.GetEntryAssembly().CodeBase;
                Uri u = new Uri(loc, UriKind.Absolute);
                string work = Path.GetDirectoryName(u.LocalPath);

                return Directory.EnumerateFiles(work, "*.dll");

            } catch {
                return Empty<string>.Array;
            }
        }

    }
}