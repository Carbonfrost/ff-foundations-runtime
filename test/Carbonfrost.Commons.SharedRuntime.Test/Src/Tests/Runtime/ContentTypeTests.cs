//
// - ContentTypeTests.cs -
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
using System.Text;
using System.Text.RegularExpressions;
using Carbonfrost.Commons.Shared;
using Carbonfrost.Commons.Shared.Runtime;
using NUnit.Framework;

namespace Tests.Runtime {

    [TestFixture]
    public class ContentTypeTests {

        [Test]
        public void should_get_encoding() {
            Encoding encoding = Utility.GetEncodingFromContentType("text/html;charset=utf-8");
            Assert.That(encoding, Is.EqualTo(Encoding.UTF8));
        }

        [Test]
        public void support_parsing() {
            ContentType type = ContentType.Parse("application/propertytrees+xml");
            Assert.That(type.MediaType, Is.EqualTo("application/propertytrees+xml"));
            Assert.That(type.Type, Is.EqualTo("application"));
            Assert.That(type.Subtype, Is.EqualTo("propertytrees+xml"));

            Assert.That(type.ToString(), Is.EqualTo("application/propertytrees+xml"));
        }

        [Test]
        public void get_paraent_content_type() {
            ContentType type = ContentType.Parse("application/propertytrees+xml");
            Assert.That(type.Parent.MediaType, Is.EqualTo("application/xml"));
        }
    }

}
