//
// - Component.cs -
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

namespace Carbonfrost.Commons.Shared.Runtime.Components {

    [Serializable]
    [Builder(typeof(ComponentBuilder))]
    public sealed partial class Component : IEquatable<Component> {

        private readonly ComponentName name;
        private readonly string type;
        private readonly Uri source;

        public ComponentName Name {
            get { return name; }
        }

        public bool IsAssembly {
            get {
                return Type == ComponentTypes.Assembly;
            }
        }

        public string Type {
            get { return type; }
        }

        public Uri Source {
            get { return source; }
        }

        public Component(ComponentName name, string type = null, Uri source = null) {
            if (name == null)
                throw new ArgumentNullException("name");

            this.name = name;
            if (string.IsNullOrEmpty(type))
                this.type = ComponentTypes.Anything;
            else
                this.type = type;

            this.source = source;
        }

        public Uri ToUri() {
            string text = string.Format("component:{0}:{1}", this.Type, this.Name.ToString());
            if (Source == null)
                return new Uri(text);
            else
                return new Uri(text + ":" + this.Source);
        }

        public static Component Parse(string text) {
            Component result;
            Exception ex = _TryParse(text, out result);
            if (ex == null)
                return result;
            else
                throw ex;
        }

        public static bool TryParse(string text, out Component component) {
            return _TryParse(text, out component) == null;
        }

        public bool Equals(Component other) {
            return StaticEquals(this, other);
        }

        public override bool Equals(object obj) {
            return StaticEquals(this, obj as Component);
        }

        public override int GetHashCode() {
            int hashCode = 0;
            unchecked {
                hashCode += 1000000007 * name.GetHashCode();
                hashCode += 1000000009 * type.GetHashCode();

                if (source != null)
                    hashCode += 1000000021 * source.GetHashCode();
            }

            return hashCode;
        }

        public static bool operator ==(Component lhs, Component rhs) {
            return StaticEquals(lhs, rhs);
        }

        public static bool operator !=(Component lhs, Component rhs) {
            return !StaticEquals(lhs, rhs);
        }

        static bool StaticEquals(Component lhs, Component rhs) {
            if (ReferenceEquals(lhs, rhs))
                return true;
            if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
                return false;


            return object.Equals(lhs.name, rhs.name)
                && lhs.type == rhs.type
                && object.Equals(lhs.source, rhs.source);
        }

        static Exception _TryParse(string text, out Component result) {
            result = null;

            if (text == null)
                return new ArgumentNullException("text");
            if (text.Length == 0)
                return Failure.EmptyString("text");

            Uri u;
            if (Uri.TryCreate(text, UriKind.Absolute, out u)) {
                Exception ex = _TryFromUri(u, out result);
                if (ex == null)
                    return null;
                else
                    return Failure.NotParsable("text", typeof(Component), ex);

            } else {
                return Failure.NotParsable("text", typeof(Component));
            }
        }

        static Exception _TryFromUri(Uri u, out Component result) {
            result = null;
            if (u.Scheme != "component")
                return RuntimeFailure.UnknownUriScheme(typeof(Component));

            string[] parts = u.AbsolutePath.Split(new char[] { ':' }, 3);
            if (parts.Length < 2)
                return new UriFormatException();

            ComponentName name;
            string type = parts[0].Trim();
            Uri source = null;
            Exception nameError = ComponentName._TryParse(Uri.UnescapeDataString(parts[1].Trim()), out name);
            if (nameError != null)
                return RuntimeFailure.NotValidComponentUriNamePart(nameError);

            if (parts.Length == 3) {
                if (!Uri.TryCreate(parts[2], UriKind.RelativeOrAbsolute, out source))
                    return RuntimeFailure.NotValidComponentUriSourcePart();
            }

            result = new Component(name, type, source);
            return null;
        }
    }
}
