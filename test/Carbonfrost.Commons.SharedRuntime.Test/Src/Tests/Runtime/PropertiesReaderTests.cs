//
// - PropertiesReaderTests.cs -
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
using System.IO;
using Carbonfrost.Commons.Shared.Runtime;
using NUnit.Framework;

namespace Tests {

	[TestFixture]
	public class PropertiesReaderTests {

	    [Test]
	    public void parse_nominal() {
	        Properties p = ReadAlpha();

	        Assert.That(p.InnerMap.Count, Is.EqualTo(5));
	        Assert.That(p.GetProperty("Foo"), Is.EqualTo("bar"));
	        Assert.That(p.GetProperty("Baz"), Is.EqualTo("Continued on multiple lines (prefix whitespace removed). Another line. Another."));
	    }

	    [Test]
	    public void parse_escape_sequences() {
	        Properties p = ReadAlpha();
	        Assert.That(p.GetProperty("Bash"), Is.EqualTo("Escape\nTwo lines"));
	    }

	    // TODO Url syntax test

	    static Properties ReadAlpha() {
	        Stream source = File.OpenRead("Content/alpha.properties");
	        Properties p = Properties.FromStream(source);
	        return p;
	    }
	}
}
