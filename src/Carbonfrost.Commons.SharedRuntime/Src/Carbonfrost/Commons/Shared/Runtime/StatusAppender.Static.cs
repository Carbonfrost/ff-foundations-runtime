//
// - StatusAppender.Static.cs -
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

namespace Carbonfrost.Commons.Shared.Runtime {

    partial class StatusAppender {

        static IDictionary<Type, IStatusAppender> map;

        static IStatusAppender InvokeFactory(Type type) {
            return new StatusAppender();
        }

        public static IStatusAppender Synchronized(IStatusAppender statusAppender) {
            if (statusAppender == null)
                throw new ArgumentNullException("statusAppender");

            if (object.ReferenceEquals(Null, statusAppender))
                return Null;

            SynchronizedStatusAppender ssa = statusAppender as SynchronizedStatusAppender;
            if (ssa == null)
                return new SynchronizedStatusAppender(statusAppender);
            else
                return ssa;
        }

        public static IStatusAppender InformationOnly(IStatusAppender baseAppender) {
            return Filtered(baseAppender, new Severity[] { Severity.Information } );
        }

        public static IStatusAppender ErrorsAndWarningsOnly(IStatusAppender baseAppender) {
            return Filtered(baseAppender, new Severity[] { Severity.Warning, Severity.Error} );
        }

        public static IStatusAppender ErrorsOnly(IStatusAppender baseAppender) {
            return Filtered(baseAppender, new Severity[] { Severity.Error} );
        }

        public static IStatusAppender Filtered(IStatusAppender baseAppender, Severity[] severities) {
            if (severities == null) { throw new ArgumentNullException("severities"); } // $NON-NLS-1

            if (baseAppender == null || object.ReferenceEquals(baseAppender, StatusAppender.Null)) {
                return StatusAppender.Null;
            } else if (severities.Length == 0) {
                return StatusAppender.Null;
            } else {
                // 2^0 = 1 (None)
                // 2^1 = 2 (Error)
                // 2^2 = 4 (Warning)
                // 2^3 = 8 (Information)

                int result = 0;

                for (int i = 0; i < severities.Length; i++) {
                    result &= (int) (Math.Pow(2, (int) severities[i]));
                    if (result >= 2 + 4 + 8) { // all have been set or more
                        return baseAppender;
                    }
                }

                bool appendInfos = (result & 8) > 0;
                bool appendWarnings = (result & 4) > 0;
                bool appendErrors = (result & 2) > 0;

                return new FilteredStatusAppender(baseAppender, appendErrors, appendWarnings, appendInfos);
            }
        }

        public static IStatusAppender ForType(Type type) {
            if (type == null)
                throw new ArgumentNullException("type");

            IStatusAppender result;
            if (map != null && map.TryGetValue(type, out result))
                return result;

            IStatusAppender sa = InvokeFactory(type);
            if (sa == StatusAppender.Null || sa == null)
                return StatusAppender.Null;

            if (map == null) {
                map = new Dictionary<Type, IStatusAppender>();
            }

            return map.GetValueOrCache(type, sa);
        }
    }
}
