//
// - GlobCombineTest.cs -
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
using Carbonfrost.Commons.Shared;
using NUnit.Framework;

namespace Tests {

    [TestFixture]
    public class GlobCombineTest {

        [Test]
        public void should_combine_into_semicolon_syntax() {
            Glob a = Glob.Parse("*.cs");
            Glob b = Glob.Parse("*.vb");
            Glob c = Glob.Combine(a, b);

            Assert.That(c.ToString(), Is.EqualTo("*.cs;*.vb"));
        }

        [Test]
        public void should_combine_into_semicolon_syntax_2() {
            Glob a = Glob.Parse("*.cs");
            Glob b = Glob.Parse("*.vb");
            Glob c = Glob.Parse("*.java");

            Glob d = Glob.Combine(a, b, c);

            Assert.That(d.ToString(), Is.EqualTo("*.cs;*.vb;*.java"));
        }

        [Test]
        public void should_conserve_on_reference_equality() {
            Glob a = Glob.Parse("*.java");
            Glob c = Glob.Combine(a, a, a);

            Assert.That(a, Is.SameAs(c));
            Assert.That(c.ToString(), Is.EqualTo("*.java"));
        }

        [Test]
        public void anything_should_win() {
            Glob a = Glob.Parse("**/*.*");
            Glob b = Glob.Parse("*.java");
            Glob c = Glob.Combine(a, b);

            Assert.That(c, Is.SameAs(Glob.Anything));
        }
    }
}
