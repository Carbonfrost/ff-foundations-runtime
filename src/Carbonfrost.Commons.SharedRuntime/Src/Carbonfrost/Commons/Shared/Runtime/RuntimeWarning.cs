//
// - RuntimeWarning.cs -
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
using System.Reflection;
using Carbonfrost.Commons.Shared.Runtime;
using Carbonfrost.Commons.Shared.Runtime.Components;
using Carbonfrost.Commons.SharedRuntime.Resources;

namespace Carbonfrost.Commons.Shared.Runtime {

    static class RuntimeWarning {

        public static void NotProviderType(this IStatusAppender s,
                                           Type providerType,
                                           string description) {
            // TODO Actually expose this warning
            // Provider definition was skipped because the root provider type is not defined
        }

        public static void ServiceFailedToStart(IStatusAppender s,
                                                Type serviceType,
                                                Exception ex) {
            s.AppendError(SR.ServiceFailedToStart(serviceType), ex);
        }

        public static void AssemblyNotInContext(IStatusAppender s, Component componentName, AssemblyName name) {
            Status warn = new Status(
                componentName, Severity.Warning, SR.AssemblyNotInContext(name), FileLocation.Empty);
            s.Append(warn);
        }

        public static void LateBoundTypeFailure(IStatusAppender s, string originalString, Exception ex) {
            Component component = typeof(RuntimeWarning).Assembly.AsComponent();
            Status status = new Status(
                component, SR.LateBoundTypeFailure(originalString), ex, FileLocation.Empty);
            s.Append(status);
        }

        public static void AssemblyInfoFilterFailed(IStatusAppender sa, AssemblyName name, Type attrType, Exception error) {
            sa.AppendError(SR.AssemblyInfoFilterFailed(attrType, name), error);
        }

        public static void InvalidProviderDeclared(string fullName, Exception ex) {
            var sa = StatusAppender.ForType(typeof(Adaptable));
            sa.AppendError(SR.InvalidProviderDeclared(fullName, ex.Message), ex);
        }

    }
}
