//
// - Relationship.cs -
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
using System.Globalization;
using Carbonfrost.Commons.Shared;
using Carbonfrost.Commons.Shared.Runtime;

namespace Carbonfrost.Commons.ComponentModel.Annotations {

    [Builder(typeof(RelationshipBuilder))]
    public sealed class Relationship : IEquatable<Relationship> {

        public QualifiedName Name { get; private set; }
        public Uri Uri { get; private set; }
        public CultureInfo Culture { get; private set; }
        public string Role { get; private set; }
        public string Media { get; private set; }

        public Relationship(QualifiedName name,
                            Uri uri) : this(name, uri, null, null, CultureInfo.InvariantCulture)
        {
        }

        public Relationship(QualifiedName name,
                            Uri uri,
                            string role,
                            string media,
                            CultureInfo culture)
        {
            if (name == null)
                throw new ArgumentNullException("name"); // $NON-NLS-1

            if (uri == null)
                throw new ArgumentNullException("uri"); // $NON-NLS-1

            this.Name = name;
            this.Uri = uri;
            this.Media = media ?? string.Empty;
            this.Culture = culture ?? CultureInfo.InvariantCulture;
            this.Role = role ?? string.Empty;
        }

        public bool Equals(Relationship other) {
            if (other == null)
                return false;

            return object.Equals(this.Name, other.Name)
                && object.Equals(this.Uri, other.Uri)
                && object.Equals(this.Culture, other.Culture)
                && this.Role == other.Role
                && this.Media == other.Media;
        }

        public override bool Equals(object obj) {
            return Equals(obj as Relationship);
        }

        public override int GetHashCode() {
            int hashCode = 0;
            unchecked {
                hashCode += 1000000007 * Name.GetHashCode();
                hashCode += 1000000009 * Uri.GetHashCode();
                hashCode += 1000000021 * Culture.GetHashCode();
                hashCode += 1000000033 * Role.GetHashCode();
                hashCode += 1000000087 * Media.GetHashCode();
            }
            return hashCode;
        }
    }
}
