//
// - Glob.Static.cs -
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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Carbonfrost.Commons.Shared.Runtime;

namespace Carbonfrost.Commons.Shared {

    using Item = Tuple<string, Glob.FileSystemEntity>;

    partial class Glob {

        static readonly string NON_PATH_SEP = @"[^/\:]"; // \.
        static readonly char[] SPECIAL_CHARS = { '*', '?', '[', ']' };
        static readonly Regex DEVICE = new Regex(@"(?<Name> [a-z])\:", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        static readonly string PlatformMatchDirectorySeparator = GetDirectorySeparator();

        public static readonly Glob Anything = Glob.Parse("**/*.*");

        public static Glob Combine(IEnumerable<Glob> items) {
            if (items == null)
                throw new ArgumentNullException("items");

            return Combine(items.ToArray<Glob>());
        }

        public static Glob Combine(params Glob[] items) {
            if (items == null)
                throw new ArgumentNullException("items");

            switch (items.Length) {
                case 0:
                    return Anything;

                case 1:
                    return items[0];

                case 2:
                    return Combine(items[0], items[1]);

                case 3:
                    return Combine(items[0], items[1], items[2]);
            }

            if (items.Any(t => t.IsAnything))
                return Anything;

            return new Glob(
                string.Join(";", items.Select(t => t.text)),
                items.SelectMany(t => t.segments).ToArray());
        }

        public static Glob Combine(Glob arg1, Glob arg2) {
            if (arg1 == null)
                throw new ArgumentNullException("arg1");

            if (arg2 == null)
                throw new ArgumentNullException("arg2");

            if (object.ReferenceEquals(arg1, arg2))
                return arg1;

            if (arg1.IsAnything || arg2.IsAnything)
                return Glob.Anything;

            return new Glob(arg1.text + ";" + arg2.text,
                            arg1.segments.Concat(arg2.segments).ToArray<SegmentSequence>());
        }

        public static Glob Combine(Glob arg1, Glob arg2, Glob arg3) {
            if (arg1 == null)
                throw new ArgumentNullException("arg1");

            if (arg2 == null)
                throw new ArgumentNullException("arg2");

            if (arg3 == null)
                throw new ArgumentNullException("arg3");

            return Combine(arg1, Combine(arg2, arg3));
        }

        public static Glob Parse(string text) {
            Glob result;
            Exception ex = _TryParse(text, out result);
            if (ex == null)
                return result;
            else
                throw ex;
        }

        public static bool TryParse(string text, out Glob value) {
            return _TryParse(text, out value) == null;
        }

        static Exception _TryParseList(string text, out IteratedSegmentSequence segments) {
            segments = null;
            string[] items = text.Split('/', '\\');
            List<Segment> results = new List<Segment>(items.Length);

            foreach (string s in items) {
                Segment sgt;
                if (s.Length == 0) {
                    // TODO Only apply at real root; enforce match segment as file or directory
                    sgt = new RootSegment();

                } else if (DEVICE.IsMatch(s)) {
                    sgt = new DeviceSegment(DEVICE.Match(s).Groups["Name"].Value);

                } else if (s == "*") {
                    sgt = new AnyDirectorySegment();

                } else if (s == "**") {
                    sgt = new RecursiveSegment();

                    // TODO Support directory navigation
                } else if (s == "..") {
                    throw new NotImplementedException();

                } else if (s == ".") {
                    throw new NotImplementedException();

                } else {
                    string segment = ExpandSegment(s);
                    if (segment == null)
                        return Failure.NotParsable("text", typeof(Glob));

                    sgt = new MatchSegment(segment);
                }

                results.Add(sgt);
            }

            segments = new IteratedSegmentSequence(results.ToArray());
            return null;
        }

        static Exception _TryParse(string text, out Glob value) {
            text = ExpandEnvironmentVariables(text);

            SegmentSequence[] segments;
            Exception ex = _TryParse(text, out segments);
            value = null;

            if (ex == null) {
                value = new Glob(text, segments);
                return null;
            } else
                return ex;
        }

        static Exception _TryParse(string text, out SegmentSequence[] results2) {
            results2 = null;
            if (text == null)
                return new ArgumentNullException("text");
            if (text.Length == 0)
                return Failure.EmptyString("text");

            List<SegmentSequence> results = new List<SegmentSequence>();

            foreach (string sub in text.Split(';')) {

                if (Path.IsPathRooted(sub) && NoSpecialChars(sub)) {
                    RootedSegmentSequence sequence = new RootedSegmentSequence(sub);
                    results.Add(sequence);

                } else {
                    IteratedSegmentSequence segments;
                    _TryParseList(sub, out segments);
                    results.Add(segments);
                }
            }

            results2 = results.ToArray();
            return null;
        }

        static bool NoSpecialChars(string s) {
            return s.IndexOfAny(SPECIAL_CHARS) < 0;
        }

        static string ExpandSegment(string s) {
            StringBuilder sb = new StringBuilder(s.Length + 3);
            sb.Append(PlatformMatchDirectorySeparator);

            StringBuilder b = null;
            CharEnumerator e = s.GetEnumerator();

            while (e.MoveNext()) {
                char c = e.Current;
                switch (c) {
                    case '[':
                        if (b != null)
                            return null;
                        b = new StringBuilder("(");
                        break;

                    case ']':
                        if (b == null)
                            return null;
                        b.Append(")");
                        sb.Append(b.ToString());
                        b = null;
                        break;

                    case '.':
                    case '(':
                    case ')':
                        sb.Append('\\');
                        sb.Append(c);
                        break;

                    case '?':
                        sb.Append(NON_PATH_SEP);
                        break;
                    case '*':
                        sb.Append(NON_PATH_SEP + "*");
                        break;

                    default:
                        if (b != null)
                            b.Append(b.Length > 1 ? "|" : null).Append(c);
                        else
                            sb.Append(c);
                        break;
                }
            }

            sb.Append("$");
            return sb.ToString();
        }

        internal abstract class Segment {
            public abstract IEnumerable<Item> Enumerate(IEnumerable<Item> items,
                                                        FileSystemEnumerator enumerator);

            public abstract string ToRegexString();
        }

        internal enum FileSystemEntity { File, Directory, FileOrDirectory }

        internal delegate IEnumerable<Item> FileSystemEnumerator(FileSystemEntity type, string directory);

        internal static IEnumerable<string> FilterFiles(Glob glob, IEnumerable<string> files) {
            var regex = glob.ToRegex();
            return files.Select(t => ExpandEnvironmentVariables(t)).Where(t => regex.IsMatch(t));
        }

        internal static IEnumerable<string> FilterDirectories(Glob glob,
                                                              IEnumerable<string> directories,
                                                              FileSystemEntity resultType,
                                                              FileSystemEnumerator enumerator) {

            IEnumerable<Item> items = Enumerable.Empty<Item>();
            IEnumerable<Item> subItemsStart = directories.Select(
                t => new Item(ExpandEnvironmentVariables(t), FileSystemEntity.Directory)).ToArray();
            foreach (SegmentSequence sub in glob.segments) {

                IEnumerable<Item> subItems = subItemsStart;
                subItems = sub.Enumerate(subItems, enumerator);

                items = items.Concat(subItems);
            }

            // TODO Should inform the last segment of the result type rather than filter here
            return items
                .Where(t => t.Item2 == resultType)
                .Select(t => t.Item1)
                .Distinct();
        }

        static string GetDirectorySeparator() {
            if (Path.DirectorySeparatorChar == Path.AltDirectorySeparatorChar
                || Path.AltDirectorySeparatorChar == '\0') {

                return Regex.Escape(Path.DirectorySeparatorChar.ToString());
            } else {

                return string.Format("({0}|{1})",
                                     Regex.Escape(Path.DirectorySeparatorChar.ToString()),
                                     Regex.Escape(Path.AltDirectorySeparatorChar.ToString()));
            }
        }

        internal abstract class SegmentSequence {

            public abstract IEnumerable<Item> Enumerate(IEnumerable<Item> items,
                                                        FileSystemEnumerator enumerator);

            public abstract void AppendRegex(StringBuilder text);
        }

        // Corresponds to a sequence that is rooted and contains
        // no patterns
        class RootedSegmentSequence : SegmentSequence {

            private readonly string text;

            public RootedSegmentSequence(string text) {
                this.text = text;
            }

            public override IEnumerable<Item> Enumerate(IEnumerable<Item> items,
                                                        FileSystemEnumerator enumerator) {
                return new [] { new Item(text, FileSystemEntity.File) };
            }

            public override void AppendRegex(StringBuilder text)
            {
                text.Append(this.text);
            }
        }

        class IteratedSegmentSequence : SegmentSequence {

            private readonly Segment[] segments;

            public IteratedSegmentSequence(Glob.Segment[] segments) {
                this.segments = segments;
            }

            public override IEnumerable<Item> Enumerate(IEnumerable<Item> items,
                                                        FileSystemEnumerator enumerator) {
                foreach (Segment s in this.segments) {
                    items = s.Enumerate(items, enumerator);
                }

                return items;
            }

            public override void AppendRegex(StringBuilder text)
            {
                foreach (Segment s in segments) {
                    text.Append(s.ToRegexString());
                }
            }

        }

        class MatchSegment : Segment {

            private readonly Regex regex;

            public MatchSegment(string pattern) {
                RegexOptions options = RegexOptions.None;
                if (!Platform.Current.FileSystemCaseSensitive) {
                    options |= RegexOptions.IgnoreCase;
                }
                this.regex = new Regex(pattern, options);
            }

            public override IEnumerable<Item> Enumerate(IEnumerable<Item> items, FileSystemEnumerator enumerator) {
                var result = items.SelectMany(t => enumerator.Invoke(FileSystemEntity.FileOrDirectory, t.Item1)).Where(t => this.regex.IsMatch(t.Item1));
                return result;
            }

            public override string ToRegexString() {
                // Remove trailing $
                string s = regex.ToString();
                return s.Substring(0, s.Length - 1);
            }
        }

        class RecursiveSegment : Segment {

            public override IEnumerable<Item> Enumerate(IEnumerable<Item> items, FileSystemEnumerator enumerator) {

                Queue<Item> results = new Queue<Item>(items.OnlyDirectories());
                while (results.Count > 0) {
                    Item item = results.Dequeue();
                    yield return item;

                    IEnumerable<Item> descendents = enumerator(FileSystemEntity.Directory, item.Item1);
                    foreach (Item d in descendents)
                        results.Enqueue(d);
                }
            }

            public override string ToRegexString() {
                return "(" + NON_PATH_SEP + "+/)*";
            }
        }

        class DeviceSegment : Segment {

            private readonly string device;

            public DeviceSegment(string device) {
                this.device = device;
            }

            public override IEnumerable<Item> Enumerate(
                IEnumerable<Item> items, FileSystemEnumerator enumerator) {

                string deviceName = (device + ":" + Path.DirectorySeparatorChar);
                return items.Concat(new [] { new Item(deviceName, FileSystemEntity.Directory) });
                // return items.Where(t => string.Equals(t.Item1, device + ":", StringComparison.OrdinalIgnoreCase));
            }

            public override string ToRegexString() {
                return "^" + device + ":" + Path.DirectorySeparatorChar;
            }
        }

        class RootSegment : Segment {

            public override IEnumerable<Item> Enumerate(
                IEnumerable<Item> items, FileSystemEnumerator enumerator) {

                return items.Where(t => t.Item1.Length == 1 && (t.Item1[0] == Path.DirectorySeparatorChar
                                                                || t.Item1[0] == Path.AltDirectorySeparatorChar));
            }

            // TODO On Windows, should match / and C:/

            public override string ToRegexString() {
                return "^/";
            }
        }

        class AnyDirectorySegment : Segment {

            public override IEnumerable<Item> Enumerate(
                IEnumerable<Item> items, FileSystemEnumerator enumerator) {

                return items.SelectMany(t => enumerator.Invoke(FileSystemEntity.Directory, t.Item1));
            }

            public override string ToRegexString() {
                return NON_PATH_SEP + "+" + Path.DirectorySeparatorChar;
            }
        }

    }


}
