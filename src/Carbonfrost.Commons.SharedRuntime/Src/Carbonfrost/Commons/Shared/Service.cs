//
// - Service.cs -
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
using Carbonfrost.Commons.Shared.Runtime;

namespace Carbonfrost.Commons.Shared {

    public abstract class Service : DisposableObject, IService, IServiceProvider {

        private IStatusAppender status;

        public Exception InitializationError { get; private set; }

        public bool IsStarted {
            get { return (this.ServiceState & ServiceStates.Started) == ServiceStates.Started; }
            private set {
                if (value)
                    this.ServiceState |= ServiceStates.Started;
                else
                    this.ServiceState &= ~ServiceStates.Started;
            }
        }

        public bool IsEnded { get { return this.IsDisposed; } }

        protected Service() {
            this.status = StatusAppender.ForType(GetType());
        }

        protected virtual void StartServiceCore(IStatusAppender status) {
        }

        protected virtual void EndServiceCore() {
        }

        protected virtual void OnStarting(EventArgs e) {
            if (Starting != null)
                Starting(this, e);
        }

        protected virtual void OnStarted(EventArgs e) {
            if (Started != null)
                Started(this, e);
        }

        protected virtual void OnEnded(EventArgs e) {
            if (Ended != null)
                Ended(this, e);
        }

        protected virtual void OnEnding(EventArgs e) {
            if (Ending != null)
                Ending(this, e);
        }

        // IService
        public event EventHandler Starting;
        public event EventHandler Started;
        public event EventHandler Ended;
        public event EventHandler Ending;

        public ServiceStates ServiceState { get; private set; }
        public IStatus Status { get { return status; } }

        public void StartService() {
            if (!this.IsStarted) {
                try {
                    this.status = new StatusAppender();
                    StartServiceCore(this.status);

                } catch (Exception ex) {
                    if (Require.IsCriticalException(ex))
                        throw;

                    this.InitializationError = ex;
                    this.ServiceState |= ServiceStates.WithErrors;
                    RuntimeWarning.ServiceFailedToStart(
                        this.status, GetType(), ex);
                }

                this.IsStarted = true;
            }
        }

        public void EndService() {
            if (!this.IsDisposed) {

                this.ServiceState |= ServiceStates.Disposing;
                EndServiceCore();

                this.ServiceState &= ~ServiceStates.Disposing;
                this.ServiceState |= ServiceStates.Disposed;
            }
        }

        // DisposableObject overrides
        protected sealed override void Dispose(bool disposing) {
            if (disposing)
                EndService();

            base.Dispose(disposing);
        }

        // IServiceProvider implementation
        public virtual object GetService(Type serviceType) {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");

            if (typeof(IService).Equals(serviceType))
                return this;

            if (typeof(IStatusAppender).Equals(serviceType))
                return this.status;

            if (typeof(Service).Equals(serviceType))
                return this;

            return null;
        }
    }
}
