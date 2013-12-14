//
// - HierarchyObject{T}.cs -
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using Carbonfrost.Commons.Shared.Runtime;

namespace Carbonfrost.Commons.Shared {

    [Serializable]
    public abstract class HierarchyObject<T>
        : IHierarchyObject, IHierarchyNavigable
        where T : HierarchyObject<T>, IHierarchyObject
    {

        private T parent;

        // TODO Casts could be invalid (rare)
        // class A : HierarchyObject<B>,  class B : HierarchyObject<B>
        // but A can't cast to B

        public IEnumerable<T> Ancestors {
            get { return this.GetAncestors().Cast<T>(); }
        }
        
        public IEnumerable<T> AncestorsAndSelf {
            get { return this.GetAncestorsAndSelf().Cast<T>(); }
        }
        
        public IEnumerable<T> Descendents {
            get {
                return this.Children.SelectMany(t => t.DescendentsAndSelf);
            }
        }
        
        public IEnumerable<T> DescendentsAndSelf {
            get {
                return (new [] { this}).Cast<T>().Concat(this.Descendents);
            }
        }

        public T FirstChild {
            get { return this.SelectChild(0); }
        }

        public T Self {
            get { return (T) this; }
        }

        protected HierarchyObject() {}

        protected HierarchyObject(T parent) {
            this.parent = parent;
        }

        protected virtual void SetParent(T parent) {
            this.parent = parent;
        }

        // 'IHierarchyObject' implementation.

        IHierarchyObject IHierarchyObject.ParentObject {
            get { return this.ParentObject; }
            set { this.ParentObject = (T) value; }
        }

        IEnumerable<IHierarchyObject> IHierarchyObject.ChildrenObjects {
            get { return this.Children; }
        }

        protected T ParentObject {
            get {
                return this.parent;
            }
            set {
                this.SetParent(value);
            }
        }

        // 'IHierarchyNavigable' implementation.

        IHierarchyNavigable IHierarchyNavigable.SelectChild(string childId) {
            return SelectChild(childId);
        }

        IHierarchyNavigable IHierarchyNavigable.SelectChild(int index) {
            return SelectChild(index);
        }

        public virtual T SelectChild(string name) {
            if (name == null)
                throw new ArgumentNullException("name"); // $NON-NLS-1

            if (name.Length == 0)
                throw Failure.EmptyString("name"); // $NON-NLS-1

            return default(T);
        }

        public virtual T SelectChild(int index) {
            Require.WithinRange(null, this.Children, "index", index); // $NON-NLS-1
            return this.Children[index];
        }

        #region Properties.

        public ReadOnlyCollection<T> Children {
            get {
                return new ReadOnlyCollection<T>(this.ChildrenImpl);
            }
        }

        protected virtual IList<T> ChildrenImpl {
            get {
                return Empty<T>.Array;
            }
        }

        public int ChildrenCount {
            get { return this.Children.Count; }
        }

        public T LastChild {
            get { return this.SelectChild(this.ChildrenCount - 1); }
        }

        #endregion

        public void AppendChild(T child) {
            this.ChildrenImpl.Add(child);
            ((IHierarchyObject) child).ParentObject = this;
        }

        public void AppendChildren(T[] children) {
            for (int i = 0; i < children.Length; i++) {
                AppendChild(children[i]);
            }
        }

        public bool ContainsChild(T item) {
            return (IndexOfChild(item) >= 0);
        }

        public void CopyChildrenTo(T[] array, int arrayIndex) {
            this.Children.CopyTo(array, arrayIndex);
        }

        public T GetChild(int index) {
            ValidateIndex(index);
            return this.Children[index];
        }

        public void InsertChild(int index, T value) {
            ValidateIndex(index);
            this.ChildrenImpl.Insert(index, value);
        }

        public bool RemoveChild(T child) {
            for (int i = 0; i < this.Children.Count - 1; i++) {
                if (object.Equals(child, this.Children[i])) {
                    RemoveChildAt(i);
                    return true;
                }
            }

            return false;
        }

        public void RemoveChildAt(int index) {
            ValidateIndex(index);

            T value = Children[index];
            ChildrenImpl.RemoveAt(index);
        }

        public void SetChild(int index, T value) {
            if (value == null)
                throw new ArgumentNullException("value");  // $NON-NLS-1

            ValidateIndex(index);
            this.ChildrenImpl[index] = value;
        }

        public void RemoveAllChildren() {
            ChildrenImpl.Clear();
        }

        public int IndexOfChild(T item) {
            return this.Children.IndexOf(item);
        }

        private void ValidateIndex(int index) {
            if (index < 0 || index > Children.Count - 1)
                throw Failure.IndexOutOfRange("index", index, 0, Children.Count - 1);
        }

    }
}


