//
// - ActivationTests.cs -
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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using Carbonfrost.Commons.ComponentModel;
using Carbonfrost.Commons.Shared;
using Carbonfrost.Commons.Shared.Runtime;
using NUnit.Framework;

namespace Tests.Runtime {

    [TestFixture]
    public class ActivationTests {

        struct UriContext : IUriContext {
            private Uri uri;

            public UriContext(string uri) {
                this.uri = new Uri(uri);
            }
            public Uri BaseUri {
                get { return this.uri; }
                set { }
            }
        }

        class A : IUriContext {
            public Uri BaseUri { get; set; }
        }

        class B {

            private int c;

            public B(int a, int c) {
                this.A = a;
                this.c = c * 10;
            }

            public int A { get; set; }
            public int C {
                get { return c; }
                set { c = value * 2; }
            }
        }


        [Test]
        public void parameters_and_properties_activate_once() {
            IDictionary<string, object> values = new Dictionary<string, object>();
            values.Add("c", 50);

            // C should be activated once via parameter, not property
            B b = Activation.CreateInstance<B>(values);
            Assert.That(b.C, Is.EqualTo(50 * 10));
        }

        [Test]
        public void numeric_constructor_activation() {
            B b = Activation.CreateInstance<B>(Properties.FromArray(4, 20));
            Assert.That(b.A, Is.EqualTo(4));
            Assert.That(b.C, Is.EqualTo(20 * 10));
        }

        // TODO Tests: - missing ctor args, - optonal ctor args, - duplicated key-value pair

        [Test]
        public void implicit_default_public_constructor_is_used() {
            Assert.That(Adaptable.GetActivationConstructor(typeof(A)), Is.Not.Null);
            Assert.That(typeof(A).GetConstructors(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Length == 1);
        }

        [Test]
        public void should_apply_uri_context() {
            ServiceContainer sc = new ServiceContainer();
            sc.AddService(typeof(IUriContext), new UriContext("http://carbonfrost.com"));

            A a = Activation.CreateInstance<A>(sc);
            Assert.That(a.BaseUri, Is.EqualTo(new Uri("http://carbonfrost.com")));
        }

        [Test]
        public void should_set_subscription_values() {
            ServiceContainer sc = new ServiceContainer();

            D d = Activation.CreateInstance<D>(sc);
            Assert.That(d.Container, Is.SameAs(sc));
        }

        [Test]
        public void should_set_subscription_values_inherited() {
            ServiceContainer sc = new ServiceContainer();

            E e = Activation.CreateInstance<E>(sc);
            Assert.That(e.Container, Is.SameAs(sc));
        }

        [Test]
        public void should_obtain_service_provider_current() {
            ServiceContainer sc = new ServiceContainer();

            E e = Activation.CreateInstance<E>(sc);
            Assert.That(e.captured, Is.SameAs(sc));
        }

        class C {

            private B b;
            private A a;

            public B B { get { return b; } }
            public A A { get { return a; } }

            [ActivationConstructor]
            public C(B b, A a) {
                if (b == null)
                    throw new ArgumentNullException("b");
                if (a == null)
                    throw new ArgumentNullException("a");

                this.b = b;
                this.a = a;
            }
        }

        class D {

            [Subscribe]
            private IServiceContainer sp;

            public IServiceContainer Container { get { return sp; } }
            public string A { get; set; }
            public string B { get { return null; } }

            public D() {
                sp = null;
            }
        }

        class E : D {

            internal IServiceProvider captured;

            public E() {
                captured = ServiceProvider.Current;
            }

        }

        [Test]
        public void parameter_matching_is_case_insensitive() {
            C c = (C) Activation.CreateInstance(typeof(C), Properties.FromValue(new { A = new A(), B = new B(4, 4) }), null, null);
        }

        [Test]
        public void ignore_readonly_properties() {
            D d = (D) Activation.CreateInstance(typeof(D), Properties.FromValue(new { A = "a", B = "b" }), null, null);
            Assert.That(d.A, Is.EqualTo("a"));
            Assert.That(d.B, Is.Null);
        }

        [Test]
        public void ignore_duplicated_properties() {
            D d = (D) Activation.CreateInstance(typeof(D), Properties.FromValue(new { A = "a", B = "b" }), null, null);
            Assert.That(d.A, Is.EqualTo("a"));
            Assert.That(d.B, Is.Null);
        }

        class DumbActivationFactory : IActivationFactory {

            public object CreateInstance(Type type,
                                         IEnumerable<KeyValuePair<string, object>> values,
                                         IPopulateComponentCallback callback,
                                         IServiceProvider serviceProvider, params Attribute[] attributes) {
                return Glob.Anything;
            }
        }

        [Test]
        public void request_service_provider_activation_factory() {
            Assert.That(
                Activation.CreateInstance(typeof(D), serviceProvider: ServiceProvider.FromValue(new DumbActivationFactory()))
                is Glob);
        }
    }
}
