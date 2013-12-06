//
// - ComponentNameConverter.cs -
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

namespace Carbonfrost.Commons.Shared.Runtime.Components {

	public sealed class ComponentNameConverter : TypeConverter {

	    // TypeConverter' overrides.

	    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
	        if (sourceType == null) { throw new ArgumentNullException("sourceType"); } // $NON-NLS-1
	        if (sourceType.Equals(typeof(string))) {
	            return true;
	        } else {
	            return base.CanConvertFrom(context, sourceType);
	        }
	    }

	    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
	        if (destinationType == null) { throw new ArgumentNullException("destinationType"); } // $NON-NLS-1
	        if (destinationType.Equals(typeof(string))) {
	            return true;
	        } else {
	            return base.CanConvertTo(context, destinationType);
	        }
	    }

	    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
	        if (value == null) { throw new ArgumentNullException("value"); } // $NON-NLS-1
	        string s = value as string;

	        if (s == null) {
	            throw base.GetConvertFromException(value);
	        } else {
	            return ComponentName.Parse(s);
	        }
	    }

	    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
	        if (destinationType == null) { throw new ArgumentNullException("destinationType"); } // $NON-NLS-1
	        ComponentName vr = value as ComponentName;
	        if (vr == null) {
	            throw base.GetConvertToException(value, destinationType);
	        }

	        if (destinationType.Equals(typeof(string))) {
	            return vr.ToString();
	        } else {
	            throw base.GetConvertToException(value, destinationType);
	        }
	    }

	}
}
