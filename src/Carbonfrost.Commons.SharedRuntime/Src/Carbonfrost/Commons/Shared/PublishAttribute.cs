//
// - PublishAttribute.cs -
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

    [AttributeUsage(AttributeTargets.Field
                    | AttributeTargets.Property
                    | AttributeTargets.ReturnValue,
                    AllowMultiple = true, Inherited = false)]
	public sealed class PublishAttribute : Attribute {

        public Type Type { get; set; }
        public string Name { get; set; }

        public PublishAttribute() {}

        public PublishAttribute(string typeFullyQualifiedName) {
            this.Type = Type.GetType(typeFullyQualifiedName, true);
        }
	}
}
