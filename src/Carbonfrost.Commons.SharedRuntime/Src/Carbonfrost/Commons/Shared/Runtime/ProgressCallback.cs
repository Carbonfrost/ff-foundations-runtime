//
// - ProgressCallback.cs -
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

    public sealed class ProgressCallback : IProgressCallback, IProgressCallbackExtension {

        public const int InfiniteWork = 0;

        private static readonly IProgressCallback s_instance = new NullProgressCallbackImpl();

        private double previousReportedProgress;
        private decimal extendedWorkEstimate;
        private decimal totalWorkToDo;
        private decimal workDoneSoFar;
        private string text = string.Empty;
        private bool infinite;
        private DateTime startedAt;
        private IStatusAppender status;

        // Properties.

        [SelfDescribingPriority((int) PriorityLevel.Low - 1)]
        public bool InfiniteProgress { get { return infinite; } }

        public static IProgressCallback Null { get { return s_instance; } }

        [SelfDescribingPriority(PriorityLevel.High)]
        public double PercentDone {
            get {
                // No zero or negative denominators
                if (this.totalWorkToDo == 0) {
                    return 0;
                } else {
                    // If the work estimate is present, only use it if this would be greater
                    double usingEstimate = Math.Min(1.0, (double) (this.workDoneSoFar / this.extendedWorkEstimate));
                    double usingLegacy = Math.Min(1.0, (double) (this.workDoneSoFar / this.totalWorkToDo));

                    if (usingEstimate > previousReportedProgress) {
                        previousReportedProgress = usingEstimate;
                    } else {
                        previousReportedProgress = usingLegacy;
                    }

                    return previousReportedProgress;
                }
            }
        }

        [SelfDescribingPriority((int) PriorityLevel.High - 1)]
        public string TaskDescription { get { return this.text; } }

        public TimeSpan TimeElapsed {
            get { return DateTime.Now - startedAt; } }

        public TimeSpan TimeRemaining {
            get {
                return (new TimeSpan((long) (TimeElapsed.Ticks / PercentDone))) - TimeElapsed;
            }
        }

        // Constructors.
        public ProgressCallback() {}

        public static IDisposable Work(IProgressCallback callback, int units) {
            if (callback == null)
                throw new ArgumentNullException("callback"); // $NON-NLS-1
            if (units <= 0)
                throw Failure.NegativeOrZero("unit", units);

            return new SimpleContext(callback, units);
        }

        // 'IDisposable' implementation. ]

        void IDisposable.Dispose() {
            Done();
        }

        // 'IProgressCallback' implementation. ]
        public IStatusAppender Status {
            get {
                return status ?? StatusAppender.Null;
            }
            set {
                status = value;
            }
        }

        public void Start(int workToDo) {
            Start(string.Empty, workToDo);
        }

        public void Start(string description, int workToDo) {
            if (description == null) { description = string.Empty; }

            if (workToDo < 0)
                throw Failure.Negative("workToDo", workToDo);

            else if (this.totalWorkToDo > 0)
                throw RuntimeFailure.CannotCallStartTwice();

            if (workToDo > 0)
                this.startedAt = DateTime.Now;

            this.infinite = (workToDo == 0);
            this.totalWorkToDo = workToDo;
            this.workDoneSoFar = 0;
            this.text = description;
        }

        public void Work(int units) {
            ThrowIfNotStarted();
            if (units <= 0)
                throw Failure.Negative("units", units);

            InternalWork(units);
        }

        public void Done() {
            // Make sure that start is called first
            if (this.totalWorkToDo == 0) {
                Start(1);
            }

            InternalWork(this.totalWorkToDo);
        }

		[SelfDescribingPriority(PriorityLevel.High)]
		public bool Canceled {
			get;
			set;
		}

        public IProgressCallback StartSubtask(int workToDo) {
            ThrowIfNotStarted();
            if (workToDo <= 0)
                throw Failure.Negative("workToDo", workToDo); // $NON-NLS-1

            // Do not go over
            decimal d = Math.Min(workToDo, this.totalWorkToDo - this.workDoneSoFar);
            return new SubProgressCallback(this, d);
        }

        // `IProgressCallbackExtension' implementation. ]

        public void ChangeWorkEstimate(int count) {
            if (count < 0)
                throw Failure.NegativeOrZero("count", count); // $NON-NLS-1

            if (count < totalWorkToDo)
                return;

            this.extendedWorkEstimate = count;
        }

        // Nested types. ]

        private class SimpleContext : IDisposable {
            private readonly IProgressCallback callback;
            private readonly int units;

            public SimpleContext(IProgressCallback c, int u) { callback = c; units = u; }
            public void Dispose() { callback.Work(units); }
        }

        private class NullProgressCallbackImpl : IProgressCallback {

            // 'IProgressCallback' implementation. ]

            void IDisposable.Dispose() {}
            void IProgressCallback.Start(int workToDo) {
            }
            void IProgressCallback.Start(string description, int workToDo) {
            }
            void IProgressCallback.Work(int units) {}
            void IProgressCallback.Done() { }
            bool IProgressCallback.Canceled { get { return false; } set { } }
            IProgressCallback IProgressCallback.StartSubtask(int workToDo) { return this; }

            IStatusAppender IProgressCallback.Status {
                get {
                    return StatusAppender.Null;
                }
                set { }
            }
        }

        internal void InternalWork(decimal units) {
            if (units < 0) { throw new ArgumentOutOfRangeException(); }
            if (units == 0) return;

            // Set this to infinite if we continue to work once we are done
            this.infinite = (this.workDoneSoFar >= this.totalWorkToDo
                             && this.workDoneSoFar >= this.extendedWorkEstimate);

            this.workDoneSoFar += units;

            // If we have run over
            if (this.workDoneSoFar > this.totalWorkToDo) {
                this.workDoneSoFar = this.totalWorkToDo;
            }
        }

        private void ThrowIfNotStarted() {
            if (this.totalWorkToDo == 0) {
                throw RuntimeFailure.ProgressCallbackNotStarted();
            }
        }

    }
}
