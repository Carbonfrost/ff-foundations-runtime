//
// - UrlAttribute.cs -
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

namespace Carbonfrost.Commons.ComponentModel.Annotations {

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class UrlAttribute : Attribute {

        public Uri Url { get; private set; }

        public UrlAttribute(string source) {
            this.Url = new Uri(source, UriKind.Absolute);
        }

    }
}
