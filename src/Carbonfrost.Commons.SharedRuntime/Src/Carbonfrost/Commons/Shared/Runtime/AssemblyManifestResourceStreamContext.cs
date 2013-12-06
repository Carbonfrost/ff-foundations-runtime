//
// - AssemblyManifestResourceStreamContext.cs -
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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Carbonfrost.Commons.Shared.Runtime {

	internal sealed class AssemblyManifestResourceStreamContext : StreamContext {

	    private readonly CultureInfo culture;
	    private readonly string resourceName;
	    private readonly Assembly assembly;

	    public AssemblyManifestResourceStreamContext(Assembly assembly, string resourceName)
	        : this(assembly, resourceName, null)
	    {
	    }

	    public AssemblyManifestResourceStreamContext(
	        Assembly assembly, string resourceName,
	        CultureInfo culture) {

	        if (resourceName == null) { throw new ArgumentNullException("resourceName"); } // $NON-NLS-1
	        if (assembly == null) { throw new ArgumentNullException("assembly"); } // $NON-NLS-1

	        this.culture = culture ?? CultureInfo.InvariantCulture;
	        this.resourceName = resourceName;
	        this.assembly = assembly;
	    }

	    internal static StreamContext CreateResFromUri(Uri uri) {
	        // res://http:,,localhost,MyAssembly.dll/.#images/splash.png
	        string resourceName = uri.Fragment; // Includes the '#' sign
	        string assemblyPath = uri.AbsolutePath;

	        // Replace / in assembly name
	        assemblyPath = assemblyPath.Replace(',', '/');
	        Assembly assembly = null;

	        if (assemblyPath == ".") { // $NON-NLS-1
	            assembly = Assembly.GetCallingAssembly();
	        } else {
	            assembly = Assembly.LoadFrom(assemblyPath);
	        }

	        if (resourceName.Length > 1) {
	            resourceName= resourceName.Substring(1);
	        }

	        return FromAssemblyManifestResource(assembly, resourceName);
	    }

	    // StreamContext overrides
	    public override CultureInfo Culture { get { return this.culture; } }

	    public override Uri Uri {
	        get {
	    		return new Uri("res://" + assembly.CodeBase.Replace('/', ',') + "#" + resourceName); // $NON-NLS-1, // $NON-NLS-2
	        }
	    }

	    public override StreamContext ChangePath(string relativePath) {
	        return new AssemblyManifestResourceStreamContext(this.assembly, relativePath, this.culture);
	    }

	    public override StreamContext ChangeCulture(CultureInfo resourceCulture) {
	        return new AssemblyManifestResourceStreamContext(this.assembly, this.resourceName, this.culture);
	    }

	    protected override Stream GetStreamCore(FileAccess access) {
	        try {
	            Assembly realAssembly = assembly.GetSatelliteAssembly(this.culture);
	            return realAssembly.GetManifestResourceStream(this.resourceName);

	        } catch (BadImageFormatException) {
	            return null;

	        } catch (IOException) {
	            return null;
	        }

	    }

	}
}
