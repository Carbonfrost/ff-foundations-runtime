//
// - FileLocationTests.cs -
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

namespace Tests.Runtime {

    [TestFixture]
    public class FileLocationTests {

        [Test]
        public void empty_file_name_to_string() {
            Assert.That(FileLocation.Empty.ToString(),
                        Is.EqualTo(""));
        }

        [Test]
        public void empty_location_normal_file_name_to_string() {
            Assert.That(new FileLocation(-1, -1, "cool.txt").ToString(),
                        Is.EqualTo("cool.txt"));
        }

        [Test]
        public void empty_file_name_normal_location_to_string() {
            Assert.That(new FileLocation(2, 1, "cool.txt").ToString(),
                        Is.EqualTo("cool.txt (2, 1)"));
        }
    }
}
