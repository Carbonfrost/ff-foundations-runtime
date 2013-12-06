//
// - FileLocation.cs -
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
using System.Text;
using System.Text.RegularExpressions;

namespace Carbonfrost.Commons.Shared.Runtime {

    [Serializable]
    [TypeConverter(typeof(FileLocationConverter))]
    public struct FileLocation : IEquatable<FileLocation>, IFormattable {

        private static readonly Regex REGEX =
            new Regex(@"^(?<FileName>.*) \s* \( \s* (?<LineNo>(\d)*) \s* \, \s* (?<LinePos>(\d)*) \)$"); // $NON-NLS-1

        private readonly int linePosition;
        private readonly int lineNumber;
        private readonly string fileName;

        public static FileLocation Empty { get { return new FileLocation(); } }

        public bool IsEmpty {
            get {
                return string.IsNullOrEmpty(FileName)
                    && LineNumber < 1 && LinePosition < 1;
            }
        }

        public string FileName {
            get { return fileName; }
        }

        public FileLocation(int lineNumber, int linePosition, string fileName) {
            this.linePosition = linePosition;
            this.lineNumber = lineNumber;
            this.fileName = fileName;
        }

        public FileLocation(int lineNumber, int linePosition)
            : this(lineNumber, linePosition, null) { }

        public FileLocation(string fileName) : this(-1, -1, fileName) { }

        public FileLocation(FileLocation textLocation, Uri location) {
            this.linePosition = textLocation.LinePosition;
            this.lineNumber = textLocation.LineNumber;

            if (location == null) {
                this.fileName = null;
            } else {
                this.fileName = location.ToString();
            }
        }

        public static FileLocation Parse(string text) {
            if (text == null)
                throw new ArgumentNullException("text"); // $NON-NLS-1

            FileLocation result;
            if (TryParse(text, out result))
                return result;
            else
                throw Failure.NotParsable("text", typeof(FileLocation)); // $NON-NLS-1
        }

        public static bool TryParse(string text, out FileLocation result) {
            result = default(FileLocation);

            if (string.IsNullOrEmpty(text))
                return false;

            text = text.Trim();
            if (text.Length == 0)
                return false;

            Match match = REGEX.Match(text);
            if (match.Success) {
                result = new FileLocation(int.Parse(match.Groups["LineNo"].Value), // $NON-NLS-1
                                          int.Parse(match.Groups["LinePos"].Value), // $NON-NLS-1
                                          match.Groups["FileName"].Value // $NON-NLS-1
                                         );
                return true;
            }
            else return false;
        }

        public static bool operator ==(FileLocation lhs, FileLocation rhs) {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(FileLocation lhs, FileLocation rhs) {
            return !(lhs == rhs);
        }

        public int LinePosition {
            get { return linePosition; }
        }

        public int LineNumber {
            get { return lineNumber; }
        }

        // 'IEquatable<FileLocation> implementation.

        bool IEquatable<FileLocation>.Equals(FileLocation other) {
            return Equals(other);
        }

        // 'Object' overrides.
        public override int GetHashCode() {
            int hashCode = linePosition.GetHashCode() ^ lineNumber.GetHashCode();
            if (fileName != null) { hashCode ^= fileName.GetHashCode(); }

            return hashCode;
        }

        public override bool Equals(object obj) {
            if (obj is FileLocation) {
                FileLocation other = (FileLocation) obj;
                return (other.LinePosition == this.LinePosition)
                    && (other.LineNumber == this.LineNumber)
                    && (other.FileName == this.FileName);
            } else {
                return false;
            }
        }

        public override string ToString() {
            if (LinePosition > 0 && LineNumber > 0) {
                if (string.IsNullOrEmpty(FileName))
                    return ToString("h");

                return string.Format("{0} ({1}, {2})", // $NON-NLS-1
                                     Uri.EscapeUriString(FileName ?? string.Empty), LineNumber, LinePosition);
            } else {
                return FileName ?? string.Empty;
            }
        }

        public string ToString(string format, IFormatProvider formatProvider = null) {
            if (string.IsNullOrEmpty(format))
                return this.ToString();

            if (format.Length == 1)
                return this.ToStringFormat(format[0]);

            StringBuilder sb = new StringBuilder();
            foreach (char d in format) {
                switch (char.ToLowerInvariant(d)) {
                    case 'r':
                    case 'g':
                    case 's':
                    case 'l':
                    case 'c':
                    case 'n':
                    case 'h':
                        sb.Append(this.ToStringFormat(d));
                        break;

                    default:
                        sb.Append(d);
                        break;
                }
            }
            return sb.ToString();
        }

        private string ToStringFormat(char d) {
            switch (char.ToLowerInvariant(d)) {
                case 'r':
                case 'g':
                    return this.ToString();

                case 's':
                    return string.Format("{0}, {1}", this.LineNumber, this.LinePosition);

                case 'h':
                    return string.Format("line {0}, pos {1}", this.LineNumber, this.LinePosition);

                case 'n':
                    return this.FileName ?? string.Empty;

                case 'l':
                    return this.LineNumber.ToString();

                case 'c':
                    return this.LinePosition.ToString();
                default:
                    throw new FormatException();
            }
        }

    }
}
