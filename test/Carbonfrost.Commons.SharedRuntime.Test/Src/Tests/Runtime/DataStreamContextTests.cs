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
using System.Security.Cryptography;
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
        }
    }
}
