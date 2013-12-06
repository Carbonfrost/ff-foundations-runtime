//
// - ServiceStates.cs -
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

namespace Carbonfrost.Commons.Shared {

	[Flags]
	public enum ServiceStates {
	    None= 0,
	    Starting = 1,
	    Started = 2,
	    StartedWithErrors = Started | WithErrors,
	    Disposing = 4,
	    Disposed = 8,
	    DisposedWithErrors = Disposed | WithErrors,
	    WithErrors = 0x100,
	    WithDeferral = 0x200,
	    WithFrozenState = 0x400,
	}
}
