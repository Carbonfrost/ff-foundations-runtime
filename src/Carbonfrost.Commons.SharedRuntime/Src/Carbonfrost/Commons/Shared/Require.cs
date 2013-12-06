//
// - Require.cs -
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
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using Carbonfrost.Commons.Shared.Runtime.Components;
using Carbonfrost.Commons.SharedRuntime.Resources;

namespace Carbonfrost.Commons.Shared {

    public static class Require {

        public static void Rank(string arrayArgumentName, Array arrayArgumentValue, int rank) {
            Require.NotNull(arrayArgumentName, arrayArgumentValue);
            if (arrayArgumentValue.Rank != rank)
                throw new RankException();
        }

        public static void RankOne(string arrayArgumentName, Array arrayArgumentValue) {
            Require.NotNull(arrayArgumentName, arrayArgumentValue);
            if (arrayArgumentValue.Rank != 1)
                throw Failure.RankNotOne(arrayArgumentName);
        }

        public static void DefinedEnum(string argumentName, Enum argumentValue) {
            if (argumentValue == null || !Enum.IsDefined(argumentValue.GetType(), argumentValue))
                throw Failure.NotDefinedEnum(argumentName, argumentValue);
        }

        public static bool IsCriticalException(Exception exception) {
            return (exception is NullReferenceException) || (exception is StackOverflowException)
                || (exception is OutOfMemoryException) || (exception is ThreadAbortException)
                || (exception is IndexOutOfRangeException)
                || (exception is SecurityException)
                || (exception is SEHException)
                || (exception is AccessViolationException);
        }

        public static void NotNullOrAllWhitespace(string argumentName, string argumentValue) {
            if (argumentValue == null)
                throw Failure.Null(argumentName);
            if (argumentValue.Trim().Length == 0)
                throw new ArgumentException(SR.AllWhitespace(), argumentName);
        }

        public static void NotNull(string argumentName, object argumentValue) {
            if (argumentValue == null)
                throw Failure.Null(argumentName);
        }

        public static void NotNullOrEmptyString(string argumentName, string argumentValue) {
            if (argumentValue == null)
                throw Failure.Null(argumentName);
            if (argumentValue.Length == 0)
                throw new ArgumentException(SR.EmptyStringNotValid(), argumentName);
        }

        public static void WithinRange<T>(string listArgumentName,
                                          ICollection<T> listArgumentValue,
                                          string indexArgumentName,
                                          int indexArgumentValue) {
            Require.NotNull(listArgumentName, listArgumentValue);

            if (indexArgumentValue < 0 || indexArgumentValue > listArgumentValue.Count - 1)
                throw Failure.IndexOutOfRange(indexArgumentName, indexArgumentValue, 0, listArgumentValue.Count - 1);
        }

        public static void WithinRange<T>(string listArgumentName,
                                          ICollection<T> listArgumentValue,
                                          string indexArgumentName,
                                          int indexArgumentValue,
                                          string countArgumentName,
                                          int countArgumentValue) {
            Require.NotNull(listArgumentName, listArgumentValue);

            if (indexArgumentValue < 0 || indexArgumentValue > listArgumentValue.Count - 1)
                throw Failure.IndexOutOfRange(indexArgumentName, indexArgumentValue, 0, listArgumentValue.Count - 1);
            int itemCount = listArgumentValue.Count - indexArgumentValue;
            if (countArgumentValue < 0 || countArgumentValue > itemCount)
                throw Failure.CountOutOfRange(countArgumentName, countArgumentValue, 0, itemCount); // $NON-NLS-1
        }

        public static void ReferenceType(string argumentName, Type argumentValue) {
            if (argumentValue == null)
                throw new ArgumentNullException(argumentName);
            if (argumentValue.IsValueType)
                throw new NotImplementedException();
        }

        public static void SubclassOf(string argumentName, Type argumentValue, Type requiredSuperType) {
            if (argumentValue == null)
                throw new ArgumentNullException(argumentName);

            if (!argumentValue.IsSubclassOf(requiredSuperType))
                throw Failure.NotSubclassOf(argumentName, argumentValue, requiredSuperType);
        }

        public static void AssignableFrom(Type value, Type requiredType) {
            if (value == null)
                throw Failure.Null();

            if (!requiredType.IsAssignableFrom(value))
                throw Failure.NotAssignableFrom(value, requiredType);
        }

        public static void AssignableFrom(string argumentName, Type argumentValue, Type requiredType) {
            if (argumentValue == null)
                throw new ArgumentNullException(argumentName);

            if (!requiredType.IsAssignableFrom(argumentValue))
                throw Failure.NotAssignableFrom(argumentName, argumentValue, requiredType);
        }
    }

}
