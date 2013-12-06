//
// - AdapterRole.cs -
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
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace Carbonfrost.Commons.Shared.Runtime {

    public static class AdapterRole {

        public const string Builder = "Builder";
        public const string StreamingSource = "StreamingSource";
        public const string ActivationProvider = "ActivationProvider";
        public const string Null = "Null";

        public static class BuilderFunctions {

            public static readonly Expression<Func<IServiceProvider, object>> Build
                = (a) => (default(object));
        }

        public static class StreamingSourceFunctions {

            public static readonly Expression<Func<Stream, Encoding, StreamingSource>> FromStream
                = (a, b) => default(StreamingSource);

            public static readonly Expression<Func<StreamContext, Encoding, StreamingSource>> FromStreamContext
                = (a, b) => (default(StreamingSource));
        }

        public static bool IsActivationProviderType(Type type) {
            if (type == null)
                throw new ArgumentNullException("type"); // $NON-NLS-1

            return typeof(IActivationProvider).IsAssignableFrom(type);
        }

        public static bool IsBuilderType(Type type) {
            if (type == null)
                throw new ArgumentNullException("type"); // $NON-NLS-1

            // Can't itself be a builder type, must define Build
            return !type.IsDefined(typeof(BuilderAttribute), false)
                && Adaptable.GetMethodBySignature(type, "Build", BuilderFunctions.Build) != null;
        }

        public static bool IsStreamingSourceType(Type type) {
            if (type == null)
                throw new ArgumentNullException("type"); // $NON-NLS-1

            return typeof(StreamingSource).IsAssignableFrom(type);
        }

    }
}
