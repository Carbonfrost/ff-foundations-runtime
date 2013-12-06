//
// - NullStatus.cs -
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
using System.Collections.ObjectModel;
using Carbonfrost.Commons.Shared.Runtime.Components;

namespace Carbonfrost.Commons.Shared.Runtime {

	internal class NullStatus : IStatus {

	    internal NullStatus() {}

	    public string Message { get { return string.Empty; } }
	    public Exception Exception { get { return null; } }
	    public FileLocation FileLocation { get { return new FileLocation(); } }
	    public Severity Level { get { return Severity.None; } }
	    public Component Component { get { return null; } set {} }
	    public int ErrorCode { get { return 0; } }

	    public ReadOnlyCollection<IStatus> Children {
	        get {
	            return Empty<IStatus>.ReadOnly;
	        }
	    }

	    public bool Equals(IStatus other) { return other is NullStatus; }
	    public override bool Equals(object obj) { return obj is NullStatus; }
	    public override int GetHashCode() { return 0; }
	    public override string ToString() { return "Status {}"; } // $NON-NLS-1

	}
}
