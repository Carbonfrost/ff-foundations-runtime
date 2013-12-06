//
// - ExtensionTests.cs -
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
using System.Text.RegularExpressions;
using Carbonfrost.Commons.Shared;
using NUnit.Framework;

namespace Tests.Runtime {

	[TestFixture]
	public class ExtensionTests {

		[Test]
		public void should_apply_extension_list() {
			Regex regex = Utility.SplitExtensionList(".xrms;.xr?;.*xr");
			Assert.That(regex.IsMatch(".xrms"));
			Assert.That(regex.IsMatch(".xrt"));
			Assert.That(regex.IsMatch(".ilxr"));
			Assert.That(!regex.IsMatch(".txt"));
		}
	}
}
