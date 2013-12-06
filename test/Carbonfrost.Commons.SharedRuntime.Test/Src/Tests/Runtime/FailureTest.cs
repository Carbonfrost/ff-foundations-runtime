//
// - FailureTest.cs -
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
    public class FailureTest {

        static readonly int[] EMPTY_ARRAY = new int[0];
        static readonly int[] ARRAY_12 = new int[12];

        [Test]
        public void not_enough_space_in_array_parity_with_array_copy() {
            // Ensure that the same exception type is thrown as Array.Copy,
            // since this is the main use case

            Exception arrayException = null;

            try {
                Array.Copy(ARRAY_12, EMPTY_ARRAY, 12);
            } catch (Exception ex) {
                arrayException = ex;
            }

            Assert.That(Failure.NotEnoughSpaceInArray("argumentName", EMPTY_ARRAY),
                        Is.InstanceOf(arrayException.GetType()));
        }
    }
}
