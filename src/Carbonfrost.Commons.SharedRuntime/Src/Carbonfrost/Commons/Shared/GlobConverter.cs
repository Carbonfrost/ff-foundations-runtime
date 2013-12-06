//
// - GlobConverter.cs -
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using Carbonfrost.Commons.Shared.Runtime;

namespace Carbonfrost.Commons.Shared {

	public sealed class GlobConverter : TypeConverter {

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
			if (sourceType == typeof(string))
				return true;
			else
				return base.CanConvertFrom(context, sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			if (destinationType == typeof(string))
				return true;
			else if (destinationType == typeof(InstanceDescriptor))
				return true;
			else
				return base.CanConvertTo(context, destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
			if (value == null)
				throw base.GetConvertFromException(value);

			string text = value as string;
			if (text == null)
				return base.ConvertFrom(context, culture, value);

			return Glob.Parse(text);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
			if (destinationType == null)
				throw new ArgumentNullException("destinationType");

			if (!(value is Glob))
				throw base.GetConvertToException(value, destinationType);

			Glob loc = (Glob) value;
			if (destinationType == typeof(string))
				return loc.ToString();

			if (destinationType == typeof(InstanceDescriptor))
				return new InstanceDescriptor(typeof(Glob).GetMethod("Parse", new Type[] { typeof(string) }),
				                              new object[] { loc.ToString() });

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
