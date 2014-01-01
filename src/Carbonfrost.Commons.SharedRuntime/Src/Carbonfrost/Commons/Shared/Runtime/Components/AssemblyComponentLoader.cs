//
// - AssemblyComponentLoader.cs -
//
// Copyright 2013 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using ReflectionAssembly = System.Reflection.Assembly;

namespace Carbonfrost.Commons.Shared.Runtime.Components {

    class AssemblyComponentLoader : RuntimeComponentLoader {

        public override IRuntimeComponent ReflectionOnlyLoad(string componentType, Uri source) {
            return LoadCore(componentType, source, ReflectionAssembly.ReflectionOnlyLoadFrom);
        }

        public override IRuntimeComponent Load(string componentType, Uri source) {
            return LoadCore(componentType,
                            source,
                            s => (AppDomain.CurrentDomain.Load(File.ReadAllBytes(s))));
        }

        public override IPropertyStore LoadMetadata(string componentType, Uri source) {
            var am = (AssemblyInfo) Load(componentType, source);
            return am.Properties;
        }

        static AssemblyInfo LoadCore(string componentType, Uri source, Func<string, ReflectionAssembly> func) {
            string path = source.IsAbsoluteUri ? source.LocalPath : source.ToString();

            if (componentType == null)
                throw new ArgumentNullException("componentType");
            if (componentType.Length == 0)
                throw Failure.EmptyString("componentType");
            if (source == null)
                throw new ArgumentNullException("source");

            if (componentType != ComponentTypes.Assembly)
                return null;

            if (source.IsAbsoluteUri && source.Scheme != "file")
                throw RuntimeFailure.AssemblyComponentCanOnlyLoadLocalFile("source");

            return AssemblyInfo.GetAssemblyInfo(func(path));
        }

        internal static IEnumerable<KeyValuePair<string, object>> EnumerateMetadata(Assembly assembly) {
            ComponentName cname = ComponentName.FromAssemblyName(assembly.GetName());

            var data = assembly.GetCustomAttributesData();
            TargetFrameworkAttribute targetAttr = null;
            AssemblyConfigurationAttribute configAttr = null;
            AssemblyFileVersionAttribute fileAttr = null;

            foreach (var m in data) {
                var decl = m.Constructor.DeclaringType;
                if (decl == typeof(TargetFrameworkAttribute))
                    targetAttr = new TargetFrameworkAttribute((string) m.ConstructorArguments[0].Value);

                else if (decl == typeof(AssemblyConfigurationAttribute))
                    configAttr = new AssemblyConfigurationAttribute((string) m.ConstructorArguments[0].Value);

                else if (decl == typeof(AssemblyFileVersionAttribute))
                    fileAttr = new AssemblyFileVersionAttribute((string) m.ConstructorArguments[0].Value);
            }

            FrameworkName target = GetFrameworkName(assembly, targetAttr);
            string config = GetConfiguration(configAttr);
            string platform = GetPlatform(assembly);
            Version version = assembly.GetName().Version;

            return Properties.FromValue(new {
                                            name = cname.Name,
                                            configuration = config,
                                            assemblyName = cname,
                                            platform = platform,
                                            targetFramework = target,
                                            version = version, });
        }

        public static FrameworkName GetFrameworkName(Assembly assembly, TargetFrameworkAttribute target) {
            if (target == null) {
                Match match = Regex.Match(assembly.ImageRuntimeVersion, @"v(?<Version>\d+(\.\d+)+)");
                Version version = Version.Parse(match.Groups["Version"].Value);
                return new FrameworkName(".NETFramework", version);

            } else {
                return new FrameworkName(target.FrameworkName);

            }
        }

        private static string GetConfiguration(AssemblyConfigurationAttribute target) {
            // TODO Might be other ways to detect this using informational version, symbols
            if (target == null)
                return "release";
            else
                return (target.Configuration ?? string.Empty).ToLowerInvariant();
        }

        private static string GetPlatform(Assembly assembly) {
            // TODO Might be other ways to detect this using informational version
            Module module = assembly.GetModules(false).FirstOrDefault();
            if (module == null)
                return "anycpu";

            PortableExecutableKinds pe;
            ImageFileMachine machine;
            module.GetPEKind(out pe, out machine);

            if (pe.HasFlag(PortableExecutableKinds.ILOnly))
                return "anycpu";
            if (pe.HasFlag(PortableExecutableKinds.Unmanaged32Bit))
                return "x86";

            switch (machine) {
                case ImageFileMachine.I386:
                    return "x86";
                case ImageFileMachine.IA64:
                case ImageFileMachine.AMD64:
                    return "x64";

                default:
                    return machine.ToString().ToLowerInvariant();
            }
        }
    }
}
