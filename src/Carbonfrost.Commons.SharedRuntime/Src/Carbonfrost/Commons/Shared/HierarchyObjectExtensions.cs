//
// - HierarchyObjectExtensions.cs -
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
using System.Linq;

namespace Carbonfrost.Commons.Shared {

	public static class HierarchyObjectExtensions {

	    public static IHierarchyObject GetCommonAncestor(this IHierarchyObject source, IHierarchyObject other) {
	        if (object.ReferenceEquals(other, source)) {
	            return source;

	        } else if (other.IsAncestorOf(source)) {
	            return other;

	        } else if (other.IsDescendentOf(source)) {
	            return source;

	        } else {
	            // Scan the hierarchy
	            foreach (IHierarchyObject ancestor in GetAncestors(other)) {
	                // Check the ancestry - skip the starting instance; we already
	                // performed that check
	                if (ancestor.ParentObject != null
	                    && ancestor.ParentObject.IsAncestorOf(source)) {

	                    return ancestor.ParentObject;
	                }
	            }

	            return null;

	        }

	    }

	    public static bool IsAncestorOf(this IHierarchyObject source, IHierarchyObject descendant) {
	        if (descendant == null) { return false; }

	        // Scan the hierarchy
	        IEnumerator<IHierarchyObject> e = GetAncestors(descendant).GetEnumerator();

	        while (e.MoveNext()) {
	            if (e.Current == source)
	                return true;
	        }

	        return false;
	    }

	    public static bool IsDescendentOf(this IHierarchyObject source, IHierarchyObject ancestor) {
	        if (ancestor == null) { return false; }

	        return ancestor.IsAncestorOf(source);
	    }

	    public static IEnumerable<IHierarchyObject> GetAncestors(this IHierarchyObject source) {
	        if (source == null) { throw new ArgumentNullException("source"); } // $NON-NLS-1
	        return GetAncestorsAndSelf(source).Skip(1);
	    }

	    public static IEnumerable<IHierarchyObject> GetAncestorsAndSelf(this IHierarchyObject source) {
	        if (source == null) { throw new ArgumentNullException("source"); } // $NON-NLS-1
	        IHierarchyObject current = source;
	        while (current != null) {
	            yield return current;
	            current = current.ParentObject;
	        }
	    }

	}
}
