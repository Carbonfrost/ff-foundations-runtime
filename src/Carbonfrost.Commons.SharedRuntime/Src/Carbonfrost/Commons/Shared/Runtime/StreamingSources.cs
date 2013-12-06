//
// - StreamingSources.cs -
//
// Copyright 2012 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

namespace Carbonfrost.Commons.Shared.Runtime {

    [Providers]
    public static class StreamingSources {

        [StreamingSourceUsage(ContentTypes = ContentTypes.BinaryFormatterBase64)]
        public static readonly StreamingSource BinaryFormatterBase64;

        [StreamingSourceUsage(ContentTypes = ContentTypes.BinaryFormatter)]
        public static readonly StreamingSource BinaryFormatter;

        public static readonly StreamingSource XmlFormatter;
        public static readonly StreamingSource Text;

        [StreamingSourceUsage(ContentTypes = "text/x-properties; text/x-ini", Extensions = ".ini; .conf; .cfg; .properties")]
        public static readonly StreamingSource Properties;

        static StreamingSources() {
            Text = new TextStreamingSource();
            XmlFormatter = new XmlFormatterStreamingSource();
            BinaryFormatter = new BinaryFormatterStreamingSource();
            BinaryFormatterBase64 = new Base64BinaryFormatterStreamingSource();
            Properties = new PropertiesStreamingSource();
        }
    }
}

