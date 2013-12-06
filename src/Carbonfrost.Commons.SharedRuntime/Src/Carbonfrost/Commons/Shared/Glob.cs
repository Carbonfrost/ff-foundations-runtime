//
// - Glob.cs -
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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Carbonfrost.Commons.Shared.Runtime;

namespace Carbonfrost.Commons.Shared {

    using Item = Tuple<string, Glob.FileSystemEntity>;

    [Serializable]
    [TypeConverter(typeof(GlobConverter))]
    public partial class Glob {

        private readonly string text;
        private readonly SegmentSequence[] segments;
        private readonly Lazy<Regex> regexCache;
        private readonly FileSystemEnumerator Enumerator;

        public bool IsAnything {
            get { return this.Equals(Glob.Anything); }
        }

        internal IEnumerable<SegmentSequence> Segments {
            get { return segments; }
        }

        private Glob(string text, SegmentSequence[] segments) {
            this.text = text;
            this.segments = segments;
            this.regexCache = new Lazy<Regex>(MakeRegex);

            this.Enumerator = MakeEnumerator();
        }

        protected Glob(string text) {
            _TryParse(text, out segments);
            Glob g = Glob.Parse(text);
            this.text = text;
            this.regexCache = new Lazy<Regex>(MakeRegex);

            this.Enumerator = MakeEnumerator();
        }

        private FileSystemEnumerator MakeEnumerator() {
            return (e, path) => {

                if (e == FileSystemEntity.File)
                    return CoreEnumerateFiles(path).Select(t => new Item(t, FileSystemEntity.File));

                else if (e == FileSystemEntity.Directory)
                    return CoreEnumerateDirectories(path).Select(t => new Item(t, FileSystemEntity.Directory));

                else
                    return CoreEnumerateFileSystemEntries(path).Select(t => new Item(t, CoreDirectoryExists(t) ? FileSystemEntity.Directory : FileSystemEntity.File));
            };
        }

        static string ExpandEnvironmentVariables(string path) {
            // Prefer the platform-specific form
            return (Environment.ExpandEnvironmentVariables(path) ?? string.Empty).Replace(
                Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        protected virtual bool CoreDirectoryExists(string path) {
            return Directory.Exists(path);
        }

        protected virtual bool CoreFileExists(string path) {
            return File.Exists(path);
        }

        protected virtual IEnumerable<string> CoreEnumerateFiles(string path) {
            return Directory.EnumerateFiles(path);
        }

        protected virtual IEnumerable<string> CoreEnumerateDirectories(string path) {
            return Directory.EnumerateDirectories(path);
        }

        protected virtual IEnumerable<string> CoreEnumerateFileSystemEntries(string path) {
            return Directory.EnumerateFileSystemEntries(path);
        }

        public IEnumerable<string> EnumerateDirectories() {
            return EnumerateDirectories("/");
        }

        public IEnumerable<string> EnumerateDirectories(params string[] paths) {
            return Glob.FilterDirectories(this, paths, FileSystemEntity.Directory, this.Enumerator);
        }

        public IEnumerable<string> EnumerateFiles() {
            if (IsWindows)
                return EnumerateFiles(Path.GetPathRoot(Environment.SystemDirectory));
            else
                return EnumerateFiles("/");
        }

        public IEnumerable<string> EnumerateFiles(params string[] paths) {
            return Glob.FilterDirectories(this, paths, FileSystemEntity.File, this.Enumerator);
        }

        public IEnumerable<string> EnumerateFileSystemEntries(params string[] paths) {
            throw new NotImplementedException();
        }

        public bool IsMatch(string input) {
            return ToRegex().IsMatch(input);
        }

        public Regex ToRegex() {
            return this.regexCache.Value;
        }

        public override string ToString() {
            return this.text;
        }

        // `object' overrides
        public override bool Equals(object obj) {
            return StaticEquals(this, obj as Glob);
        }

        public override int GetHashCode() {
            int hashCode = 0;
            unchecked {
                if (text != null)
                    hashCode += 1000000007 * text.GetHashCode();
            }
            return hashCode;
        }

        public static bool operator ==(Glob lhs, Glob rhs) {
            return StaticEquals(lhs, rhs);
        }

        public static bool operator !=(Glob lhs, Glob rhs) {
            return !StaticEquals(lhs, rhs);
        }

        static bool StaticEquals(Glob lhs, Glob rhs) {
            if (ReferenceEquals(lhs, rhs))
                return true;
            if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
                return false;

            return lhs.text == rhs.text;
        }

        private Regex MakeRegex() {
            StringBuilder text = new StringBuilder();
            bool needPipe = false;

            foreach (SegmentSequence segments in this.segments) {
                if (needPipe)
                    text.Append("|");

                segments.AppendRegex(text);

                needPipe = true;
            }

            text.Append("$");
            return new Regex(text.ToString());
        }

        static bool IsWindows {
            get {
                switch (Environment.OSVersion.Platform) {
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                    case PlatformID.WinCE:
                        return true;
                    default:
                        return false;
                }
            }
        }
    }
}
