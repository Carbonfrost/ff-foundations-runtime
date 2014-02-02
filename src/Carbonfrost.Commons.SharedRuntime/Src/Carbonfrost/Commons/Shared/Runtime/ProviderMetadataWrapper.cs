//
// - ProviderMetadataWrapper.cs -
//
// Copyright 2013 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Linq;
using System.Reflection;
using Carbonfrost.Commons.Shared.Runtime.Components;

namespace Carbonfrost.Commons.Shared.Runtime {

    sealed class ProviderMetadataWrapper : IProviderMetadata {

        readonly object value;

        public ProviderMetadataWrapper(object value) {
            this.value = value;
        }

        public ProviderValueSource Source { get; set; }

        public int MatchCriteria(object criteria) {
            return MatchMemberCriteria(this.Source, criteria)
                + ProviderMetadataWrapper.MemberwiseEquals(value, criteria);
        }

        internal static int MatchMemberCriteria(ProviderValueSource source, object criteria) {
            if (criteria == null)
                return 0;

            var cpp = Properties.FromValue(criteria);

            int result = 0;

            // Match member and assembly
            var asm = cpp.GetProperty("Assembly", (Assembly) null);
            var mem = cpp.GetProperty("Member", (MemberInfo) null);

            if (asm != null) {

                result += asm.GetName().FullName
                    == source.Assembly.GetName().FullName ? 1 : 0;
            }
            if (mem != null) {
                result += mem == source.Member ? 1 : 0;
            }

            return result;
        }

        internal static int MemberwiseEquals(object criteria, object other) {
            var pp = (other == null ? PropertyProvider.Null : PropertyProvider.FromValue(other));

            return Properties.FromValue(criteria).Count(
                m =>
                object.Equals(pp.GetProperty(m.Key), m.Value));
        }

        public static IProviderMetadata Create(object metadata) {
            var pm = metadata as IProviderMetadata;
            if (pm == null)
                return new ProviderMetadataWrapper(metadata ?? DBNull.Value);
            else
                return pm;
        }

        object IProviderMetadata.Value {
            get { return this.value; } }

        sealed class NullProviderMetadata : IProviderMetadata {

            object IProviderMetadata.Value {
                get { return null; } }

            ProviderValueSource IProviderMetadata.Source {
                get { return null; }
                set {} }

            public int MatchCriteria(object criteria) {
                return 0;
            }
        }
    }
}
