//
// - AdaptableBuilderTests.cs -
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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Carbonfrost.Commons.Shared;
using Carbonfrost.Commons.Shared.Runtime;
using Carbonfrost.Commons.Shared.Runtime.Components;
using NUnit.Framework;

namespace Tests.Runtime {

    [TestFixture]
    public class AdaptableBuilderTests {

        [Builder(typeof(ABuilder3))]
        [Adapter(typeof(ABuilder4), "builder")]
        class A {}

        [Adapter(typeof(ABuilder3), "Builder")]
        class B {}
        class ABuilder3 {}
        class ABuilder4 {}

        [Builder(typeof(C))] // Not allowed to use self
        class C {}

        [Builder(typeof(A))]
        class D {} // Not allowed since A specifies a Builder

        class E {}
        class EBuilder { // Its implicit builder
            public E Build(IServiceProvider serviceProvider) { return default(E); }
        }

        class F {}

        [Builder(typeof(ABuilder3))]
        class FBuilder {} // Would be the implicit builder except
        // that it itself specifies a builder

        [Test]
        public void should_ignore_implicit_builder_on_referential_error() {
            // Cannot be F -> FBuilder due to FBuilder defining a builder
            Type result = Adaptable.GetImplicitBuilderType(typeof(F));

            Assert.That(result, Is.Null);
        }

        [Test]
        public void should_apply_implicitly_builder() {
            Assert.That(Adaptable.IsValidAdapter(typeof(EBuilder), "Builder"));
            Assert.That(Adaptable.GetBuilderType(typeof(E)),
                        Is.EqualTo(typeof(EBuilder)));
            Assert.That(Adaptable.GetAdapterType(typeof(E), "Builder"),
                        Is.EqualTo(typeof(EBuilder)));
        }

//        [Test]
//        public void specified_builder_cannot_specify_another_builder() {
//            try {
//                Adaptable.GetBuilderType(typeof(D));
//
//            } catch (Exception ex) {
//                Assert.That(ex, Is.InstanceOf(typeof(FormatException)));
//                Assert.That(ex.Message, Is.EqualTo(SR.BuilderCannotBeSelf(typeof(D))));
//                return;
//            }
//
//            Assert.Fail("Expected an exception to be thrown.");
//        }

//        [Test]
//        public void cannot_use_self_as_builder_adapter() {
//            Assert.That(() => {
//                            Adaptable.GetBuilderType(typeof(C));
//                        }, Throws.InstanceOf(typeof(FormatException)));
//        }

        [Test]
        public void role_names_are_case_sensitive() {
            Type correct = typeof(ABuilder3);
            Type correct2 = typeof(ABuilder4);

            Assert.That(Adaptable.GetAdapterType(typeof(A), "Builder"), Is.EqualTo(correct));
            Assert.That(Adaptable.GetAdapterType(typeof(A), "builder"), Is.EqualTo(correct2));
            Assert.That(Adaptable.GetBuilderType(typeof(A)), Is.EqualTo(correct));
        }

        [Test]
        public void should_be_equivalent_to_name_adapters_or_use_builder_attribute() {
            Type correct = typeof(ABuilder3);
            // Using the BuilderAttribute
            Assert.That(Adaptable.GetBuilderType(typeof(A)), Is.EqualTo(correct));
            Assert.That(Adaptable.GetAdapterType(typeof(A), "Builder"), Is.EqualTo(correct));

            // Using the AdapterAttribute
            Assert.That(Adaptable.GetBuilderType(typeof(B)), Is.EqualTo(correct));
            Assert.That(Adaptable.GetAdapterType(typeof(B), "Builder"), Is.EqualTo(correct));
        }

    }
}
