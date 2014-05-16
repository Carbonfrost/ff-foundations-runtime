//
// - StatusAppenderDecorator.cs -
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
using System.Collections.ObjectModel;
using Carbonfrost.Commons.Shared.Runtime.Components;

namespace Carbonfrost.Commons.Shared.Runtime {


    internal abstract class StatusAppenderDecorator : DisposableObject, IStatusAppender {

        private readonly IStatusAppender baseAppender;

        protected IStatusAppender BaseAppender {
            get {
                return baseAppender;
            }
        }

        protected StatusAppenderDecorator(IStatusAppender baseAppender) {
            if (baseAppender == null)
                throw new ArgumentNullException("baseAppender"); // $NON-NLS-1

            this.baseAppender = baseAppender;
            this.BaseAppender.StatusChanged += HandleStatusChanged;
        }

        protected override void Dispose(bool manualDispose) {
            if (manualDispose) {
                this.BaseAppender.StatusChanged -= HandleStatusChanged;
            }
            base.Dispose(manualDispose);
        }

        private void HandleStatusChanged(object sender, EventArgs e) {
            OnStatusChanged(EventArgs.Empty);
        }

        // 'IStatusAppender' implementation.
        public event EventHandler StatusChanged;

        protected virtual void OnStatusChanged(EventArgs e) {
            var handler = StatusChanged;
            if (handler != null)
                handler(this, e);
        }

        public virtual int ErrorCode {
            get {
                return this.BaseAppender.ErrorCode;
            }
        }

        public virtual Severity Level {
            get {
                return this.BaseAppender.Level;
            }
        }

        public virtual string Message {
            get {
                return this.BaseAppender.Message;
            }
        }

        public virtual Exception Exception {
            get {
                return this.BaseAppender.Exception;
            }
        }

        public virtual FileLocation FileLocation {
            get {
                return this.BaseAppender.FileLocation;
            }
        }

        public virtual Component Component {
            get {
                return this.BaseAppender.Component;
            }
        }

        public virtual ReadOnlyCollection<IStatus> Children {
            get {
                return BaseAppender.Children;
            }
        }

        public virtual bool Append(IStatus status) {
            if (status == null)
                throw new ArgumentNullException("status");  // $NON-NLS-1

            return this.BaseAppender.Append(status);
        }

        public bool Equals(IStatus other) {
            return Status.StaticEquals(this, other);
        }

    }

}
