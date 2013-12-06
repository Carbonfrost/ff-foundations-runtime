//
// - StatusBuilder.cs -
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
using System.Collections.ObjectModel;
using Carbonfrost.Commons.Shared.Runtime.Components;

namespace Carbonfrost.Commons.Shared.Runtime {

    [Serializable]
    public class StatusBuilder : IStatus {

        public Status Build() {
            return new Status(this);
        }

        // IStatus implementation
        public Exception Exception { get; set; }
        public FileLocation FileLocation { get; set; }
        public Severity Level { get; set; }
        public string Message { get; set; }
        public Component Component { get; set; }
        public int ErrorCode { get; set; }

        public ReadOnlyCollection<IStatus> Children {
            get {
                return Empty<IStatus>.ReadOnly;
            }
        }

        public override bool Equals(object obj) {
            return Equals(obj as IStatus);
        }

        public override int GetHashCode() {
            int hashCode = 0;
            unchecked {
                if (Exception != null)
                    hashCode += 1000000007 * Exception.GetHashCode();

                hashCode += 1000000009 * FileLocation.GetHashCode();
                hashCode += 1000000021 * Level.GetHashCode();
                if (Message != null)
                    hashCode += 1000000033 * Message.GetHashCode();
                if (Component != null)
                    hashCode += 1000000087 * Component.GetHashCode();
                hashCode += 1000000093 * ErrorCode.GetHashCode();
            }
            return hashCode;
        }

        public bool Equals(IStatus other) {

            if (other == null)
                return false;

            return object.Equals(this.Exception, other.Exception)
                && this.FileLocation == other.FileLocation
                && this.Level == other.Level
                && this.Message == other.Message
                && object.Equals(this.Component, other.Component)
                && this.ErrorCode == other.ErrorCode;
        }

    }
}
