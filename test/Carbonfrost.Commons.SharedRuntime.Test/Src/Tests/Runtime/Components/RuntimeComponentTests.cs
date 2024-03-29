//
// - RuntimeComponentTests.cs -
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
using Carbonfrost.Commons.Shared;
using Carbonfrost.Commons.Shared.Runtime.Components;
using NUnit.Framework;

namespace Tests.Runtime.Components {

    [TestFixture]
    public class RuntimeComponentTests {

        [Test]
        public void assembly_info_is_assembly_component_type() {
            Assert.That(RuntimeComponent.GetRuntimeComponentType(ComponentTypes.Assembly),
                        Is.EqualTo(typeof(AssemblyInfo)));
        }

        [Test]
        public void define_component_using_component_attribute() {
            Assert.That(RuntimeComponent.GetRuntimeComponentType("TestComponent"), Is.EqualTo(typeof(MyComponent)));
            Assert.That(RuntimeComponent.GetRuntimeComponentType("NonExistant"), Is.Null);
        }

    }

    [RuntimeComponentUsage(Name = "TestComponent")]
    public class MyComponent : IRuntimeComponent {

        Uri IRuntimeComponent.Source {
            get { return null; } }

        string IRuntimeComponent.ComponentType {
            get { return null; } }

        ComponentName IRuntimeComponent.ComponentName {
            get { return null; } }

        ComponentCollection IRuntimeComponent.Dependencies {
            get { return null; } }

    }

}
