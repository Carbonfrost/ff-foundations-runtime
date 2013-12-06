//
// - LazyDescriptionCollection.cs -
//
// Copyright 2012 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.Shared.Runtime {

    class LazyDescriptionCollection<TValue> : ICollection<TValue>, IAssemblyInfoFilter, IEnumerable<TValue>, IObservable<TValue> {

        private readonly List<ObserverDisposer> observers = new List<ObserverDisposer>();

        private readonly ICollection<TValue> inner;
        private readonly Func<Assembly, IEnumerable<TValue>> selector;
        private readonly object root = new object();

        protected ICollection<TValue> Inner {
            get { return inner; }
        }

        public LazyDescriptionCollection(Func<Assembly, IEnumerable<TValue>> func)
            : this(func, new List<TValue>()) {}

        public LazyDescriptionCollection(
            Func<Assembly, IEnumerable<TValue>> selector,
            ICollection<TValue> inner) {
            if (selector == null)
                throw new ArgumentNullException("selector");

            this.selector = selector;
            this.inner = inner;
            AssemblyInfo.AddStaticFilter(this);
        }

        public bool IsReadOnly {
            get {
                return false;
            }
        }

        public int Count {
            get {
                Complete();
                return Inner.Count;
            }
        }

        public void Add(TValue item) {
            AddValue(item);
        }

        public virtual bool Contains(TValue item) {
            return Continue(item);
        }

        public void CopyTo(TValue[] array, int arrayIndex) {
            Inner.CopyTo(array, arrayIndex);
        }

        public bool Remove(TValue item) {
            throw new NotSupportedException();
        }

        public void Clear() {
            throw new NotSupportedException();
        }

        protected void Complete() {
            AssemblyInfo.CompleteAppDomain();
        }

        protected bool Continue(TValue value) {
            do {
                if (this.Inner.Contains(value))
                    return true;

            } while (AssemblyInfo.ContinueAppDomain());

            return false;
        }

        // `IObservable<TValue>' implementation
        public IDisposable Subscribe(IObserver<TValue> observer) {
            if (observer == null)
                throw new ArgumentNullException("observer");

            var result = new DefaultObserverDisposer(this, observer);
            this.observers.Add(result);
            return result;
        }

        // IAssemblyInfoFilter implementation
        public void ApplyToAssembly(AssemblyInfo info) {
            var items = this.selector(info.Assembly);
            if (items == null)
                return;

            foreach (var item in items) {
                AddValue(item);
            }
        }

        protected virtual void AddValue(TValue value) {
            lock (root) {
                this.Inner.Add(value);
            }

            int count = 0;
            foreach (var o in this.observers) {
                if (!o.Disposed) {
                    o.OnNext(value);
                    count++;
                }
            }

            if (count != this.observers.Count) {
                this.observers.RemoveAll(t => t.Disposed);
            }
        }

        public IEnumerator<TValue> GetEnumerator() {
            if (AssemblyInfo.AppDomainInSync())
                return Inner.GetEnumerator();

            var result = new Enumerator(this);
            this.observers.Add(result);
            return result;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        abstract class ObserverDisposer : IDisposable, IObserver<TValue> {

            protected LazyDescriptionCollection<TValue> owner;
            public bool Disposed { get; private set; }

            protected ObserverDisposer(LazyDescriptionCollection<TValue> owner) {
                this.owner = owner;
            }

            public void Dispose() {
                this.Disposed = true;
            }

            public abstract void OnNext(TValue item);

            public void OnError(Exception error) {}
            public void OnCompleted() {}
        }

        class DefaultObserverDisposer : ObserverDisposer {

            private readonly IObserver<TValue> observer;

            public DefaultObserverDisposer(LazyDescriptionCollection<TValue> owner, IObserver<TValue> observer)
                : base(owner)
            {
                this.observer = observer;
            }

            public override void OnNext(TValue item) {
                observer.OnNext(item);
            }

        }

        class Enumerator : ObserverDisposer, IEnumerator<TValue> {

            private readonly ConcurrentQueue<TValue> existingValues;

            public Enumerator(LazyDescriptionCollection<TValue> owner) : base(owner) {
                this.existingValues = new ConcurrentQueue<TValue>();
                this.Reset();
            }

            public TValue Current {
                get {
                    if (this.existingValues.Count == 0)
                        throw Failure.OutsideEnumeration();

                    TValue result;
                    existingValues.TryPeek(out result);
                    return result;
                }
            }

            object System.Collections.IEnumerator.Current {
                get { return this.Current; } }

            public bool MoveNext() {
                while (existingValues.Count == 0 && AssemblyInfo.ContinueAppDomain()) {
                }

                if (existingValues.Count > 0) {
                    TValue result;
                    existingValues.TryDequeue(out result);
                }

                return (existingValues.Count > 0);
            }

            public void Reset() {
                foreach (var value in owner.Inner)
                    this.existingValues.Enqueue(value);
            }

            public override void OnNext(TValue value) {
                this.existingValues.Enqueue(value);
            }

        }

    }
}
