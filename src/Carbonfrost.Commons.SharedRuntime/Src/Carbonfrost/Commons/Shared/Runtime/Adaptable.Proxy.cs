//
// - Adaptable.Proxy.cs -
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

    static partial class Adaptable {

        public static TResult Implement<TResult>(this object instance, FallbackBehavior fallback)
            where TResult : class
        {

            TResult result = CoreImplement<TResult>(instance, fallback);
            if (result == null)
                throw RuntimeFailure.FailedToGenerateProxy(instance.GetType());
            return result;
        }

        public static TResult Implement<TResult>(this object instance)
            where TResult : class
        {
            return Implement<TResult>(instance, FallbackBehavior.ThrowException);
        }

        public static TResult TryImplement<TResult>(this object instance) {
            TResult result = CoreImplement<TResult>(instance, FallbackBehavior.None);
            return result;
        }

        static TResult CoreImplement<TResult>(this object instance, FallbackBehavior fallback) {
            if (instance == null)
                throw new ArgumentNullException("instance");

            if (instance is TResult)
                return (TResult) instance;

            if (!(typeof(TResult).IsPublic || typeof(TResult).IsNestedPublic))
                throw RuntimeFailure.PublicTypeRequiredAdapter(typeof(TResult), "TResult");

            var builder = AdaptableProxyBuilder.Create(
                fallback, instance.GetType(), typeof(TResult));

            return (TResult) builder.CreateInstance(instance);
        }

    }
}
