//
// - InterfaceUsageMap.cs -
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
using Carbonfrost.Commons.Shared;
using Carbonfrost.Commons.Shared.Runtime;

namespace Carbonfrost.Commons.ComponentModel {

    public class InterfaceUsageMap<T> : IDictionary<InterfaceUsage, T> {
        
        private static readonly InterfaceUsage[] INTERFACE_USAGES
            = (InterfaceUsage[]) Enum.GetValues(typeof(InterfaceUsage));
        private static readonly int INTERFACE_USAGE_COUNT = INTERFACE_USAGES.Length;
        
        private int version;
        private readonly T[] myValues;

        [SelfDescribingPriority(PriorityLevel.None)]
        public T All {
            get {
                for (int i = 0; i < INTERFACE_USAGE_COUNT; i++) {
                    if (!object.Equals(myValues[0], myValues[i]))
                        return default(T);
                }

                return myValues[0];
            }
            set {
                for (int i = 0; i < INTERFACE_USAGE_COUNT; i++) {
                    myValues[i] = value;
                }
                version++;
            }
        }

        [SelfDescribingPriority(PriorityLevel.None)]
        public T this[InterfaceUsage usage] {
            get {
                return myValues[GetUsageIndex(usage)];
            }
            set {
                myValues[GetUsageIndex(usage)] = value;
                version++;
            }
        }

        [SelfDescribingPriority(PriorityLevel.Medium)]
        public T Missing {
            get { return this[InterfaceUsage.Missing]; }
            set {
                this[InterfaceUsage.Missing] = value;
            }
        }

        [SelfDescribingPriority(PriorityLevel.Medium)]
        public T Obsolete {
            get { return this[InterfaceUsage.Obsolete]; }
            set {
                this[InterfaceUsage.Obsolete] = value;
            }
        }

        [SelfDescribingPriority(PriorityLevel.Medium)]
        public T Preliminary {
            get { return this[InterfaceUsage.Preliminary]; }
            set {
                this[InterfaceUsage.Preliminary] = value;
            }
        }

        // Constructors.

        public InterfaceUsageMap() {
            this.myValues = new T[INTERFACE_USAGE_COUNT];
        }

        // `IDictionary' implementation.

        ICollection<InterfaceUsage> IDictionary<InterfaceUsage, T>.Keys {
            get { return INTERFACE_USAGES; }
        }

        ICollection<T> IDictionary<InterfaceUsage, T>.Values { get { return this.myValues; } }

        int ICollection<KeyValuePair<InterfaceUsage, T>>.Count { get { return INTERFACE_USAGE_COUNT; } }
        bool ICollection<KeyValuePair<InterfaceUsage, T>>.IsReadOnly { get { return false; } }

        bool IDictionary<InterfaceUsage, T>.ContainsKey(InterfaceUsage key) {
            return Enum.IsDefined(typeof(InterfaceUsage), key);
        }

        void IDictionary<InterfaceUsage, T>.Add(InterfaceUsage key, T value) {
            this[key] = value;
        }

        bool IDictionary<InterfaceUsage, T>.Remove(InterfaceUsage key) {
            this[key] = default(T);
            return true;
        }

        bool IDictionary<InterfaceUsage, T>.TryGetValue(InterfaceUsage key, out T value) {
            value = this[key];
            return true;
        }

        void ICollection<KeyValuePair<InterfaceUsage, T>>.Add(KeyValuePair<InterfaceUsage, T> item) {
            this[item.Key] = item.Value;
        }

        void ICollection<KeyValuePair<InterfaceUsage, T>>.Clear() { this.All = default(T); }

        bool ICollection<KeyValuePair<InterfaceUsage, T>>.Contains(KeyValuePair<InterfaceUsage, T> item) {
            return object.Equals(this[item.Key], item.Value);
        }

        void ICollection<KeyValuePair<InterfaceUsage, T>>.CopyTo(KeyValuePair<InterfaceUsage, T>[] array, int arrayIndex) {
            if (array == null)
                throw new ArgumentNullException("array"); // $NON-NLS-1
            if (arrayIndex < 0)
                throw Failure.Negative("arrayIndex", arrayIndex); // $NON-NLS-1
            if (array.Rank != 1)
                throw Failure.RankNotOne("array");
            if (array.Length - arrayIndex < INTERFACE_USAGE_COUNT)
                throw Failure.NotEnoughSpaceInArray("arrayIndex", arrayIndex); // $NON-NLS-1

            foreach (KeyValuePair<InterfaceUsage, T> kvp in this)
                array[arrayIndex++] = kvp;
        }

        bool ICollection<KeyValuePair<InterfaceUsage, T>>.Remove(KeyValuePair<InterfaceUsage, T> item) {
            if (object.Equals(this[item.Key], item.Value)) {
                this[item.Key] = default(T);
                return true;
            }

            return false;
        }

        public IEnumerator<KeyValuePair<InterfaceUsage, T>> GetEnumerator() {
            int startedVersion = this.version;
            for (int i = 0; i < INTERFACE_USAGE_COUNT; i++) {
                if (this.version != startedVersion)
                    throw Failure.CollectionModified();

                yield return new KeyValuePair<InterfaceUsage, T>((InterfaceUsage) i, myValues[i]);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        private int GetUsageIndex(InterfaceUsage usage) {
            int usageValue = (int) usage;
            if (usageValue < 0 || usageValue >= INTERFACE_USAGE_COUNT)
                throw Failure.NotDefinedEnum("usage", usage, typeof(InterfaceUsage)); // $NON-NLS-1

            return usageValue;
        }

    }
}

