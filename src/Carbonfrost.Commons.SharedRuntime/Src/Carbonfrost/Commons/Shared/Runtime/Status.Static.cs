//
// - Status.Static.cs -
//
// Copyright 2010 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using Carbonfrost.Commons.Shared.Runtime.Components;

namespace Carbonfrost.Commons.Shared.Runtime {

    public partial class Status {

        [CLSCompliant(false)]
        public static Status<TEnum> Create<TEnum>(TEnum errorCode, Component component = null, string message = null, Exception exception = null, FileLocation location = default(FileLocation))
            where TEnum : struct, IFormattable, IConvertible, IComparable
        {
            Enum value = errorCode as Enum;
            if (value == null)
                value = (Enum) Enum.ToObject(typeof(TEnum), errorCode);

            return new Status<TEnum>(component, message, exception, location, errorCode);
        }

        [CLSCompliant(false)]
        public static Status<TEnum> Create<TEnum>(TEnum errorCode, Exception exception)
            where TEnum : struct, IFormattable, IConvertible, IComparable
        {
            return Create<TEnum>(errorCode, null, null, exception, FileLocation.Empty);
        }

        [CLSCompliant(false)]
        public static Status<TEnum> Create<TEnum>(TEnum errorCode)
            where TEnum : struct, IFormattable, IConvertible, IComparable
        {
            return Create<TEnum>(errorCode, null, null, null, FileLocation.Empty);
        }

        internal static int StaticGetHashCode(IStatus status) {
            unchecked {
                int hashCodeCache = 0;
                if (status.Message != null) hashCodeCache += 1000000007 * status.Message.GetHashCode();
                if (status.Exception != null) hashCodeCache += 1000000009 * status.Exception.GetHashCode();
                hashCodeCache += 1000000021 * status.FileLocation.GetHashCode();
                hashCodeCache += 1000000033 * status.Level.GetHashCode();
                if (status.Component != null) hashCodeCache += 1000000087 * status.Component.GetHashCode();
                hashCodeCache += 1000000093 * status.ErrorCode.GetHashCode();
                return hashCodeCache;
            }
        }

        internal static bool StaticEquals(IStatus a, IStatus b) {
            if (b == a)
                return true;

            // Chose this order for speed
            if (b.Level == a.Level
                && b.Exception == a.Exception
                && b.ErrorCode == a.ErrorCode
                && Object.Equals(b.Component, a.Component)
                && b.FileLocation.Equals(a.FileLocation)
                && b.Message == a.Message) {

                return true;

            } else {
                var aa = a.Children;
                var bb = b.Children;

                if (aa.Count == bb.Count)
                    return aa.SequenceEqual(bb, new StaticEqualsComparer());
                else
                    return false;
            }
        }

        class StaticEqualsComparer : IEqualityComparer<IStatus> {
            public bool Equals(IStatus x, IStatus y) {
                return Status.StaticEquals(x, y);
            }

            public int GetHashCode(IStatus obj) {
                return Status.StaticGetHashCode(obj);
            }
        }
    }
}
