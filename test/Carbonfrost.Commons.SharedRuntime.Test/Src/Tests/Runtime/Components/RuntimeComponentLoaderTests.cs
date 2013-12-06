//
// - RuntimeComponentLoaderTests.cs -
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
using System.Reflection;
using Carbonfrost.Commons.Shared;
using Carbonfrost.Commons.Shared.Runtime;
using Carbonfrost.Commons.Shared.Runtime.Components;
using NUnit.Framework;

namespace Tests.Runtime.Components {

    [TestFixture]
    public class RuntimeComponentLoaderTests {

        [Test]
        public void load_assembly_nominal() {
            var a = RuntimeComponentLoader.Assembly;
            var uri = new Uri(GetType().Assembly.CodeBase);
            var asm = a.Load(ComponentTypes.Assembly, uri);

            Assert.That(asm, Is.InstanceOf<AssemblyInfo>());
            Assert.That(asm.Adapt<Assembly>(), Is.EqualTo(GetType().Assembly));
        }

        [Test]
        public void create_assembly_by_criteria() {
            Assert.That(RuntimeComponentLoader.Create(ComponentTypes.Assembly),
                        Is.SameAs(RuntimeComponentLoader.Assembly));
        }

        [Test]
        public void create_assembly_by_name() {
            Assert.That(RuntimeComponentLoader.FromName("assembly"),
                        Is.SameAs(RuntimeComponentLoader.Assembly));
        }
    }
}
