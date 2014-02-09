//
// - SharedRuntimeOptionsAttribute.cs -
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

namespace Carbonfrost.Commons.Shared.Runtime {

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class SharedRuntimeOptionsAttribute : Attribute {

        internal static readonly SharedRuntimeOptionsAttribute Default = new SharedRuntimeOptionsAttribute();
        internal static readonly SharedRuntimeOptionsAttribute Optimized = new SharedRuntimeOptionsAttribute {
            Optimizations = SharedRuntimeOptimizations.DisableScanning
        };

        public SharedRuntimeOptimizations Optimizations { get; set; }

        internal bool Templates {
            get { return !Optimizations.HasFlag(SharedRuntimeOptimizations.DisableTemplateScanning); }
        }

        internal bool Adapters {
            get { return !Optimizations.HasFlag(SharedRuntimeOptimizations.DisableAdapterScanning); }
        }

        internal bool Providers {
            get { return !Optimizations.HasFlag(SharedRuntimeOptimizations.DisableProviderScanning); }
        }

    }
}
