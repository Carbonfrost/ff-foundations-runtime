//
// - SynchronizedStatusAppender.cs -
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
using System.Threading;
using Carbonfrost.Commons.Shared.Runtime.Components;

namespace Carbonfrost.Commons.Shared.Runtime {

    sealed class SynchronizedStatusAppender : IStatusAppender {

        private readonly IStatusAppender inner;
        private readonly ReaderWriterLockSlim syncRoot = new ReaderWriterLockSlim();

        public SynchronizedStatusAppender(IStatusAppender inner) {
            this.inner = inner;
        }

        public event EventHandler StatusChanged {
            add {
                inner.StatusChanged += value;
            }
            remove {
                inner.StatusChanged -= value;
            }
        }

        public Exception Exception {
            get {
                try {
                    syncRoot.EnterReadLock();
                    return inner.Exception;
                } finally {
                    syncRoot.ExitReadLock();
                }
            }
        }

        public FileLocation FileLocation {
            get {
                try {
                    syncRoot.EnterReadLock();
                    return inner.FileLocation;
                } finally {
                    syncRoot.ExitReadLock();
                }
            }
        }

        public ReadOnlyCollection<IStatus> Children {
            get {
                return inner.Children;
            }
        }

        public Severity Level {
            get {
                try {
                    syncRoot.EnterReadLock();
                    return inner.Level;
                } finally {
                    syncRoot.ExitReadLock();
                }
            }
        }

        public string Message {
            get {
                try {
                    syncRoot.EnterReadLock();
                    return inner.Message;
                } finally {
                    syncRoot.ExitReadLock();
                }
            }
        }

        public int ErrorCode {
            get {
                try {
                    syncRoot.EnterReadLock();
                    return inner.ErrorCode;
                } finally {
                    syncRoot.ExitReadLock();
                }
            }
        }

        public Component Component {
            get {
                try {
                    syncRoot.EnterReadLock();
                    return inner.Component;
                } finally {
                    syncRoot.ExitReadLock();
                }
            }
        }

        public bool Append(IStatus status) {
            if (status == null)
                throw new ArgumentNullException("status");

            try {
                syncRoot.EnterWriteLock();
                return inner.Append(status);
            } finally {
                syncRoot.ExitWriteLock();
            }
        }

        public bool Equals(IStatus other) {
            return Status.StaticEquals(this, other);
        }
    }
}
