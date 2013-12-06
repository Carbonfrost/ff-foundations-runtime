//
// - FrameworkServiceContainerTests.cs -
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
using System.ComponentModel.Design;
using System.Linq;

using Carbonfrost.Commons.Shared;
using Carbonfrost.Commons.Shared.Runtime;
using NUnit.Framework;

namespace Tests.Runtime {

    [TestFixture]
    public class FrameworkServiceContainerTests {

        class ServiceA {}
        class ServiceB {}
        class ServiceC : ServiceB {}

        [Test]
        public void add_singleton_service() {
            FrameworkServiceContainer c = new FrameworkServiceContainer();
            ServiceA a = new ServiceA();
            c.AddService(typeof(ServiceA), a);

            Assert.That(c.GetService<ServiceA>(), Is.SameAs(a));
        }

        [Test]
        public void add_multiple_services() {
            FrameworkServiceContainer c = new FrameworkServiceContainer();
            ServiceA a1 = new ServiceA();
            ServiceA a2 = new ServiceA();
            c.AddService(typeof(ServiceA), a1);
            c.AddService("other", a2);

            Assert.That(c.GetServices<ServiceA>(), Contains.Item(a1));
            Assert.That(c.GetServices<ServiceA>(), Contains.Item(a2));
        }

        [Test]
        public void dedupe_by_type_and_by_name_services() {
            FrameworkServiceContainer c = new FrameworkServiceContainer();
            ServiceA a1 = new ServiceA();
            ServiceA a2 = new ServiceA();
            c.AddService(typeof(ServiceA), a1);
            c.AddService("hello", a1);
            c.AddService("other", a2);

            Assert.That(c.GetServices<ServiceA>().Count(), Is.EqualTo(2));
        }

        [Test]
        public void dedupe_by_name_services() {
            FrameworkServiceContainer c = new FrameworkServiceContainer();
            ServiceA a1 = new ServiceA();
            c.AddService("hello", a1);
            c.AddService("other", a1);

            Assert.That(c.GetServices<ServiceA>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void dedupe_by_type_services_polymorphic() {
            FrameworkServiceContainer c = new FrameworkServiceContainer();
            ServiceC d = new ServiceC();
            c.AddService(typeof(ServiceB), d);
            c.AddService(typeof(ServiceC), d);

            Assert.That(c.GetServices<ServiceB>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void apply_polymorphism_to_services() {
            FrameworkServiceContainer c = new FrameworkServiceContainer();
            ServiceB b = new ServiceB();
            ServiceC d = new ServiceC();
            c.AddService(typeof(ServiceB), b);
            c.AddService(typeof(ServiceC), d);

            Assert.That(c.GetServices<ServiceB>().Count(), Is.EqualTo(2));
            Assert.That(c.GetServices<ServiceB>(), Contains.Item(d));
        }

        [Test]
        public void obtains_service_container_trivially() {
            FrameworkServiceContainer c = new FrameworkServiceContainer();
            Assert.That(c.GetService(typeof(IServiceContainer)), Is.SameAs(c));
            Assert.That(c.GetService(typeof(IServiceProvider)), Is.SameAs(c));
            Assert.That(c.GetService(typeof(IServiceProviderExtension)), Is.SameAs(c));
        }

        [Test]
        public void obtains_service_container_extension_trivially() {
            FrameworkServiceContainer c = new FrameworkServiceContainer();
            Assert.That(c.GetService(typeof(IServiceContainerExtension)), Is.SameAs(c));
            Assert.That(c.GetService(typeof(FrameworkServiceContainer)), Is.SameAs(c));
        }

    }
}
