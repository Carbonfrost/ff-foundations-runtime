//
// - ComponentTest.cs -
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
using System.Globalization;
using Carbonfrost.Commons.Shared;
using Carbonfrost.Commons.Shared.Runtime.Components;
using NUnit.Framework;

namespace Tests.Runtime.Components {

    [TestFixture]
    public class ComponentTest {

        // TODO Add invalid source test

        [Test]
        public void parse_nominal() {
            string text = "component:assembly:Carbonfrost.Commons.SharedRuntime,Version=1.0,Culture=neutral:https://tempuri.org/toolchain";
            Component component = Component.Parse(text);
            Assert.That(component.Name.Culture, Is.EqualTo(CultureInfo.InvariantCulture));
            Assert.That(component.Name.Name, Is.EqualTo("Carbonfrost.Commons.SharedRuntime"));
            Assert.That(component.Type, Is.EqualTo(ComponentTypes.Assembly));
            Assert.That(component.Source, Is.EqualTo(new Uri("https://tempuri.org/toolchain")));
        }

        [Test]
        public void parse_with_uri_encodings() {
            // The spaces will induce URI encoded %20
            string text = "component:assembly:Carbonfrost.Commons.SharedRuntime, Version=1.0, Culture=neutral:https://tempuri.org/toolchain";
            Component component = Component.Parse(text);
            Assert.That(component.Name.Culture, Is.EqualTo(CultureInfo.InvariantCulture));
            Assert.That(component.Name.Name, Is.EqualTo("Carbonfrost.Commons.SharedRuntime"));
            Assert.That(component.Type, Is.EqualTo(ComponentTypes.Assembly));
            Assert.That(component.Source, Is.EqualTo(new Uri("https://tempuri.org/toolchain")));
        }

        [Test]
        public void parse_without_source() {
            string text = "component:assembly:Carbonfrost.Commons.SharedRuntime, Version=1.0, Culture=neutral";
            Component component = Component.Parse(text);
            Assert.That(component.Name.Culture, Is.EqualTo(CultureInfo.InvariantCulture));
            Assert.That(component.Name.Name, Is.EqualTo("Carbonfrost.Commons.SharedRuntime"));
            Assert.That(component.Type, Is.EqualTo(ComponentTypes.Assembly));
            Assert.That(component.Source, Is.Null);
        }

        [Test]
        public void parse_without_type() {
            string text = "component::Toolchain, Version=1.0:https://tempuri.org/toolchain";
            Component component = Component.Parse(text);

            Assert.That(component.Name.Name, Is.EqualTo("Toolchain"));
            Assert.That(component.Type, Is.EqualTo(ComponentTypes.Anything));
        }

        [Test]
        public void missing_type_implies_anything() {
            Component component = new Component(ComponentName.Parse("A"), null, null);
            Assert.That(component.Type, Is.EqualTo(ComponentTypes.Anything));

            component = new Component(ComponentName.Parse("A"), string.Empty, null);
            Assert.That(component.Type, Is.EqualTo(ComponentTypes.Anything));
        }

    }


}
