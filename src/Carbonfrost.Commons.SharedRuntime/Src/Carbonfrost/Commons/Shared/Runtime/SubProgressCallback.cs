//
// - SubProgressCallback.cs -
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

namespace Carbonfrost.Commons.Shared.Runtime {

    internal class SubProgressCallback : IProgressCallback, IProgressCallbackExtension  {

        private decimal totalWorkToDo;
        private decimal workSentToParent;
        private decimal parentScalar;
        private readonly decimal parentUnits;
        private readonly ProgressCallback parent;

        public SubProgressCallback(ProgressCallback parent, decimal parentUnits) {
            this.parent = parent;
            this.parentUnits = parentUnits;
        }


        // `IProgressCallback' implementation.
        public IStatusAppender Status {
            get {
                return parent.Status;
            }
            set { }
        }

        public bool Canceled {
            get {
                return this.parent.Canceled;
            } set {
                this.parent.Canceled = value;
            }
        }

        public void Start(string description, int workToDo) {
            if (workToDo < 0) {
                throw Failure.Negative("workToDo", workToDo);

            } else if (this.totalWorkToDo > 0) {
                throw RuntimeFailure.CannotCallStartTwice();
            }

            if (workToDo == 0) {
                this.parentScalar = 0;
            } else {
                this.parentScalar = (decimal) this.parentUnits / (decimal) workToDo;
            }

            this.totalWorkToDo = workToDo;
        }

        public void Start(int workToDo) {
            Start(null, workToDo);
        }

        public IProgressCallback StartSubtask(int workToDo) {
            ThrowIfNotStarted();
            return new SubProgressCallback(this.parent, this.parentScalar * workToDo);
        }

        public void Work(int units) {
            InternalWork(units);
        }

        public void Done() {
            // Make sure that start is called first
            if (this.totalWorkToDo == 0) {
                Start(1);
            }

            InternalWork(this.parentUnits);
        }

        public void Dispose() {
            Done();
        }


        // `IProgressCallbackExtension' implementation.
        public void ChangeWorkEstimate(int count) {
            if (count < 0)
                throw Failure.NegativeOrZero("count", count); // $NON-NLS-1

            if (count < totalWorkToDo)
                return;

            // Change the total work to do
            this.totalWorkToDo = Math.Max(count, this.totalWorkToDo);
            this.parentScalar = (decimal) this.parentUnits / (decimal) totalWorkToDo;
        }

        private void InternalWork(decimal units) {
            if (units < 0)
                throw Failure.Negative("units", units);

            ThrowIfNotStarted();

            // Send units to parent
            decimal realWork = this.parentScalar * units;

            if (this.workSentToParent + realWork >= this.parentUnits) {
                // Fix real work so we don't go over
                realWork = (this.parentUnits - this.workSentToParent);
            }

            this.parent.InternalWork(realWork);
            this.workSentToParent += realWork;
        }

        private void ThrowIfNotStarted() {
            if (this.totalWorkToDo == 0) {
                throw RuntimeFailure.ProgressCallbackNotStarted();
            }
        }
    }
}
