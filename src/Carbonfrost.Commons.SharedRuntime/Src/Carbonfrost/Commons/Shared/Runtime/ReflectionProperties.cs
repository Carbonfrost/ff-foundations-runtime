//
// - ReflectionProperties.cs -
//
// Copyright 2005, 2006, 2010, 2013 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Shared.Runtime {

    internal sealed class ReflectionProperties : ReflectionPropertyProvider, IProperties {

        private PropertyChangedEventHandler PropertyChangedValue;

        public ReflectionProperties(object component) : base(component) {
        }

        void OnPropertyChanged(PropertyChangedEventArgs e) {
            if (PropertyChangedValue != null)
                PropertyChangedValue(this, e);
        }

        public override string ToString() {
            return Properties.ToKeyValuePairs(this);
        }

        // IProperties implementation
        public event PropertyChangedEventHandler PropertyChanged {
            add {
                EnsureEventHandler();
                PropertyChangedValue += value;
            }
            remove {
                EnsureEventHandler();
                PropertyChangedValue -= value;
            }
        }

        public void ClearProperties() {
            foreach (PropertyDescriptor pd in _EnsureProperties())
                _ResetProperty(pd);
        }

        public void ClearProperty(string property) {
            PropertyDescriptor pd = _GetProperty(property);
            if (pd != null)
                _ResetProperty(pd);
        }

        public bool IsReadOnly(string property) {
            PropertyDescriptor descriptor = _GetProperty(property);
            return descriptor != null && descriptor.IsReadOnly;
        }

        public void SetProperty(string property, object value) {
            PropertyDescriptor pd = _GetProperty(property);
            if (pd != null && pd.GetValue(objectContext) != value) {
                pd.SetValue(objectContext, value);
            }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
            foreach (PropertyDescriptor pd in _EnsureProperties())
                yield return new KeyValuePair<string, object>(pd.Name, pd.GetValue(objectContext));
        }

        // object overrides
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        private PropertyDescriptor _GetProperty(string property) {
            return _EnsureProperties().Find(property, false);
        }

        private void _ResetProperty(PropertyDescriptor pd) {
            if (pd.CanResetValue(this.objectContext)) {
                pd.ResetValue(this.objectContext);
                OnPropertyChanged(new PropertyChangedEventArgs(pd.Name));
            }
        }

        private void EnsureEventHandler() {
            INotifyPropertyChanged change = this.objectContext as INotifyPropertyChanged;
            if (change != null) {
                change.PropertyChanged += (sender, e) => { OnPropertyChanged(e); };
                return;
            }

            var events = TypeDescriptor.GetEvents(this.objectContext);
            var properties = _EnsureProperties();
            foreach (EventDescriptor ed in events) {
                if (ed.EventType == typeof(EventHandler) && ed.Name.EndsWith("Changed")) {
                    string property = ed.Name.Substring(0, ed.Name.Length - "Changed".Length);

                    if (property.Length > 0 && properties.Find(property, false) != null) {
                        EventHandler handler = (sender, e) => {
                            OnPropertyChanged(new PropertyChangedEventArgs(property));
                        };

                        ed.AddEventHandler(this.objectContext, handler);
                    }
                }
            }

        }

    }
}
