//
// - FileLocationConverter.cs -
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

namespace Carbonfrost.Commons.Shared.Runtime {

    public sealed class FileLocationConverter : TypeConverter {

        public FileLocationConverter() {}

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            return (sourceType == typeof(string) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            return (destinationType == typeof(string)
                    || destinationType == typeof(InstanceDescriptor)
                    || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
                                           CultureInfo culture,
                                           object value) {
            if (value == null)
                throw GetConvertFromException(value);
            else {
                string text = value as string;

                if (text == null)
                    return base.ConvertFrom(context, culture, value);
                else
                    return FileLocation.Parse(text);
            }
        }

        public override object ConvertTo(ITypeDescriptorContext context,
                                         CultureInfo culture,
                                         object value,
                                         Type destinationType) {

            if (destinationType == null) { throw new ArgumentNullException("destinationType"); } // $NON-NLS-1
            if (!(value is FileLocation))
                throw GetConvertToException(value, destinationType);

            FileLocation loc = (FileLocation) value;

            if (destinationType == typeof(string))
                return loc.ToString();

            else if (destinationType == typeof(InstanceDescriptor)) {
                ConstructorInfo ci =
                    typeof(FileLocation).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(string) });
                return new InstanceDescriptor(ci, new object[] { loc.LineNumber, loc.LinePosition, loc.FileName });

            } else
                return base.ConvertTo(context, culture, value, destinationType);

        }
    }
}



