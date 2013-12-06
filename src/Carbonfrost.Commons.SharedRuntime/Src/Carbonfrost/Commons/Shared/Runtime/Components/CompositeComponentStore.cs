//
// - CompositeComponentStore.cs -
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
using System.Linq;

using Carbonfrost.Commons.ComponentModel.Annotations;

namespace Carbonfrost.Commons.Shared.Runtime.Components {

    sealed class CompositeComponentStore : ComponentStore {

        readonly ComponentStore[] stores;

        public CompositeComponentStore(IEnumerable<ComponentStore> stores) {
            this.stores = this.stores.ToArray();
        }

        public override ICollection<string> ComponentTypes {
            get {
                return this.stores.SelectMany(s => s.ComponentTypes).Distinct().ToArray();
            }
        }

        public override Component FindComponentByName(string componentType, ComponentName componentName) {
            throw new NotImplementedException();
        }

        public override Component FindComponentBySource(string componentType, Uri source) {
            throw new NotImplementedException();
        }

        public override Component FindComponentByUrl(string componentType, Uri source) {
            throw new NotImplementedException();
        }

        public override IDictionary<QualifiedName, string> GetComponentMetadata(string componentType, ComponentName componentName) {
            throw new NotImplementedException();
        }
    }
}
