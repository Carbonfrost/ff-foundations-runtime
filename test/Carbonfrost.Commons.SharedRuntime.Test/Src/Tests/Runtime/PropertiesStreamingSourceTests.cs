//
// - PropertiesStreamingSourceTests.cs -
//
// Copyright 2014 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

namespace Tests.Runtime
{

    [TestFixture]
    public class PropertiesStreamingSourceTests
    {

        [Test]
        public void Load_should_initialize_properties_existing()
        {
            var sc = StreamingSources.Properties;
            var properties = new Properties();
            sc.Load(StreamContext.FromText("a=b\nc=d"), properties);
            Assert.That(properties["a"], Is.EqualTo("b"));
            Assert.That(properties["c"], Is.EqualTo("d"));
        }
    }
}


