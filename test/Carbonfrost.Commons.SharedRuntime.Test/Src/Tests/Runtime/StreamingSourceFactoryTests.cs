//
// - StreamingSourceFactoryTests.cs -
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
    public class StreamingSourceFactoryTests {

        [Test]
        public void test_streaming_source_from_properties() {
            var me = StreamingSourceFactory.FromAssembly(typeof(StreamingSource).Assembly);
            var type = me.GetStreamingSourceType(typeof(Properties));
            Assert.That(type, Is.EqualTo(typeof(PropertiesStreamingSource)));
        }
    }

}
