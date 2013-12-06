//
// - TypeReferenceConverterTests.cs -
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
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;

using Carbonfrost.Commons.Shared.Runtime;
using NUnit.Framework;

namespace Tests.Runtime {

    [TestFixture]
    public class TypeReferenceConverterTests {

        [Test]
        public void type_converter_supported_conversions() {
            TypeConverter tc = TypeDescriptor.GetConverter(typeof(TypeReference));
            Assert.That(tc.CanConvertFrom(typeof(string)));
            Assert.That(tc.CanConvertFrom(typeof(Type)));

            Assert.That(tc.CanConvertTo(typeof(string)));
            Assert.That(tc.CanConvertTo(typeof(Type)));
            Assert.That(tc.CanConvertTo(typeof(InstanceDescriptor)));
        }

        [Test]
        public void type_conversion_integration() {
            TypeConverter tc = TypeDescriptor.GetConverter(typeof(TypeReference));
            tc.ConvertFromInvariantString("System.String");
            // TODO Additional test
        }


        [Test]
        public void get_instance_descriptor() {
            TypeConverter tc = TypeDescriptor.GetConverter(typeof(TypeReference));

            TypeReference tr = TypeReference.Parse("System.Int32, mscorlib");
            InstanceDescriptor instanceDescriptor = (InstanceDescriptor) tc.ConvertTo(tr, typeof(InstanceDescriptor));
            Assert.That(instanceDescriptor.Arguments, Is.EquivalentTo(new [] { "System.Int32, mscorlib", null }));
        }
    }
}
