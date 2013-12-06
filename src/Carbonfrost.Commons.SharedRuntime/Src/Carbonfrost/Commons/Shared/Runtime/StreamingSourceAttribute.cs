//
// - StreamingSourceAttribute.cs -
//
// Copyright 2005, 2006, 2010, 2012 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

namespace Carbonfrost.Commons.Shared.Runtime {

    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property,
                    AllowMultiple = false, Inherited = false)]
    public sealed class StreamingSourceAttribute : AdapterAttribute  {

        public StreamingSourceAttribute(KnownStreamingSource knownStreamingSource)
            : base(StreamingSource.GetStreamingSourceType(knownStreamingSource), AdapterRole.StreamingSource) {}

        public StreamingSourceAttribute(string type) : base(type, AdapterRole.StreamingSource) {}
        public StreamingSourceAttribute(Type type) : base(type, AdapterRole.StreamingSource) {}

    }
}
