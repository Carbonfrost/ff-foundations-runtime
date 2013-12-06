//
// - StatusAppender.cs -
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
using System.Collections.ObjectModel;

using Carbonfrost.Commons.Shared.Runtime.Components;
using RuntimeComponent = Carbonfrost.Commons.Shared.Runtime.Components.Component;

namespace Carbonfrost.Commons.Shared.Runtime {

    [Serializable]
    public partial class StatusAppender : IStatusAppender, ICloneable {

        private List<IStatus> children;
        private string message;
        private Exception exception;
        private FileLocation fileLocation;
        private Severity level;
        private Component component;
        private int errorCode;

        [NonSerialized]
        private int hashCodeCache;
        private static readonly NullStatusAppender s_instance = new NullStatusAppender();

        public static IStatusAppender Null { get { return s_instance; } }

        public StatusAppender() {}

        public StatusAppender Clone() {
            StatusAppender result = new StatusAppender();

            if (this.children != null) {
                List<IStatus> newChildren;
                newChildren = new List<IStatus>(this.children.ToArray());

                result.children =  newChildren;
            }

            return result;
        }

        // IStatus implementation

        [SelfDescribingPriority(PriorityLevel.Medium)]
        public string Message { get { return this.message; } }

        [SelfDescribingPriority(PriorityLevel.High)]
        public Exception Exception { get { return this.exception; } }

        [SelfDescribingPriority(PriorityLevel.High)]
        public int ErrorCode { get { return this.errorCode; } }

        [SelfDescribingPriority(PriorityLevel.Low)]
        public FileLocation FileLocation { get { return this.fileLocation; } }

        [SelfDescribingPriority(PriorityLevel.High)]
        public Severity Level { get { return this.level; } }

        [SelfDescribingPriority(PriorityLevel.High)]
        public Component Component { get { return this.component; } }

        public ReadOnlyCollection<IStatus> Children {
            get {
                if (children == null)
                    return Empty<IStatus>.ReadOnly;
                else
                    return children.AsReadOnly();
            }
        }

        // IEquatable<IStatus> implementation.

        bool IEquatable<IStatus>.Equals(IStatus other) { return Equals(other); }

        // 'Object' overrides.

        public override int GetHashCode() {
            if (this.hashCodeCache == 0) {
                unchecked {
                    if (message != null) hashCodeCache += 1000000007 * message.GetHashCode();
                    if (exception != null) hashCodeCache += 1000000009 * exception.GetHashCode();
                    hashCodeCache += 1000000021 * fileLocation.GetHashCode();
                    hashCodeCache += 1000000033 * level.GetHashCode();
                    if (component != null) hashCodeCache += 1000000087 * component.GetHashCode();
                    hashCodeCache += 1000000093 * hashCodeCache.GetHashCode();
                    hashCodeCache += 1000000037 * errorCode.GetHashCode();
                }

                if (this.hashCodeCache == 0)
                    this.hashCodeCache++;
            }

            return this.hashCodeCache;
        }

        public override bool Equals(object obj) {
            IStatus other = obj as IStatus;
            if (other == null) {
                return false;

            } else if (other == this) {
                return true;

            } else if (this.Children.Count > 0 || other.Children.Count > 0) {
                // $gm: Do not waste time comparing children, because the scenario is not
                // likely or useful
                return false;

            } else {
                // Chose this order for speed
                return this.level == other.Level
                    && this.exception == other.Exception
                    && Object.Equals(this.Component, other.Component)
                    && this.fileLocation.Equals(other.FileLocation)
                    && this.message == other.Message;
            }
        }

        // 'ICloneable' implementation
        object ICloneable.Clone() {
            return this.Clone();
        }

        // 'IStatusAppender' implementation.

        public event EventHandler StatusChanged;

        public virtual bool Append(IStatus status) {
            if (status == null) { throw new ArgumentNullException("status"); } // $NON-NLS-1
            if (children == null) { children = new List<IStatus>(2); }

            children.Add(status);

            // Update the status if this is more or as severe
            // N.B. Recall that the actual values of the Severity enumeration are
            // Error=1,  Warning =2,  Information = 3
            if (status.Level != Severity.None && (this.Level == Severity.None || status.Level <= this.Level)) {
                InitializeCore(status.Component, status.Message, status.Exception,
                               status.Level, status.FileLocation, status.ErrorCode);

                OnStatusChanged(EventArgs.Empty);
                return true;
            }

            return false;
        }

        protected virtual void OnStatusChanged(EventArgs e) {
            if (StatusChanged != null) { StatusChanged(this, e); }
        }

        private void InitializeCore(RuntimeComponent component, string message, Exception ex,
                                    Severity level, FileLocation location, int errorCode) {

            this.message = message;
            this.fileLocation = location;
            this.level = level;
            this.exception = ex;
            this.component = component;
            this.errorCode = errorCode;
        }
    }

}
