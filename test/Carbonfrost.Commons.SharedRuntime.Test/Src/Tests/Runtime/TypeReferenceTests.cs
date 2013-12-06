//
// - TypeReferenceTests.cs -
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
using System.ComponentModel.Design;
using System.Net.Mail;
using System.Xml;

using Carbonfrost.Commons.Shared;
using Carbonfrost.Commons.Shared.Runtime;
using NUnit.Framework;

namespace Tests.Runtime {

    [TestFixture]
    public class TypeReferenceTests {

        [Test]
        public void simple_name_resolves() {
            TypeReference tr = TypeReference.Parse("String");
            Assert.AreEqual(typeof(string), tr.Resolve());
        }

        [Test]
        public void builtin_identifiers_resolve() {
            TypeReference tr = TypeReference.Parse("uint");
            Assert.AreEqual(typeof(UInt32), tr.Resolve());

            tr = TypeReference.Parse("long");
            Assert.AreEqual(typeof(long), tr.Resolve());

            tr = TypeReference.Parse("decimal");
            Assert.AreEqual(typeof(decimal), tr.Resolve());
        }

        [Test]
	    public void resolve_type_from_type_nominal() {
	        TypeReference tr = TypeReference.FromType(typeof(int));
	        Assert.AreEqual(typeof(int), tr.Resolve());
	    }

        [Test]
        public void name_resolves_without_assembly() {
            TypeReference tr = TypeReference.Parse("System.Net.Mail.MailAddress");
            Assert.AreEqual(typeof(MailAddress), tr.Resolve());
        }

        [Test]
        public void qualified_name_resolves() {
            string fullName = string.Format("{{{0}}} TypeReference", Xmlns.SharedRuntime2008);

            TypeReference tr = TypeReference.Parse(fullName);
            Assert.AreEqual(typeof(TypeReference), tr.Resolve());
        }

        [Test]
        public void trivial_type_resolution() {
            TypeReference tr = TypeReference.FromType(typeof(int));
            Assert.AreEqual(typeof(int), tr.Resolve());
        }

        [Test]
        public void qualified_name_using_prefix_resolves() {
            ServiceContainer services = new ServiceContainer();
            XmlNameTable table = new NameTable();
            XmlNamespaceManager mgr = new XmlNamespaceManager(table);
            services.AddService(typeof(IXmlNamespaceResolver), mgr);
            mgr.AddNamespace("f", Xmlns.SharedRuntime2008);

            TypeReference tr = TypeReference.Parse("f:TypeReference", services);
            Assert.AreEqual(typeof(TypeReference), tr.Resolve());
        }

        [Test]
        public void default_qualified_name_using_prefix_resolves() {
            ServiceContainer services = new ServiceContainer();
            XmlNameTable table = new NameTable();
            XmlNamespaceManager mgr = new XmlNamespaceManager(table);
            services.AddService(typeof(IXmlNamespaceResolver), mgr);
            mgr.AddNamespace("", Xmlns.SharedRuntime2008);

            TypeReference tr = TypeReference.Parse(":TypeReference", services);
            Assert.AreEqual(typeof(TypeReference), tr.Resolve());
        }

        [Test]
        public void missing_type_should_throw() {
            TypeReference tr = TypeReference.Parse("System.Glob");
            Assert.Throws(typeof(InvalidOperationException), () => tr.Resolve());
        }

        [Test]
        public void missing_type_should_return_null() {
            TypeReference tr = TypeReference.Parse("System.Glob");
            Assert.That(tr.TryResolve(), Is.Null);
        }
    }
}
