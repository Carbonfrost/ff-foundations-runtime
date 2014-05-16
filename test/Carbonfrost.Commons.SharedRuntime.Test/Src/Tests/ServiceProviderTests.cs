//
// - ServiceProviderTests.cs -
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
using Carbonfrost.Commons.Shared;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class ServiceProviderTests
    {

        [Test]
        public void Compose_should_select_first_result_in_GetService()
        {
            var value1 = new Exception();
            var value2 = new Exception();
            var sp1 = ServiceProvider.FromValue(value1);
            var sp2 = ServiceProvider.FromValue(value2);
            var sp = ServiceProvider.Compose(sp1, sp2);

            Assert.That(sp.GetService(typeof(Exception)),
                        Is.SameAs(value1));
        }

        [Test]
        public void Compose_should_combine_results_in_GetServices()
        {
            var value1 = new Exception();
            var value2 = new Exception();
            var sp1 = ServiceProvider.FromValue(value1).Extend();
            var sp2 = ServiceProvider.FromValue(value2).Extend();
            var sp = ServiceProvider.Compose(sp1, sp2);

            Assert.That(sp.GetServices(typeof(Exception)),
                        Is.EquivalentTo(new[] { value1, value2 }));
        }

        [Test]
        public void Compose_should_return_null_on_empty()
        {
            var sp = ServiceProvider.Compose(Enumerable.Empty<IServiceProvider>());
            Assert.That(sp, Is.SameAs(ServiceProvider.Null));
        }

        [Test]
        public void Compose_should_return_null_on_empty_extended()
        {
            var sp = ServiceProvider.Compose(Enumerable.Empty<IServiceProviderExtension>());
            Assert.That(sp, Is.SameAs(ServiceProvider.Null));
        }

        [Test]
        public void Compose_should_return_null_on_null_instances()
        {
            var sp = ServiceProvider.Compose(null, null, null);
            Assert.That(sp, Is.SameAs(ServiceProvider.Null));
        }
    }
}


