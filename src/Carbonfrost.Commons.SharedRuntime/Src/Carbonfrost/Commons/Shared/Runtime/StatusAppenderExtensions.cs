//
// - StatusAppenderExtensions.cs -
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
using Carbonfrost.Commons.Shared.Runtime.Components;
using Component = Carbonfrost.Commons.Shared.Runtime.Components.Component;

namespace Carbonfrost.Commons.Shared.Runtime {

    public static partial class StatusAppenderExtensions {

        public static IStatus AppendWarning(this IStatusAppender source, Exception exception) {
            return AppendCore(source, new Status(Severity.Warning, exception));
        }

        public static IStatus AppendWarning(this IStatusAppender source, string message) {
            return AppendCore(source, new Status(Severity.Warning, message));
        }

        public static IStatus AppendWarning(this IStatusAppender source, string message, Exception exception) {
            return AppendCore(source, new Status(null, Severity.Warning, message, exception, FileLocation.Empty));
        }

        public static IStatus AppendWarning(this IStatusAppender source, Component component, Exception exception, FileLocation location) {
            return AppendCore(source, new Status(component, Severity.Warning, exception, location));
        }

        public static IStatus AppendWarning(this IStatusAppender source, Component component, string message, FileLocation location) {
            return AppendCore(source, new Status(component, Severity.Warning, message, location));
        }

        public static IStatus AppendWarning(this IStatusAppender source, Component component, string message, Exception exception, FileLocation location) {
            return AppendCore(source, new Status(exception));
        }

        public static IStatus AppendWarning(this IStatusAppender source, Component component, string message, Exception exception, FileLocation location, int errorCode) {
            return AppendCore(source, new Status(component, Severity.Warning, message, exception, location, errorCode));
        }

        public static IStatus AppendError(this IStatusAppender source, Exception exception) {
            return AppendCore(source, new Status(exception));
        }

        public static IStatus AppendError(this IStatusAppender source, string message, Exception exception) {
            return AppendCore(source, new Status(null, Severity.Error, message, exception, FileLocation.Empty, 0));
        }

        public static IStatus AppendError(this IStatusAppender source, Component component, Exception exception, FileLocation location) {
            return AppendCore(source, new Status(component, Severity.Error, exception, location));
        }

        public static IStatus AppendError(this IStatusAppender source, Component component, string message, FileLocation location) {
            return AppendCore(source, new Status(component, Severity.Error, message, location));
        }

        public static IStatus AppendError(this IStatusAppender source, Component component, string message, Exception exception, FileLocation location) {
            return AppendCore(source, new Status(exception));
        }

        public static IStatus AppendError(this IStatusAppender source, Component component, string message, Exception exception, FileLocation location, int errorCode) {
            return AppendCore(source, new Status(component, Severity.Error, message, exception, location, errorCode));
        }

        public static IStatus Append(this IStatusAppender source, Exception exception) {
            return AppendCore(source, new Status(exception));
        }

        public static IStatus Append(this IStatusAppender source, Severity level, string message) {
            return AppendCore(source, new Status(level, message));
        }

        public static IStatus Append(this IStatusAppender source, string message, Exception exception) {
            return AppendCore(source, new Status(message, exception));
        }

        public static IStatus Append(this IStatusAppender source, Component component, Exception exception, FileLocation location) {
            return AppendCore(source, new Status(component, exception, location));
        }

        public static IStatus Append(this IStatusAppender source, Component component, Severity level, string message, FileLocation location) {
            return AppendCore(source, new Status(component, level, message, location));
        }

        public static IStatus Append(this IStatusAppender source, Component component, string message, Exception exception, FileLocation location) {
            return AppendCore(source, new Status(component, message, exception, location));
        }

        public static IStatus Append(this IStatusAppender source, Component component, string message, Exception exception, FileLocation location, int errorCode) {
            return AppendCore(source, new Status(component, message, exception, location, errorCode));
        }

        public static IStatus Append(this IStatusAppender source, Component component, string message, Exception exception, Severity level, FileLocation location) {
            return AppendCore(source, new Status(component, level, message, exception, location));
        }

        public static IStatus Append(this IStatusAppender source, Component component, string message, Exception exception, Severity level, FileLocation location, int errorCode) {
            return AppendCore(source, new Status(component, level, message, exception, location, errorCode));
        }

        static IStatus AppendCore(IStatusAppender source, IStatus status) {
            if (source == null)
                throw new ArgumentNullException("source"); // $NON-NLS-1

            source.Append(status);
            return status;
        }
    }
}
