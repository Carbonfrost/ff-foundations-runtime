//
// - ComponentNameBuilder.cs -
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
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Shared.Runtime.Components {

    public class ComponentNameBuilder {

        private byte[] publicKey;
        private byte[] publicKeyToken;

        // Properties.
        public ProcessorArchitecture Architecture { get; set; }
        public CultureInfo Culture { get; set; }
        public ComponentHash Hash { get; set; }
        public string Name { get; set; }

        public byte[] PublicKey {
            get {
                if (publicKey == null)
                    return null;
                else
                    return (byte[]) publicKey.Clone();
            }
            set {
                if (value != null)
                    value = (byte[]) value.Clone();
                publicKey = value;
            }
        }

        public byte[] PublicKeyToken {
            get {
                if (publicKeyToken == null)
                    return null;
                else
                    return (byte[]) publicKeyToken.Clone();
            }
            set {
                if (value != null)
                    value = (byte[]) value.Clone();
                publicKeyToken = value;
            }
        }

        public Version Version { get; set; }

        public ComponentNameBuilder() {}

        public ComponentNameBuilder(ComponentName name) {
            if (name == null)
                return;

            this.Name = name.Name;
            this.Version = name.Version;
            this.Culture = name.Culture;
            this.Architecture = name.Architecture;
            this.PublicKey = name.GetPublicKey();
            this.PublicKeyToken = name.GetPublicKeyToken();
            this.Hash = name.Hash;
        }

        public ComponentName Build() {
            if (this.PublicKey == null)
                return new ComponentName(Name, Version, this.PublicKeyToken, Hash, Culture, Architecture, true);
            else
                return new ComponentName(Name, Version, this.PublicKey, Hash, Culture, Architecture);
        }

        internal Exception ParseDictionary(IDictionary<string, string> d) {
            string s = null;

            if (d.TryGetValue("Version", out s)) { // $NON-NLS-1
                this.Version = new Version(s);
                d.Remove("Version"); // $NON-NLS-1
            }

            if (d.TryGetValue("Culture", out s)) { // $NON-NLS-1
                // Special handling for neutral

                if (s == "neutral" || s == "null") {
                    this.Culture = CultureInfo.InvariantCulture;
                } else {
                    this.Culture = CultureInfo.GetCultureInfo(s);
                }

                d.Remove("Culture"); // $NON-NLS-1
            }

            if (d.TryGetValue("Architecture", out s)) { // $NON-NLS-1
                this.Architecture = (ProcessorArchitecture) Enum.Parse(typeof(ProcessorArchitecture), s);
                d.Remove("Architecture"); // $NON-NLS-1
            }

            if (d.TryGetValue("PublicKeyToken", out s)) { // $NON-NLS-1
                this.PublicKeyToken = Utility.ConvertHexToBytes(s);
                d.Remove("PublicKeyToken"); // $NON-NLS-1
            }

            if (d.TryGetValue("PublicKey", out s)) { // $NON-NLS-1
                this.PublicKey = Utility.ConvertHexToBytes(s);
                d.Remove("PublicKey"); // $NON-NLS-1
            }

            if (this.PublicKey != null && this.PublicKeyToken != null)
                return RuntimeFailure.CannotSpecifyPublicKeyToken();

            if (d.TryGetValue("ComponentHash", out s)) { // $NON-NLS-1
                this.Hash = ComponentHash.Parse(s);
                d.Remove("ComponentHash"); // $NON-NLS-1
            }

            if (d.Count > 0)
                return RuntimeFailure.NotSupportedIdentity(d.Keys.First());

            return null;
        }

    }
}
