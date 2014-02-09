//
// - SharedRuntimeOptionsAttributeTests.cs -
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
using System.Reflection;
using Carbonfrost.Commons.Shared;
using Carbonfrost.Commons.Shared.Runtime;
using NUnit.Framework;

namespace Tests.Runtime {

    [TestFixture]
    public class SharedRuntimeOptionsAttributeTests {

        [Test]
        public void system_assemblies_optimized() {
            Assembly system = typeof(Uri).Assembly;
            Assert.False(system.GetSharedRuntimeOptions().Providers);
            Assert.False(system.GetSharedRuntimeOptions().Templates);

            Assembly mscorlib = typeof(Uri).Assembly;
            Assert.False(mscorlib.GetSharedRuntimeOptions().Providers);
            Assert.False(mscorlib.GetSharedRuntimeOptions().Templates);
        }

        [Test]
        public void this_assembly_scannable() {
            Assembly shared = typeof(Glob).Assembly;
            Assert.True(shared.GetSharedRuntimeOptions().Providers);

            Assembly self = GetType().Assembly;
            Assert.True(shared.GetSharedRuntimeOptions().Providers);
        }
    }
}
