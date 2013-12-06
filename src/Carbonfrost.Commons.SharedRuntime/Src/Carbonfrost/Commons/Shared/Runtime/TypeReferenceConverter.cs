//
// - TypeReferenceConverter.cs -
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

namespace Carbonfrost.Commons.Shared.Runtime {

    public sealed class TypeReferenceConverter : TypeConverter {

        // 'TypeConverter' overrides.
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            return (sourceType == typeof(Type))
                || (sourceType == typeof(string))
                || base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            return (destinationType == typeof(Type))
                || (destinationType == typeof(string))
                || (destinationType == typeof(InstanceDescriptor))
                || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value == null) {
                throw base.GetConvertFromException(value);
            }

            string source = value as string;
            if (source != null) {
                return TypeReference.Parse(source, context);
            }

            Type type = value as Type;
            if (type != null)
                return TypeReference.FromType(type);

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if ((destinationType != null) && (value is TypeReference)) {
                TypeReference tr = (TypeReference) value;
                if (destinationType == typeof(string)) {
                    return tr.ConvertToString();

                } else if (destinationType == typeof(Type) && tr.Type != null) {
                    return tr.Type;
                } else if (destinationType == typeof(InstanceDescriptor)) {

                    return new InstanceDescriptor(
                        typeof(TypeReference).GetMethod("Parse"),
                        new [] { tr.ToString(), null });

                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

