//
// - AdaptableProvidersTests.cs -
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
using System.Linq.Expressions;
using Carbonfrost.Commons.Shared;
using Carbonfrost.Commons.Shared.Runtime;
using Carbonfrost.Commons.Shared.Runtime.Components;
using NUnit.Framework;

namespace Tests.Runtime {

    [TestFixture]
    public class AdaptableProvidersTests {

        [Test]
        public void root_providers() {
            var cs = AppDomain.CurrentDomain.GetProviderTypes();

            Assert.That(cs, Contains.Item(typeof(IActivationFactory)));
            Assert.That(cs, Contains.Item(typeof(IAdapterFactory)));
            Assert.That(cs, Contains.Item(typeof(IAssemblyInfoFilter)));
            Assert.That(cs, Contains.Item(typeof(IService)));
            Assert.That(cs, Contains.Item(typeof(StreamingSource)));
            Assert.That(cs, Contains.Item(typeof(ComponentStore)));
        }

        [Test]
        public void non_existent_provider_type() {
            var cs = AppDomain.CurrentDomain.GetProviders<Glob>();
            Assert.That(cs, Is.Empty);
        }

        [Test]
        public void non_existent_provider_type_by_name() {
            var cs = AppDomain.CurrentDomain.GetProvider<StreamingSource>("J");
            Assert.That(cs, Is.Null);
        }

        [Test]
        public void provider_names_case_insensitive() {
            var cs1 = AppDomain.CurrentDomain.GetProvider<ComponentStore>("gac");
            var cs2 = AppDomain.CurrentDomain.GetProvider<ComponentStore>("Gac");
            Assert.That(cs2, Is.SameAs(ComponentStore.Gac));
            Assert.That(cs1, Is.SameAs(ComponentStore.Gac));
        }

        [Test]
        public void get_provider_name() {
            var cs = AppDomain.CurrentDomain.GetProviderName(typeof(ComponentStore), ComponentStore.Gac);
            Assert.That(cs.LocalName, Is.EqualTo("gac"));
        }

        [Test]
        public void get_provider_using_attribute_criteria() {
            var cs = AppDomain.CurrentDomain.GetProvider(typeof(StreamingSource),
                                                         new { contentType = ContentTypes.BinaryFormatterBase64 });
            Assert.That(cs,
                        Is.SameAs(StreamingSource.BinaryFormatterBase64));
        }

        [Test]
        public void get_provider_using_assembly_criteria() {
            var cs = AppDomain.CurrentDomain.GetProviders(typeof(StreamingSource),
                                                          new { Assembly = typeof(object).Assembly });
            Assert.That(cs, Is.Empty);

            cs = AppDomain.CurrentDomain.GetProviders(typeof(StreamingSource),
                                                      new { Assembly = typeof(StreamingSource).Assembly }).ToList();
            Assert.That(cs, Has.Count.EqualTo(5));
        }

        [Test]
        public void GetProviderMember_using_attribute_criteria() {
            var cs = AppDomain.CurrentDomain.GetProviderMember(typeof(StreamingSource),
                                                               new { contentType = ContentTypes.BinaryFormatterBase64 });
            var field = typeof(StreamingSource).GetField("BinaryFormatterBase64");
            Assert.That(cs,
                        Is.SameAs(field));
        }

        [Test]
        public void GetProviderMetadata_using_provider_brief() {
            var cs1 = AppDomain.CurrentDomain.GetProviderMetadata(StreamingSource.BinaryFormatterBase64);
            var cs2 = AppDomain.CurrentDomain.GetProviderMetadata(typeof(StreamingSource), StreamingSource.BinaryFormatterBase64);
            Assert.That(cs1, Is.SameAs(cs2));
        }

        [Test]
        public void provider_names_by_qualified_name() {
            var name = NamespaceUri.Create(Xmlns.SharedRuntime2008) + "null";
            var cs1 = AppDomain.CurrentDomain.GetProvider<ComponentStore>(name);
            Assert.That(cs1, Is.SameAs(ComponentStore.Null));
        }

        [Test]
        public void providers_using_static_fields_component_store() {
            var cs = AppDomain.CurrentDomain.GetProviders<ComponentStore>();

            Assert.That(cs, Contains.Item(ComponentStore.Gac));
            Assert.That(cs, Contains.Item(ComponentStore.Null));
        }

        [Test]
        public void provider_names_by_name_repeatable() {
            var name = "null";
            var cs1 = AppDomain.CurrentDomain.GetProvider<ComponentStore>(name);
            var cs2 = AppDomain.CurrentDomain.GetProvider<ComponentStore>(name);

            Assert.That(cs1, Is.SameAs(ComponentStore.Null));
            Assert.That(cs2, Is.SameAs(ComponentStore.Null));
        }

        [Test]
        public void provider_illegal_base_type() {
            // Test for illegal providers
            var s = AppDomain.CurrentDomain.GetProvider<IRuntimeComponent>("TestComponent2");
            Assert.That(s, Is.Null);

            var sa = StatusAppender.ForType(typeof(Adaptable));
            Assert.That(sa.Children.Count, Is.EqualTo(1));
            Assert.That(sa.Children[0].Message,
                        Is.StringMatching(@"Invalid provider `Tests\.Runtime\.IllegalComponent': Given .+"));
        }
    }

    [RuntimeComponentUsage(Name = "TestComponent2")]
    public class IllegalComponent {}
}

