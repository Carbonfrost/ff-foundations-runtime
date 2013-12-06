//
// - NameValueCollectionAdapter.cs -
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Carbonfrost.Commons.Shared.Runtime {

    internal sealed class NameValueCollectionAdapter : PropertiesImpl {

        private readonly NameValueCollection nvc;

        public NameValueCollectionAdapter(NameValueCollection nvc) {
            this.nvc = nvc;
        }

        protected override void SetPropertyCore(string key, object defaultValue, bool isImplicit) {
            nvc[key] = Convert.ToString(defaultValue);
        }

        public override IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
            return nvc.AllKeys.Select(t => new KeyValuePair<string, object>(t, nvc[t])).GetEnumerator();
        }

        public override void ClearProperties() {
            nvc.Clear();
        }
    }
}
