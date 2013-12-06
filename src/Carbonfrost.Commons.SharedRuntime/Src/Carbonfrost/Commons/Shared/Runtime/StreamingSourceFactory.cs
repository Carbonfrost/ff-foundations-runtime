//
// - StreamingSourceFactory.cs -
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
using System.Reflection;

namespace Carbonfrost.Commons.Shared.Runtime {

    public class StreamingSourceFactory : AdapterFactory<StreamingSource> {

        private static readonly StreamingSourceFactory _default
            = new StreamingSourceFactory(AdapterFactory.Default);

        public StreamingSourceFactory(IAdapterFactory implementation)
            : base(AdapterRole.StreamingSource, implementation) {
        }

        public static StreamingSourceFactory Default {
            get { return _default; }
        }

        public static StreamingSourceFactory FromAssembly(Assembly assembly) {
            return new StreamingSourceFactory(AdapterFactory.FromAssembly(assembly));
        }

        public Type GetStreamingSourceType(object adaptee) {
            return base.GetAdapterType(adaptee);
        }

        public Type GetStreamingSourceType(Type adapteeType) {
            return base.GetAdapterType(adapteeType);
        }
    }

}

