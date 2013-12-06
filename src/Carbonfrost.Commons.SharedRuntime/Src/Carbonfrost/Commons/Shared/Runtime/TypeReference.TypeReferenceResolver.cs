//
// - TypeReference.TypeReferenceResolver.cs -
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
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Shared.Runtime {

    partial class TypeReference {

        abstract class TypeReferenceResolver {
            public abstract string CanonicalString { get; }
            public abstract Type Resolve();
        }

        class QualifiedNameResolver : TypeReferenceResolver {

            private QualifiedName qn;

            public QualifiedNameResolver(QualifiedName qn) {
                this.qn = qn;
            }

            public override string CanonicalString {
                get {
                    return qn.ToString();
                }
            }

            public override Type Resolve() {
                return AppDomain.CurrentDomain.GetTypeByQualifiedName(qn);
            }
        }

        class TrivialResolver : TypeReferenceResolver {

            private readonly Type type;

            public TrivialResolver(Type type) {
                this.type = type;
            }

            public override string CanonicalString {
                get {
                    return (type == null) ? string.Empty : type.AssemblyQualifiedName;
                }
            }

            public override Type Resolve() {
                return type;
            }
        }

        class SlowResolver : TypeReferenceResolver {

            private readonly string typeName;

            public SlowResolver(string typeName) {
                this.typeName = typeName;
            }

            public override string CanonicalString {
                get {
                    return typeName;
                }
            }

            public override Type Resolve() {
                foreach (AssemblyName an in AssemblyInfo.ALL) {
                    Type type = Type.GetType(typeName + ", " + an);
                    if (type != null)
                        return type;
                }

                if (!typeName.Contains(".")) {
                    return typeof(object).Assembly.GetType("System." + typeName);
                }

                return null;
            }
        }

        class DefaultResolver : TypeReferenceResolver {

            private readonly AssemblyName assemblyName;
            private readonly string typeName;

            public DefaultResolver(AssemblyName assemblyName, string typeName) {
                this.assemblyName = assemblyName;
                this.typeName = typeName;
            }

            public override string CanonicalString {
                get {
                    return typeName + ", " + assemblyName;
                }
            }

            public override Type Resolve() {
                Assembly assembly = Assembly.Load(assemblyName);
                return FindType(assembly, typeName);
            }
        }
    }
}
