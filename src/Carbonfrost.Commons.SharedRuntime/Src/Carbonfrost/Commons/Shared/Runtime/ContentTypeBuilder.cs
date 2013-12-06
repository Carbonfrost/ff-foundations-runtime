//
// - ContentTypeBuilder.cs -
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
using System.Collections.Generic;
using System.Linq;

namespace Carbonfrost.Commons.Shared.Runtime {

    public class ContentTypeBuilder {

        private readonly IDictionary<string, string> parameters
            = new Dictionary<string, string>();

        public string Type { get; set; }
        public string Subtype { get; set; }

        public IDictionary<string, string> Parameters {
            get { return parameters; }
        }

        public ContentTypeBuilder() {}

        public ContentType Build() {
            return new ContentType(Type, Subtype, Parameters);
        }

        public override string ToString() {
            if (string.IsNullOrEmpty(Type) || string.IsNullOrEmpty(Subtype))
                return SelfDescribing.ToString(this);
            else
                return Build().ToString();
        }

    }
}
