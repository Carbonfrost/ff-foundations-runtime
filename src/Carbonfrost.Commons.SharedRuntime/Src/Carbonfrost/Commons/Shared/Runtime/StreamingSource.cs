//
// - StreamingSource.cs -
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
using System.Linq;

using System.Reflection;
using Carbonfrost.Commons.ComponentModel;

namespace Carbonfrost.Commons.Shared.Runtime {

    public abstract partial class StreamingSource {

        protected StreamingSource() {}

        public virtual InterfaceUsageInfo GetInterfaceUsage(StreamContext inputSource) {
            return null;
        }

        public virtual bool IsValidInput(StreamContext inputSource) {
            return true;
        }

        public abstract object Load(StreamContext inputSource, Type instanceType);
        public abstract void Save(StreamContext outputTarget, object value);

        public virtual void Load(StreamContext inputSource, object instance) {
            if (inputSource == null)
                throw new ArgumentNullException("inputSource");
            if (instance == null)
                throw new ArgumentNullException("instance");

            LoadByHydration(inputSource, instance);
        }

        internal void LoadByHydration(StreamContext inputSource, object instance) {
            var copyFrom = Load(inputSource, instance.GetType());
            Template.Copy(copyFrom, instance);
        }

    }
}
