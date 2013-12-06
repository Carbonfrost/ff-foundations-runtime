//
// - AdapterRoleDataTests.cs -
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
using System.Linq.Expressions;

using Carbonfrost.Commons.Shared.Runtime;
using NUnit.Framework;

namespace Tests.Runtime {

    [TestFixture]
    public class AdapterRoleDataTests {

        static class MyAdapterRoles {

            public const string Role1 = "Role1";
            public const string Role2 = "Role2";
            public const string Role3 = "Role3";

            public static bool IsRole1Type(Type a) {
                return true;
            }

            public static bool IsRole2Type(Type a) {
                return false;
            }

            public static class Role1Functions {

                public static readonly Expression<Func<bool, bool, int>> Hello = null;
                public static readonly Expression<Func<bool, bool, object>> Goodbye = null;
            }

        }

        class Role1Implementer {

            public int Hello(bool b, bool c) {
                return 0;
            }
        }

        class Role1ImplementerTooManyArguments {

            public int Hello(bool b, bool c, bool d) {
                return 0;
            }
        }

        class Role1ImplementerDifferentReturnType {

            public string Goodbye(bool b, bool c) {
                return "";
            }
        }

        [Test]
        public void should_() {
            var results = new Dictionary<string, AdapterRoleData>();
            foreach (var kvp in AdapterRoleData.FromStaticClass(typeof(MyAdapterRoles)))
                results.Add(kvp.Key, kvp.Value);


            Assert.That(results["Role1"].IsValidAdapter(null), Is.True);
            Assert.That(results["Role2"].IsValidAdapter(null), Is.False);
            Assert.That(results["Role3"].IsValidAdapter(null), Is.True);
            Assert.That(results["Role1"].FindAdapterMethod(typeof(Role1Implementer), "Hello"),
                        Is.EqualTo(typeof(Role1Implementer).GetMethod("Hello")));
            Assert.That(results["Role1"].FindAdapterMethod(typeof(Role1ImplementerTooManyArguments), "Hello"), Is.Null);

        }

        [Test]
        public void adapter_method_different_return_type() {
            var results = new Dictionary<string, AdapterRoleData>();
            foreach (var kvp in AdapterRoleData.FromStaticClass(typeof(MyAdapterRoles)))
                results.Add(kvp.Key, kvp.Value);

            Assert.That(results["Role1"].FindAdapterMethod(typeof(Role1ImplementerDifferentReturnType), "Goodbye"),
                        Is.EqualTo(typeof(Role1ImplementerDifferentReturnType).GetMethod("Goodbye")));

        }

        [Test]
        public void should_pickup_role_names() {
            var results = new Dictionary<string, AdapterRoleData>();
            foreach (var kvp in AdapterRoleData.FromStaticClass(typeof(MyAdapterRoles)))
                results.Add(kvp.Key, kvp.Value);

            Assert.That(results.Count, Is.EqualTo(3));
            Assert.That(results.Keys, Contains.Item("Role1"));
            Assert.That(results.Keys, Contains.Item("Role2"));
            Assert.That(results.Keys, Contains.Item("Role3"));
        }

        [Test]
        public void should_pickup_role_names2() {
            var results = new Dictionary<string, AdapterRoleData>();
            foreach (var kvp in AdapterRoleData.FromStaticClass(typeof(AdapterRole)))
                results.Add(kvp.Key, kvp.Value);

            Assert.That(results.Keys, Contains.Item("Builder"));
            Assert.That(results.Keys, Contains.Item("StreamingSource"));
            Assert.That(results.Keys, Contains.Item("ActivationProvider"));
        }
    }
}

