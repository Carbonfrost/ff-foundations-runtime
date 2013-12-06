//
// - AdapterFactoryTests.cs -
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
    public class AdapterFactoryTests {

        [Test]
        public void compose_zero_implies_null() {
            IAdapterFactory fc = AdapterFactory.Compose();
            Assert.That(fc.GetType().FullName, Contains.Substring("Null"));
        }

        [Test]
        public void adapter_factory_t_should_filter_on_role() {
            IAdapterFactory fc = new AdapterFactory<StreamingSource>(AdapterRole.StreamingSource);
            Assert.That(fc.GetAdapterType(typeof(IProperties), "Builder"),
                        Is.Null);
        }

        [Test]
        public void default_adapter_factory_composed_of_all() {
            IAdapterFactory fc = AdapterFactory.Default;
            Assert.That(fc.GetAdapterType(typeof(IProperties), "StreamingSource"),
                        Is.Null);
        }

    }
}
