//
// - NamespaceBindingConverter.cs -
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
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace Carbonfrost.Commons.Shared.Runtime.Components {

    public sealed class NamespaceBindingConverter : TypeConverter {

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(String))
                return true;
            if (sourceType == typeof(Uri))
                return true;
            else
                return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            if (destinationType == typeof(string))
                return true;
            if (destinationType == typeof(InstanceDescriptor))
                return true;
            if (destinationType == typeof(Uri))
                return true;
            else
                return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value == null)
                throw base.GetConvertFromException(value);

            string text = value as string;
            if (text != null)
                return NamespaceBinding.Parse(text);

            Uri uri = value as Uri;
            if (uri != null)
                return NamespaceBinding.FromUri(uri);

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType == null)
                throw new ArgumentNullException("destinationType");

            NamespaceBinding b = value as NamespaceBinding;
            if (b == null)
                throw base.GetConvertToException(value, destinationType);

            if (destinationType == typeof(string))
                return b.ToString();

            if (destinationType == typeof(Uri))
                return b.ToUri();

            if (destinationType == typeof(InstanceDescriptor)) {
                ConstructorInfo ctor = typeof(NamespaceBinding).GetConstructor(
                    new Type[] { typeof(string), typeof(Assembly) });
                return new InstanceDescriptor(ctor, new object[] { b.Namespace, b.AssemblyName });
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

    }
}
