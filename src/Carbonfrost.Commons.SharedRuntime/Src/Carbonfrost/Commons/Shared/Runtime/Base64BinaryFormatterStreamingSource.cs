//
// - Base64BinaryFormatterStreamingSource.cs -
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
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace Carbonfrost.Commons.Shared.Runtime {

	sealed class Base64BinaryFormatterStreamingSource : StreamingSource {

		// Stop after reading this many characters
		const int OPTIMISTIC_STOP = 8;

		public override bool IsValidInput(StreamContext inputSource) {
			if (inputSource == null)
				throw new ArgumentNullException("inputSource"); // $NON-NLS-1

			using (TextReader tr = inputSource.OpenText()) {
				int i;
				int count = 0;
				while (count++ < OPTIMISTIC_STOP && (i = tr.Read()) >= 0) {
					if (i >= 'A' && i <= 'F')
						continue;
					else if (i >= 'a' && i <= 'f')
						continue;
					else if (i >= '0' && i <= '9')
						continue;
					else if (char.IsWhiteSpace((char) i))
						continue;
					else
						return false;
				}
			}

			return true;
		}

		public override void Save(StreamContext outputTarget,
		                          object value) {
			if (outputTarget == null)
				throw new ArgumentNullException("outputTarget"); // $NON-NLS-1

			using (Stream s = outputTarget.OpenWrite()) {
				using (Stream bs = new CryptoStream(s, new ToBase64Transform(), CryptoStreamMode.Write)) {
					BinaryFormatter bf = new BinaryFormatter();
					bf.Serialize(bs, value);
					s.Flush();
				}
			}
		}

		public override object Load(StreamContext inputSource,
		                            Type instanceType) {
			if (inputSource == null)
				throw new ArgumentNullException("inputSource"); // $NON-NLS-1
			instanceType = instanceType ?? typeof(object);

			using (Stream s = inputSource.OpenRead()) {
				using (Stream bs = new CryptoStream(s, new FromBase64Transform(FromBase64TransformMode.IgnoreWhiteSpaces), CryptoStreamMode.Read)) {
					BinaryFormatter bf = new BinaryFormatter();
					object result = bf.Deserialize(bs);
					if (!instanceType.IsInstanceOfType(result))
					    throw Failure.NotInstanceOf("result", result, instanceType);

					return result;
				}
			}
		}
	}

}

