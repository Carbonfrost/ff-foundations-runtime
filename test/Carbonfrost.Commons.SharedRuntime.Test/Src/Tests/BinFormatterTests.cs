//
// - BinFormatterTests.cs -
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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework;

namespace Tests {

    [TestFixture]
    public class BinFormatterTests {

        // N.B. These tests are to ensure that our logic for the BinaryFormatter-based
        // streaming sources is appropriate

        [Serializable]
        public class S {
            public object opaque;
            public Type value;
            public Int32 something;
        }

        [Test]
        public void get_bin_magic() {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, new S { opaque = "object", value = typeof(string), something = 1});
            byte[] b = ms.ToArray();
            int index = 0;
            Assert.That(b[index++], Is.EqualTo((byte) 0));
            Assert.That(b[index++], Is.EqualTo((byte) 1));
            Assert.That(b[index++], Is.EqualTo((byte) 0));
            Assert.That(b[index++], Is.EqualTo((byte) 0));
            Assert.That(b[index++], Is.EqualTo((byte) 0));
            Assert.That(b[index++], Is.EqualTo((byte) 255));
            Assert.That(b[index++], Is.EqualTo((byte) 255));
            Assert.That(b[index++], Is.EqualTo((byte) 255));
            Assert.That(b[index++], Is.EqualTo((byte) 255));
        }

    }
}
