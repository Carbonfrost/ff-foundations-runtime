//
// - PropertiesStreamingSource.cs -
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
using System.IO;
using System.Linq;

using Carbonfrost.Commons.ComponentModel;

namespace Carbonfrost.Commons.Shared.Runtime {

    sealed class PropertiesStreamingSource : TextSource {

        public override object Load(TextReader reader, Type instanceType) {
            if (reader == null)
                throw new ArgumentNullException("reader");

            using (PropertiesReader pr = new PropertiesReader(reader)) {
                var items = pr.ReadToEnd().Select(t => new KeyValuePair<string, object>(t.Key, t.Value));

                if (typeof(IProperties).Equals(instanceType)
                    || typeof(IPropertyProviderExtension).Equals(instanceType)
                    || typeof(Properties).Equals(instanceType)
                    || typeof(IPropertyProvider).Equals(instanceType)) {
                    return new Properties(items);
                }

                return Activation.CreateInstance(instanceType,
                                                 items,
                                                 PopulateComponentCallback.Default,
                                                 ServiceProvider.Null);
            }
        }

        public override void Save(TextWriter writer, object value) {
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (value == null)
                throw new ArgumentNullException("value");

            IPropertyStore pp = Adaptable.TryAdapt<IPropertyStore>(value);
            if (pp == null) {
                pp = Properties.FromValue(value);
            }

            if (pp == null)
                throw Failure.NotAdaptableTo("value", value, typeof(IPropertyStore));

            using (PropertiesWriter pw = new PropertiesWriter(writer)) {
                pw.WriteProperties(pp);
            }
        }

    }
}

