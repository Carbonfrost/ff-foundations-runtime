//
// - AdaptableInformationTests.cs -
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
using System.Security;
using Carbonfrost.Commons.Shared.Runtime;
using Carbonfrost.Commons.Shared.Runtime.Components;
using NUnit.Framework;

namespace Tests.Runtime {



    [TestFixture]
    public class AdaptableInformationTests { // NAme?

        [Test]
        public void eligible_service_types() {
            Assert.IsTrue(Adaptable.IsServiceType(typeof(IUriContext)));
            Assert.IsTrue(Adaptable.IsServiceType(typeof(Exception)));
            Assert.IsFalse(Adaptable.IsServiceType(typeof(int)));
            Assert.IsFalse(Adaptable.IsServiceType(typeof(UriKind)));
            Assert.IsFalse(Adaptable.IsServiceType(typeof(Adaptable)));
        }

        // TODO Check non-reentrancy in activation providers

    }
}

