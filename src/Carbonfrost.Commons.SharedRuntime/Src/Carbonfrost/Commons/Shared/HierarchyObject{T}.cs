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
using System.Linq;
using System.Collections.Generic;
using Carbonfrost.Commons.Shared.Runtime;

namespace Carbonfrost.Commons.Shared {

    [Serializable]
    public abstract class HierarchyObject<T>
        : IHierarchyObject, IHierarchyObject<T>, IHierarchyNavigable
        where T : HierarchyObject<T>
    {

        private T parent;

        public IEnumerable<T> Ancestors {
            get { return this.GetAncestors().Cast<T>(); }
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

        protected virtual void OnParentChanged() {}
        protected virtual void OnParentChanging(IHierarchyObject newParent) {}

        // 'IHierarchyObject' implementation.

        IHierarchyObject IHierarchyObject.ParentObject {
            get { return this.ParentObject; }
            set { this.ParentObject = (T) value; }
        }

        // 'IHierarchyObject<T>' implementation.

        public T ParentObject {
            get {
                return this.parent;
            }
            set {
                if (!object.Equals(this.parent, value)) {
                    OnParentChanging(value);
                    this.parent = value;
                    OnParentChanged();
                }
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

            if (AllowRelativeTraversal) {
                if (name == ".") // $NON-NLS-1
                    return (T) this;

                if (name == "..") // $NON-NLS-1
                    return this.ParentObject;
            }

            if (name == "." || name == "..") // $NON-NLS-1, $NON-NLS-2
                throw RuntimeFailure.NoRelativeTraversal();

            return default(T);
        }

        public virtual T SelectChild(int index) {
            Require.WithinRange(null, this.Children, "index", index); // $NON-NLS-1

            if (AllowOrdinalTraversal)
                return this.Children[index];
            else
                throw new InvalidOperationException();
        }

        public virtual object SelectAttribute(string attribute) {
            throw RuntimeFailure.SelectAttributesNotSupported();
        }

        #region Properties.

        [SelfDescribingPriority(PriorityLevel.None)]
        protected virtual bool AllowRelativeTraversal {
            get { return true; }
        }

        [SelfDescribingPriority(PriorityLevel.None)]
        protected virtual bool AllowOrdinalTraversal {
            get { return true; }
        }

        protected virtual IList<T> Children {
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
            this.Children.Add(child);
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
            this.Children.Insert(index, value);
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
            Children.RemoveAt(index);
        }

        public void SetChild(int index, T value) {
            if (value == null)
                throw new ArgumentNullException("value");  // $NON-NLS-1

            ValidateIndex(index);
            this.Children[index] = value;
        }

        public void RemoveAllChildren() {
            Children.Clear();
        }

        public IEnumerator<T> GetChildrenEnumerator() {
            return this.Children.GetEnumerator();
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


