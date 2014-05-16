//
// - StatusAppenderTest.cs -
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
using Carbonfrost.Commons.Shared;
using Carbonfrost.Commons.Shared.Runtime;
using NUnit.Framework;

namespace Tests.Runtime {

    [TestFixture]
    public class StatusAppenderTest {

        [Test]
        public void highest_error_level_wins() {
            StatusAppender sa = new StatusAppender();
            sa.AppendError("error", null);
            sa.AppendWarning("warning");

            Assert.That(sa.Level, Is.EqualTo(Severity.Error));
            Assert.That(sa.Children.Count, Is.EqualTo(2));
        }

        [Test]
        public void Filtered_should_apply_to_error_level() {
            var sa = new StatusAppender();
            var filtered = StatusAppender.ErrorsOnly(sa);
            filtered.AppendError("error", null);
            filtered.AppendWarning("warning");

            Assert.That(filtered.Children.Count, Is.EqualTo(1));
            Assert.That(sa.Children.Count, Is.EqualTo(1));
        }

        [Test]
        public void Filtered_should_apply_predicate() {
            StatusAppender sa = new StatusAppender();
            sa.AppendError("error", null);
            sa.AppendWarning("warning");

            Assert.That(sa.Level, Is.EqualTo(Severity.Error));
        }
    }
}

