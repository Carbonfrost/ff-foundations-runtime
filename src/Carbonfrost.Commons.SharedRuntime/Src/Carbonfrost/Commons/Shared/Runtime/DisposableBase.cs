//
// - DisposableBase.cs -
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
using System.Diagnostics;
using System.Threading;

namespace Carbonfrost.Commons.Shared.Runtime {

	[Serializable]
	public abstract class DisposableObject : MarshalByRefObject, IDisposable {

	    // Keeps track of when the object is disposed.  Zero when the object is live, negative for disposal
	    [NonSerializedAttribute]
	    private int isDisposed;

	    #if DEBUG
	    private readonly System.Diagnostics.StackTrace _allocStack;
	    #endif

	    protected DisposableObject() {
	        #if DEBUG
	        _allocStack = new StackTrace();
	        #endif
	    }

	    ~DisposableObject() {
	        // Call disposal, only unmanaged resources
	        Dispose(false);
	    }

	    [System.ComponentModel.Browsable(false)]
	    protected bool IsDisposed {
	        [DebuggerStepThrough]
	        get { return this.isDisposed < 0; }
	    }

	    protected void ThrowIfDisposed() {
	        if (IsDisposed)
	            throw Failure.Disposed(this.GetType().ToString());
	    }

	    protected virtual void Dispose(bool manualDispose) {}

	    // 'IDisposable' implementation.

	    public void Dispose() {
	        // Dispose and suppress finalization
	        if (isDisposed == 0) {
	            // Only do client disposal the first time
	            System.GC.SuppressFinalize(this);
	            try {
	                this.Dispose(true);
	            } finally {
	                Interlocked.Decrement(ref isDisposed);
	            }
	        } else {
	            // $gm: This is a subsequent time, and client disposal shouldnt occur
	        }
	    }

	}
}
