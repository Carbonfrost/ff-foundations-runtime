//
// - ComponentName.cs -
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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Carbonfrost.Commons.Shared.Runtime.Components {

    [TypeConverter(typeof(ComponentNameConverter))]
    [Serializable]
    [Builder(typeof(ComponentNameBuilder))]
    public sealed class ComponentName
        : IEquatable<ComponentName>, IFormattable {

        private readonly string name;
        private readonly Version version;
        private readonly CultureInfo culture;
        private readonly ProcessorArchitecture architecture;
        private readonly byte[] publicKey;
        private readonly bool usePublicKeyToken;
        private readonly ComponentHash hash;
        [NonSerialized] private int hashCodeCache;

        private static readonly ComponentName s_instance = new ComponentName("Unknown", null); // $NON-NLS-1

        [SelfDescribingPriority(PriorityLevel.Low)]
        public ProcessorArchitecture Architecture { get { return architecture; } }

        [SelfDescribingPriority(PriorityLevel.High)]
        public CultureInfo Culture { get { return culture; } }

        public bool IsUnknown { get { return object.ReferenceEquals(this, ComponentName.Unknown); } }

        public ComponentHash Hash { get { return this.hash; } }

        [SelfDescribingPriority(PriorityLevel.High)]
        public string Name { get { return this.name; } }

        [SelfDescribingPriority(PriorityLevel.Low)]
        public ComponentNameQuality Quality {
            get {
                bool hasName = (Name != null && !Utility.IsAnonymousName(Name));
                bool hasVersion = (Version != null);
                bool hasPublicKey = (this.publicKey != null && this.publicKey.Length > 0);
                bool hasHash = (this.Hash != null);
                bool hasCulture = !object.Equals(this.Culture, CultureInfo.InvariantCulture);

                ComponentNameQuality result
                    = ((hasName) ? ComponentNameQuality.SpecifiedName : 0)
                    | ((hasVersion) ? ComponentNameQuality.SpecifiedVersion : 0)
                    | ((hasCulture) ? ComponentNameQuality.SpecifiedCulture : 0)
                    | ((hasPublicKey) ? ComponentNameQuality.SpecifiedPublicKey : 0)
                    | ((hasHash) ? ComponentNameQuality.SpecifiedHash : 0);
                return result;
            }
        }

        public static ComponentName Unknown { get { return s_instance; } }

        [SelfDescribingPriority(PriorityLevel.High)]
        public Version Version { get { return this.version; } }

        public ComponentName(string name, Version version)
            : this(name, version, null, null, CultureInfo.InvariantCulture) {}

        public ComponentName(string name, int majorVersion, int minorVersion, int build, int revision)
            : this(name, new Version(majorVersion, minorVersion, build, revision)) {}

        public ComponentName(string name, Version version, byte[] publicKey, byte[] hash, CultureInfo culture) {
            this.name = NameOrDefault(name);
            this.version = version;
            this.culture = culture ?? CultureInfo.InvariantCulture;
            this.publicKey = publicKey;

            if (hash != null) {
                this.hash = ComponentHash.Create(ComponentHashAlgorithm.MD5, hash);
            }
        }

        public ComponentName(string name, Version version,
                             byte[] publicKey, ComponentHash hash,
                             CultureInfo culture,
                             ProcessorArchitecture architecture)
            : this(name, version, publicKey, hash, culture, architecture, false)
        {
        }

        internal ComponentName(string name, Version version,
                               byte[] publicKey, ComponentHash hash,
                               CultureInfo culture,
                               ProcessorArchitecture architecture,
                               bool usePublicKeyToken)
        {
            this.name = NameOrDefault(name);
            this.version = version ?? new Version(0, 0, 0, 0);
            this.culture = culture ?? CultureInfo.InvariantCulture;
            this.usePublicKeyToken = usePublicKeyToken;
            this.publicKey = publicKey;

            this.architecture = architecture;

            this.hash = hash;
        }

        public static ComponentName FromAssemblyName(AssemblyName name) {
            if (name == null) { throw new ArgumentNullException("name"); } // $NON-NLS-1

            return new ComponentName(
                name.Name, name.Version, name.GetPublicKey(),
                null, name.CultureInfo, name.ProcessorArchitecture);
        }

        public static bool TryParse(string text, out ComponentName result) {
            return _TryParse(text, out result) == null;
        }

        internal static Exception _TryParse(string text, out ComponentName result) {
            result = null;

            if (text == null)
                return new ArgumentNullException("text"); // $NON-NLS-1
            if (text.Length == 0)
                return Failure.EmptyString("text");

            string[] items = text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (items.Length == 1)
                return _TryParseShortForm(text, out result);

            try {
                string name = items[0];

                Dictionary<string, string> lookup = new Dictionary<string, string>();
                for (int i = 1; i < items.Length; i++) {
                    string s = items[i];
                    int index = s.IndexOf('=');
                    if (index < 0) {
                        throw Failure.NotParsable("text", typeof(ComponentName)); // $NON-NLS-1

                    } else {
                        string key = s.Substring(0, index).Trim();
                        string value = s.Substring(index + 1).Trim();
                        lookup.Add(key, value);
                    }
                }

                ComponentNameBuilder builder = new ComponentNameBuilder();
                builder.Name = name;

                Exception exception = builder.ParseDictionary(lookup);
                if (exception == null) {
                    result = builder.Build();
                    return null;
                } else
                    return exception;

            } catch (ArgumentException a) {
                return Failure.NotParsable("text", typeof(ComponentName), a); // $NON-NLS-1

            } catch (FormatException f) {
                return Failure.NotParsable("text", typeof(ComponentName), f); // $NON-NLS-1
            }
        }

        static Exception _TryParseShortForm(string text,
                                            out ComponentName result)
        {
            // Name=1.0-ABCD-en-x86
            result = null;
            string[] items = text.Split('=');
            if (items.Length == 1) {
                result = new ComponentName(text, null);
                return null;
            }

            if (items.Length != 2)
                return ParseError();

            string[] qualifiers = items[1].Split('-');

            Version version;
            ProcessorArchitecture arch = ProcessorArchitecture.None;
            CultureInfo culture = null;
            int cultureStart = 1;
            int cultureLength = qualifiers.Length - 1;
            byte[] key = null;

            if (!Version.TryParse(qualifiers[0], out version))
                return ParseError();

            if (Enum.TryParse(qualifiers[qualifiers.Length - 1], true, out arch)) {
                cultureLength--;
            }

            if (qualifiers.Length > 1 && qualifiers[1].Length > 2) {
                try {
                    key = Utility.ConvertHexToBytes(qualifiers[1], false);
                    cultureStart++;
                } catch (FormatException) {
                }
            }

            try {
                string cultureText =
                    string.Join("-", qualifiers.Skip(cultureStart).Take(cultureLength));

                if (cultureText.Length > 0)
                    culture = CultureInfo.GetCultureInfo(cultureText);

            } catch (CultureNotFoundException) {
                return ParseError();
            }

            result = new ComponentName(items[0], version, key, null, culture, arch, true);
            return null;
        }

        private static Exception ParseError() {
            return Failure.NotParsable("text", typeof(ComponentName));
        }

        public static ComponentName Parse(string text) {
            ComponentName result;
            Exception ex = _TryParse(text, out result);
            if (ex == null)
                return result;
            else
                throw ex;
        }

        public byte[] GetPublicKey() {
            if (publicKey == null || usePublicKeyToken) {
                return Empty<byte>.Array;
            } else {
                return (byte[]) this.publicKey.Clone();
            }
        }

        public byte[] GetPublicKeyToken() {
            if (usePublicKeyToken)
                return publicKey ?? Empty<byte>.Array;

            if (publicKey == null) {
                return Empty<byte>.Array;
            } else {
                SHA1 sha1 = SHA1.Create();
                byte[] hashed = sha1.ComputeHash(publicKey);
                byte[] result = new byte[8];
                Array.Copy(hashed, hashed.Length - 8, result, 0, 8);
                Array.Reverse(result);
                return result;
            }
        }

        public AssemblyName ToAssemblyName() {
            return new AssemblyName(this.ToString());
        }

        public bool Matches(ComponentName name) {
            if (name == null)
                throw new ArgumentNullException("name");
            if (object.ReferenceEquals(this, name))
                return true;
            else
                return this.Name == name.Name
                    && BlobMatches(this, name)
                    && CultureMatches(this.Culture, name.Culture)
                    && VersionMatches(this.Version, name.Version);
        }

        static bool CultureMatches(CultureInfo a, CultureInfo b) {
            return a == null || (a.Equals(b));
        }

        static bool BlobMatches(ComponentName a, ComponentName b) {
            if (a.publicKey == null)
                return true;

            // TODO Possible (but highly unlikely) that this comparison needs to support the full key
            return a.GetPublicKeyToken().SequenceEqual(b.GetPublicKeyToken());
        }

        static bool VersionMatches(Version a, Version b) {
            return a == null || (a.Major == b.Major
                                 && a.Minor == b.Minor
                                 && (a.Build == -1 || a.Build == b.Build)
                                 && (a.Revision == -1 || a.Revision == b.Revision));
        }

        #region 'Object' overrides.

        public override int GetHashCode() {
            if (hashCodeCache == 0) {
                int hashCode = 0;
                unchecked {
                    hashCode += 1000000007 * name.GetHashCode();
                    if (version != null) {
                        hashCode += 1000000009 * version.GetHashCode();
                    }
                    if (culture != null) {
                        hashCode += 1000000021 * culture.GetHashCode();
                    }
                    if (publicKey != null) {
                        hashCode += 1000000087 * publicKey.GetHashCode();
                    }
                    if (hash != null) {
                        hashCode += 1000000093 * hash.GetHashCode();
                    }

                    hashCode += 37 * usePublicKeyToken.GetHashCode();
                    hashCode += 1000000097 * architecture.GetHashCode();
                }

                this.hashCodeCache = hashCode;
                if (this.hashCodeCache == 0) { hashCodeCache++; }
            }

            return this.hashCodeCache;
        }

        public override bool Equals(object obj) {
            ComponentName myIdentity = obj as ComponentName;

            if (myIdentity == null) {
                return false;
            } else if (this == obj) {
                return true;
            } else {

                return this.name == myIdentity.name
                    && object.Equals(this.version, myIdentity.version)
                    && object.Equals(this.culture, myIdentity.culture)
                    && object.Equals(this.usePublicKeyToken, myIdentity.usePublicKeyToken)
                    && this.GetPublicKeyEncoding() == myIdentity.GetPublicKeyEncoding()
                    && this.architecture == myIdentity.architecture
                    && object.Equals(this.Hash, myIdentity.Hash);
            }
        }

        public override string ToString() {
            StringBuilder result = new StringBuilder();

            // Carbonfrost.Framework, Version=1.0, Culture=en-US, PublicKey=deadbeef, Hash=MD5:deadbeef, Architecture=...
            // Carbonfrost.Framework;version:1.0;culture:en-US;publicKey:deadbeef;hash:MD5:deadbeef;os:...;architecture:...
            result.Append(this.name);

            if (this.version != null) {
                result.Append(", Version="); // $NON-NLS-1
                result.Append(version.ToString());
            }

            if (this.culture != null) {
                result.Append(", Culture="); // $NON-NLS-1

                if (this.culture.Equals(CultureInfo.InvariantCulture))
                    result.Append("neutral");
                else
                    result.Append(culture.ToString());
            }

            if (this.publicKey != null) {
                if (this.usePublicKeyToken) {
                    result.Append(", PublicKeyToken="); // $NON-NLS-1
                } else {
                    result.Append(", PublicKey="); // $NON-NLS-1
                }
                result.Append(this.GetPublicKeyEncoding());
            }

            if (this.hash != null) {
                result.Append(", Hash="); // $NON-NLS-1
                result.Append(this.hash);
            }

            if (this.architecture != ProcessorArchitecture.None) {
                result.Append(", Architecture="); // $NON-NLS-1
                result.Append(this.architecture);
            }

            return result.ToString();
        }

        #endregion

        #region 'IEquatable`1' implementation.

        bool IEquatable<ComponentName>.Equals(ComponentName other) {
            return Equals(other);
        }

        #endregion

        private string GetPublicKeyEncoding() {
            if (this.publicKey == null) {
                return string.Empty;
            } else {
                return Utility.BytesToHex(this.publicKey);
            }
        }

        static string NameOrDefault(string name) {
            if (string.IsNullOrEmpty(name))
                return Guid.NewGuid().ToString();
            else return name;
        }

        public string ToString(string format, IFormatProvider formatProvider = null) {
            if (string.IsNullOrEmpty(format))
                format = "G";

            switch (format[0]) {
                case 'g':
                case 'G':
                    return ToString();

                case 's':
                    return ConvertToShortString();

                case 'c':
                    return Culture.Name;

                default:
                    throw new FormatException();
            }
        }

        private string ConvertToShortString() {
            StringBuilder sb = new StringBuilder();
            sb.Append(Name);
            sb.Append('=');
            sb.Append(Version);

            if (this.publicKey != null) {
                sb.Append('-');
                sb.Append(GetPublicKeyEncoding());
            }

            if (Culture != null) {
                sb.Append('-');
                sb.Append(Culture);
            }

            if (Architecture != ProcessorArchitecture.None) {
                sb.Append('-');
                sb.Append(Architecture.ToString().ToLowerInvariant());
            }

            return sb.ToString();
        }

    }
}
