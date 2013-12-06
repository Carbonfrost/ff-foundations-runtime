//
// - FileSystem.cs -
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
using System.IO;

using Carbonfrost.Commons.Shared.Runtime;

namespace Carbonfrost.Commons.Shared {
    // TODO Move these according to actual use (remove the others)
    static class FileSystem {

        public static string FindBestNativeLanguageFile(string defaultFileName, CultureInfo culture) {
            if (defaultFileName == null) { throw new ArgumentNullException("defaultFileName"); } // $NON-NLS-1
            if (culture == null) { throw new ArgumentNullException("culture"); } // $NON-NLS-1

            // Adjust to full name, expanding environment variables
            defaultFileName = Environment.ExpandEnvironmentVariables(Path.GetFullPath(defaultFileName));

            // Now - check for the candidates in order en-us, 1033, en
            return FirstExistingFile(
                GetNativeLanguageFile(defaultFileName, culture, NativeLanguageStyle.Name),
                GetNativeLanguageFile(defaultFileName, culture, NativeLanguageStyle.Lcid),
                GetNativeLanguageFile(defaultFileName, culture, NativeLanguageStyle.TwoLetterIsoLanguageName))
                ?? defaultFileName;
        }

        public static string FindBestNativeLanguageDirectory(string defaultPath, CultureInfo culture) {
            if (defaultPath == null) { throw new ArgumentNullException("defaultPath"); } // $NON-NLS-1
            if (culture == null) { throw new ArgumentNullException("culture"); } // $NON-NLS-1

            // Adjust to full name, expanding environment variables
            defaultPath = Environment.ExpandEnvironmentVariables(Path.GetFullPath(defaultPath));

            // Now - check for the candidates in order C:/vin/en-us, C:/vin/1033, C:/vin/en
            return FirstExistingDirectory(
                GetNativeLanguageFile(defaultPath, culture, NativeLanguageStyle.Name),
                GetNativeLanguageFile(defaultPath, culture, NativeLanguageStyle.Lcid),
                GetNativeLanguageFile(defaultPath, culture, NativeLanguageStyle.TwoLetterIsoLanguageName))
                ?? defaultPath;
        }

        static string FirstExistingDirectory(params string[] directories) {
            foreach (string f in directories) {
                if (Directory.Exists(f)) {
                    return f;
                }
            }

            return null;
        }

        static string FirstExistingFile(params string[] directories) {
            foreach (string f in directories) {
                if (File.Exists(f)) {
                    return f;
                }
            }

            return null;
        }

        public static CultureInfo GetLocaleFromFileName(string fileName) {
            // Remove the file extension and search for the first preceeding period
            string noExt = Path.GetFileNameWithoutExtension(fileName);

            int index = noExt.LastIndexOf("."); // $NON-NLS-1
            if (index < 0) {
                // There was no period, so it cannot be any form we expect
                return CultureInfo.InvariantCulture;
            }

            // The text after the period is the result
            string cultureString = noExt.Substring(index + 1).Trim();

            if (Char.IsDigit(cultureString[0])) {
                return _GetCultureFromLcid(cultureString);

            } else {
                return _GetCultureFromIetf(cultureString);
            }
        }

        public static string GetNativeLanguageFile(string defaultFileName, CultureInfo culture, NativeLanguageStyle style) {
            if (defaultFileName == null) { throw new ArgumentNullException("defaultFileName"); } // $NON-NLS-1
            if (culture == null) { throw new ArgumentNullException("culture"); } // $NON-NLS-1

            if (CultureInfo.InvariantCulture.Equals(culture )) {
                return defaultFileName;
            }

            // This is a directory name
            if (Path.GetExtension(defaultFileName) == string.Empty) {
                defaultFileName = defaultFileName + Path.DirectorySeparatorChar.ToString();
                // C:/vin/en-us, C:/vin/1033, C:/vin/en
                switch (style) {
                    case NativeLanguageStyle.Lcid:
                        return Path.Combine(defaultFileName, culture.LCID.ToString(CultureInfo.InvariantCulture));

                    case NativeLanguageStyle.TwoLetterIsoLanguageName:
                        return Path.Combine(defaultFileName, culture.TwoLetterISOLanguageName);

                    case NativeLanguageStyle.Name:
                    default:
                        return Path.Combine(defaultFileName, culture.Name);;
                }

            } else {
                // Create a format string such as 'example.{0}.txt' from 'example.txt'
                string format = Path.ChangeExtension(
                    defaultFileName, ".{0}" + Path.GetExtension(defaultFileName)); // $NON-NLS-1
                switch (style) {
                    case NativeLanguageStyle.Lcid:
                        return string.Format(CultureInfo.InvariantCulture, format, culture.LCID);

                    case NativeLanguageStyle.TwoLetterIsoLanguageName:
                        return string.Format(CultureInfo.InvariantCulture, format, culture.TwoLetterISOLanguageName);

                    case NativeLanguageStyle.Name:
                    default:
                        return string.Format(CultureInfo.InvariantCulture, format, culture.Name);
                }
            }
        }

        private static CultureInfo _GetCultureFromIetf(string ietfName) {
            if (string.IsNullOrEmpty(ietfName)) { return CultureInfo.InvariantCulture; }

            try {
                CultureInfo result = CultureInfo.GetCultureInfoByIetfLanguageTag(ietfName);

                return result;
            } catch (ArgumentException) {
                return CultureInfo.InvariantCulture;
            }
        }

        // Gets a culture from a particular LCID
        private static CultureInfo _GetCultureFromLcid(string lcidString) {
            // Assume it is a number
            int lcid = 0;
            if (int.TryParse(
                lcidString, NumberStyles.Integer, CultureInfo.InvariantCulture, out lcid)) {

                try {
                    CultureInfo info = CultureInfo.GetCultureInfo(lcid);
                    return info;
                } catch (ArgumentException) {
                    return CultureInfo.InvariantCulture;
                }

            } else {
                return CultureInfo.InvariantCulture;
            }
        }

    }
}
