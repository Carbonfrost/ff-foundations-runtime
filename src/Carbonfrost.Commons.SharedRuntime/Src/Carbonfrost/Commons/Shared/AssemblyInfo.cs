//
// - AssemblyInfo.cs -
//
// Copyright 2005, 2006, 2010, 2012 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Carbonfrost.Commons.ComponentModel.Annotations;
using Carbonfrost.Commons.Shared;
using Carbonfrost.Commons.Shared.Runtime;
using Carbonfrost.Commons.Shared.Runtime.Components;

namespace Carbonfrost.Commons.Shared {

    [Serializable]
    [RuntimeComponentUsage(Name = ComponentTypes.Assembly)]
    public sealed partial class AssemblyInfo : MarshalByRefObject, IRuntimeComponent, IAdaptable {

        const string PROP_SOURCE_ATTACHMENT = "sourceAttachment";
        const string PROP_PRIVATE_KEY = "privateKey";
        const string PROP_LICENSE = "license";
        const string PROP_BASE = "base";
        const string PROP_DEVELOPMENT = "development";
        const string PROP_URL = "url";

        private readonly SortedList<string, NamespaceUri> xmlns = new SortedList<string, NamespaceUri>();
        private readonly IDictionary<NamespaceUri, string> xmlnsPrefixes
            = new Dictionary<NamespaceUri, string>();
        private static readonly IDictionary<Assembly, AssemblyInfo> map = new Dictionary<Assembly, AssemblyInfo>();
        private readonly Assembly assembly;
        private Assembly _extensionAssembly;
        private Assembly _developerAssembly;
        private bool initExtension;
        private readonly ICustomAttributeProvider attributes;
        private readonly IProperties properties;
        private readonly Lazy<ComponentCollection> dependencies;
        private readonly SharedRuntimeOptionsAttribute options;

        internal static readonly IEnumerable<AssemblyName> ALL;

        // Need this comparer because reference comparisons would otherwise be used
        private static readonly HashSet<AssemblyName> pendingAssemblyNames
            = new HashSet<AssemblyName>(new AssemblyNameComparer());

        private static readonly List<IAssemblyInfoFilter> staticFilters;

        public IProperties Properties { get { return properties; } }

        public Uri SourceAttachment {
            get { return GetUriVia(PROP_SOURCE_ATTACHMENT); }
            internal set { this.Properties.SetProperty(PROP_SOURCE_ATTACHMENT, value); }
        }

        public Uri Url {
            get { return GetUriVia(PROP_URL); }
            internal set { this.Properties.SetProperty(PROP_URL, value); }
        }

        public Uri PrivateKey {
            get { return GetUriVia(PROP_PRIVATE_KEY); }
            internal set { this.Properties.SetProperty(PROP_PRIVATE_KEY, value); }
        }

        public Uri License {
            get { return GetUriVia(PROP_LICENSE); }
            internal set { this.Properties.SetProperty(PROP_LICENSE, value); }
        }

        public Assembly ExtensionAssembly {
            get {
                EnsureExtensionAssembly();
                return _extensionAssembly;
            }
        }

        public Assembly DeveloperAssembly {
            get {
                EnsureExtensionAssembly();
                return _developerAssembly;
            }
        }

        internal bool ScanForTemplates {
            get { return Scannable && options.Templates; }
        }

        internal bool ScanForAdapters {
            get { return Scannable && options.Adapters; }
        }

        internal bool ScanForProviders {
            get { return Scannable && options.Providers; }
        }

        internal bool Scannable {
            get; private set;
        }

        public Uri Base {
            get {
                object o = this.Properties.GetProperty(PROP_BASE);
                Uri u = o as Uri;
                if (o == null)
                    return new Uri(this.Assembly.CodeBase);

                else if (u != null)
                    return u;

                else
                    return new Uri(Convert.ToString(o));
            }
            internal set { this.Properties.SetProperty(PROP_BASE, value); }
        }

        public bool IsDevelopment {
            get { return this.Properties.GetBoolean(PROP_DEVELOPMENT); }
            internal set { this.Properties.SetProperty(PROP_DEVELOPMENT, value); }
        }

        public Assembly Assembly { get { return assembly; } }

        public string DefaultManifestResource {
            get { return BuildManifestStreamName(this.assembly); }
        }

        static AssemblyInfo() {
            staticFilters = new List<IAssemblyInfoFilter>();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                pendingAssemblyNames.Add(asm.GetName());
            }

            ALL = AppDomain.CurrentDomain.DescribeAssemblies(
                a => new [] { a.GetName() });
        }

        private AssemblyInfo(Assembly a) {
            this.assembly = a;
            this.properties = new Properties(AssemblyComponentLoader.EnumerateMetadata(a));
            this.Base = new Uri(a.CodeBase);
            this.dependencies = new Lazy<ComponentCollection>(
                () => (new ComponentCollection(
                    this.Assembly.GetReferencedAssemblies().Select(t => Component.Assembly(t)))));

            if (a.ReflectionOnly)
                this.attributes = new ReflectOnlyAssemblyAttributeProvider(a);
            else
                this.attributes = a;

            var baseAttribute = CustomAttributeProvider.GetCustomAttribute<BaseAttribute>(this.attributes, false);
            if (baseAttribute != null) {
                this.Base = baseAttribute.Source;
            }

            var ua = CustomAttributeProvider.GetCustomAttribute<UrlAttribute>(this.attributes, false);
            if (ua != null) {
                this.Url = ua.Url;
            }

            var sc = CustomAttributeProvider.GetCustomAttribute<SharedRuntimeOptionsAttribute>(this.attributes, false);
            this.options = sc ?? SharedRuntimeOptionsAttribute.Default;
            this.Scannable = Utility.IsScannableAssembly(a);
        }

        public IEnumerable<string> GetNamespaces(string pattern) {
            return Utility.FilterNamespaces(this.Assembly.GetNamespaces(), pattern);
        }

        public Stream GetManifestResourceStream(string name = null) {
            if (string.IsNullOrEmpty(name))
                name = this.DefaultManifestResource;

            return this.assembly.GetManifestResourceStream(name);
        }

        public static AssemblyInfo GetAssemblyInfo(Assembly assembly) {
            if (assembly == null)
                throw new ArgumentNullException("assembly"); // $NON-NLS-1

            AssemblyInfo result;
            if (!map.TryGetValue(assembly, out result)) {
                result = map[assembly] = new AssemblyInfo(assembly);

                var filters = result.attributes.GetCustomAttributes(false).OfType<IAssemblyInfoFilter>()
                    .Concat(AssemblyInfo.staticFilters);

                foreach (IAssemblyInfoFilter filter in filters) {
                    try {
                        filter.ApplyToAssembly(result);
                    } catch (Exception ex) {
                        if (Require.IsCriticalException(ex))
                            throw;

                        IStatusAppender sa = StatusAppender.ForType(typeof(AssemblyInfo));
                        RuntimeWarning.AssemblyInfoFilterFailed(sa, assembly.GetName(), filter.GetType(), ex);
                    }
                }
                AddReferencedAssemblies(assembly);
            }

            return result;
        }

        public IEnumerable<string> GetClrNamespaces(NamespaceUri namespaceUri) {
            if (namespaceUri == null)
                throw new ArgumentNullException("namespaceUri"); // $NON-NLS-1

            return xmlns.Where(t => namespaceUri.Equals(t.Value)).Select(t => t.Key);
        }

        public string GetClrNamespace(NamespaceUri namespaceUri) {
            if (namespaceUri == null)
                throw new ArgumentNullException("namespaceUri"); // $NON-NLS-1

            return GetClrNamespaces(namespaceUri).SingleOrThrow(RuntimeFailure.MultipleNamespaces);
        }

        public NamespaceUri GetXmlNamespace(string clrNamespace) {
            NamespaceUri ns;
            if (xmlns.TryGetValue(clrNamespace ?? string.Empty, out ns))
                return ns;
            else
                return null;
        }

        public string GetXmlNamespacePrefix(NamespaceUri namespaceUri) {
            if (namespaceUri == null)
                throw new ArgumentNullException("namespaceUri");

            return this.xmlnsPrefixes.GetValueOrDefault(namespaceUri, string.Empty);
        }

        private Uri GetUri(string relativeUri) {
            if (this.Base == null)
                return new Uri(relativeUri);
            else
                return new Uri(this.Base, relativeUri);
        }

        internal void AddXmlns(string prefix, string clrNamespacePattern, Uri xmlns) {
            NamespaceUri nu = NamespaceUri.Create(xmlns);

            var allNamespaces = AllNamespaces();
            foreach (var m in Utility.FilterNamespaces(allNamespaces, clrNamespacePattern)) {
                this.xmlns.Add(m ?? string.Empty, nu);
            }

            if (!string.IsNullOrEmpty(prefix))
                this.xmlnsPrefixes.Add(nu, prefix);
        }

        static string BuildManifestStreamName(Assembly a) {
            string name = a.ManifestModule.Name;
            if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) // $NON-NLS-1
                || name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) { // $NON-NLS-1
                name = name.Substring(0, name.Length - 4);
            }
            return name + ".g"; // $NON-NLS-1
        }

        private void EnsureExtensionAssembly() {
            if (!initExtension) {
                try {
                    _extensionAssembly = Adaptable.GetExtensionAssembly(this.Assembly);
                } catch (FileNotFoundException) {
                } catch (FileLoadException) {
                } catch (BadImageFormatException) {
                }

                try {

                    _developerAssembly = Adaptable.GetDeveloperAssembly(this.Assembly);
                } catch (FileNotFoundException) {
                } catch (FileLoadException) {
                } catch (BadImageFormatException) {
                }

                initExtension = true;
            }
        }

        private string[] AllNamespaces() {
            return this.assembly.GetTypesHelper().Select(t => t.Namespace).Distinct().ToArray();
        }

        private Uri GetUriVia(string key) {
            object s = this.Properties.GetProperty(key);
            Uri u = s as Uri;
            if (u == null)
                return new Uri(this.Base, Convert.ToString(s));
            else
                return new Uri(this.Base, u);
        }

        internal static void CompleteAppDomain() {
            while (ContinueAppDomain())
                ;
        }

        private static void AddReferencedAssemblies(Assembly assembly) {
            foreach (AssemblyName m in assembly.GetReferencedAssemblies()) {
                pendingAssemblyNames.Add(m);
            }
        }

        internal static bool ContinueAppDomain() {
            while (pendingAssemblyNames.Count > 0) {
                AssemblyName any = pendingAssemblyNames.First();
                pendingAssemblyNames.Remove(any);
                Exception error = null;

                try {
                    AssemblyInfo.GetAssemblyInfo(AppDomain.CurrentDomain.Load(any));
                    break;

                } catch (FileNotFoundException ex) {
                    error = ex;
                } catch (FileLoadException ex) {
                    error = ex;
                } catch (BadImageFormatException ex) {
                    error = ex;
                }

                IStatusAppender sa = StatusAppender.ForType(typeof(AssemblyInfo));
                sa.AppendError(error);
            }

            return pendingAssemblyNames.Count > 0;
        }

        internal static bool AppDomainInSync() {
            return pendingAssemblyNames.Count == 0;
        }

        internal static void AddStaticFilter(IAssemblyInfoFilter filter) {
            staticFilters.Add(filter);

            foreach (var assemblyInfo in map.Values) {
                filter.ApplyToAssembly(assemblyInfo);
            }
        }

        sealed class AssemblyNameComparer : IEqualityComparer<AssemblyName> {

            public bool Equals(AssemblyName x, AssemblyName y) {
                return x.FullName.Equals(y.FullName);
            }

            public int GetHashCode(AssemblyName obj) {
                return obj.FullName.GetHashCode();
            }
        }

        // IRuntimeComponent implementation
        Uri IRuntimeComponent.Source {
            get { return this.Base; } }

        string IRuntimeComponent.ComponentType {
            get { return ComponentTypes.Assembly; } }

        ComponentName IRuntimeComponent.ComponentName {
            get {
                return ComponentName.FromAssemblyName(this.Assembly.GetName());
            }
        }

        ComponentCollection IRuntimeComponent.Dependencies {
            get {
                return dependencies.Value;
            }
        }

        internal IProviderRegistration GetProviderRegistration() {
            if (!ScanForProviders)
                return ProviderRegistration.None;

            var sc = Attribute.GetCustomAttribute(this.assembly, typeof(ProviderRegistrationAttribute)) as ProviderRegistrationAttribute
                ?? ProviderRegistrationAttribute.Default;

            return sc.Registration;
        }

        public object GetAdapter(Type targetType) {
            if (targetType == null)
                throw new ArgumentNullException("targetType");
            if (typeof(Assembly).Equals(targetType))
                return this.Assembly;
            if (typeof(IRuntimeComponent).Equals(targetType))
                return this;
            if (typeof(IProviderRegistration).Equals(targetType))
                return this.GetProviderRegistration();

            return null;
        }
    }

}
