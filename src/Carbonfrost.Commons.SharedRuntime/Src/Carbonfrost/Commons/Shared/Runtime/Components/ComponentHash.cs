//
// - ComponentHash.cs -
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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Carbonfrost.Commons.SharedRuntime.Resources;

namespace Carbonfrost.Commons.Shared.Runtime.Components {

    [TypeConverter(typeof(ComponentHashConverter))]
    [Serializable]
    public sealed class ComponentHash
        : IEquatable<ComponentHash>, IComparable<ComponentHash>, IFormattable {

        private readonly ComponentHashAlgorithm algorithm;

        [NonSerialized]
        private readonly byte[] hashBytes;

        public ComponentHashAlgorithm Algorithm { get { return algorithm; } }

        public string Hash {
            get {
                return Utility.BytesToHex(hashBytes, false);
            }
        }

        private ComponentHash(ComponentHashAlgorithm algorithm, byte[] hash) {
            this.algorithm = algorithm;
            this.hashBytes = hash;
        }

        public byte[] ToByteArray() {
            return (byte[]) this.hashBytes.Clone();
        }

        public bool Verify(Stream inputStream) {
            if (inputStream == null)
                throw new ArgumentNullException("inputStream"); // $NON-NLS-1

            return ComponentHash.Create(this.Algorithm, inputStream).Equals(this);
        }

        public bool Verify(byte[] inputBytes, int offset, int count) {
            Require.NotNull("inputBytes", inputBytes); // $NON-NLS-1
            Require.WithinRange("inputBytes", inputBytes, "offset", offset); // $NON-NLS-1, $NON-NLS-2

            if (inputBytes.Length > offset + count) {
                throw new ArgumentOutOfRangeException("count", count, SR.CollectionCountOutOfRange(0, inputBytes.Length - 1)); // $NON-NLS-1
            }

            return this.Matches(ComponentHash.Create(this.Algorithm, inputBytes, offset, count));
        }

        public bool Matches(ComponentHash other) {
            if (other == null)
                throw new ArgumentNullException("other");

            if (this.algorithm == other.algorithm && this.hashBytes.Length <= other.hashBytes.Length) {
                return this.hashBytes.SequenceEqual(other.hashBytes.Take(this.hashBytes.Length));
            }

            return false;
        }

        public bool Verify(byte[] inputBytes) {
            return Verify(inputBytes, 0, inputBytes.Length);
        }

        public bool Verify(string inputString) {
            return Verify(_GetBytesFromString(inputString));
        }

        // Implicit construction.

        public static ComponentHash Create(ComponentHashAlgorithm algorithm, Stream inputStream) {
            if (inputStream == null)
                throw new ArgumentNullException("inputStream"); // $NON-NLS-1

            if (!inputStream.CanRead)
                throw Failure.NotReadableStream("inputStream"); // $NON-NLS-1

            HashAlgorithm hash = CreateHasher(algorithm);
            byte[] result = hash.ComputeHash(inputStream);
            return new ComponentHash(algorithm, result);
        }

        public static ComponentHash Create(ComponentHashAlgorithm algorithm, string inputString) {
            if (inputString == null) { throw new ArgumentNullException("inputString"); } // $NON-NLS-1
            return Create(algorithm, _GetBytesFromString(inputString));
        }

        public static ComponentHash Create(ComponentHashAlgorithm algorithm, byte[] inputBytes, int offset, int count) {
            if (inputBytes == null)
                throw new ArgumentNullException("inputBytes"); // $NON-NLS-1

            Require.WithinRange("inputBytes", inputBytes, "offset", offset, "count", count); // $NON-NLS-1

            if (inputBytes.Length > offset + count) {
                throw new ArgumentOutOfRangeException("count", count, SR.CollectionCountOutOfRange(0, inputBytes.Length - 1)); // $NON-NLS-1
            }

            var hasher = CreateHasher(algorithm);
            byte[] result = hasher.ComputeHash(inputBytes, offset, count);
            return new ComponentHash(algorithm, result);
        }

        static HashAlgorithm CreateHasher(ComponentHashAlgorithm alg) {
            switch (alg) {
                case ComponentHashAlgorithm.MD5:
                    return MD5.Create();

                case ComponentHashAlgorithm.SHA1:
                    return SHA1.Create();

                case ComponentHashAlgorithm.SHA256:
                    return SHA256.Create();

                default:
                    throw Failure.NotDefinedEnum("algorithm", alg); // $NON-NLS-1
            }
        }

        public static ComponentHash Create(ComponentHashAlgorithm algorithm, byte[] inputBytes) {
            return Create(algorithm, inputBytes, 0, inputBytes.Length);
        }

        public static bool TryParse(string text, out ComponentHash result) {
            return _TryParse(text, out result) == null;
        }

        public static ComponentHash Parse(string text) {
            ComponentHash result;
            Exception ex = _TryParse(text, out result);
            if (ex == null)
                return result;
            else
                throw ex;
        }

        static Exception _TryParse(string text, out ComponentHash result) {
            result = null;
            if (text == null)
                return new ArgumentNullException("text");  // $NON-NLS-1

            string[] parts = text.Trim().Split(new [] { ':' }, 2);

            if (parts.Length == 1) {
                result = new ComponentHash(ComponentHashAlgorithm.MD5, Utility.ConvertHexToBytes(text));
                return null;
            }

            ComponentHashAlgorithm al;
            if (Enum.TryParse(parts[0], out al)) {
                result = new ComponentHash(al, Utility.ConvertHexToBytes(parts[1]));
                return null;
            }

            return Failure.NotParsable("text", typeof(ComponentHash));
        }

        // 'object' overrides.

        public override int GetHashCode() {
            int hashCode = 0;
            unchecked {
                // N.B.: algorithm and hash are never null
                hashCode += 1000000007 * algorithm.GetHashCode();
                hashCode += 1000000009 * Hash.GetHashCode();
            }
            return hashCode;
        }

        public override bool Equals(object obj) {
            ComponentHash h = obj as ComponentHash;
            if (h == null) {
                return false;
            } else {
                return Equals(h);
            }
        }

        public override string ToString() {
            return ToString("G");
        }

        // 'IComparable' implementation.

        public int CompareTo(ComponentHash other) {
            if (other == null) { throw new ArgumentNullException("other"); } // $NON-NLS-1

            int result = this.algorithm.CompareTo(other.algorithm);

            if (result == 0) {
                return StringComparer.OrdinalIgnoreCase.Compare(this.Hash, other.Hash);
            } else {
                return result;
            }
        }

        // 'IEquatable' implementation.
        public bool Equals(ComponentHash other) {
            return StaticEquals(this, other);
        }

        static bool StaticEquals(ComponentHash left, ComponentHash right) {
            if (object.ReferenceEquals(left, right))
                return true;
            if (object.ReferenceEquals(null, left) || object.ReferenceEquals(null, right))
                return false;
            else
                return left.algorithm == right.algorithm
                    && left.hashBytes.Length == right.hashBytes.Length
                    && left.hashBytes.SequenceEqual(right.hashBytes);
        }

        public static bool operator ==(ComponentHash left, ComponentHash right) {
            return StaticEquals(left, right);
        }

        public static bool operator !=(ComponentHash left, ComponentHash right) {
            return !StaticEquals(left, right);
        }

        private static byte[] _GetBytesFromString(string str) {
            Encoder enc = Encoding.Unicode.GetEncoder();
            byte[] unicodeText = new byte[str.Length * 2];
            enc.GetBytes(str.ToCharArray(), 0, str.Length, unicodeText, 0, true);
            return unicodeText;
        }

        // `IFormattable' implementation
        public string ToString(string format, IFormatProvider formatProvider = null) {
            if (string.IsNullOrEmpty(format))
                return ToString();

            if (format.Length == 1) {
                char c = char.ToLowerInvariant(format[0]);
                bool upper = c == format[0];

                switch (c) {
                    case 'g':
                        return this.algorithm + ":" + Utility.BytesToHex(hashBytes, upper); // $NON-NLS-1

                    case 'a':
                        return this.algorithm.ToString();

                    case 'x':
                        return Utility.BytesToHex(hashBytes, upper);

                    case 'z':
                        return Convert.ToBase64String(hashBytes);
                }
            }

            throw new FormatException();
        }
    }
}
