//
// - StreamContext.cs -
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
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;

namespace Carbonfrost.Commons.Shared.Runtime {

    [TypeConverter(typeof(StreamContextConverter))]
    public abstract class StreamContext : IUriContext {

        public static readonly StreamContext Null = new NullStreamContext();
        public static readonly StreamContext Invalid = new InvalidStreamContext();

        [SelfDescribingPriority(PriorityLevel.Medium)]
        public abstract CultureInfo Culture { get; }

        [SelfDescribingPriority(PriorityLevel.Low)]
        public virtual ContentType ContentType {
            get { return ContentType.Parse(ContentTypes.Binary); }
        }

        protected Uri BaseUri { get; set; }

        [SelfDescribingPriority(PriorityLevel.Medium)]
        public virtual string DisplayName {
            get { return Uri.PathAndQuery; }
        }

        [SelfDescribingPriority(PriorityLevel.Low)]
        public virtual Encoding Encoding {
            get { return Utility.GetEncodingFromContentType(this.ContentType.Parameters.GetValueOrDefault("charset")); }
        }

        [SelfDescribingPriority(PriorityLevel.Low)]
        public virtual bool IsAnonymous { get { return false; } }

        [SelfDescribingPriority(PriorityLevel.Low)]
        public virtual bool IsLocal { get { return true; } }

        [SelfDescribingPriority(PriorityLevel.Low)]
        public virtual bool IsMultipart { get { return false; } }

        [SelfDescribingPriority(PriorityLevel.High)]
        public abstract Uri Uri { get; }

        protected StreamContext() {}

        public StreamWriter AppendText() {
            return AppendText(null);
        }

        public StreamWriter AppendText(Encoding encoding) {
            return new StreamWriter(
                GetStream(FileAccess.Write, FallbackBehavior.ThrowException), encoding ?? this.Encoding);
        }

        public abstract StreamContext ChangeCulture(CultureInfo resourceCulture);

        public abstract StreamContext ChangePath(string relativePath);

        public virtual StreamContext ChangeContentType(ContentType contentType) {
            throw new NotSupportedException();
        }

        public virtual StreamContext ChangeEncoding(Encoding encoding) {
            throw new NotSupportedException();
        }

        public StreamContext ChangeExtension(string extension) {
            string localName = Path.GetFileName(Uri.LocalPath);
            int index = localName.LastIndexOf('.'); // $NON-NLS-1

            // Replace the local name with the extension
            index = (index < 0) ? localName.Length - 1 : index;
            string targetFile
                = localName.Substring(0, index) + extension; // $NON-NLS-1

            return ChangePath("./" + targetFile); // $NON-NLS-1
        }

        public Stream GetStream(FileAccess fileAccess, FallbackBehavior fallbackOn) {
            Stream result = GetStreamCore(fileAccess);

            if (result == null) {
                switch (fallbackOn) {
                    case FallbackBehavior.CreateDefault:
                        return Stream.Null;

                    case FallbackBehavior.None:
                        return null;

                    case FallbackBehavior.ThrowException:
                    default:
                        throw RuntimeFailure.StreamContextDoesNotExist();
                }
            } else {
                return result;
            }
        }

        public Stream GetStream(FallbackBehavior fallbackOn) {
            return GetStream(FileAccess.ReadWrite, fallbackOn);
        }

        public Stream GetStream() { return GetStream(FallbackBehavior.CreateDefault); }

        protected abstract Stream GetStreamCore(FileAccess access);

        public StreamReader OpenText() {
            return OpenText(null);
        }

        public StreamReader OpenText(Encoding encoding) {
            encoding = encoding ?? this.Encoding;
            if (encoding == null)
                return new StreamReader(
                    GetStream(FileAccess.Read, FallbackBehavior.ThrowException));
            else
                return new StreamReader(
                    GetStream(FileAccess.Read, FallbackBehavior.ThrowException), encoding);
        }

        public Stream OpenRead() {
            return GetStream(FileAccess.Read, FallbackBehavior.ThrowException);
        }

        public Stream OpenWrite() {
            return GetStream(FileAccess.Write, FallbackBehavior.ThrowException);
        }

        public object ReadValue(Type componentType) {
            return Activation.FromStreamContext(componentType, this);
        }

        public T ReadValue<T>() {
            return (T) ReadValue(typeof(T));
        }

        public byte[] ReadAllBytes() {
            using (Stream s = GetStream(FileAccess.Read, FallbackBehavior.ThrowException))
                return s.BufferedCopyToBytes();
        }

        public string[] ReadAllLines() {
            return ReadAllLines(null);
        }

        public IEnumerable<string> ReadLines() {
            return ReadLines(null);
        }

        public IEnumerable<string> ReadLines(Encoding encoding) {
            using (StreamReader reader = OpenText(encoding)) {
                string s;
                while ((s = reader.ReadLine()) != null) {
                    yield return s;
                }
            }
        }

        public string[] ReadAllLines(Encoding encoding) {
            List<string> result = new List<string>();
            result.AddRange(ReadLines(encoding));
            return result.ToArray();
        }

        public string ReadAllText(Encoding encoding) {
            using (StreamReader sr = OpenText(encoding)) {
                return sr.ReadToEnd();
            }
        }

        public string ReadAllText() {
            return ReadAllText(null);
        }

        public void WriteValue(Type componentType, object value) {
            if (componentType == null)
                throw new ArgumentNullException("componentType");
            if (value == null)
                throw new ArgumentNullException("value");

            StreamingSource ss = StreamingSource.Create(
                componentType, this.ContentType, null, ServiceProvider.Null);
            ss.Save(this, value);
        }

        public void WriteValue<T>(T value) {
            WriteValue(typeof(T), value);
        }

        public void WriteAllBytes(byte[] value) {
            if (value == null)
                throw new ArgumentNullException("value");

            using (Stream s = GetStream(FileAccess.Write, FallbackBehavior.ThrowException))
                new MemoryStream(value).CopyTo(s);
        }

        public void WriteAllLines(string[] lines) {
            if (lines == null)
                throw new ArgumentNullException("lines");

            WriteAllLines(lines, null);
        }

        public void WriteAllLines(string[] lines, Encoding encoding) {
            if (lines == null)
                throw new ArgumentNullException("lines");

            using (TextWriter w = AppendText(encoding)) {
                foreach (string line in lines)
                    w.WriteLine(line);
            }
        }

        public void WriteAllText(string text, Encoding encoding) {
            using (StreamWriter sr = AppendText(encoding)) {
                sr.Write(text);
            }
        }

        public void WriteAllText(string text) {
            WriteAllText(text, null);
        }

        // 'Object' overrides.
        public override string ToString() { return this.Uri.ToString(); }

        // Static construction methods.
        public static StreamContext FromAssemblyManifestResource(Assembly assembly, string resourceName) {
            if (assembly == null) { throw new ArgumentNullException("assembly"); } // $NON-NLS-1
            Require.NotNullOrEmptyString("resourceName", resourceName); // $NON-NLS-1

            return new AssemblyManifestResourceStreamContext(assembly, resourceName);
        }

        public static StreamContext FromByteArray(byte[] data) {
            if (data == null)
                throw new ArgumentNullException("data");

            var ms = new MemoryStream(data, false);
            return new DataStreamContext(ms, ContentType.Parse(ContentTypes.Binary));
        }

        public static StreamContext FromText(string text, Encoding encoding) {
            if (string.IsNullOrEmpty(text))
                return StreamContext.Null;

            encoding = (encoding ?? Encoding.UTF8);
            MemoryStream ms = new MemoryStream(encoding.GetBytes(text));

            var param = new Dictionary<string, string> {
                { "charset", encoding.WebName }
            };

            return new DataStreamContext(ms, new ContentType("text", "plain", param));
        }

        public static StreamContext FromText(string text) {
            return FromText(text, Encoding.UTF8);
        }

        public static StreamContext FromFile(string fileName) {
            return new FileSystemStreamContext(fileName);
        }

        public static StreamContext FromFile(string fileName,
                                             CultureInfo resourceCulture) {
            Require.NotNullOrEmptyString("fileName", fileName); // $NON-NLS-1

            if (resourceCulture == null) { throw new ArgumentNullException("resourceCulture"); } // $NON-NLS-1
            return new FileSystemStreamContext(fileName, resourceCulture, null);
        }

        public static StreamContext FromStream(Stream stream) {
            if (stream == null) { throw new ArgumentNullException("stream"); } // $NON-NLS-1
            return new StreamStreamContext(stream, null);
        }

        public static StreamContext FromStream(Stream stream, Encoding encoding) {
            if (stream == null)
                throw new ArgumentNullException("stream");  // $NON-NLS-1
            return new StreamStreamContext(stream, encoding);
        }

        public static StreamContext FromSource(Uri source) {
            if (source == null)
                throw new ArgumentNullException("source");

            // Look for native providers (file, res, iso, mem, stream)
            if (source.IsAbsoluteUri) {
                switch (source.Scheme) {
                    case "file": // $NON-NLS-1
                        return FromFile(source.LocalPath);

                    case "res": // $NON-NLS-1
                        return AssemblyManifestResourceStreamContext.CreateResFromUri(source);

                    case "data": // $NON-NLS-1
                        return new DataStreamContext(source);

                    case "invalid": // $NON-NLS-1
                        return StreamContext.Invalid;

                    case "null": // $NON-NLS-1
                        return StreamContext.Null;

                    case "stdout": // $NON-NLS-1
                        return new StreamStreamContext(new Uri("stdout://"),
                                                       Console.OpenStandardOutput(),
                                                       Console.OutputEncoding);

                    case "stderr": // $NON-NLS-1
                        return new StreamStreamContext(new Uri("stderr://"),
                                                       Console.OpenStandardError(),
                                                       Console.OutputEncoding);

                    case "stdin": // $NON-NLS-1
                        return new StreamStreamContext(new Uri("stdin://"),
                                                       Console.OpenStandardInput(),
                                                       Console.InputEncoding);

                    case "stream": // $NON-NLS-1
                        throw RuntimeFailure.ForbiddenStreamStreamContext();

                    default:
                        // Fall back to the URI
                        return new UriStreamContext(source, null);
                }
            } else {
                // Relative URIs must be handled in this way
                return FromFile(source.ToString());
            }
        }

        // IUriContext
        Uri IUriContext.BaseUri {
            get { return BaseUri; }
            set { BaseUri = value; }
        }

    }

}
