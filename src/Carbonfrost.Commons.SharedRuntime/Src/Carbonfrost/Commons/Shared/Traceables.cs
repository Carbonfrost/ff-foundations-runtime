//
// - Traceables.cs -
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
using System.Diagnostics;

namespace Carbonfrost.Commons.Shared {

    static class Traceables {

        // TODO Move to localization

        [Conditional("DEBUG")]
        public static void TypeReferenceResolvingType(string fullTypeName) {
            LateBoundLog.Fail(string.Format("Type reference resolving type '{0}'.", fullTypeName)); // $NON-NLS-1
        }

        [Conditional("DEBUG")]
        public static void TypeReferenceResolveError(Exception ex) {
            LateBoundLog.Fail(string.Format("An exception occurred while resolving the type: {0}", ex)); // $NON-NLS-1
        }

        [Conditional("DEBUG")]
        public static void RootServiceProviderInitFailure(string rootType,
                                                          Exception ex) {
            LateBoundLog.Fail(string.Format("Failed to initialize custom root service provider type `{0}': {1}.", // $NON-NLS-1
                                            rootType, ex));
        }

        [Conditional("DEBUG")]
        public static void ProbingForAssemblies(Type type) {
            LateBoundLog.Fail(string.Format("Probing for assemblies ({0})", type)); // $NON-NLS-1
        }
    }
}
