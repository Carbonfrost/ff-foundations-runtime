//
// - RuntimeFailure.cs -
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
using System.ComponentModel;
using System.Reflection;
using System.Security;

using Carbonfrost.Commons.Shared.Runtime.Components;
using Carbonfrost.Commons.SharedRuntime.Resources;

namespace Carbonfrost.Commons.Shared.Runtime {

    static class RuntimeFailure {

        public static InvalidOperationException PublishAttributeTypeMismatch(MemberInfo m, Type serviceType) {
            return Failure.Prepare(new InvalidOperationException(SR.PublishAttributeTypeMismatch(m, serviceType)));
        }

        public static NotSupportedException NotSupportedIdentity(string identityKey) {
            return Failure.Prepare(new NotSupportedException(SR.NotSupportedIdentityKey(identityKey)));
        }

        public static ArgumentException PropertyNotFound(string propertyNameArgument, string propertyName) {
            return Failure.Prepare(new ArgumentException(SR.PropertyNotFound(propertyName), propertyNameArgument));
        }

        public static FormatException NotValidHexString() {
            return Failure.Prepare(new FormatException(SR.NotValidHexString()));
        }

        public static ArgumentException OnlyTypedAdapterFactory() {
            return Failure.Prepare(new ArgumentException(SR.OnlyTypedAdapterFactory()));
        }

        public static ArgumentException ServiceAlreadyExists(string argumentName, object serviceTypeOrName) {
            return Failure.Prepare(new ArgumentException(SR.ServiceAlreadyExists(serviceTypeOrName), argumentName));
        }

        public static InvalidOperationException StreamContextDoesNotExist() {
            return Failure.Prepare(new InvalidOperationException(SR.StreamContextDoesNotExist()));
        }

        public static NotSupportedException SeekNotSupportedByBase() {
            return Failure.Prepare(new NotSupportedException(SR.SeekNotSupportedByBase()));
        }

        public static ArgumentOutOfRangeException SeekNegativeBegin(string argName, long offset) {
            return Failure.Prepare(new ArgumentOutOfRangeException(argName, offset, SR.CannotBeNegative())); // $NON-NLS-1
        }

        public static InvalidOperationException DirectedStreamCannotRead() {
            return Failure.Prepare(new InvalidOperationException(SR.DirectedStreamCannotRead()));
        }

        public static ArgumentException CannotCallStartTwice() {
            return Failure.Prepare(new ArgumentException(SR.CannotCallStartTwice()));
        }

        public static InvalidOperationException DirectedStreamCannotWrite() {
            return Failure.Prepare(new InvalidOperationException(SR.DirectedStreamCannotWrite()));
        }

        public static InvalidOperationException ProgressCallbackNotStarted() {
            return Failure.Prepare(new InvalidOperationException(SR.ProgressCallbackNotStarted()));
        }

        public static ArgumentException CannotExpandPrefixNotFound(string prefix) {
            prefix = string.IsNullOrEmpty(prefix) ? SR.Empty() : prefix;
            return Failure.Prepare(new ArgumentException(SR.CannotExpandPrefixNotFound(prefix)));
        }

        public static InvalidOperationException TypeMissing(TypeReference tr) {
            return Failure.Prepare(new InvalidOperationException(SR.TypeMissing(tr)));
        }

        public static FormatException ExpectedLeftBraceOrDollar() {
            return Failure.Prepare(new FormatException(SR.ExpectedLeftBraceOrDollar()));
        }

        public static InvalidOperationException RequiredSubscriptionConstructor(MemberInfo ctor, string name) {
            return Failure.Prepare(new InvalidOperationException(SR.RequiredSubscriptionConstructor(ctor, name)));
        }

        public static ArgumentException CannotActivateNoConstructor(string argName, Type type) {
            return Failure.Prepare(new ArgumentException(SR.CannotActivateNoConstructor(type), argName));
        }

        public static ArgumentException CannotActivateNoConstructorOrBuilder(string argName, Type type) {
            return Failure.Prepare(new ArgumentException(SR.CannotActivateNoConstructorOrBuilder(type), argName));
        }

        public static UriFormatException NotValidDataUri() {
            return Failure.Prepare(new UriFormatException(SR.NotValidDataUri()));
        }

        public static ServiceNotFoundException ActivationRequiredServiceMissing(object name) {
            return Failure.Prepare(new ServiceNotFoundException(SR.ActivationRequiredServiceMissing(name)));
        }

        public static FormatException ExpectedIdentifier() {
            return Failure.Prepare(new FormatException(SR.ExpectedIdentifier()));
        }

        public static FormatException ExpectedRightBrace() {
            return Failure.Prepare(new FormatException(SR.ExpectedRightBrace()));
        }

        public static SecurityException AssemblyFullyQualifiedRequired() {
            return Failure.Prepare(new SecurityException(SR.AssemblyFullyQualifiedRequired()));
        }

        public static FormatException IncompleteEscapeSequence() {
            return Failure.Prepare(new FormatException(SR.IncompleteEscapeSequence()));
        }

        public static FormatException PropertiesParseKeyNameExpected() {
            return Failure.Prepare(new FormatException(SR.PropertiesParseKeyNameExpected()));
        }

        public static InvalidOperationException PropertyMissing(string key) {
            return Failure.Prepare(new InvalidOperationException(SR.PropertyMissing(key)));
        }

        public static FormatException BuilderCannotBeSelf(Type type) {
            return Failure.Prepare(new FormatException(SR.BuilderCannotBeSelf(type)));
        }

        public static InvalidOperationException PropertiesCategoryMissingBrackets() {
            return Failure.Prepare(new InvalidOperationException(SR.PropertiesCategoryMissingBrackets()));
        }

        public static ArgumentException NotValidLocalName(string argName) {
            return Failure.Prepare(new ArgumentException(SR.NotValidLocalName(), argName));
        }

        public static ArgumentException ForbiddenStreamStreamContext() {
            return Failure.Prepare(new ArgumentException(SR.ForbiddenStreamStreamContext()));
        }

        public static InvalidOperationException PropertyConversionFailed(string name, object value, Exception exception) {
            return Failure.Prepare(new InvalidOperationException(SR.PropertyConversionFailed(name, Utility.Display(value)), exception));
        }

        public static ArgumentException UnknownAdapterRole(string argName, string adapterRole) {
            return Failure.Prepare(new ArgumentException(SR.UnknownAdapterRole(adapterRole), argName));
        }

        public static ArgumentException PublicTypeRequiredAdapter(Type argType, string argName) {
            return Failure.Prepare(new ArgumentException(SR.PublicTypeRequiredAdapter(argType), argName));
        }

        public static ArgumentException ValueTypesNotSupported() {
            return Failure.Prepare(new ArgumentException(SR.ValueTypesNotSupported()));
        }

        public static TypeLoadException TypeMissingFromQualifiedName(QualifiedName name) {
            return Failure.Prepare(new TypeLoadException(SR.TypeMissingFromQualifiedName(name)));
        }

        public static UriFormatException UnknownUriScheme(Type type) {
            return Failure.Prepare(new UriFormatException(SR.UnknownUriScheme(type)));
        }

        public static ArgumentException NoAcceptableStreamingSource(Type instanceType) {
            return Failure.Prepare(new ArgumentException(SR.NoAcceptableStreamingSource(instanceType)));
        }

        public static UriFormatException NotValidComponentUriNamePart(Exception nameError) {
            return Failure.Prepare(new UriFormatException(SR.NotValidComponentUriNamePart(), nameError));
        }

        public static UriFormatException NotValidComponentUriSourcePart() {
            return Failure.Prepare(new UriFormatException(SR.NotValidComponentUriSourcePart()));
        }

        public static ArgumentOutOfRangeException ContentTypeNotStandard(string argumentName, string argumentType) {
            return Failure.Prepare(new ArgumentOutOfRangeException(argumentName, argumentType, SR.ContentTypeNotStandard()));
        }

        public static ArgumentException ComponentTypeCannotBeAnything(string argumentName) {
            return Failure.Prepare(new ArgumentException(SR.ComponentTypeCannotBeAnything(), argumentName));
        }

        public static ArgumentException AssemblyComponentCanOnlyLoadLocalFile(string argumentName) {
            return Failure.Prepare(new ArgumentException(SR.AssemblyComponentCanOnlyLoadLocalFile(), argumentName));
        }

        public static InvalidOperationException PropertyDeclarationMissingKey() {
            return Failure.Prepare(new InvalidOperationException(SR.PropertyDeclarationMissingKey()));
        }

        public static AmbiguousMatchException MultipleNamespaces() {
            return Failure.Prepare(new AmbiguousMatchException(SR.MultipleNamespaces()));
        }

        public static AmbiguousMatchException MultipleProviders() {
            return Failure.Prepare(new AmbiguousMatchException(SR.MultipleProviders()));
        }

        public static AmbiguousMatchException MultipleProviderTypes() {
            return Failure.Prepare(new AmbiguousMatchException(SR.MultipleProviderTypes()));
        }

        public static InvalidOperationException CannotSpecifyPublicKeyToken() {
            return Failure.Prepare(new InvalidOperationException(SR.CannotSpecifyPublicKeyToken()));
        }

        public static ServiceNotFoundException ServiceNotFound(Type type) {
            return Failure.Prepare(new ServiceNotFoundException(type));
        }

        public static InvalidOperationException CannotBuildTagUri() {
            return Failure.Prepare(new InvalidOperationException(SR.CannotBuildTagUri()));
        }

        public static InvalidOperationException TemplateNotFound(string templateName) {
            return Failure.Prepare(new InvalidOperationException(SR.TemplateNotFound(templateName)));
        }

        public static ActivationException CouldNotActivateProperty(PropertyDescriptor property, Exception ex) {
            return Failure.Prepare(new ActivationException(SR.CouldNotActivateProperty(property.Name), ex));
        }

        public static InvalidOperationException FailedToGenerateProxy(Type type) {
            return Failure.Prepare(new InvalidOperationException(SR.FailedToGenerateProxy(type)));
        }

        public static InvalidOperationException PropertyObsolete(string property, string message) {
            return Failure.Prepare(new InvalidOperationException(SR.PropertyObsolete(property, message)));
        }

        public static InvalidOperationException PropertyPreliminary(string property, string message) {
            return Failure.Prepare(new InvalidOperationException(SR.PropertyPreliminary(property, message)));
        }

        public static InvalidOperationException EventObsolete(string _event, string message) {
            return Failure.Prepare(new InvalidOperationException(SR.EventObsolete(_event, message)));
        }

        public static InvalidOperationException EventPreliminary(string _event, string message) {
            return Failure.Prepare(new InvalidOperationException(SR.EventPreliminary(_event, message)));
        }

        public static InvalidOperationException EventMissing(string _event) {
            return Failure.Prepare(new InvalidOperationException(SR.EventMissing(_event)));
        }

        public static FormatException ConcreteClassError(Type baseType) {
            return Failure.Prepare(new FormatException(SR.ConcreteClassError(baseType)));
        }

        public static ArgumentOutOfRangeException UseProviderRegistrationAttributeOverload(
            string argumentName, ProviderRegistrationType type) {

            return Failure.Prepare(new ArgumentOutOfRangeException(argumentName, type, SR.UseProviderRegistrationAttributeOverload()));
        }

        public static ArgumentException QualifiedNameCannotBeGeneratedFromConstructed(string argumentName) {
            return Failure.Prepare(new ArgumentException(SR.QualifiedNameCannotBeGeneratedFromConstructed(), argumentName));
        }

        public static ArgumentException InvalidProviderInstanceType(string argumentName) {
            return Failure.Prepare(new ArgumentException(SR.InvalidProviderInstanceType(), argumentName));
        }

        public static ArgumentException InvalidProviderFieldOrMethod(string argumentName) {
            return Failure.Prepare(new ArgumentException(SR.InvalidProviderFieldOrMethod(), argumentName));
        }
    }
}
