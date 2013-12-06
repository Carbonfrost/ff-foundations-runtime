//
// - Failure.cs -
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
using Carbonfrost.Commons.SharedRuntime.Resources;

namespace Carbonfrost.Commons.Shared {

    public static class Failure {

        public static ArgumentException AllWhitespace(string argumentName) {
            return Failure.Prepare(new ArgumentException(SR.AllWhitespace(), argumentName));
        }

        public static InvalidOperationException AlreadyInitialized() {
            return Failure.Prepare(new InvalidOperationException(SR.AlreadyInitialized()));
        }

        public static InvalidOperationException AlreadyInitialized(Exception innerException) {
            return Failure.Prepare(new InvalidOperationException(SR.AlreadyInitialized(), innerException));
        }

        public static ArgumentException CollectionContainsNullElement(string argumentName) {
            return Failure.Prepare(new ArgumentException(SR.CollectionContainsNullElement(), argumentName));
        }

        public static ObjectDisposedException Closed(string objectName) {
            return Failure.Prepare(new ObjectDisposedException(objectName, SR.Closed()));
        }

        public static ObjectDisposedException Closed() {
            return Failure.Prepare(new ObjectDisposedException(null, SR.Closed()));
        }

        public static ArgumentException CollectionContainsNullElement(string argumentName, Exception innerException) {
            return Failure.Prepare(new ArgumentException(SR.ItemRequiredToExistInCollection(), argumentName, innerException));
        }

        public static ArgumentException CollectionElementMissing(string argumentName) {
            return Failure.Prepare(new ArgumentException(SR.ItemRequiredToExistInCollection(), argumentName));
        }

        public static ArgumentException CollectionElementMissing(string argumentName, Exception innerException) {
            return Failure.Prepare(new ArgumentException(SR.ItemRequiredToExistInCollection(),
                                                         argumentName, innerException));
        }

        public static NotSupportedException CollectionFixedSize() {
            return Failure.Prepare(new NotSupportedException(SR.CollectionFixedSize()));
        }

        public static InvalidOperationException CollectionModified() {
            return Failure.Prepare(new InvalidOperationException(SR.Modified()));
        }

        public static ArgumentException ComparisonOperandsMustMatch() {
            return Failure.Prepare(new ArgumentException(SR.ComparisonOperandsMustMatch()));
        }

        public static ArgumentOutOfRangeException CountOutOfRange(string argumentName, object argumentValue, int lowerBound, int upperBound) {
            return Failure.Prepare(new ArgumentOutOfRangeException(argumentName, argumentValue, SR.CollectionCountOutOfRange(lowerBound, upperBound)));
        }

        public static ArgumentOutOfRangeException CountOutOfRange(string argumentName, object argumentValue, int upperBound) {
            return CountOutOfRange(argumentName, argumentValue, 0, upperBound);
        }

        public static ObjectDisposedException Disposed() {
            return Failure.Prepare(new ObjectDisposedException(SR.Disposed(null)));
        }

        public static ObjectDisposedException Disposed(string objectName) {
            return Failure.Prepare(new ObjectDisposedException(objectName, SR.Disposed(objectName)));
        }

        public static InvalidOperationException EmptyCollection() {
            return Failure.Prepare(new InvalidOperationException(SR.CollectionCannotBeEmpty()));
        }

        public static ArgumentException EmptyCollection(string argumentName) {
            return Failure.Prepare(new ArgumentException(SR.EmptyCollectionNotValid(), argumentName));
        }

        public static ArgumentException EmptyCollection(string argumentName, Exception innerException) {
            return Failure.Prepare(new ArgumentException(SR.EmptyCollectionNotValid(), argumentName, innerException));
        }

        public static ArgumentException EmptyString(string argumentName) {
            return Failure.Prepare(new ArgumentException(SR.EmptyStringNotValid(), argumentName));
        }

        public static ArgumentException EmptyString(string argumentName, Exception innerException) {
            return Failure.Prepare(new ArgumentException(SR.EmptyStringNotValid(), argumentName, innerException));
        }

        public static EndOfStreamException Eof() {
            return Failure.Prepare(new EndOfStreamException(SR.UnexpectedEof()));
        }

        public static ArgumentException ForbiddenType(string argumentName, Type forbiddenType) {
            return Failure.Prepare(new ArgumentException(SR.ForbiddenType(forbiddenType), argumentName));
        }

        public static ArgumentException ItemAlreadyExists(string argumentName, object argumentValue) {
            return Failure.Prepare(new ArgumentException(SR.ItemAlreadyExists(argumentValue), argumentName));
        }

        public static ArgumentException KeyAlreadyExists(string argumentName, object argumentValue) {
            return Failure.Prepare(new ArgumentException(SR.KeyAlreadyExists(argumentValue), argumentName));
        }

        public static ArgumentOutOfRangeException IndexOutOfRange(string argumentName, object argumentValue, int lowerBound, int upperBound) {
            return Failure.Prepare(new ArgumentOutOfRangeException(argumentName, argumentValue, SR.CollectionIndexOutOfRange(lowerBound, upperBound)));
        }

        public static ArgumentOutOfRangeException IndexOutOfRange(string argumentName, object argumentValue) {
            return Failure.Prepare(new ArgumentOutOfRangeException(argumentName, argumentValue, SR.CollectionIndexOutOfRangeNoElements()));
        }

        public static ArgumentOutOfRangeException IndexOutOfRange(string argumentName, object argumentValue, int upperBound) {
            return IndexOutOfRange(argumentName, argumentValue, 0, upperBound);
        }

        public static ArgumentException InvalidPathName(string argumentName, object argumentValue) {
            return Failure.Prepare(new ArgumentException(SR.NotValidPathCharacter(argumentValue), argumentName));
        }

        public static ArgumentOutOfRangeException MinMustBeLessThanMax(string argumentName, object argumentValue) {
            return Failure.Prepare(new ArgumentOutOfRangeException(
                argumentName, argumentValue, SR.MinMustBeLessThanMax()));
        }

        public static ArgumentOutOfRangeException NaN(string argumentName, object argumentValue) {
            return Failure.Prepare(new ArgumentOutOfRangeException("argumentName", argumentValue, SR.NaN())); // $NON-NLS-1
        }

        public static ArgumentOutOfRangeException Negative(string argumentName, object argumentValue) {
            return Failure.Prepare(new ArgumentOutOfRangeException(argumentName, argumentValue, SR.CannotBeNegative()));
        }

        public static ArgumentOutOfRangeException NegativeOrZero(string argumentName, object argumentValue) {
            return Failure.Prepare(new ArgumentOutOfRangeException(argumentName, argumentValue, SR.CannotBeNonPositive()));
        }

        public static ArgumentOutOfRangeException NotDefinedEnum(string argumentName, object argumentValue) {
            return NotDefinedEnum(argumentName, argumentValue, (argumentValue == null) ? null : argumentValue.GetType());
        }

        public static ArgumentOutOfRangeException NotDefinedEnum(string argumentName, object argumentValue, Type enumType) {
            return Failure.Prepare(new ArgumentOutOfRangeException(argumentName, argumentValue, SR.NotWithinEnum(enumType)));
        }

        public static ArgumentException NotEnoughSpaceInArray(string argumentName, object argumentValue) {
            // N.B. ArgumentException conforms with Array.Copy even though ArgumentOutOfRangeException would make more sense
            return Failure.Prepare(new ArgumentException(SR.NotEnoughSpaceInArray(), argumentName));
        }

        public static NotFiniteNumberException NotFiniteNumber(string argumentName, double argumentValue) {
            return Failure.Prepare(new NotFiniteNumberException(argumentName, argumentValue));
        }

        public static InvalidOperationException NotInitialized() {
            return Failure.Prepare(new InvalidOperationException(SR.NotInitialized()));
        }

        public static InvalidOperationException NotInitialized(Exception innerException) {
            return Failure.Prepare(new InvalidOperationException(SR.NotInitialized(), innerException));
        }

        public static ArgumentException NotParsable(string argumentName, Type parseType) {
            return Failure.Prepare(new ArgumentException(SR.ParseFailure(parseType), argumentName));
        }

        public static ArgumentException NotParsable(string argumentName, Type parseType, Exception innerException) {
            return Failure.Prepare(new ArgumentException(SR.ParseFailure(parseType), argumentName, innerException));
        }

        [Obsolete("Use the overload that specified argumentName then argumentValue")]
        public static ArgumentException NotCastableAdaptableToRequiredType(string argumentName, Type requiredType, Type actualType) {
            return Failure.Prepare(new ArgumentException(SR.TypeNotCastableAdaptable(actualType, requiredType), argumentName)); // $NON-NLS-1
        }

        [Obsolete("Use the overload that specified argumentName then argumentValue")]
        public static ArgumentException NotCastableAdaptableToRequiredType(string argumentName, Type requiredType, Type actualType, Exception innerException) {
            return Failure.Prepare(new ArgumentException(SR.TypeNotCastableAdaptable(actualType, requiredType), argumentName, innerException)); // $NON-NLS-1
        }

        public static ArgumentException NotReadableStream(string argumentName) {
            return Failure.Prepare(new ArgumentException(SR.StreamCannotRead(), argumentName));
        }

        public static InvalidOperationException NotReadableStream() {
            return Failure.Prepare(new InvalidOperationException(SR.StreamCannotRead()));
        }

        public static InvalidOperationException NotWritableStream() {
            return Failure.Prepare(new InvalidOperationException(SR.StreamCannotWrite()));
        }

        public static ArgumentException NotWritableStream(string argumentName) {
            return Failure.Prepare(new ArgumentException(SR.StreamCannotWrite(), argumentName));
        }

        [Obsolete]
        public static InvalidOperationException NotAnyRequiredType(Type[] anyRequiredTypes, Type actualType) {
            string requiredTypeString =
                string.Join(", ", Array.ConvertAll<Type, string>(anyRequiredTypes, Convert.ToString)); // $NON-NLS-1

            return Failure.Prepare(new InvalidOperationException(
                SR.NotAnyRequiredType(actualType, requiredTypeString)));
        }

        [Obsolete("Use NotAssignableFrom, NotSubclassOf, NotInstanceOf appropriately.")]
        public static InvalidOperationException NotRequiredType(Type requiredType, Type actualType) {
            return Failure.Prepare(new InvalidOperationException(
                SR.NotRequiredType(actualType, requiredType)));
        }

        [Obsolete("Use NotAssignableFrom, NotSubclassOf, NotInstanceOf appropriately.")]
        public static ArgumentException NotRequiredType(string argumentName, Type requiredType, Type actualType) {
            return Failure.Prepare(new ArgumentException(SR.NotRequiredType(actualType, requiredType), argumentName));
        }

        [Obsolete("Use NotAssignableFrom, NotSubclassOf, NotInstanceOf appropriately.")]
        public static ArgumentException NotRequiredType(string argumentName, Type requiredType, Type actualType, Exception innerException) {
            return Failure.Prepare(new ArgumentException(SR.NotRequiredType(actualType, requiredType), argumentName, innerException));
        }

        public static InvalidOperationException NotAdaptableTo(object value, string requiredRole) {
            return Failure.Prepare(new InvalidOperationException(SR.NoAdapterForRole(GetTypeString(value), requiredRole)));
        }

        public static ArgumentException NotAdaptableTo(string argumentName, object argumentValue, string requiredRole, Exception innerException = null) {
            return Failure.Prepare(new ArgumentException(SR.NoAdapterForRole(GetTypeString(argumentValue), requiredRole), argumentName, innerException));
        }

        public static InvalidOperationException NotAdaptableTo(object value, Type requiredType) {
            return Failure.Prepare(new InvalidOperationException(SR.TypeNotCastableAdaptable(GetTypeString(value), requiredType)));
        }

        public static ArgumentException NotAdaptableTo(string argumentName, object argumentValue, Type requiredType, Exception innerException = null) {
            return Failure.Prepare(new ArgumentException(SR.TypeNotCastableAdaptable(GetTypeString(argumentValue), requiredType), argumentName, innerException));
        }

        public static InvalidOperationException NotAssignableFrom(Type value, Type requiredType) {
            return Failure.Prepare(new InvalidOperationException(SR.NotAssignableFrom(value, requiredType)));
        }

        public static ArgumentException NotAssignableFrom(string argumentName, Type requiredType, Type argumentValue, Exception innerException = null) {
            return Failure.Prepare(new ArgumentException(SR.NotAssignableFrom(argumentValue, requiredType), argumentName, innerException));
        }

        public static ArgumentException NotInstanceOf(string argumentName, object argumentValue, Type requiredType) {
            return Failure.Prepare(new ArgumentException(SR.NotInstanceOf(requiredType), argumentName));
        }

        public static ArgumentException NotInstanceOf(string argumentName, object argumentValue, Type requiredType, Exception innerException) {
            return Failure.Prepare(new ArgumentException(SR.NotInstanceOf(requiredType), argumentName, innerException));
        }

        public static ArgumentException NotSubclassOf(string argumentName, object argumentValue, Type requiredType) {
            return Failure.Prepare(new ArgumentException(SR.NotSubclassOf(GetTypeString(argumentValue), requiredType), argumentName));
        }

        public static ArgumentException NotSubclassOf(string argumentName, object argumentValue, Type requiredType, Exception innerException) {
            return Failure.Prepare(new ArgumentException(SR.NotSubclassOf(GetTypeString(argumentValue), requiredType), argumentName, innerException));
        }

        public static InvalidOperationException NotSealed() {
            return Failure.Prepare(new InvalidOperationException(SR.SealableNotSealed()));
        }

        public static InvalidOperationException Null() {
            return Failure.Prepare(new InvalidOperationException(SR.UnexpectedlyNull()));
        }

        public static ArgumentNullException Null(string argumentName) {
            return Failure.Prepare(new ArgumentNullException(argumentName));
        }

        public static ArgumentNullException Null(string argumentName, Exception innerException) {
            return Failure.Prepare(new ArgumentNullException(argumentName, innerException));
        }

        public static ArgumentException NullableHasNoValue(string argumentName) {
            return Failure.Prepare(new ArgumentException(SR.NullableMustHaveValue(), argumentName));
        }

        public static ArgumentException NullableHasNoValue(string argumentName, Exception innerException) {
            return Failure.Prepare(new ArgumentException(SR.NullableMustHaveValue(), argumentName, innerException));
        }

        public static InvalidOperationException PropertyMustBeSet(string propertyName) {
            return Failure.Prepare(new InvalidOperationException(SR.PropertyMustBeSet(propertyName)));
        }

        public static InvalidOperationException OutsideEnumeration() {
            return Failure.Prepare(new InvalidOperationException(SR.OutsideEnumeration()));
        }

        public static ArgumentOutOfRangeException Positive(string argumentName, object argumentValue) {
            return Failure.Prepare(new ArgumentOutOfRangeException(argumentName, argumentValue, SR.CannotBePositive()));
        }

        public static ArgumentOutOfRangeException PositiveOrZero(string argumentName, object argumentValue) {
            return Failure.Prepare(new ArgumentOutOfRangeException(argumentName, argumentValue, SR.CannotBeNonNegative()));
        }

        public static RankException RankNotOne(string arrayArgumentName) {
            return Failure.Prepare(new RankException(SR.CollectionCannotCopyToMultidimensionalArray()));
        }

        public static InvalidOperationException ReadOnlyCollection() {
            return Failure.Prepare(new InvalidOperationException(SR.ReadOnlyCollection()));
        }

        public static ArgumentException ReadOnlyCollection(string argumentName) {
            return Failure.Prepare(new ArgumentException(SR.ReadOnlyCollection(), argumentName));
        }

        public static ArgumentException ReadOnlyCollection(string argumentName, Exception innerException) {
            return Failure.Prepare(new ArgumentException(SR.ReadOnlyCollection(), argumentName, innerException));
        }

        public static InvalidOperationException ReadOnlyProperty(string propertyName) {
            return Failure.Prepare(new InvalidOperationException(SR.ReadOnlyProperty(propertyName)));
        }

        public static InvalidOperationException ReadOnlyProperty() {
            return Failure.Prepare(new InvalidOperationException(SR.ReadOnlyPropertyNoName()));
        }

        public static InvalidOperationException Sealed() {
            return Failure.Prepare(new InvalidOperationException(SR.SealableReadOnly()));
        }

        public static NotSupportedException StreamSeekNotSupported() {
            throw new NotSupportedException(SR.CannotSeekOrSetPosition());
        }

        public static ArgumentOutOfRangeException ValueOutOfRange(string argumentName,
                                                                  object lowerBoundInclusive,
                                                                  object upperBoundInclusive,
                                                                  object argumentValue) {
            return Failure.Prepare(new ArgumentOutOfRangeException(argumentName, argumentValue, SR.OutOfRangeInclusive(lowerBoundInclusive, upperBoundInclusive)));
        }

        public static ArgumentOutOfRangeException Zero(string argumentName, object argumentValue) {
            return Failure.Prepare(new ArgumentOutOfRangeException(argumentName, argumentValue, SR.CannotBeZero()));
        }

        public static T Prepare<T>(T exception)
            where T : Exception {

            if (exception == null)
                return null;

            if (exception.HelpLink == null) {
                // TODO Set the correct HelpURL
            }

            return exception;
        }

        public static ArgumentException NotCompliantIdentifier(string argumentName, string argumentValue) {
            return Failure.Prepare(new ArgumentOutOfRangeException(argumentName, argumentValue, SR.NotCompliantIdentifier2()));
        }

        static string GetTypeString(object argumentValue) {
            string actualType = argumentValue == null ? "null" : argumentValue.GetType().Name;
            return actualType;
        }
    }
}
