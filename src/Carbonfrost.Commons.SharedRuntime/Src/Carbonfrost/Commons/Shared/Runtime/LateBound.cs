//
// - LateBound.cs -
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
using System.Reflection;
using System.Threading;

namespace Carbonfrost.Commons.Shared.Runtime {

    public class LateBound<T> : Lazy<T> {

        private readonly TypeReference typeReference;

        public TypeReference TypeReference {
            get { return typeReference; }
        }

        public LateBound(TypeReference typeReference, IServiceProvider serviceProvider)
            : base(ValueFactory(typeReference, serviceProvider)) {

            this.typeReference = typeReference;
        }

        public LateBound(TypeReference typeReference, bool isThreadSafe, IServiceProvider serviceProvider = null)
            : base(ValueFactory(typeReference, serviceProvider), isThreadSafe) {

            this.typeReference = typeReference;
        }

        public LateBound(TypeReference typeReference, LazyThreadSafetyMode mode, IServiceProvider serviceProvider = null)
            : base(ValueFactory(typeReference, serviceProvider), mode) {

            this.typeReference = typeReference;
        }

        static Func<T> ValueFactory(TypeReference typeReference, IServiceProvider serviceProvider) {
            if (typeReference == null)
                throw new ArgumentNullException("typeReference");
            serviceProvider = serviceProvider ?? ServiceProvider.Null;

            return () => (Activation.CreateInstance<T>(typeReference.Resolve(),
                                                       serviceProvider: serviceProvider));
        }
    }
}
