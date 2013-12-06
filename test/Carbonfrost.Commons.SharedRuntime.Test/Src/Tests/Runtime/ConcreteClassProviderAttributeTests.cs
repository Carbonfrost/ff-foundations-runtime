//
// - ConcreteClassProviderAttributeTests.cs -
//
// Copyright 2012 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
    public class ConcreteClassProviderAttributeTests {

        [ConcreteClassImpl]
        interface IP<T> {}
        class P : IP<P> {}

        sealed class ConcreteClassImplAttribute : ConcreteClassProviderAttribute {

            protected override Type GetConcreteClassCore(Type sourceType, IServiceProvider serviceProvider) {
                return sourceType.GetGenericArguments()[0];
            }
        }

        [Test]
        public void test_concrete_class_provider_nominal() {
            Assert.That(typeof(IP<P>).GetConcreteClass(), Is.EqualTo(typeof(P)));
        }
    }
}
