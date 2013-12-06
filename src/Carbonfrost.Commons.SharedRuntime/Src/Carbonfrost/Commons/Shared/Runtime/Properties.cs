//
// - Properties.cs -
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
using System.ComponentModel;
using System.IO;
using System.Text;

namespace Carbonfrost.Commons.Shared.Runtime {

    [Serializable]
    [TypeConverter(typeof(PropertiesConverter))]
    [StreamingSource(typeof(PropertiesStreamingSource))]
    public partial class Properties : PropertyProvider, IProperties, IMakeReadOnly {

        private IDictionary<string, object> properties = new Dictionary<string, object>();
        private bool suppressEvents;
        private bool isThisReadOnly;

        internal IDictionary<string, object> InnerMap { get { return this.properties; } }

        public bool RaiseEvents {
            get { return !this.suppressEvents; }
            set { this.suppressEvents = !value; }
        }

        // Constructors.
        public Properties() {
        }

        public Properties(IEnumerable<KeyValuePair<string, object>> items) {
            if (items != null) {
                foreach (var kvp in items) {
                    this.SetProperty(kvp.Key, kvp.Value);
                }
            }
        }

        public void Load(string fileName) {
            if (fileName == null) { throw new ArgumentNullException("fileName"); } // $NON-NLS-1
            using (PropertiesReader pr = new PropertiesReader(
                StreamContext.FromFile(fileName))) {
                pr.AllowUriDereferences = true;
                LoadCore(pr);
            }
        }

        public void Load(StreamContext streamContext, Encoding encoding) {
            if (streamContext == null)
                throw new ArgumentNullException("streamContext");  // $NON-NLS-1

            using (PropertiesReader pr = new PropertiesReader(streamContext, encoding)) {
                LoadCore(pr);
            }
        }

        public void Load(Stream stream, Encoding encoding = null) {
            if (stream == null)
                throw new ArgumentNullException("stream");  // $NON-NLS-1

            using (StreamReader sr = Utility.MakeStreamReader(stream, encoding)) {
                using (PropertiesReader pr = new PropertiesReader(sr)) {
                    LoadCore(pr);
                }
            }
        }

        public void Save(string fileName) {
            Save(fileName, false, Encoding.UTF8);
        }

        public void Save(Stream stream, Encoding encoding) {
            using (StreamWriter writer = new StreamWriter(stream, encoding)) {
                using (PropertiesWriter pw = new PropertiesWriter(writer)) {
                    SaveCore(pw);
                }
            }
        }

        public void Save(string fileName, bool append, Encoding encoding) {
            using (StreamWriter writer = new StreamWriter(fileName, append, encoding)) {
                using (PropertiesWriter pw = new PropertiesWriter(writer)) {
                    SaveCore(pw);
                }
            }
        }

        protected virtual bool IsReadOnlyCore(string property) { return false; }

        protected virtual void MakeReadOnlyCore() {}

        protected virtual void SetPropertyCore(string property, object value) {
            properties[property] = value;
        }

        protected void ThrowIfReadOnly() {
            if (this.isThisReadOnly)
                throw Failure.ReadOnlyCollection();
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) {
            if (!suppressEvents)
                if (PropertyChanged != null) { PropertyChanged(this, e); }
        }

        // IPropertyStore implementation.
        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
            return this.properties.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        // 'IProperties' implementation.
        public void ClearProperties() {
            ThrowIfReadOnly();
            this.properties.Clear();
        }

        public void ClearProperty(string property) {
            Require.NotNullOrEmptyString("property", property); // $NON-NLS-1
            ThrowIfReadOnly();
            ClearPropertyCore(property);
            OnPropertyChanged(new PropertyChangedEventArgs(property));
        }

        public bool IsReadOnly(string property) {
            Require.NotNullOrEmptyString("property", property); // $NON-NLS-1
            return this.isThisReadOnly || this.IsReadOnlyCore(property);
        }

        public void SetProperty(string property, object value) {
            Require.NotNullOrEmptyString("property", property); // $NON-NLS-1
            ThrowIfReadOnly();

            object currentValue;
            bool hasCurrentValue = TryGetPropertyCore(property, typeof(object), out currentValue);

            if (!hasCurrentValue || !object.Equals(value, currentValue)) {
                SetPropertyCore(property, value);
                OnPropertyChanged(new PropertyChangedEventArgs(property));
            }
        }

        // 'IMakeReadOnly' implementation.
        bool IMakeReadOnly.IsReadOnly { get { return isThisReadOnly; } }

        public void MakeReadOnly() {
            if (!this.isThisReadOnly) {
                MakeReadOnlyCore();
                this.isThisReadOnly = true;
            }
        }

        protected virtual void ClearPropertyCore(string property) {
            this.properties.Remove(property);
        }

        public override Type GetPropertyType(string property) {
            Require.NotNullOrEmptyString("property", property); // $NON-NLS-1

            // Must reimplement this to take advantage of parenting and to
            // check chaining modes
            object objValue;
            if (this.properties.TryGetValue(property, out objValue)) {
                if (objValue == null)
                    return typeof(object);
                else
                    return objValue.GetType();
            }

            return null;
        }

        protected override bool TryGetPropertyCore(string property, Type requiredType, out object value) {
            // Review the local storage to determine if it is contained
            requiredType = requiredType ?? typeof(object);
            if (properties.TryGetValue(property, out value)) {
                if (requiredType.IsInstanceOfType(value))
                    return true;

                // Type coercion
				var str = value as string;
                if (str != null) {
                    TypeConverter c = TypeDescriptor.GetConverter(requiredType);
                    if (c != null && c.CanConvertFrom(typeof(string))) {
                        value = c.ConvertFromInvariantString(str);
                        return true;
                    }
                }

                // Try adapters
                value = Adaptable.TryAdapt(value, requiredType);
                if (value != null)
                    return true;
            }

            value = null;
            return false;
        }

        private void LoadCore(PropertiesReader reader) {
            try {
                RaiseEvents = false;
                while (reader.MoveToProperty()) {
                    this.SetProperty(reader.QualifiedKey, reader.Value);
                }
            } finally {
                RaiseEvents = true;
            }
        }

        private void SaveCore(PropertiesWriter writer) {
            foreach (KeyValuePair<string, object> prop in this.properties) {
                writer.WriteProperty(prop.Key, prop.Value);
            }
        }

    }
}
