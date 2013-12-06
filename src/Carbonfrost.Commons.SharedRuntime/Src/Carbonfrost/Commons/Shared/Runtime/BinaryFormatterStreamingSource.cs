//
// - BinaryFormatterStreamingSource.cs -
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
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Carbonfrost.Commons.Shared.Runtime {

    sealed class BinaryFormatterStreamingSource : StreamingSource {

        readonly byte[] BINARY_MAGIC = { 0, 1, 0, 0, 0, 255, 255, 255, 255 };

        public override bool IsValidInput(StreamContext inputSource) {
            if (inputSource == null)
                throw new ArgumentNullException("inputSource"); // $NON-NLS-1

            using (Stream s = inputSource.OpenRead()) {
                byte[] b = new byte[BINARY_MAGIC.Length];
                s.Read(b, 0, BINARY_MAGIC.Length);
                if (b.SequenceEqual(BINARY_MAGIC))
                    return true;
                else
                    return false;
            }
        }

        public override void Save(StreamContext outputTarget,
                                  object value) {
            if (outputTarget == null)
                throw new ArgumentNullException("outputTarget"); // $NON-NLS-1

            using (Stream s = outputTarget.OpenWrite()) {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(s, value);
                s.Flush();
            }
        }

        public override object Load(StreamContext inputSource,
                                    Type instanceType) {
            if (inputSource == null)
                throw new ArgumentNullException("inputSource"); // $NON-NLS-1
            instanceType = instanceType ?? typeof(object);

            using (Stream s = inputSource.OpenRead()) {
                BinaryFormatter bf = new BinaryFormatter();
                object result = bf.Deserialize(s);
                if (!instanceType.IsInstanceOfType(result))
                    throw Failure.NotInstanceOf("instanceType", instanceType, result.GetType());

                return result;
            }
        }
    }
}

