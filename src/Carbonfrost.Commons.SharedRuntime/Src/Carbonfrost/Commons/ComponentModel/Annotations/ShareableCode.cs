//
// - ShareableCode.cs -
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

    public static class ShareableCode {

        static readonly NamespaceUri ShareableCodeMetadata2011 = Xmlns.ShareableCodeMetadata2011;

        public static readonly QualifiedName Documentation = ShareableCodeMetadata2011 + "documentation";
        public static readonly QualifiedName License = ShareableCodeMetadata2011 + "license";

        public static readonly QualifiedName Self = ShareableCodeMetadata2011 + "self";
        public static readonly QualifiedName Base = ShareableCodeMetadata2011 + "base";
        public static readonly QualifiedName Url = ShareableCodeMetadata2011 + "url";

        public static readonly QualifiedName SourceAttachment = ShareableCodeMetadata2011 + "sourceAttachment";
        public static readonly QualifiedName ExceptionCatalog = ShareableCodeMetadata2011 + "exceptionCatalog";
        public static readonly QualifiedName PrivateKey = ShareableCodeMetadata2011 + "privateKey";
        public static readonly QualifiedName Translations = ShareableCodeMetadata2011 + "translations";
        public static readonly QualifiedName StandardsCompliant = ShareableCodeMetadata2011 + "standardsCompliant";

        public static class Roles {

            public const string Reference = "reference";
            public const string SourceControl = "sourceControl";

        }

    }
}
