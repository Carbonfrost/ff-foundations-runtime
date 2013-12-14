//
// - ComponentNameTest.cs -
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
using System.Reflection;
using Carbonfrost.Commons.Shared.Runtime.Components;
using NUnit.Framework;

namespace Tests.Runtime.Components {

    [TestFixture]
    public class ComponentNameTest {

        [Test]
        public void parse_neutral_culture() {
            ComponentName name = ComponentName.Parse("A, Culture=neutral");
            Assert.That(name.Culture, Is.EqualTo(CultureInfo.InvariantCulture));
        }

        [Test]
        public void parse_null_culture() {
            ComponentName name = ComponentName.Parse("A, Culture=null");
            Assert.That(name.Culture, Is.EqualTo(CultureInfo.InvariantCulture));
        }

        [Test]
        public void parse_public_key_token_equivalence() {
            var subject = ComponentName.Parse("System, Version=4.0.0.0, Culture=neutral, PublicKey=00000000000000000400000000000000");

            Assert.That(BitConverter.ToString(subject.GetPublicKeyToken()),
                        Is.EqualTo("B7-7A-5C-56-19-34-E0-89"));
        }

        [Test]
        public void parse_component_name_short_form() {
            ComponentName name = ComponentName.Parse("A=1.0.0.0-en-x86");
            Assert.That(name.Version, Is.EqualTo(new Version(1, 0, 0, 0)));
            Assert.That(name.Architecture, Is.EqualTo(ProcessorArchitecture.X86));
            Assert.That(name.Culture.Name, Is.EqualTo("en"));
            Assert.That(name.Name, Is.EqualTo("A"));
        }

        [Test]
        public void parse_component_name_short_form_arch() {
            ComponentName name = ComponentName.Parse("A=1.0.0.0-x86");
            Assert.That(name.Version, Is.EqualTo(new Version(1, 0, 0, 0)));
            Assert.That(name.Name, Is.EqualTo("A"));
        }

        [Test]
        public void parse_component_name_short_form_culture() {
            ComponentName name = ComponentName.Parse("A=1.0.0.0-en-US-msil");
            Assert.That(name.Version, Is.EqualTo(new Version(1, 0, 0, 0)));
            Assert.That(name.Architecture, Is.EqualTo(ProcessorArchitecture.MSIL));
            Assert.That(name.Culture.Name, Is.EqualTo("en-US"));
            Assert.That(name.Name, Is.EqualTo("A"));
        }

        [Test]
        public void parse_component_name_short_form_no_arch() {
            ComponentName name = ComponentName.Parse("A=1.0.0.0-fr-FR");
            Assert.That(name.Version, Is.EqualTo(new Version(1, 0, 0, 0)));
            Assert.That(name.Architecture, Is.EqualTo(ProcessorArchitecture.None));
            Assert.That(name.Culture.Name, Is.EqualTo("fr-FR"));
            Assert.That(name.Name, Is.EqualTo("A"));
        }

        [Test]
        public void parse_component_name_short_form_key() {
            byte[] key = { 0x20, 0x40, 0x60, 0x80, 0xA0, 0xC0, 0xE0, 0xFF };
            ComponentName name = ComponentName.Parse("A=1.0.0.0-20406080A0C0E0FF-fr-FR");
            Assert.That(name.Version, Is.EqualTo(new Version(1, 0, 0, 0)));
            Assert.That(name.GetPublicKeyToken(), Is.EqualTo(key));
            Assert.That(name.Architecture, Is.EqualTo(ProcessorArchitecture.None));
            Assert.That(name.Culture.Name, Is.EqualTo("fr-FR"));
            Assert.That(name.Name, Is.EqualTo("A"));
        }

        [Test]
        public void convert_component_name_to_string_short_form() {
            byte[] key = { 0x20, 0x40, 0x60, 0x80, 0xA0, 0xC0, 0xE0, 0xFF };
            ComponentName name = new ComponentName("A", new Version(1, 0, 0, 0), key, null, CultureInfo.GetCultureInfo("en-US"), ProcessorArchitecture.Amd64);
            Assert.That(name.ToString("s"), Is.EqualTo("A=1.0.0.0-20406080a0c0e0ff-en-US-amd64"));
        }

        [Test]
        public void parse_public_key_token() {
            ComponentName name = ComponentName.Parse("A, PublicKeyToken=20406080A0C0E0FF");
            Assert.That(name.GetPublicKey(), Is.Empty);
            Assert.That(BitConverter.ToString(name.GetPublicKeyToken()),
                        Is.EqualTo("20-40-60-80-A0-C0-E0-FF"));
        }

        public void matches_reflexive() {
            var name = ComponentName.Parse("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            Assert.True(name.Matches(name));
        }

        [Test]
        public void matches_nominal() {
            var subject = ComponentName.Parse("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            var name = ComponentName.Parse("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

            Assert.True(name.Matches(subject));
            Assert.True(subject.Matches(name));
        }

        [Test]
        public void matches_based_on_mismatches() {
            var subject = ComponentName.Parse("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

            Assert.False(ComponentName.Parse("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089").Matches(subject));
            Assert.False(ComponentName.Parse("System, Version=3.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089").Matches(subject));
            Assert.False(ComponentName.Parse("System, Version=4.0.0.0, Culture=en, PublicKeyToken=b77a5c561934e089").Matches(subject));

            // Different key token
            Assert.False(ComponentName.Parse("System, Version=4.0.0.0, Culture=en, PublicKeyToken=ff7a5c561934e089").Matches(subject));
        }

        [Test]
        public void matches_based_on_public_key_token_equivalence() {
            var subject = ComponentName.Parse("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

            Assert.True(ComponentName.Parse("System, Version=4.0.0.0, Culture=neutral, PublicKey=00000000000000000400000000000000").Matches(subject));
        }

        [Test]
        public void matches_less_specificity() {
            var subject = ComponentName.Parse("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

            Assert.True(ComponentName.Parse("System, Version=4.0.0.0, Culture=neutral").Matches(subject));
            Assert.True(ComponentName.Parse("System, Version=4.0.0.0").Matches(subject));
            Assert.True(ComponentName.Parse("System").Matches(subject));
        }

        [Test]
        public void matches_less_specificity_in_version() {
            var subject = ComponentName.Parse("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

            Assert.True(ComponentName.Parse("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089").Matches(subject));
            Assert.True(ComponentName.Parse("System, Version=4.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089").Matches(subject));
            Assert.True(ComponentName.Parse("System, Version=4.0, Culture=neutral, PublicKeyToken=b77a5c561934e089").Matches(subject));
        }
    }
}
