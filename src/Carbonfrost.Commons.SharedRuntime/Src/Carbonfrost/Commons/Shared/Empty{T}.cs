//
// - Empty{T}.cs -
//
// Copyright 2010 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Carbonfrost.Commons.Shared {

	static class Empty<T> {

        public static readonly T[] Array = {};
        public static readonly IList<T> List = Array;
	    public static readonly IEnumerator<T> Enumerator = List.GetEnumerator();
	    public static readonly ReadOnlyCollection<T> ReadOnly = new ReadOnlyCollection<T>(List);

	}
}
