//
// - AdaptableQualifiedNameTests.cs -
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
using NUnit.Framework;

namespace Tests.Runtime {

    [TestFixture]
    public class AdaptableQualifiedNameTests {

        [Test]
        public void get_type_from_qualified_name_nominal() {
            string fullName = string.Format("{{{0}}} TypeReference", Xmlns.SharedRuntime2008);
            Assert.That(
                AppDomain.CurrentDomain.GetTypeByQualifiedName(QualifiedName.Parse(fullName)),
                Is.EqualTo(typeof(TypeReference)));

            TypeReference tr = TypeReference.Parse(fullName);
            Assert.AreEqual(typeof(TypeReference), tr.Resolve());
        }

        [Test]
        public void get_type_from_qualified_name_nested_types() {
            var nested = typeof(TypeReference).GetNestedType("TrivialResolver", BindingFlags.NonPublic);

            string fullName = string.Format("{{{0}}} TypeReference.TrivialResolver", Xmlns.SharedRuntime2008);
            Assert.That(
                nested.GetQualifiedName().ToString(),
                Is.EqualTo(fullName));
            Assert.That(
                AppDomain.CurrentDomain.GetTypeByQualifiedName(QualifiedName.Parse(fullName)),
                Is.EqualTo(nested));
        }

        [Test]
        public void get_type_from_qualified_name_open_generic_type() {
            var open = typeof(IHierarchyObject<>);

            string fullName = string.Format("{{{0}}} IHierarchyObject-1", Xmlns.SharedRuntime2008);
            Assert.That(
                open.GetQualifiedName().ToString(),
                Is.EqualTo(fullName));
            Assert.That(
                AppDomain.CurrentDomain.GetTypeByQualifiedName(QualifiedName.Parse(fullName)),
                Is.EqualTo(open));
        }

        [Test]
        public void get_type_from_qualified_name_closed_generic_type() {
            var closed = typeof(Status<BindingFlags>);

            Assert.That(
                () => { closed.GetQualifiedName().ToString(); },
                Throws.ArgumentException);
        }
    }
}
