//
// - PropertiesWriter.cs -
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
using System.IO;

namespace Carbonfrost.Commons.Shared.Runtime {

    class PropertiesWriter : DisposableObject {

        public int WrapWidth = 76;
        public int ContinuationIndentWidth = 4;
        public char IndentChar = ' ';
        private readonly TextWriter source;

        private TextWriter BaseWriter {
            get { return this.source; }
        }

		public bool OutputTypes {
			get;
			set;
		}

        public PropertiesWriter(TextWriter source) {
            if (source == null)
                throw new ArgumentNullException("source"); // $NON-NLS-1
            this.source = source;
        }

        public static string Escape(string text) {
            if (text == null) { return string.Empty; }
            return text.Replace("\\", "\\\\") // $NON-NLS-1, $NON-NLS-2
                .Replace("\r\n", "\\n") // Windows // $NON-NLS-1, $NON-NLS-2
                .Replace("\r", "\\n")   // Mac // $NON-NLS-1, $NON-NLS-2
                .Replace("\n", "\\n"); // Unix // $NON-NLS-1, $NON-NLS-2
        }

        public virtual void WriteCategory(string category) {
            Require.NotNullOrAllWhitespace("category", category); // $NON-NLS-1
            BaseWriter.Write('[');
            BaseWriter.Write(category);
            BaseWriter.WriteLine(']');
        }

        public virtual void WriteComment(string commentText) {
            BaseWriter.Write('#');
            BaseWriter.Write(' ');
            BaseWriter.WriteLine(commentText);
        }

        public void WriteProperties(IEnumerable<KeyValuePair<string, object>> properties) {
            if (properties == null) { throw new ArgumentNullException("properties"); } // $NON-NLS-1
            foreach (KeyValuePair<string, object> kvp in properties) {
                WriteProperty(kvp.Key, kvp.Value);
            }
        }

        public virtual void WriteProperty(string key, object value) {
            Require.NotNullOrAllWhitespace("key", key); // $NON-NLS-1
            BaseWriter.Write(key);
            BaseWriter.Write('=');
            if (value == null) {
                BaseWriter.WriteLine();
                return;
            }
            // Use type converters to get the string representation
            TypeConverter c = TypeDescriptor.GetConverter(value.GetType());
            char[] outputValue = Escape(c.ConvertToInvariantString(value)).ToCharArray();
            int column = key.Length + 1;
            int pos = 0;
            bool needLineContinuation = false;
            while (pos < outputValue.Length) {
                if (needLineContinuation) {
                    BaseWriter.Write("\\"); // $NON-NLS-1
                    BaseWriter.WriteLine();
                    for (int i = 0; i < this.ContinuationIndentWidth; i++) {
                        BaseWriter.Write(this.IndentChar);
                    }
                }
                // Compute the number of characters that will fit
                int subStringLength = Math.Min(outputValue.Length - pos,
                                               this.WrapWidth - column);
                BaseWriter.Write(outputValue, pos, subStringLength);
                pos += subStringLength;
                while (pos < outputValue.Length && outputValue[pos] == ' ') {
                    // We need any spaces to preceed the continuation character so that
                    // the reader consumes them properly.
                    pos++;
                    BaseWriter.Write(' ');
                }
                needLineContinuation = true;
                column = this.ContinuationIndentWidth;
            }
            BaseWriter.WriteLine();
        }
    }
}
