//
// - FilteredStatusAppender.cs -
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
using System.Collections.Specialized;

using Carbonfrost.Commons.Shared.Runtime.Components;

namespace Carbonfrost.Commons.Shared.Runtime {

    [Serializable]
    internal sealed class FilteredStatusAppender : StatusAppenderDecorator {

        private readonly IStatusAppender baseAppender;
        private readonly BitVector32 flags;

        private const int APPEND_WARNINGS = 0;
        private const int APPEND_ERRORS = 1;
        private const int APPEND_INFOS = 2;

        public FilteredStatusAppender(IStatusAppender baseAppender, bool appendErrors, bool appendWarnings, bool appendInfos) : base(baseAppender) {
            flags = new BitVector32();
            flags[APPEND_WARNINGS] = appendWarnings;
            flags[APPEND_ERRORS] = appendErrors;
            flags[APPEND_INFOS] = appendInfos;
            this.baseAppender = baseAppender;
        }

        public override bool Append(IStatus status) {
            if (status == null)
                throw new ArgumentNullException("status");  // $NON-NLS-1

            switch (status.Level) {
                case Severity.Error:
                    if (this.flags[APPEND_ERRORS]) {
                        return this.baseAppender.Append(status);
                    }
                    break;

                case Severity.Warning:
                    if (this.flags[APPEND_WARNINGS]) {
                        return this.baseAppender.Append(status);
                    }
                    break;

                case Severity.Information:
                    if (this.flags[APPEND_INFOS]) {
                        return this.baseAppender.Append(status);
                    }
                    break;

                case Severity.None:
                default:
                    return this.baseAppender.Append(status);
            }

            return false;
        }

    }
}
