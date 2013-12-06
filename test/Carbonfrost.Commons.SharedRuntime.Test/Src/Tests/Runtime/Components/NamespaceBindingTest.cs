//
// - NamespaceBindingTest.cs -
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Carbonfrost.Commons.Shared.Runtime.Components;
using NUnit.Framework;

namespace Tests.Runtime.Components {

    [TestFixture]
    public class NamespaceBindingTest {

        static readonly AssemblyName expectedName = new AssemblyName("Carbonfrost.Commons.SharedRuntime");

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void parse_text_cannot_be_null() {
            NamespaceBinding.Parse(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void parse_text_cannot_be_empty_string() {
            NamespaceBinding.Parse(string.Empty);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void from_uri_cannot_be_null() {
            NamespaceBinding.FromUri(null);
        }

        [Test]
        public void from_uri_nominal() {
            NamespaceBinding result = NamespaceBinding.FromUri(
                new Uri("clr-namespace:Carbonfrost.Commons.Shared;assembly:Carbonfrost.Commons.SharedRuntime"));
            Assert.That(result.AssemblyName.FullName,  Is.EqualTo(expectedName.FullName));
        }

        [Test]
        public void to_string_and_to_uri_support() {
            NamespaceBinding binding = new NamespaceBinding("Carbonfrost.Commons.Shared", expectedName);
            Assert.That(binding.ToUri().ToString(),
                        Is.EqualTo("clr-namespace:Carbonfrost.Commons.Shared;assembly:" + expectedName));
        }

        [Test]
        public void from_uri_featuring_wildcard() {
            NamespaceBinding result = NamespaceBinding.FromUri(
                new Uri("clr-namespace:Carbonfrost.*;assembly:Carbonfrost.Commons.SharedRuntime"));

            Assert.That(result.Namespace, Is.EqualTo("Carbonfrost.*"));
            Assert.That(result.AssemblyName.FullName, Is.EqualTo(expectedName.FullName));

            var expectedNamespaces = new HashSet<string>(typeof(NamespaceBinding).Assembly.GetTypes()
                                                         .Select(t => t.Namespace));
            expectedNamespaces.Remove(null);

            Assert.That(result.EnumerateNamespaces().ToArray(),
                        Is.EquivalentTo(expectedNamespaces.ToArray()));
        }
    }
}


