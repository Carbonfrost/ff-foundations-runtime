//
// - InvalidStreamContext.cs -
//
// Copyright 2005, 2006, 2010 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Globalization;
using System.IO;
using System.Text;

namespace Carbonfrost.Commons.Shared.Runtime {

	internal sealed class InvalidStreamContext : StreamContext {

	    public override bool Equals(object obj) {
	        return (obj is InvalidStreamContext);
	    }

	    public override int GetHashCode() {
	        return Int32.MaxValue / 2;
	    }

	    public override bool IsLocal { get { return true; } }
	    public override CultureInfo Culture { get { return CultureInfo.InvariantCulture; } }

	    [SelfDescribingPriority(PriorityLevel.None)]
	    public override Uri Uri {
	        get { return new Uri("invalid://"); } // $NON-NLS-1
	    }

	    public override StreamContext ChangePath(string relativePath) {
	        return this;
	    }

	    public override StreamContext ChangeCulture(CultureInfo resourceCulture) {
	        return this;
	    }

	    protected override Stream GetStreamCore(FileAccess access) {
	        return Stream.Null;
	    }

        public override StreamContext ChangeEncoding(Encoding encoding) {
            return this;
        }
	}
}
