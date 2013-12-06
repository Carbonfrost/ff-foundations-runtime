//
// - CoreMetadata.cs -
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
using Carbonfrost.Commons.Shared;

namespace Carbonfrost.Commons.ComponentModel.Annotations {

    public static class CoreMetadata {

        public static readonly NamespaceUri dc = "http://purl.org/dc/elements/1.1/";
        public static readonly NamespaceUri dcterms = "http://purl.org/dc/terms/";
        public static readonly NamespaceUri dcmitype = "http://purl.org/dc/dcmitype/";


        public static readonly QualifiedName Creator = dc + "creator";
        public static readonly QualifiedName LastModifiedBy = dc + "lastModifiedBy";
        public static readonly QualifiedName Revision = dc + "revision";
        public static readonly QualifiedName Created = dcterms + "created";
        public static readonly QualifiedName Modified = dcterms + "modified";
    }
}
