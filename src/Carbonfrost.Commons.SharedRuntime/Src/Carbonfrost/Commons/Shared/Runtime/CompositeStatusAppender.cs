//
// - CompositeStatusAppender.cs -
//
// Copyright 2014 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Collections.ObjectModel;
using System.Linq;
using Carbonfrost.Commons.Shared.Runtime.Components;

namespace Carbonfrost.Commons.Shared.Runtime {

    class CompositeStatusAppender : DisposableObject, IStatusAppender, IEnumerable<IStatusAppender> {

        private readonly IReadOnlyCollection<IStatusAppender> items;

        public CompositeStatusAppender(IReadOnlyCollection<IStatusAppender> items) {
            this.items = items;
        }

        protected override void Dispose(bool manualDispose) {
            if (manualDispose) {
                Safely.Dispose(items);
            }

            base.Dispose(manualDispose);
        }

        protected virtual void OnStatusChanged(EventArgs e) {
            var handler = StatusChanged;
            if (handler != null)
                handler(this, e);
        }

        // 'IStatusAppender' implementation.
        public event EventHandler StatusChanged;

        public virtual int ErrorCode {
            get {
                return this.First().ErrorCode;
            }
        }

        public virtual Severity Level {
            get {
                return this.First().Level;
            }
        }

        public virtual string Message {
            get {
                return this.First().Message;
            }
        }

        public virtual Exception Exception {
            get {
                return this.First().Exception;
            }
        }

        public virtual FileLocation FileLocation {
            get {
                return this.First().FileLocation;
            }
        }

        public virtual Component Component {
            get {
                return this.First().Component;
            }
        }

        public virtual ReadOnlyCollection<IStatus> Children {
            get {
                return this.First().Children;
            }
        }

        public virtual bool Append(IStatus status) {
            bool result = this.First().Append(status);

            foreach (var m in items.Skip(1)) {
                m.Append(status);
            }

            return result;
        }

        public bool Equals(IStatus other) {
            return Status.StaticEquals(this, other);
        }

        public IEnumerator<IStatusAppender> GetEnumerator() {
            return this.items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}


