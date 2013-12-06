//
// - Status.cs -
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
using System.Globalization;

using Carbonfrost.Commons.Shared.Runtime;
using Carbonfrost.Commons.Shared.Runtime.Components;

using RuntimeComponent = Carbonfrost.Commons.Shared.Runtime.Components.Component;

namespace Carbonfrost.Commons.Shared.Runtime {

    [Serializable]
    [Builder(typeof(StatusBuilder))]
    public partial class Status : IStatus, IFormattable, IFileLocationConsumer {

        private string message;
        private Exception exception;
        private FileLocation fileLocation;
        private Severity level;
        private int errorCode;
        private RuntimeComponent component;

        private int hashCodeCache;

        private static readonly NullStatus s_instance = new NullStatus();

        // Properties.

        public static IStatus Null { get { return s_instance; } }

        // Constructors.

        public Status(RuntimeComponent component, string message, Exception exception, FileLocation location, int errorCode)
            : this(component ?? RuntimeComponent.Unknown,
                   Severity.Error,
                   message,
                   exception,
                   location,
                   errorCode) {
        }

        public Status(RuntimeComponent component, string message, Exception exception, FileLocation location)
            : this(component ?? RuntimeComponent.Unknown,
                   Severity.Error,
                   message,
                   exception,
                   location,
                   GuessErrorLevel(Severity.Error)) {
        }

        public Status(RuntimeComponent component,
                      Severity level,
                      string message,
                      FileLocation location)
            : this(component ?? RuntimeComponent.Unknown,
                   level,
                   message,
                   null,
                   location,
                   GuessErrorLevel(level)) {
        }

        public Status(RuntimeComponent component, Severity level, Exception exception, FileLocation location)
            : this(component ?? RuntimeComponent.Unknown,
                   level,
                   _Require(exception).Message,
                   exception,
                   location) {
        }

        public Status(RuntimeComponent component, Exception exception, FileLocation location)
            : this(component ?? RuntimeComponent.Unknown,
                   Severity.Error,
                   _Require(exception).Message,
                   exception,
                   location) {
        }

        public Status(Severity level, string message)
            : this(null,
                   level,
                   message,
                   null,
                   new FileLocation()) {
        }

        public Status(Severity level, Exception exception)
            : this(null,
                   level,
                   _Require(exception).Message,
                   exception,
                   new FileLocation()) {
        }

        public Status(Exception exception)
            : this(null,
                   Severity.Error,
                   _Require(exception).Message,
                   exception,
                   new FileLocation()) {
        }

        public Status(string message, Exception exception)
            : this(null,
                   Severity.Error,
                   message,
                   exception,
                   new FileLocation()) {
        }

        public Status(RuntimeComponent component,
                      Severity level,
                      string message,
                      Exception exception,
                      FileLocation location) {

            InitializeCore(component,
                           message,
                           exception,
                           level,
                           location,
                           GuessErrorLevel(level));
        }

        public Status(RuntimeComponent component,
                      Severity level,
                      string message,
                      Exception exception,
                      FileLocation location,
                      int errorCode) {

            InitializeCore(component,
                           message,
                           exception,
                           level,
                           location,
                           errorCode);
        }


        public Status(IStatus copyFrom) {
            InitializeCore(_Require(copyFrom).Component,
                           copyFrom.Message,
                           copyFrom.Exception,
                           copyFrom.Level,
                           copyFrom.FileLocation,
                           copyFrom.ErrorCode);
        }

        // 'IStatus' implementation.

        [SelfDescribingPriority(PriorityLevel.Medium)]
        public string Message { get { return this.message; } }

        [SelfDescribingPriority(PriorityLevel.High)]
        public Exception Exception { get { return this.exception; } }

        [SelfDescribingPriority(PriorityLevel.Low)]
        public FileLocation FileLocation { get { return this.fileLocation; } }

        [SelfDescribingPriority(PriorityLevel.High)]
        public Severity Level { get { return this.level; } }

        [SelfDescribingPriority(PriorityLevel.High)]
        public int ErrorCode { get { return this.errorCode; } }

        [SelfDescribingPriority(PriorityLevel.High)]
        public RuntimeComponent Component { get { return this.component; } }

        // IEquatable<IStatus> implementation.

        bool IEquatable<IStatus>.Equals(IStatus other) { return Equals(other); }

        // 'Object' overrides.

        public override int GetHashCode() { return this.hashCodeCache; }

        public override bool Equals(object obj) {
            return StaticEquals(this, obj as IStatus);
        }

        public ReadOnlyCollection<IStatus> Children {
            get {
                return Empty<IStatus>.ReadOnly;
            }
        }

        static IStatus _Require(IStatus copyFrom)  {
            if (copyFrom == null) { throw new ArgumentNullException("copyFrom"); } // $NON-NLS-1
            return copyFrom;
        }

        static Exception _Require(Exception exception)  {
            if (exception == null) { throw new ArgumentNullException("exception"); } // $NON-NLS-1
            return exception;
        }

        private void InitializeCore(RuntimeComponent component, string message, Exception ex,
                                    Severity level, FileLocation location, int errorCode) {

            this.message = message;
            this.fileLocation = location;
            this.level = level;
            this.exception = ex;
            this.errorCode = errorCode;
            this.component = component;
            this.hashCodeCache = StaticGetHashCode(this);
        }

        // IFormattable implementation
        public string ToString(string format) {
            return ToString(format, CultureInfo.InvariantCulture);
        }

        public string ToString(string format, IFormatProvider formatProvider = null) {
            format = format ?? "G";
            if (format.Length > 1)
                throw new FormatException();

            switch (char.ToUpperInvariant(format[0])) {
                case 'D':
                    return this.Message;
                case 'C': // compact
                    return string.Format("{0} ({1})", this.Level, this.ErrorCode);
                case 'G': // general
                    return SelfDescribing.ToString(this, PriorityLevel.Medium);
                case 'F': // full
                    return SelfDescribing.ToString(this, PriorityLevel.None);
                default:
                    throw new FormatException();
            }
        }

        static int GuessErrorLevel(Severity level) {
            switch (level) {
                case Severity.Warning:
                case Severity.Error:
                    return 1;
                case Severity.None:
                    return -1;
                default:
                    return 0;
            }
        }

        // `IFileLocationContext' implementation
        void IFileLocationConsumer.SetFileLocation(int lineNumber, int linePosition) {
            this.fileLocation = new FileLocation(lineNumber, linePosition, this.fileLocation.FileName);
        }
    }
}
