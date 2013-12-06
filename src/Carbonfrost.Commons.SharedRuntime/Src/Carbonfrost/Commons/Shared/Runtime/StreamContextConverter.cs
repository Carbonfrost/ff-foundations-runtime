//
// - StreamContextConverter.cs -
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
using System.Globalization;
using System.IO;

namespace Carbonfrost.Commons.Shared.Runtime {

	public sealed class StreamContextConverter : TypeConverter {

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
			if (sourceType == null) { throw new ArgumentNullException("sourceType"); } // $NON-NLS-1
			if (sourceType.Equals(typeof(string))
			    || sourceType.Equals(typeof(Uri))
			    || sourceType.Equals(typeof(Stream))) {

				return true;
			} else {
				return base.CanConvertFrom(context, sourceType);
			}
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			if (destinationType == null) { throw new ArgumentNullException("destinationType"); } // $NON-NLS-1
			if (destinationType.Equals(typeof(string))
			    || destinationType.Equals(typeof(Uri)))
				return true;

			return base.CanConvertTo(context, destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
			if (value == null) { throw new ArgumentNullException("value"); } // $NON-NLS-1

			Uri u = value as Uri;
			if (u != null)
				return StreamContext.FromSource(u);

			string s = value as string;
			if (s != null)
				return StreamContext.FromSource(new Uri(s));

			Stream sm = value as Stream;
			if (sm != null)
				return StreamContext.FromStream(sm);

			throw base.GetConvertFromException(value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
			if (destinationType == null) { throw new ArgumentNullException("destinationType"); } // $NON-NLS-1
			StreamContext vr = value as StreamContext;
			if (vr == null)
				throw base.GetConvertToException(value, destinationType);

			if (destinationType.Equals(typeof(Uri)))
				return vr.Uri;

			if (destinationType.Equals(typeof(string)))
				return vr.Uri.ToString();

			throw base.GetConvertToException(value, destinationType);
		}

	}
}
