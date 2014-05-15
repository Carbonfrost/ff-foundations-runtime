//
// - DataStreamContextTests.cs -
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
using System.Security.Cryptography;
using System.Text;
using Carbonfrost.Commons.Shared.Runtime;
using NUnit.Framework;

namespace Tests.Runtime {

    [TestFixture]
    public class DataStreamContextTests {

        [Test]
        public void sanity_checks() {
            byte[] data = new byte[32];
            RandomNumberGenerator.Create().GetNonZeroBytes(data);
            string bytes = Convert.ToBase64String(data);
            Uri u = new Uri("data:application/octet-stream;base64," + bytes);

            Assert.That(u.PathAndQuery, Is.EqualTo("application/octet-stream;base64," + bytes));

        }

        [Test]
        public void should_update_uri() {
            // URI is updated when we write to stream
            StreamContext sc = StreamContext.FromSource(new Uri("data:application/octet-stream;base64,"));
            byte[] data = { 0x42, 0x20, 0xFF };
            sc.OpenWrite().Write(data, 0, 3);

            Assert.That(sc.Uri, Is.EqualTo(
                new Uri("data:application/octet-stream;base64," + Convert.ToBase64String(data))));
        }

        [Test]
        public void should_get_data_uri() {
            byte[] data = new byte[32];
            RandomNumberGenerator.Create().GetNonZeroBytes(data);

            string bytes = Convert.ToBase64String(data);
            StreamContext sc = StreamContext.FromSource(new Uri("data:application/octet-stream;base64," + bytes));

            Assert.That(sc.OpenRead().ReadAllBytes(), Is.EquivalentTo(Convert.FromBase64String(bytes)));
        }

        [Test]
        public void should_get_data_uri_empty_() {
            byte[] data = new byte[0];
            string bytes = Convert.ToBase64String(data);
            StreamContext sc = StreamContext.FromSource(new Uri("data:application/octet-stream;base64," + bytes));

            Assert.That(sc.OpenRead().ReadAllBytes(), Is.EquivalentTo(Convert.FromBase64String(bytes)));
        }

        [Test]
        public void should_get_data_uri_empty_string() {
            byte[] data = new byte[0];
            string bytes = Convert.ToBase64String(data);
            StreamContext sc = StreamContext.FromSource(new Uri("data:application/octet-stream;base64,"));

            Assert.That(sc.OpenRead().ReadAllBytes(), Is.EquivalentTo(Convert.FromBase64String(bytes)));
        }

        [Test]
        public void should_get_content_type() {
            byte[] data = new byte[0];
            const string CONTENT_TYPE = "application/x-carbonfrost-hwd";
            string bytes = Convert.ToBase64String(data);
            StreamContext sc = StreamContext.FromSource(new Uri("data:" + CONTENT_TYPE + ";base64,"));
            Assert.That(sc.ContentType.ToString(), Is.EqualTo(CONTENT_TYPE));
            Assert.That(sc.Uri.ToString(), Contains.Substring(";base64"));
        }

        [Test]
        public void should_get_read_stream_twice() {
            string bytes = Convert.ToBase64String(Encoding.UTF8.GetBytes("abc"));
            var sc = StreamContext.FromSource(new Uri("data:text/plain;base64," + bytes));
            Assert.That(sc.ReadAllText(), Is.EqualTo("abc"));
            Assert.That(sc.ReadAllText(), Is.EqualTo("abc"));
        }

        [Test]
        public void should_get_from_text() {
            StreamContext sc = StreamContext.FromText("abc");
            Assert.That(sc.ContentType.ToString(), Is.EqualTo("text/plain; charset=utf-8"));
            Assert.That(() => sc.ContentType.Parameters["base64"], Throws.InstanceOf<KeyNotFoundException>());
            Assert.That(sc.ReadAllText(), Is.EqualTo("abc"));
        }

        [Test]
        public void should_allow_optional_content_type_and_uri_encoding() {
            var sc = StreamContext.FromSource(new Uri("data:,A%20brief%20note"));
            Assert.That(sc.ContentType.ToString(), Is.EqualTo("text/plain"));
            Assert.That(sc.ReadAllText(), Is.EqualTo("A brief note"));
        }

        [Test]
        public void should_allow_uri_encoding() {
            var sc = StreamContext.FromSource(new Uri("data:text/p,A%20brief%20note"));
            Assert.That(sc.ContentType.ToString(), Is.EqualTo("text/p"));
            Assert.That(sc.ReadAllText(), Is.EqualTo("A brief note"));
            Assert.That(sc.Uri.OriginalString, Is.EqualTo("data:text/p,A%20brief%20note"));
        }

        [Test]
        public void should_allow_non_standard_non_uri() {
            var sc = StreamContext.FromSource(new Uri("data:text/html,Encode s p aces"));
            Assert.That(sc.ReadAllText(), Is.EqualTo("Encode s p aces"));
            Assert.That(sc.Uri.OriginalString, Is.EqualTo("data:text/html,Encode%20s%20p%20aces"));
        }

    }
}
