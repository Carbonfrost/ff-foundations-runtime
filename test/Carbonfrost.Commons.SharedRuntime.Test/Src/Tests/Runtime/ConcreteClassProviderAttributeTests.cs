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
using Carbonfrost.Commons.Shared;
using Carbonfrost.Commons.Shared.Runtime;
using NUnit.Framework;

namespace Tests.Runtime {

    [TestFixture]
    public class ConcreteClassProviderAttributeTests {

        [ConcreteClassImpl]
        interface IP<T> {}
        class P : IP<P>, IQ<P>, IR {}

        [ConcreteClassImpl2]
        interface IQ<T> {}

        [ConcreteClassProvider(typeof(MyConcreteClassProvider))]
        interface IR {}

        [ConcreteClassProvider(typeof(MyConcreteClassProvider))]
        interface IS {} // P doesn't implement IS as required

        class MyConcreteClassProvider : IConcreteClassProvider {

            public Type GetConcreteClass(Type sourceType, IServiceProvider serviceProvider) {
                return typeof(P);
            }
        }

        sealed class ConcreteClassImplAttribute : ConcreteClassProviderAttributeBase {

            protected override Type GetConcreteClassCore(Type sourceType, IServiceProvider serviceProvider) {
                return sourceType.GetGenericArguments()[0];
            }
        }

        sealed class ConcreteClassImpl2Attribute : Attribute, IConcreteClassProvider {

            Type IConcreteClassProvider.GetConcreteClass(Type sourceType, IServiceProvider serviceProvider) {
                return typeof(P);
            }
        }

        [Test]
        public void test_concrete_class_provider_custom_impl() {
            Assert.That(typeof(IP<P>).GetConcreteClass(), Is.EqualTo(typeof(P)));
            Assert.That(typeof(IP<P>).GetConcreteClassProvider(), Is.InstanceOf<ConcreteClassImplAttribute>());
        }

        [Test]
        public void test_concrete_class_provider_via_interface() {
            Assert.That(typeof(IQ<P>).GetConcreteClass(), Is.EqualTo(typeof(P)));
            Assert.That(typeof(IQ<P>).GetConcreteClassProvider(), Is.InstanceOf<ConcreteClassImpl2Attribute>());
        }

        [Test]
        public void test_concrete_class_provider_via_parameter() {
            Assert.That(typeof(IR).GetConcreteClass(), Is.EqualTo(typeof(P)));
            Assert.That(typeof(IR).GetConcreteClassProvider(), Is.InstanceOf<ConcreteClassProviderAttribute>());

            var a = (ConcreteClassProviderAttribute ) typeof(IR).GetConcreteClassProvider();
            Assert.That(a.Value, Is.InstanceOf<MyConcreteClassProvider>());
        }

        [Test]
        public void Constructor_should_throw_on_non_implementer() {
            Assert.That(() => new ConcreteClassProviderAttribute(typeof(Glob)), Throws.ArgumentException);
        }

        [Test]
        public void GetConcreteClass_should_throw_on_non_implementer() {
            Assert.That(() => typeof(IS).GetConcreteClass(), Throws.InstanceOf<FormatException>());
        }
    }
}
