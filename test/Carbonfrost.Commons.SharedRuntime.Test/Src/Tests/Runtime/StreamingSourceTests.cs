//
// - StreamingSourceTests.cs -
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

using Carbonfrost.Commons.Shared.Runtime;
using NUnit.Framework;

namespace Tests.Runtime {

    [TestFixture]
    public class StreamingSourceTests {

        [Test]
        public void test_known_nominal() {
            Assert.That(StreamingSource.FromName("xmlFormatter"), Is.Not.Null);
            Assert.That(StreamingSource.FromName("text"), Is.Not.Null);
            Assert.That(StreamingSource.FromName("binaryFormatter"), Is.Not.Null);
        }

        [Test]
        public void test_known_adapter_names() {
            Assert.That(AppDomain.CurrentDomain.GetProviderNames(typeof(StreamingSource))
                        .Select(t => t.LocalName),
                        Is.EquivalentTo(new [] {
                                            "xmlFormatter",
                                            "text",
                                            "properties",
                                            "binaryFormatter",
                                            "binaryFormatterBase64",
                                        }));
        }

        [Test]
        public void test_known_content_types() {
            Assert.That(StreamingSource.Create(typeof(object), ContentType.Parse(ContentTypes.BinaryFormatterBase64)),
                        Is.SameAs(StreamingSources.BinaryFormatterBase64));

            Assert.That(StreamingSource.Create(typeof(object), ContentType.Parse(ContentTypes.BinaryFormatter)),
                        Is.SameAs(StreamingSources.BinaryFormatter));
        }
    }
}