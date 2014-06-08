//
// - StreamingSourceUsageAttribute.cs -
//
// Copyright 2012 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class StreamingSourceUsageAttribute : ProviderAttribute {

        private string[] extensionCache;
        private string[] contentTypeCache;

        public string Extensions { get; set; }
        public string ContentTypes { get; set; }
        public Type OutputType { get; set; }

        public StreamingSourceUsageAttribute() : base(typeof(StreamingSource)) {
        }

        protected override int MatchCriteriaCore(object criteria) {
            if (criteria == null)
                return 0;

            int result = 0;
            var pp = PropertyProvider.FromValue(criteria);
            result += EnumerateExtensions().Contains(pp.GetString("Extension")) ? 1 : 0;
            result += EnumerateContentTypes().Contains(pp.GetString("ContentType")) ? 1 : 0;
            result += MatchType(pp.GetProperty("OutputType"));

            return result;
        }

        private int MatchType(object outputType) {
            if (outputType == null || this.OutputType == null)
                return 0;

            Type type = outputType as Type;
            if (type == null) {
                TypeReference tr = (outputType as TypeReference) ?? TypeReference.Null;
                type = tr.Resolve();
            }
            if (type != null) {
                if (this.OutputType.Equals(type))
                    return 3;
                else
                    return this.OutputType.IsAssignableFrom(type) ? 1 : 0;
            }

            return 0;
        }

        public IEnumerable<string> EnumerateContentTypes() {
            // Only on  '\t', '\r', '\n', '|' because content types could contain params
            return Utility.SplitText(ref this.contentTypeCache, this.ContentTypes, new [] { '|', '\t', '\r', '\n', });
        }

        public IEnumerable<string> EnumerateExtensions() {
            return Utility.SplitText(ref this.extensionCache, this.Extensions);
        }

    }
}

