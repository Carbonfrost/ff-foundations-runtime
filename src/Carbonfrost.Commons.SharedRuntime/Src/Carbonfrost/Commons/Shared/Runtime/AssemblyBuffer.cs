//
// - AssemblyBuffer.cs -
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
using System.IO;
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Shared.Runtime {

    class AssemblyBuffer : Buffer<Assembly> {

        private AssemblyBuffer() : base(EnumerateAppDomain()) {
        }

        public static readonly AssemblyBuffer Instance
            = new AssemblyBuffer();

        private static void AddReferencedAssemblies(Assembly assembly,
                                                    HashSet<AssemblyName> pendingAssemblyNames) {
            foreach (AssemblyName m in assembly.GetReferencedAssemblies()) {
                pendingAssemblyNames.Add(m);
            }
        }

        private static IEnumerable<Assembly> EnumerateAppDomain() {
            var comparer = new AssemblyNameComparer();
            var assemblies = new Dictionary<AssemblyName, Assembly>(comparer);
            var pendingAssemblyNames = new HashSet<AssemblyName>(comparer);

            var domain = AppDomain.CurrentDomain;
            domain.AssemblyLoad += (_, args) => {
                pendingAssemblyNames.Add(args.LoadedAssembly.GetName());
                AddReferencedAssemblies(args.LoadedAssembly, pendingAssemblyNames);
            };

            foreach (var assembly in domain.GetAssemblies()) {
                AddReferencedAssemblies(assembly, pendingAssemblyNames);

                if (!assemblies.ContainsKey(assembly.GetName())) {
                    assemblies.Add(assembly.GetName(), assembly);
                    yield return assembly;
                }
            }

            while (pendingAssemblyNames.Count > 0) {
                AssemblyName any = pendingAssemblyNames.First();
                pendingAssemblyNames.Remove(any);

                if (assemblies.ContainsKey(any))
                    continue;

                Exception error = null;

                Assembly assembly = null;
                try {
                    assembly = domain.Load(any);
                    AddReferencedAssemblies(assembly, pendingAssemblyNames);

                } catch (FileNotFoundException ex) {
                    error = ex;
                } catch (FileLoadException ex) {
                    error = ex;
                } catch (BadImageFormatException ex) {
                    error = ex;
                }

                if (error != null) {
                    IStatusAppender sa = StatusAppender.ForType(typeof(AssemblyInfo));
                    sa.AppendError(error);

                } else {
                    assemblies.Add(any, assembly);
                    yield return assembly;
                }
            }
        }

        sealed class AssemblyNameComparer : IEqualityComparer<AssemblyName> {

            public bool Equals(AssemblyName x, AssemblyName y) {
                return x.FullName.Equals(y.FullName);
            }

            public int GetHashCode(AssemblyName obj) {
                return obj.FullName.GetHashCode();
            }
        }
    }
}
