//
// - FileSystemStreamContext.cs -
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
using System.Security;
using System.Text;

namespace Carbonfrost.Commons.Shared.Runtime {

	internal class FileSystemStreamContext : StreamContext {

	    private readonly string baseFileName;
	    private readonly CultureInfo culture;
	    private readonly Encoding encoding;

	    #region [ Constructors. ]

	    public FileSystemStreamContext(string baseFileName, CultureInfo culture, Encoding encoding) {
	        this.baseFileName = baseFileName;
	        this.culture = culture;
	        this.encoding = encoding;
	    }

	    public FileSystemStreamContext(string fileName) {
	        if (fileName == null)
	            throw new ArgumentNullException("fileName");
	        if (fileName.Length == 0)
	            throw Failure.EmptyString("fileName");

	        // Compute the base path name
	        CultureInfo currentCulture = FileSystem.GetLocaleFromFileName(fileName);
	        if (currentCulture == null || CultureInfo.InvariantCulture.Equals(currentCulture)) {
	            this.baseFileName = fileName;
	            this.culture = CultureInfo.InvariantCulture;
	        } else {
	            // Remove the culture sensitive parts (ergo, change example.en-US.txt to example.txt)
	            string realExtension = Path.GetExtension(fileName);
	            string myBaseFileName = Path.ChangeExtension(fileName, null);
	            myBaseFileName = Path.ChangeExtension(myBaseFileName, null) + realExtension;

	            this.baseFileName = myBaseFileName;
	            this.culture = currentCulture;
	        }
	    }

	    #endregion

	    #region [ 'StreamContext' overrides. ]

	    public override bool IsLocal { get { return true; } }
	    public override CultureInfo Culture { get { return this.culture; } }
	    public override Uri Uri { get { return new Uri(FileSystem.GetNativeLanguageFile(this.baseFileName, this.culture, NativeLanguageStyle.Name)); } }

        public override Encoding Encoding {
            get { return this.encoding; }
        }

	    public override StreamContext ChangePath(string relativePath) {
	        string path =
	            Environment.ExpandEnvironmentVariables(
	                Path.Combine(Path.GetDirectoryName(this.baseFileName), relativePath));
	        return new FileSystemStreamContext(path);
	    }

	    public override StreamContext ChangeCulture(CultureInfo resourceCulture) {
	        return new FileSystemStreamContext(this.baseFileName, resourceCulture ?? CultureInfo.InvariantCulture, this.Encoding);
	    }

        public override StreamContext ChangeEncoding(Encoding encoding) {
	        return new FileSystemStreamContext(this.baseFileName, this.Culture, encoding ?? this.encoding);
        }

	    protected override Stream GetStreamCore(FileAccess fileAccess) {
	        // Find the appropriate culture sensitive path
	        string realPath = FileSystem.FindBestNativeLanguageFile(this.baseFileName, this.culture);
	        FileStream result = null;

	        try {
	            switch (fileAccess) {
	                case FileAccess.Read:
	                    bool exists = File.Exists(realPath);
	                    if (exists) {
	                        result = new FileStream(realPath, FileMode.Open, FileAccess.Read);

	                        return result;
	                    }
	                    break;
	                case FileAccess.Write:
	                    result = new FileStream(realPath, FileMode.Create, FileAccess.Write);

	                    return result;

	                case FileAccess.ReadWrite:
	                default:
	                    result = new FileStream(realPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);

	                    return result;
	            }

	        } catch (IOException) {

	        } catch (SecurityException) {
	        }

	        return null;
	    }

	    #endregion

	    #region [ 'object' overrides. ]

	    public override int GetHashCode() {
	        int hashCode = 0;
	        unchecked {
	            if (baseFileName != null) hashCode += 1000000007 * baseFileName.GetHashCode();
	            if (culture != null) hashCode += 1000000009 * culture.GetHashCode();
	        }
	        return hashCode;
	    }

	    public override bool Equals(object obj) {
	        FileSystemStreamContext other = obj as FileSystemStreamContext;
	        if (null == other) {
	            return false;
	        } else {
	            string thisFileName = Path.GetFullPath(Environment.ExpandEnvironmentVariables(this.baseFileName));
	            string otherFileName = Path.GetFullPath(Environment.ExpandEnvironmentVariables(other.baseFileName));

	            return thisFileName == otherFileName
	                && object.Equals(this.culture, other.culture);
	        }
	    }

	    #endregion

	}
}
