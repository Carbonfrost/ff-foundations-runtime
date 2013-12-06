//
// - StreamContextTests.cs -
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
using Carbonfrost.Commons.Shared.Runtime;
using NUnit.Framework;

namespace Tests.Runtime {

    [TestFixture]
    public class StreamContextTests {

        [Test]
        public void convert_from_text_encoding() {
            string chars = Convert.ToBase64String(Encoding.UTF32.GetBytes("h"));

            StreamContext sc = StreamContext.FromText("h", Encoding.UTF32);
            Assert.That(sc.Uri.ToString(),
                        Is.EqualTo("data:text/plain; charset=utf-32;base64," + chars));
        }
    }
}
