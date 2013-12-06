//
// - AdaptableTests.cs -
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Carbonfrost.Commons.Shared;
using Carbonfrost.Commons.Shared.Runtime;
using Carbonfrost.Commons.Shared.Runtime.Components;
using NUnit.Framework;

namespace Tests.Runtime {

    namespace Implementation.MySql {
        class ImplementationType {}
    }

    [TestFixture]
    public class AdaptableTests {

        private class LateImpl {
            public object Build(IServiceProvider sp) {
                return DBNull.Value;
            }
        }

        [Test]
        public void should_generate_extension_assembly_name() {
            var an = typeof(object).Assembly.GetName();
            AssemblyName tr = Adaptable.GetExtensionAssembly(an);
            Assert.That(tr.ToString(),
                        Is.EqualTo("mscorlib.Extensions, Version=4.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
        }

        [Test]
        public void should_generate_implementation_name() {
            TypeReference tr = Adaptable.GetExtensionImplementationType(typeof(object), "Name");
            Assert.That(tr.OriginalString,
                        Is.EqualTo("mscorlib.Implementation.Name.NameObject, mscorlib.Name, Version=4.0, PublicKeyToken=b77a5c561934e089"));
        }

        [Test]
        public void should_generate_name_from_implementation_type() {
            Type myType = typeof(Tests.Runtime.Implementation.MySql.ImplementationType);

            Assert.That(Adaptable.GetExtensionImplementationName(myType),
                        Is.EqualTo("MySql"));
        }

        [Test]
        public void latebound_adapter_methods() {
            LateImpl li = new LateImpl();

            Expression<Func<IServiceProvider, object>> Signature = (sp) => (default(object));
            Func<IServiceProvider, object> func = li.CreateAdapterFunction("Build", Signature);

            Assert.That(func(null), Is.EqualTo(DBNull.Value));
            Assert.That(AdapterRole.IsBuilderType(typeof(LateImpl)));
            Assert.That(Adaptable.GetMethodBySignature(typeof(LateImpl), "Build", Signature) == typeof(LateImpl).GetMethods()[0]);
        }

    }

}
