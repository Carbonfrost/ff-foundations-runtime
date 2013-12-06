//
// - AdapterRoleData.cs -
//
// Copyright 2005, 2006, 2010, 2012 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Linq.Expressions;
using System.Reflection;

namespace Carbonfrost.Commons.Shared.Runtime {

    sealed class AdapterRoleData {

        private readonly SortedList<string, Type> AdapterMethods;
        private readonly MethodInfo validTest;

        private static readonly AppDomainLocal<IDictionary<string, AdapterRoleData>> allAdapterRoles
            = new AppDomainLocal<IDictionary<string, AdapterRoleData>>(CreateData);

        public AdapterRoleData(IEnumerable<FieldInfo> adapterMethods,
                               MethodInfo validTest)
        {
            this.AdapterMethods = new SortedList<string, Type>();

            if (adapterMethods != null) {
                foreach (var f in adapterMethods) {
                    // Get the delegate types from expression
                    if (f.FieldType.IsGenericType
                        && f.FieldType.GetGenericTypeDefinition() == typeof(Expression<>)) {
                        this.AdapterMethods.Add(f.Name,
                                                f.FieldType.GetGenericArguments()[0]);
                    }
                }
            }

            this.validTest = validTest;
        }

        internal static IEnumerable<KeyValuePair<string, AdapterRoleData>> FromStaticClass(Type adapterRoleClass) {
            foreach (FieldInfo field in adapterRoleClass.GetFields()) {
                if (!field.IsStatic)
                    continue;

                IEnumerable<FieldInfo> adapterMethods = null;
                MethodInfo validMethod = adapterRoleClass.GetMethod(string.Format("Is{0}Type", field.Name),
                                                                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                var functionDeclarator = adapterRoleClass.GetNestedType(field.Name + "Functions");
                if (functionDeclarator != null) {
                    adapterMethods = functionDeclarator.GetFields();
                }

                var result = new AdapterRoleData(adapterMethods, validMethod);
                yield return new KeyValuePair<string, AdapterRoleData>(field.Name, result);
            }
        }

        public MethodInfo FindAdapterMethod(Type implicitAdapterType, string method) {
            Type type;
            if (this.AdapterMethods.TryGetValue(method, out type)) {
                Type[] argumentTypes = type.GetGenericArguments();
                Type requiredReturnType = null;

                // We only expect Action and Func in expression trees
                if (type.GetGenericTypeDefinition().FullName.StartsWith("System.Func`", StringComparison.Ordinal)) {
                    requiredReturnType = argumentTypes.Last();
                    argumentTypes = argumentTypes.Take(argumentTypes.Length - 1).ToArray();
                }

                MethodInfo result = implicitAdapterType.GetMethod(method, argumentTypes);
                if (result != null && requiredReturnType != null) {
                    return requiredReturnType.IsAssignableFrom(result.ReturnType) ? result : null;
                }

                return result;
            }

            return null;
        }

        public bool IsValidAdapter(Type adapterType) {
            if (validTest == null)
                return true;

            return (bool) validTest.Invoke(null, new [] { adapterType });
        }

        public static AdapterRoleData FromName(string adapterName) {
            AdapterRoleData result;
            if (allAdapterRoles.Value.TryGetValue(adapterName, out result))
                return result;
            else
                return null;
        }

        public static IEnumerable<KeyValuePair<string, AdapterRoleData>> FromAssembly(Assembly assembly) {
            if (!AssemblyInfo.GetAssemblyInfo(assembly).ScanForAdapters) {
                return Enumerable.Empty<KeyValuePair<string, AdapterRoleData>>();
            }

            // Lookup AdapterRole static class
            return assembly.GetTypes()
                .Where(t => t.Name == "AdapterRole").SelectMany(t => FromStaticClass(t));
        }

        static IDictionary<string, AdapterRoleData> CreateData() {
			return AppDomain.CurrentDomain.DescribeAssemblies(FromAssembly);
        }
    }
}
