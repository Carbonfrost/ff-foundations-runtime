//
// - PropertiesImpl.cs -
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Carbonfrost.Commons.Shared.Runtime {

    internal abstract class PropertiesImpl : PropertyProvider, IProperties {

        public event PropertyChangedEventHandler PropertyChanged;

        protected PropertiesImpl() : this(null) {}

        protected PropertiesImpl(object model) {
            Watch(model);
        }

        public void Watch(object model) {
            if (model == null)
                return;

            INotifyPropertyChanged n = model as INotifyPropertyChanged;
            if (n != null)
                n.PropertyChanged += model_PropertyChanged;

            IComponent c = model as IComponent;
            if (c != null)
                c.Disposed += model_Disposed;
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        public abstract void ClearProperties();

        public void ClearProperty(string key) {
            SetProperty(key, null);
        }

        protected abstract void SetPropertyCore(string key, object defaultValue, bool isImplicit);

        public virtual bool IsReadOnly(string key) {
            return false;
        }

        public void SetProperty(string key, object value) {
            Require.NotNullOrEmptyString("key", key); // $NON-NLS-1

            object currentValue;
            bool hasCurrentValue = TryGetPropertyCore(key, typeof(object), out currentValue);

            if (!hasCurrentValue || !object.Equals(value, currentValue)) {
                SetPropertyCore(key, value, false);
                OnPropertyChanged(new PropertyChangedEventArgs(key));
            }
        }

        public abstract IEnumerator<KeyValuePair<string, object>> GetEnumerator();

        public override string ToString() {
            return Properties.ToKeyValuePairs(this);
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        void model_Disposed(object sender, EventArgs e) {
            ((IComponent) sender).Disposed -= model_Disposed;
            ((INotifyPropertyChanged) sender).PropertyChanged -= model_PropertyChanged;
        }

        void model_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            OnPropertyChanged(e);
        }
    }
}
