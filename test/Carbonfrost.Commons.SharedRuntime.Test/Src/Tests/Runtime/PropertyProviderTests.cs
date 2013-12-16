//
// - PropertyProviderTests.cs -
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
using Carbonfrost.Commons.Shared.Runtime;
using NUnit.Framework;

namespace Tests {

    [TestFixture]
    public class PropertyProviderTests {

        [Test]
        public void parse_and_render_string_format() {
            var pp = PropertyProvider.FromValue(new {
                                                    planet = "Phazon",
                                                });
            Assert.That(PropertyProvider.Format("Hello, ${planet}", pp),
                        Is.EqualTo("Hello, Phazon"));
        }
    }
}
