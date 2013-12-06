//
// - QualifiedNameTests.cs -
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
using Carbonfrost.Commons.Shared;
using NUnit.Framework;

namespace Tests {

    [TestFixture]
    public class QualifiedNameTests {

        const string MUSHROOM_KINGDOM = "http://ns.example.com/mushroom-kingdom";

        [Test]
        public void should_parse_default_ns() {
            QualifiedName qn = QualifiedName.Parse("Mario");
        }

        [Test]
        public void should_parse_expanded_names() {
            QualifiedName qn = QualifiedName.Parse("{http://ns.example.com/mushroom-kingdom} Mario");
        }

        [Test]
        public void should_expand_using_prefix_lookup() {
            IDictionary<string, string> lookup = new Dictionary<string, string> {
                { "mk", MUSHROOM_KINGDOM }
            };

            QualifiedName qn = QualifiedName.Expand("mk:Mario", lookup);
            Assert.That(qn.Namespace.NamespaceName, Is.EqualTo(MUSHROOM_KINGDOM));
            Assert.That(qn.LocalName, Is.EqualTo("Mario"));
        }

        [Test]
        public void should_expand_empty_prefix() {
            IDictionary<string, string> lookup = new Dictionary<string, string>();
            lookup.Add("", "");
            QualifiedName qn = QualifiedName.Expand(":Mario", lookup);

            Assert.That(qn.Namespace.NamespaceName, Is.EqualTo(NamespaceUri.Default.NamespaceName));
            Assert.That(qn.LocalName, Is.EqualTo("Mario"));
        }

        [Test]
        public void should_throw_on_missing_prefix() {
            IDictionary<string, string> lookup = new Dictionary<string, string>();
            Assert.That(() => { QualifiedName.Expand("mk:Mario", lookup); }, Throws.ArgumentException);
        }

        [Test]
        public void invalid_names() {
            Assert.That(() => { QualifiedName.Parse("*&Ma^^rio"); }, Throws.ArgumentException);
            Assert.That(() => { QualifiedName.Parse(""); }, Throws.ArgumentException);
        }

        [Test]
        public void local_name_is_required() {
            Assert.That(() => { QualifiedName.Create(NamespaceUri.Default, ""); }, Throws.ArgumentException);
            Assert.That(() => { QualifiedName.Create(NamespaceUri.Default, null); }, Throws.InstanceOf<ArgumentNullException>());
        }

        [Test]
        public void qualified_names_equals_operator() {
            QualifiedName n = QualifiedName.Create(NamespaceUri.Default, "default");
            QualifiedName m = n;
            Assert.False(n == null);
            Assert.True(n != null);
            Assert.False(null == n);
            Assert.True(null != n);

            Assert.True(m == n);
            Assert.False(m != n);
        }

        [Test]
        public void qualified_names_equals_equatable() {
            QualifiedName n = QualifiedName.Create(NamespaceUri.Default, "default");
            Assert.False(n.Equals(null));
            Assert.True(n.Equals(n));
        }

    }
}
