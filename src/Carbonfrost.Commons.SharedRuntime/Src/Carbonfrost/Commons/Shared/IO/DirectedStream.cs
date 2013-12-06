//
// - DirectedStream.cs -
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
using System.IO;
using Carbonfrost.Commons.Shared.Runtime;

namespace Carbonfrost.Commons.Shared {

    internal class DirectedStream : Stream {

        private bool isDisposed;
        private readonly Stream baseStream;
        private readonly StreamDirection direction;

        protected Stream BaseStream {
            get {
                ThrowIfDisposed();
                return this.baseStream;
            }
        }

        public DirectedStream(StreamDirection direction, Stream baseStream) {
            this.direction = direction;
            this.baseStream = baseStream;
        }

        protected void ThrowIfDisposed() {
            if (isDisposed) {
                throw Failure.Disposed(ToString());
            }
        }

        // 'Stream' overrides.

        public override bool CanRead {
            get {
                ThrowIfDisposed();
                return this.direction == StreamDirection.Read && BaseStream.CanRead;
            }
        }

        public override bool CanWrite {
            get {
                ThrowIfDisposed();
                return this.direction == StreamDirection.Write && BaseStream.CanWrite;
            }
        }

        public override bool CanSeek {
            get {
                ThrowIfDisposed();
                return this.baseStream.CanSeek;
            }
        }

        public override long Length {
            get {
                ThrowIfDisposed();
                return BaseStream.Length;
            }
        }

        public override long Position {
            get {
                ThrowIfDisposed();
                return BaseStream.Position;
            }
            set {
                ThrowIfDisposed();
                BaseStream.Position = value;
            }
        }

        public override void SetLength(long value) {
            ThrowIfDisposed();

            if (!CanSeek)
                throw new NotSupportedException();

            this.BaseStream.SetLength(value);
        }

        public override long Seek(long offset, SeekOrigin origin) {
            if (!BaseStream.CanSeek)
                throw RuntimeFailure.SeekNotSupportedByBase();

            if (!(origin == SeekOrigin.Begin || origin == SeekOrigin.End || origin == SeekOrigin.Current)) {
                throw Failure.NotDefinedEnum("origin", origin); // $NON-NLS-1
            }

            if (offset < 0 && origin == SeekOrigin.Begin)
                throw RuntimeFailure.SeekNegativeBegin("offset", offset);

            ThrowIfDisposed();

            // Do the seek on the base
            return BaseStream.Seek(offset, origin);
        }

        public override void Flush() {
            baseStream.Flush();
        }

        protected override void Dispose(bool disposing) {
            this.isDisposed = true;

            if (disposing) {
                this.baseStream.Flush();
                this.baseStream.Close();
                this.baseStream.Dispose();
            }

            base.Dispose(disposing);
        }

        public override int Read(byte[] buffer, int offset, int count) {
            ThrowIfDisposed();

            if (CanRead) {
                return BaseStream.Read(buffer, offset, count);
            } else {
                throw RuntimeFailure.DirectedStreamCannotRead();
            }
        }

        public override void Write(byte[] buffer, int offset, int count) {
            ThrowIfDisposed();

            if (CanWrite) {
                BaseStream.Write(buffer, offset, count);
            } else {
                throw RuntimeFailure.DirectedStreamCannotWrite();
            }
        }
    }
}
