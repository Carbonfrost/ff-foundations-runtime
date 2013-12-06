//
// - GacComponentStore.cs -
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
using Carbonfrost.Commons.ComponentModel.Annotations;

namespace Carbonfrost.Commons.Shared.Runtime.Components {

    // TODO Gac component store
    sealed class GacComponentStore : ComponentStore {

        static readonly string[] _componentTypes = new string[] {
            Carbonfrost.Commons.Shared.Runtime.Components.ComponentTypes.Assembly
        };

        public override IDictionary<QualifiedName, string> GetComponentMetadata(string componentType, ComponentName componentName) {
            throw new NotImplementedException();
        }

        public override ICollection<string> ComponentTypes {
            get { return _componentTypes; } }

        public override Component FindComponentByUrl(string componentType, Uri source)
        {
            throw new NotImplementedException();
        }

        public override Component FindComponentBySource(string componentType, Uri source)
        {
            throw new NotImplementedException();
        }

        public override Component FindComponentByName(string componentType, ComponentName componentName)
        {
            throw new NotImplementedException();
        }
    }
}
