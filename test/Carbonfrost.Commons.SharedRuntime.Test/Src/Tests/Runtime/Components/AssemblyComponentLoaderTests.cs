//
// - AssemblyComponentLoaderTests.cs -
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
using System.Linq;
using Carbonfrost.Commons.Shared;
using Carbonfrost.Commons.Shared.Runtime;
using Carbonfrost.Commons.Shared.Runtime.Components;
using NUnit.Framework;

namespace Tests.Runtime.Components {

    [TestFixture]
    public class AssemblyComponentLoaderTests {

        [Test]
        public void create_assembly_expected_metadata_reflect_only() {
            RuntimeComponentLoader loader = RuntimeComponentLoader.Assembly;
            var asm = typeof(Glob).Assembly;
            var me = loader.LoadMetadata("assembly", new Uri(asm.CodeBase, UriKind.RelativeOrAbsolute));

            Assert.That(me.Select(t => QualifiedName.Parse(t.Key).LocalName),
                        Is.EquivalentTo(new [] {
                                            "base",
                                            "configuration",
                                            "license",
                                            "assemblyName",
                                            "name",
                                            "platform",
                                            "targetFramework",
                                            "url",
                                            "version",
                                        }));

            Assert.That(me.GetString("name"), Is.EqualTo("Carbonfrost.Commons.SharedRuntime"));

            string expected = ComponentName.FromAssemblyName(asm.GetName()).ToString();
            Assert.That(me.GetString("assemblyName"), Is.EqualTo(expected));
        }
    }
}
