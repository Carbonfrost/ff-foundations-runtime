//
// - AssemblyInfoTests.cs -
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
using System.Linq;
using System.Reflection;
using Carbonfrost.Commons.Shared;
using Carbonfrost.Commons.Shared.Runtime;
using NUnit.Framework;

namespace Tests {

    [TestFixture]
    public class AssemblyInfoTests {

        [Test]
        public void should_match_ns_names() {
            string[] names = { "System", "System.Collections", "System.Collections.Generic", "System.Linq", "System.Linq.Expressions", "Carbonfrost.Commons.Shared", "Carbonfrost.Commons.ComponentModel.PropertyTrees.Expressions" };
            Assert.That(Utility.FilterNamespaces(names, "*").ToArray(),
                        Is.EquivalentTo(names));

            Assert.That(Utility.FilterNamespaces(names, "System").ToArray(),
                        Is.EquivalentTo(new string[] { "System" }));

            string[] systemNames = { "System", "System.Collections", "System.Collections.Generic", "System.Linq", "System.Linq.Expressions" };
            Assert.That(Utility.FilterNamespaces(names, "System.*").ToArray(),
                        Is.EquivalentTo(systemNames));

            string[] expressionNames = { "System.Linq.Expressions", "Carbonfrost.Commons.ComponentModel.PropertyTrees.Expressions" };
            Assert.That(Utility.FilterNamespaces(expressionNames, "*.Expressions").ToArray(),
                        Is.EquivalentTo(expressionNames));
        }

        [Test]
        public void should_match_ns_names_in_situ() {
            string[] names = { "System", "System.Collections", "System.Collections.Generic", "System.Linq", "System.Linq.Expressions", "Carbonfrost.Commons.Shared", "Carbonfrost.Commons.ComponentModel.PropertyTrees.Expressions" };
            string[] expressionNames = { "System.Linq.Expressions", "Carbonfrost.Commons.ComponentModel.PropertyTrees.Expressions" };
            Assert.That(Utility.FilterNamespaces(names, "*.Expressions").ToArray(),
                        Is.EquivalentTo(expressionNames));

            string[] expressionNames2 = { "System.Linq.Expressions" };
            Assert.That(Utility.FilterNamespaces(names, "System.*.Expressions").ToArray(),
                        Is.EquivalentTo(expressionNames2));
        }

        [Test]
        public void should_get_xmlns_from_clrnamespaces() {
            AssemblyInfo ai = AssemblyInfo.GetAssemblyInfo(typeof(TypeReference).Assembly);
            Assert.That(ai.GetXmlNamespace("Carbonfrost.Commons.Shared"), Is.EqualTo(NamespaceUri.Parse(Xmlns.SharedRuntime2008)));
            Assert.That(ai.GetXmlNamespace("Carbonfrost.Commons.Shared.Runtime"), Is.EqualTo(NamespaceUri.Parse(Xmlns.SharedRuntime2008)));
            Assert.That(ai.GetXmlNamespace("Carbonfrost.Commons.ComponentModel"), Is.EqualTo(NamespaceUri.Parse(Xmlns.SharedRuntime2008)));
            Assert.That(ai.GetXmlNamespace("Carbonfrost.Commons.ComponentModel.Annotations"), Is.EqualTo(NamespaceUri.Parse(Xmlns.ShareableCodeMetadata2011)));
        }

        [Test]
        public void should_get_clr_namespaces_from_xmlns() {
            AssemblyInfo ai = AssemblyInfo.GetAssemblyInfo(typeof(TypeReference).Assembly);
            var all = ai.GetClrNamespaces(NamespaceUri.Parse(Xmlns.SharedRuntime2008));
            Assert.That(all, Contains.Item("Carbonfrost.Commons.Shared"));
            Assert.That(all, Contains.Item("Carbonfrost.Commons.Shared.Runtime"));
            Assert.That(all, Contains.Item("Carbonfrost.Commons.ComponentModel"));

            CollectionAssert.DoesNotContain(all, "Carbonfrost.Commons.ComponentModel.Annotations");
        }

        [Test]
        public void should_process_reflect_only_assembly() {
            Assembly a = Assembly.ReflectionOnlyLoad(@"System.Xml.Linq, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            var info = AssemblyInfo.GetAssemblyInfo(a);

            Assert.That(info.Url, Is.EqualTo(new Uri(a.CodeBase)));
        }
    }
}
