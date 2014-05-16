//
// - PredicateStatusAppender.cs -
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

namespace Carbonfrost.Commons.Shared.Runtime {

    internal sealed class PredicateStatusAppender : StatusAppenderDecorator {

        private readonly Func<IStatus, bool> predicate;

        public PredicateStatusAppender(IStatusAppender baseAppender, Func<IStatus, bool> predicate)
            : base(baseAppender) {
            this.predicate = predicate;
        }

        public override bool Append(IStatus status)
        {
            if (status == null)
                throw new ArgumentNullException("status");

            if (predicate(status))
                return base.Append(status);
            else
                return false;
        }

    }
}
