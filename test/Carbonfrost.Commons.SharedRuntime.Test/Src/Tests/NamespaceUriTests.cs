//
// - NamespaceUriTests.cs -
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
using NUnit.Framework;

namespace Tests {

    [TestFixture]
    public class NamespaceUriTests {

        [Test]
        public void test_to_string_tag_uri() {
            Assert.That(NamespaceUri.Xmlns.ToString("t"), Is.EqualTo("tag:www.w3.org,2000:/xmlns/"));
            Assert.That(NamespaceUri.Xmlns.ToString("T"), Is.EqualTo("tag:www.w3.org,2000:/xmlns/"));
        }

        [Test]
        public void test_to_string_tag_uri_long_date() {
            NamespaceUri nu = NamespaceUri.Parse("http://ns.example.com/2012-01-01/etc");
            Assert.That(nu.ToString("t"),
                        Is.EqualTo("tag:ns.example.com,2012-01-01:/etc"));
        }

        [Test]
        public void test_to_string_tag_uri_rooted() {
            NamespaceUri nu = NamespaceUri.Parse("http://ns.example.com/2012-01-01");
            Assert.That(nu.ToString("t"),
                        Is.EqualTo("tag:ns.example.com,2012-01-01:/"));
        }

        [Test]
        public void test_to_string_tag_uri_long_date_month() {
            NamespaceUri nu = NamespaceUri.Parse("http://ns.example.com/2012-01/etc");
            Assert.That(nu.ToString("t"),
                        Is.EqualTo("tag:ns.example.com,2012-01:/etc"));
        }
    }
}
