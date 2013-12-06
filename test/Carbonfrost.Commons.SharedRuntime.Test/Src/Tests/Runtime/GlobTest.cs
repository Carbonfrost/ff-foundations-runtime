//
// - GlobTest.cs -
//
// Copyright 2013 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

using Carbonfrost.Commons.Shared;
using NUnit.Framework;

namespace Tests {

    [TestFixture]
    public class GlobTest {

        [Test]
        public void assumptions_for_fileobject() {
            // Sanity checks on the implementation of FileObject

            var e = Fixture1Enumerator();
            var f = Fixture1();
            string[] results = Glob.FilterDirectories(Glob.Anything,
                                                      new string[] { "\\" },
                                                      Glob.FileSystemEntity.File,
                                                      e).ToArray();
            Assert.That(f.Visit().Keys.Sorted().ToArray(),
                        Is.EquivalentTo(new [] {
                                            "\\",
                                            "\\a",
                                            "\\a\\b.txt",
                                            "\\a\\c.csv",
                                            "\\a\\d.txt",
                                            "\\a\\e",
                                            "\\a\\e\\f",
                                            "\\a\\e\\f\\g",
                                            "\\a\\e\\f\\g\\h.txt",
                                            "\\a\\e\\f\\g\\i.txt",
                                            "\\a\\e\\f\\j",
                                            "\\a\\e\\.k",
                                            "\\a\\e\\l.csv",
                                        }.Sorted()));

            Assert.That(e.Invoke(Glob.FileSystemEntity.Directory, "\\").AsPaths().ToArray(),
                        Is.EquivalentTo(new [] {
                                            "\\a"
                                        }));
            Assert.That(e.Invoke(Glob.FileSystemEntity.Directory, "\\a").AsPaths().ToArray(),
                        Is.EquivalentTo(new [] {
                                            "\\a\\e"
                                        }));
            Assert.That(e.Invoke(Glob.FileSystemEntity.File, "\\a").AsPaths().ToArray(),
                        Is.EquivalentTo(new [] {
                                            "\\a\\b.txt",
                                            "\\a\\c.csv",
                                            "\\a\\d.txt"
                                        }));

        }

        [Test]
        public void enumerate_rooted_directory() {
            // When a root directory is used, it is checked directly
            var e = Fixture1Enumerator();

            string[] results = Glob.FilterDirectories(Glob.Parse("\\a\\e\\.k"),
                                                      new string[] { "\\anywhere\\s" },
                                                      Glob.FileSystemEntity.File,
                                                      e).ToArray();

            Assert.That(results, Is.EquivalentTo(
                new [] {
                    "\\a\\e\\.k",
                }));
        }

        [Test]
        public void enumerate_literal_directory() {
            var e = Fixture1Enumerator();
            var glob = Glob.Parse("a\\e\\**\\*.txt");

            string[] results = Glob.FilterDirectories(Glob.Parse("a\\e\\**\\*.txt"),
                                                      new string[] { "\\" },
                                                      Glob.FileSystemEntity.File,
                                                      e).ToArray();

            Assert.That(results, Is.EquivalentTo(
                new [] {
                    "\\a\\e\\f\\g\\h.txt",
                    "\\a\\e\\f\\g\\i.txt",
                }));
        }

        [Test]
        public void enumerate_files_in_multiglob() {
            var e = Fixture1Enumerator();

            string[] results = Glob.FilterDirectories(Glob.Parse("**\\*.csv;**\\*.txt"),
                                                      new string[] { "\\" },
                                                      Glob.FileSystemEntity.File,
                                                      e).ToArray();

            Assert.That(results, Is.EquivalentTo(
                new [] {
                    "\\a\\b.txt",
                    "\\a\\c.csv",
                    "\\a\\d.txt",
                    "\\a\\e\\f\\g\\h.txt",
                    "\\a\\e\\f\\g\\i.txt",
                    "\\a\\e\\l.csv",
                }));
        }

        [Test]
        public void enumerate_files_by_extension() {
            var e = Fixture1Enumerator();

            string[] results = Glob.FilterDirectories(Glob.Parse("**\\*.csv"),
                                                      new string[] { "\\" },
                                                      Glob.FileSystemEntity.File,
                                                      e).ToArray();

            Assert.That(results, Is.EquivalentTo(
                new [] {
                    "\\a\\c.csv",
                    "\\a\\e\\l.csv",
                }));
        }

        [Test]
        public void enumerate_every_file() {
            var e = Fixture1Enumerator();

            string[] results = Glob.FilterDirectories(Glob.Anything,
                                                      new string[] { "\\" },
                                                      Glob.FileSystemEntity.File,
                                                      e).ToArray();

            Assert.That(results, Is.EquivalentTo(
                new [] {
                    "\\a\\b.txt",
                    "\\a\\c.csv",
                    "\\a\\d.txt",
                    "\\a\\e\\f\\g\\h.txt",
                    "\\a\\e\\f\\g\\i.txt",
                    "\\a\\e\\.k",
                    "\\a\\e\\l.csv",
                }));
        }

        [Test]
        public void enumerate_top_level_files() {
            var e = Fixture1Enumerator();
            string[] results = Glob.FilterDirectories(Glob.Parse("*.*"),
                                                      new string[] { "\\a" },
                                                      Glob.FileSystemEntity.File,
                                                      e).ToArray();

            Assert.That(Glob.Parse("*.*").ToRegex().IsMatch("\\a\\b.txt"));
            Assert.That(results, Is.EquivalentTo(
                new [] {
                    "\\a\\b.txt",
                    "\\a\\c.csv",
                    "\\a\\d.txt",
                }));
        }

        [Test]
        public void enumerate_top_level_directories() {
            var e = Fixture1Enumerator();
            string[] results = Glob.FilterDirectories(Glob.Parse("*"),
                                                      new string[] { "\\" },
                                                      Glob.FileSystemEntity.Directory,
                                                      e).ToArray();

            Assert.That(results, Is.EquivalentTo(
                new [] {
                    "\\a",
                }));
        }


        [Test]
        public void enumerate_root_directory() {
            var e = Fixture1Enumerator();
            string[] results = Glob.FilterDirectories(Glob.Parse("\\*"),
                                                      new string[] { "\\" },
                                                      Glob.FileSystemEntity.Directory,
                                                      e).ToArray();

            Assert.That(results, Is.EquivalentTo(
                new [] {
                    "\\a",
                }));
        }

          [Test]
        public void enumerate_root_directory_child_by_name() {
            var e = Fixture1Enumerator();
            string[] results = Glob.FilterDirectories(Glob.Parse("a\\*.txt"),
                                                      new string[] { "\\" },
                                                      Glob.FileSystemEntity.File,
                                                      e).ToArray();

            Assert.That(results, Is.EquivalentTo(
                new [] {
                    "\\a\\b.txt",
                    "\\a\\d.txt",
                }));
        }

        [Test]
        public void enumerate_by_bracket_choice_recursive() {
            // TODO [a-z].txt could be used
            var e = Fixture1Enumerator();

            string[] results = Glob.FilterDirectories(Glob.Parse("**\\[bhi].txt"),
                                                      new string[] { "\\" },
                                                      Glob.FileSystemEntity.File,
                                                      e).ToArray();
            Assert.That(Glob.Parse("**/[bhi].txt").ToRegex().ToString(), Is.EqualTo("([^/\\:]+/)*(\\\\|/)(b|h|i)\\.txt$"));
            Assert.That(Glob.Parse("**/[bhi].txt").IsMatch("/a/e/f/g/h.txt"));
            Assert.That(results, Is.EquivalentTo(
                new [] {
                    "\\a\\b.txt",
                    "\\a\\e\\f\\g\\h.txt",
                    "\\a\\e\\f\\g\\i.txt",
                }));
        }

        [Test]
        public void enumerate_by_wildcard_recursive_similar_names() {
            var e = Fixture2Enumerator();

            // Should exclude the ~a dire
            string[] results = Glob.FilterDirectories(Glob.Parse("src\\**\\a\\*.txt"),
                                                      new string[] { "\\" },
                                                      Glob.FileSystemEntity.File,
                                                      e).ToArray();
            Assert.That(results, Is.EquivalentTo(
                new [] {
                    "\\src\\a\\bon.txt",
                    "\\src\\a\\bot.txt",
                }));
        }

        [Test]
        public void glob_anything_equivalence() {
            Assert.That(Glob.Anything, Is.EqualTo(Glob.Parse("**\\*.*")));
        }

        private static Glob.FileSystemEnumerator CreateEnumerator(FileObject file) {
            var map = file.Visit();

            Glob.FileSystemEnumerator e = (entityType, path) => {
                FileObject result;
                if (!map.TryGetValue(path, out result))
                    result = FileObject.Empty;

                IEnumerable<Tuple<string, Glob.FileSystemEntity>> items = result.Files.Select(t => t.AsTuple());
                if (entityType != Glob.FileSystemEntity.FileOrDirectory)
                    items = items.Where(t => t.Item2 == entityType);

                return items;

            };

            return e;
        }

        private static Glob.FileSystemEnumerator Fixture1Enumerator() {
            return CreateEnumerator(Fixture1());
        }

        private static FileObject Fixture1() {
            return Root(Dir("a",
                            File("b.txt"),
                            File("c.csv"),
                            File("d.txt"),
                            Dir("e",
                                Dir("f",
                                    Dir("g",
                                        File("h.txt"),
                                        File("i.txt")),
                                    File("j")),
                                File(".k"),
                                File("l.csv"))));
        }

        private static Glob.FileSystemEnumerator Fixture2Enumerator() {
            return CreateEnumerator(Fixture2());
        }

        private static FileObject Fixture2() {
            return Root(Dir("src",
                            Dir("a",
                                File("bon.txt"),
                                File("boy.csv"),
                                File("bot.txt"),
                                File("l.csv")),
                            Dir("ca",
                                File("bon.txt"),
                                File("boy.csv"),
                                File("bot.txt"),
                                File("l.csv")),
                            Dir("~a",
                                File("bon.txt"),
                                File("boy.csv"),
                                File("bot.txt"),
                                File("l.csv"))));
        }

        sealed class FileObject {

            public readonly Glob.FileSystemEntity Type;
            public readonly string Name;
            public readonly List<FileObject> Files = new List<FileObject>();
            public string Path;

            public static readonly FileObject Empty = new FileObject(Glob.FileSystemEntity.Directory, null, Enumerable.Empty<FileObject>());

            public FileObject(Glob.FileSystemEntity type, string name, IEnumerable<FileObject> files) {
                this.Name = name;
                this.Type = type;

                if (files != null) {
                    foreach (var file in files)
                        this.Files.Add(file);
                }
            }

            public Tuple<string, Glob.FileSystemEntity> AsTuple() {
                return new Tuple<string, Glob.FileSystemEntity>(Path, Type);
            }

            public IDictionary<string, FileObject> Visit() {
                var result = new Dictionary<string, FileObject>();
                Visit(string.Empty, result);
                return result;
            }

            public void Visit(string path, IDictionary<string, FileObject> map) {
                this.Path = string.Concat(path, this.Name);
                path = this.Path + "\\";

                if (this.Path == string.Empty) this.Path = "\\";
                foreach (var f in this.Files) {
                    f.Visit(path, map);
                }

                map.Add(this.Path, this);
            }
        }


        private static FileObject Root(params FileObject[] files) {
            return new FileObject(Glob.FileSystemEntity.Directory, "", files);
        }

        private static FileObject Dir(string name, params FileObject[] files) {
            return new FileObject(Glob.FileSystemEntity.Directory, name, files);
        }

        private static FileObject File(string name) {
            return new FileObject(Glob.FileSystemEntity.File, name, null);
        }

    }

}
