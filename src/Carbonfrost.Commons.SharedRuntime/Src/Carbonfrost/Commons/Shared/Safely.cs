//
// - Safely.cs -
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
using System.IO;

namespace Carbonfrost.Commons.Shared {

    public static class Safely {

        public static void Close(Stream stream) {
            if (stream == null)
                return;

            try {
                stream.Close();

            } catch (Exception ex) {
                if (Require.IsCriticalException(ex))
                    throw;
            }
        }

        public static void Dispose(object instance) {
            var e = instance as System.Collections.IEnumerable;
            if (e != null) {
                foreach (var item in e)
                    Dispose(item);
            }

            var d = instance as IDisposable;
            if (d == null)
                return;

            try {
                d.Dispose();

            } catch (Exception ex) {
                if (Require.IsCriticalException(ex))
                    throw;
            }
        }

    }

}
