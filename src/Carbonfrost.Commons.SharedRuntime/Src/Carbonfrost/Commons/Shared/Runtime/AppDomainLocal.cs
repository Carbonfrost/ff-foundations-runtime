//
// - AppDomainLocal.cs -
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

namespace Carbonfrost.Commons.Shared.Runtime {

    sealed class AppDomainLocal<T> {

        private readonly string key = Guid.NewGuid().ToString();

        private readonly Func<T> factory;

        public AppDomainLocal(Func<T> factory) {
            this.factory = factory;
        }

        public T GetValue(AppDomain appDomain) {
            object result;

            if (appDomain == AppDomain.CurrentDomain) {
                UpdateValue();
            } else {
                appDomain.DoCallBack(UpdateValue);
            }

            result = GetValueCore(appDomain);
            Exception replayException = result as Exception;
            if (replayException != null)
                throw replayException;

            return (T) result;
        }

        public T Value {
            get {
                return GetValue(AppDomain.CurrentDomain);
            }
        }

        private object GetValueCore(AppDomain appDomain) {
            return appDomain.GetData(this.key);
        }

        private void UpdateValue() {
            AppDomain appDomain = AppDomain.CurrentDomain;
            object value = GetValueCore(appDomain);

            if (object.ReferenceEquals(null, value)) {
                try {
                    appDomain.SetData(this.key, factory());

                } catch (Exception ex) {
                    appDomain.SetData(this.key, ex);
                }
            }
        }
    }

    static class AppDomainLocal {

        public static AppDomainLocal<T> Create<T>()
            where T : new()
        {
            return new AppDomainLocal<T>(() => new T());
        }
    }
}
