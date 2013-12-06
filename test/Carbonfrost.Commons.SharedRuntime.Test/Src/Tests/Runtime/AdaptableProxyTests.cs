//
// - AdaptableProxyTests.cs -
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
using System.Linq;
using Carbonfrost.Commons.Shared.Runtime;
using NUnit.Framework;

namespace Tests.Runtime {

    [TestFixture]
    public class AdaptableProxyTests {

        public interface INiladicMethod {
            void Foo();
        }

        public interface IMultipleMethods {
            FileLocation Foo(int a = 0);
            string Bar(bool a = false, string b = null);
            long Baz(string[] other = null);
        }

        public interface IReturnTypeMethod {
            ArgumentException Create(int a);
        }

        public class Alpha {}

        public class ErrorReturnTypeNotExact {
            public ArgumentNullException Create(int a) { return null; }
        }

        public class ErrorMethodPrivate {
            ArgumentNullException Create(int a) { return null; }
        }

        public class ErrorMethodStatic {
            public static ArgumentNullException Create(int a) { return null; }
        }

        public class ErrorParameterMismatch {
            public ArgumentNullException Create() { return null; }
        }

        public class ErrorParameterMissing {
            public ArgumentNullException Create(long s) { return null; }
        }

        public abstract class AbstractClass {
            public int NoOverride() { return 47; }
            public virtual int Bar() { return 47; }
            public abstract FileLocation Foo(int a = 0, int b = 0);
        }

        public class NiladicMethodLike {

            public void Foo() {
                throw new LicenseException(typeof(String));
            }
        }

        public class MultipleMethodsLike {

            public FileLocation Foo(int a) {
                throw new LicenseException(typeof(string));
            }
            public string Bar(bool a, string b) {
                throw new WarningException();
            }
            public long Baz(string[] other) {
                return 47;
            }
        }

        public class AbstractClassLike {
            public int NoOverride() { return 47; }
            public int Bar() { return 69; }

            public FileLocation Foo(int a, int b) {
                throw new NotFiniteNumberException();
            }
        }

        public class PropertyUsageLike {
            public string Name { get; set; }

            public PropertyUsageLike() {
                this.Name = "George";
            }
        }

        [Test]
        public void nominal() {
            var result = Adaptable.Implement<INiladicMethod>(new NiladicMethodLike());
            Assert.That(() => result.Foo(), Throws.InstanceOf(typeof(LicenseException)));
        }

        [Test]
        public void proxy_with_multiple_methods() {
            var result = Adaptable.Implement<IMultipleMethods>(new MultipleMethodsLike());
            Assert.That(() => result.Foo(), Throws.InstanceOf(typeof(LicenseException)));
            Assert.That(() => result.Bar(), Throws.InstanceOf(typeof(WarningException)));
            Assert.That(result.Baz(), Is.EqualTo(47));

            Type resultType = result.GetType();
            Type sourceType = typeof(MultipleMethodsLike);
            Assert.That(sourceType.GetProperties().Length, Is.EqualTo(resultType.GetProperties().Length));
            Assert.That(sourceType.GetMethods().Length, Is.EqualTo(resultType.GetMethods().Length));
        }

        [Test]
        public void require_public_method() {
            Assert.That(Adaptable.TryImplement<IReturnTypeMethod>(new ErrorMethodPrivate()),
                        Is.Null);
        }

        [Test]
        public void require_static_method() {
            Assert.That(Adaptable.TryImplement<IReturnTypeMethod>(new ErrorMethodStatic()),
                        Is.Null);
        }

        [Test]
        public void require_exact_return_type() {
            Assert.That(Adaptable.TryImplement<IReturnTypeMethod>(new ErrorReturnTypeNotExact()),
                        Is.Null);
        }

        [Test]
        public void require_exact_parameter_types() {
            Assert.That(Adaptable.TryImplement<IReturnTypeMethod>(new ErrorParameterMissing()),
                        Is.Null);
        }

        [Test]
        public void require_exact_parameter_count() {
            Assert.That(Adaptable.TryImplement<IReturnTypeMethod>(new ErrorParameterMismatch()),
                        Is.Null);
        }

        [Test]
        public void proxy_abstract_class() {
            var result = Adaptable.Implement<AbstractClass>(new AbstractClassLike());
            Assert.That(result.Bar(), Is.EqualTo(69));
            Assert.That(() => result.Foo(), Throws.InstanceOf(typeof(NotFiniteNumberException)));
        }

        [Test]
        public void ensure_reuse_types() {
            var a = Adaptable.Implement<INiladicMethod>(new NiladicMethodLike());
            var b = Adaptable.Implement<INiladicMethod>(new NiladicMethodLike());

            Assert.That(a.GetType(), Is.EqualTo(b.GetType()));
            Assert.That(AppDomain.CurrentDomain.GetAssemblies().Where(t => t.GetName().Name.StartsWith("DynamicProxy")).Count(),
                        Is.EqualTo(1));

        }

        [Test]
        public void try_implement_returns_null() {
            var a = new Alpha();
            INiladicMethod result = a.TryImplement<INiladicMethod>();
            Assert.That(result, Is.Null);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void implement_fallback_none_fails() {
            var a = new Alpha();
            a.Implement<INiladicMethod>(FallbackBehavior.None);
        }

        [Test]
        public void implement_fallback_default_returns_default() {
            var a = new Alpha();
            IMultipleMethods result = a.Implement<IMultipleMethods>(FallbackBehavior.CreateDefault);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Bar(false, null), Is.EqualTo(null));
            Assert.That(result.Baz(null), Is.EqualTo(0));
            Assert.That(result.Foo(0), Is.EqualTo(default(FileLocation)));
        }

        [Test]
        public void implement_fallback_throw_should_throw() {
            var a = new Alpha();
            IMultipleMethods result = a.Implement<IMultipleMethods>(FallbackBehavior.ThrowException);
            Assert.That(result, Is.Not.Null);
            Assert.That(() => result.Baz(null), Throws.InstanceOf(typeof(NotImplementedException)));
        }

    }
}
