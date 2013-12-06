//
// - PropertiesTests.cs -
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
using System.IO;
using System.Text;
using Carbonfrost.Commons.Shared.Runtime;
using NUnit.Framework;

namespace Tests {

    [TestFixture]
    public class PropertiesTests {

        [Test]
        public void parse_key_value_pairs_whitespace_rules() {
            Properties p = Properties.Parse("a=;b;c=");
            Assert.That(p.InnerMap.Count, Is.EqualTo(3));

            Assert.That(p.GetProperty("a"), Is.EqualTo(""));
            Assert.That(p.GetProperty("b"), Is.EqualTo(""));
            Assert.That(p.GetProperty("c"), Is.EqualTo(""));
        }

        [Test]
        public void parse_key_value_pairs() {
            Properties p = Properties.Parse("a=a;b=b;c=true");
            Assert.That(p.InnerMap.Count, Is.EqualTo(3));

            Assert.That(p.GetProperty("a"), Is.EqualTo("a"));
            Assert.That(p.GetProperty("b"), Is.EqualTo("b"));
            Assert.That(p.GetProperty("c"), Is.EqualTo("true"));
        }

        [Test]
        public void convert_key_value_pairs() {
            IProperties p = Properties.FromValue(new { a = "a", b = "b", c = true });
            Assert.That(p.ToString(), Is.EqualTo("a=a;b=b;c=True"));
        }

        // TODO Review these tests (looks like c might be incorrect)

        [Test]
        public void convert_key_value_pairs_escaping() {
            IProperties p = Properties.FromValue(new { a = "a    ", b = "\"quotat ; ions\"", c = "carriage returns\r\n", d = ";;; '' ;;;" });
            Assert.That(p.ToString(), Is.EqualTo("a='a    ';b='\"quotat ; ions\"';c='carriage returns\r\n';d=';;; \\'\\' ;;;'"));
        }


        [Test]
        public void convert_key_value_pairs_unescaping() {
            Properties p = Properties.Parse("a='a    ';b='\"quotat ; ions\"';c='carriage returns\r\n';d=';;; \\'\\' ;;;'");
            Assert.That(p.GetProperty("a"), Is.EqualTo("a    "));
            Assert.That(p.GetProperty("b"), Is.EqualTo("\"quotat ; ions\""));
            Assert.That(p.GetProperty("c"), Is.EqualTo("carriage returns\r\n"));
            Assert.That(p.GetProperty("d"), Is.EqualTo(";;; '' ;;;"));
        }

        [Test]
        public void to_string_nominal() {
            IProperties p = Properties.FromValue(new {
                                                     a = 420, b = "cool", c = false
                                                 });
            Assert.That(p.ToString(), Is.EqualTo("a=420;b=cool;c=False"));
        }

        [Test]
        public void streaming_source_adapter() {
            Assert.That(StreamingSource.Create(typeof(Properties)),
                        Is.InstanceOf<PropertiesStreamingSource>());
        }
    }
}
