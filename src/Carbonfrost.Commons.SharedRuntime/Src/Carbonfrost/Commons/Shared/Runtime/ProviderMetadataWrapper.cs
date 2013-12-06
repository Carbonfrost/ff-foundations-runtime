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

namespace Carbonfrost.Commons.Shared.Runtime {

    sealed class ProviderMetadataWrapper : IProviderMetadata {

        readonly object value;

        static readonly IProviderMetadata Null = new NullProviderMetadata();

        public ProviderMetadataWrapper(object value) {
            this.value = value;
        }

        public int MatchCriteria(object criteria) {
            return ProviderMetadataWrapper.MemberwiseEquals(value, criteria);
        }

        internal static int MemberwiseEquals(object criteria, object other) {
            var pp = (other == null ? PropertyProvider.Null : PropertyProvider.FromValue(other));

            return Properties.FromValue(criteria).Count(
                m =>
                object.Equals(pp.GetProperty(m.Key), m.Value));
        }

        public static IProviderMetadata Create(object metadata) {
            if (metadata == null)
                return Null;

            var pm = metadata as IProviderMetadata;
            if (pm == null)
                return new ProviderMetadataWrapper(metadata);
            else
                return pm;
        }

        sealed class NullProviderMetadata : IProviderMetadata {

            public int MatchCriteria(object criteria) {
                return 0;
            }
        }
    }
}
