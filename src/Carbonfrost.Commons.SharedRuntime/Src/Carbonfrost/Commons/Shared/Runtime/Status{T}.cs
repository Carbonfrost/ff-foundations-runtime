//
// - Status{T}.cs -
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
using System.Globalization;
using Carbonfrost.Commons.Shared.Runtime.Components;
using RuntimeComponent = Carbonfrost.Commons.Shared.Runtime.Components.Component;

namespace Carbonfrost.Commons.Shared.Runtime {

	[CLSCompliant(false)]
	public class Status<TEnum> : Status
		where TEnum : struct, IComparable, IFormattable, IConvertible
	{

		public Status(RuntimeComponent component, string message, Exception exception, FileLocation location, TEnum errorCode)
			: base(component, message, exception, location, errorCode.ToInt32(CultureInfo.InvariantCulture)) {}

		public Status(RuntimeComponent component, string message, Exception exception, FileLocation location)
			: base(component, message, exception, location) {}

		public Status(RuntimeComponent component, Severity level, string message, FileLocation location)
			: base(component, level, message, location) {}

		public Status(RuntimeComponent component, Severity level, Exception exception, FileLocation location)
			: base(component, level, exception, location) {}

		public Status(RuntimeComponent component, Exception exception, FileLocation location)
			: base(component, exception, location) {}

		public Status(Severity level, string message)
			: base(level, message) {}

		public Status(Exception exception)
			: base(exception) {}

		public Status(Severity level, Exception exception)
			: base(level, exception) {}

		public Status(string message, Exception exception)
			: base(message, exception) {}

		public Status(RuntimeComponent component, string message, Exception exception, Severity level, FileLocation location)
			: base(component, level, message, exception, location) {}

		public Status(RuntimeComponent component, string message, Exception exception, Severity level, FileLocation location, TEnum errorCode)
			: base(component, level, message, exception, location, errorCode.ToInt32(CultureInfo.InvariantCulture)) {}

		public Status(IStatus copyFrom)
			: base(copyFrom) {}

		public new TEnum ErrorCode {
			get { return (TEnum) Enum.ToObject(typeof(TEnum), base.ErrorCode); }
		}

		public bool IsSuccess {
			get { return ErrorCode.ToInt32(CultureInfo.InvariantCulture) == 0; }
		}

	}
}
