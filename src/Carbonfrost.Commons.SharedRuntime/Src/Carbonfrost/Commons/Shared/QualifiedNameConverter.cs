//
// - QualifiedNameConverter.cs -
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
using System.ComponentModel;
using System.Globalization;
using System.Xml;

namespace Carbonfrost.Commons.Shared {

    public sealed class QualifiedNameConverter : TypeConverter {

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == null)
                throw new ArgumentNullException("sourceType");

            return (typeof(string).Equals(sourceType) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            if (destinationType == null)
                throw new ArgumentNullException("destinationType");

            return (typeof(string).Equals(destinationType) || this.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value == null)
                throw new ArgumentNullException("value");

            string s = value as string;
            if (s != null)
                return QualifiedName.Parse(s, context);

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (value == null)
                throw new ArgumentNullException("value");

            if (destinationType == null)
                throw new ArgumentNullException("destinationType");

            QualifiedName r = value as QualifiedName;
            if ((r != null) && destinationType.Equals(typeof(string)))
                return r.ToString();

            return base.ConvertTo(context, culture, value, destinationType);
        }

    }
}
