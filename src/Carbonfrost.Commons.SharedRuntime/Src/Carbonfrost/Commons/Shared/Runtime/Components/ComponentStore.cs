//
// - ComponentStore.cs -
//
// Copyright 2005, 2006, 2010, 2012 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    [Providers]
    public abstract class ComponentStore {

        public static readonly ComponentStore Gac = new GacComponentStore();
        public static readonly ComponentStore Null = new NullComponentStore();

        public static ComponentStore FromName(string name) {
            return AppDomain.CurrentDomain.GetProvider<ComponentStore>(name);
        }

        public static ComponentStore Compose(params ComponentStore[] items) {
            return Utility.OptimalComposite(items,
                                            i => new CompositeComponentStore(i),
                                            Null);
        }

        public static ComponentStore Compose(IEnumerable<ComponentStore> items) {
            return Utility.OptimalComposite(items,
                                            i => new CompositeComponentStore(i),
                                            Null);
        }

        public abstract ICollection<string> ComponentTypes { get; }

        public abstract Component FindComponentByName(string componentType, ComponentName componentName);
        public abstract Component FindComponentBySource(string componentType, Uri source);
        public abstract Component FindComponentByUrl(string componentType, Uri url);
        public abstract Component FindComponent(string componentType, object criteria);

        // TODO Other metadata properties

        public Uri GetComponentBase(string componentType, ComponentName componentName) {
            return GetMetadataUri(componentType, componentName, ShareableCode.Base);
        }

        public Uri GetComponentUrl(string componentType, ComponentName componentName) {
            return GetMetadataUri(componentType, componentName, ShareableCode.Url);
        }

        public abstract IPropertyStore GetComponentMetadata(string componentType, ComponentName componentName);

        public virtual IEnumerable<Component> GetDependencies(string componentType, ComponentName componentName) {
            if (componentType == null)
                throw new ArgumentNullException("componentType");
            if (componentType.Length == 0)
                throw Failure.EmptyString("componentType");
            if (componentName == null)
                throw new ArgumentNullException("componentName");

            var c = this.FindComponentByName(componentType, componentName);
            if (c == null || c.Source == null)
                return null;

            var irc = RuntimeComponent.ReflectionOnlyLoad(componentType, c.Source);
            if (irc != null)
                return irc.Dependencies;
            else
                return Empty<Component>.Array;
        }

        private string GetMetadataString(string componentType,
                                         ComponentName componentName,
                                         QualifiedName property) {

            return GetComponentMetadata(componentType, componentName)
                .GetString(property.ToString());
        }

        private Uri GetMetadataUri(string componentType,
                                   ComponentName componentName,
                                   QualifiedName url) {

            string s = GetMetadataString(componentType,
                                         componentName,
                                         url);
            if (s == null)
                return null;
            else
                return new Uri(s, UriKind.RelativeOrAbsolute);
        }
    }

}
