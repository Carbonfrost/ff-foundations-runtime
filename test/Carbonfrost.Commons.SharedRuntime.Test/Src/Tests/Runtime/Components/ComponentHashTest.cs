//
// - ComponentHashTest.cs -
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
using Carbonfrost.Commons.Shared.Runtime.Components;
using NUnit.Framework;

namespace Tests.Runtime.Components {

    [TestFixture]
    public class ComponentHashTest {

        [Test]
        public void parse_unspecified_algorithm() {
            ComponentHash hash = ComponentHash.Parse("EE50D11FFBE1408097B84554B4609E07");
            Assert.That(hash.Algorithm, Is.EqualTo(ComponentHashAlgorithm.MD5));
            Assert.That(hash.Hash, Is.EqualTo("EE50D11FFBE1408097B84554B4609E07"));
        }

        [Test]
        public void parse_base64() {
            ComponentHash hash = ComponentHash.Parse("MD5:base64:00==");
            Assert.That(hash.Algorithm, Is.EqualTo(ComponentHashAlgorithm.MD5));
            Assert.That(hash.Hash, Is.EqualTo("D3"));
        }

        [Test]
        public void match_nominal() {
            ComponentHash a = ComponentHash.Parse("EE50D11FFBE1408097B84554B4609E07");
            ComponentHash b = ComponentHash.Parse("EE50D11FFBE1408097B84554B4609E07");
            Assert.That(a.Matches(b));
            Assert.That(b.Matches(a));
            Assert.That(a.Equals(b));
            Assert.That(a == b);
        }

        [Test]
        public void match_subsequence() {
            ComponentHash a = ComponentHash.Parse("EE50D11FFBE1408097B84554B4609E07");
            ComponentHash b = ComponentHash.Parse("EE");
            Assert.False(a.Equals(b));
            Assert.False(a == b);
            Assert.True(b.Matches(a));
            Assert.False(a.Matches(b));
        }

        [Test]
        public void to_string_nominal() {
            ComponentHash a = ComponentHash.Parse("EE50D11FFBE1408097B84554B4609E07");
            Assert.That(a.ToString(), Is.EqualTo("MD5:EE50D11FFBE1408097B84554B4609E07"));
        }

        [Test]
        public void to_string_format_nominal() {
            ComponentHash a = ComponentHash.Parse("EE50D11FFBE1408097B84554B4609E07");
            Assert.That(a.ToString("g"), Is.EqualTo("MD5:ee50d11ffbe1408097b84554b4609e07"));
            Assert.That(a.ToString("G"), Is.EqualTo("MD5:EE50D11FFBE1408097B84554B4609E07"));
            Assert.That(a.ToString("x"), Is.EqualTo("ee50d11ffbe1408097b84554b4609e07"));
            Assert.That(a.ToString("X"), Is.EqualTo("EE50D11FFBE1408097B84554B4609E07"));
            Assert.That(a.ToString("a"), Is.EqualTo("MD5"));
            Assert.That(a.ToString("z"), Is.EqualTo("7lDRH/vhQICXuEVUtGCeBw=="));
        }

    }
}
