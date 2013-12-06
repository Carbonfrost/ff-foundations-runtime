//
// - PrincipalProvider.cs -
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
using System.Security.Principal;

namespace Carbonfrost.Commons.Shared {

    public static class PrincipalProvider {

        private static readonly IPrincipalProvider _null = new NullPrincipalProvider();
        private static readonly IPrincipalProvider _windows = new WindowsPrincipalProvider();
        private static readonly IPrincipalProvider _generic = new GenericPrincipalProvider();

        public static IPrincipalProvider Null { get { return _null; } }
        public static IPrincipalProvider Windows { get { return _windows; } }
        public static IPrincipalProvider Generic { get { return _generic; } }

        class NullPrincipalProvider : IPrincipalProvider {
            public IPrincipal SelectPrincipal(string name) {
                return null;
            }
        }

        class GenericPrincipalProvider : IPrincipalProvider {

            public IPrincipal SelectPrincipal(string name) {
                return new GenericPrincipal(new GenericIdentity(name), null);
            }
        }

        class WindowsPrincipalProvider : IPrincipalProvider {

            public IPrincipal SelectPrincipal(string name) {
                return new WindowsPrincipal(new WindowsIdentity(name));
            }
        }
    }
}
