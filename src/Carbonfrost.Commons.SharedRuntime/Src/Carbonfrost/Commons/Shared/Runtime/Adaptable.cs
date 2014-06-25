//
// - Adaptable.cs -
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Carbonfrost.Commons.ComponentModel.Annotations;
using Carbonfrost.Commons.Shared.Runtime.Components;

namespace Carbonfrost.Commons.Shared.Runtime {

    public static partial class Adaptable {

        const string DEVELOPER = "Developer";

        static readonly WeakCache<IActivationProvider> activationProviderCache = new WeakCache<IActivationProvider>(MakeActivationProvider);

        public static Type GetConcreteClass(this PropertyDescriptor property, IServiceProvider serviceProvider = null) {
            if (property == null)
                throw new ArgumentNullException("property"); // $NON-NLS-1

            var cca = GetConcreteClassProvider(property);
            Type type = property.PropertyType;
            if (cca == null)
                return (type.IsAbstract || type.IsInterface) ? null : type;
            else
                return cca.GetConcreteClass(property.PropertyType, serviceProvider);
        }

        public static Type GetConcreteClass(this Type type, IServiceProvider serviceProvider = null) {
            if (type == null)
                throw new ArgumentNullException("type"); // $NON-NLS-1

            var cca = GetConcreteClassProvider(type);
            if (cca == null)
                return (type.IsAbstract || type.IsInterface) ? null : type;
            else
                return cca.GetConcreteClass(type, serviceProvider);
        }

        public static IConcreteClassProvider GetConcreteClassProvider(this Type type) {
            if (type == null)
                throw new ArgumentNullException("type"); // $NON-NLS-1

            return type.GetCustomAttributes(false).OfType<IConcreteClassProvider>().FirstOrDefault();
        }

        public static IConcreteClassProvider GetConcreteClassProvider(this PropertyDescriptor property) {
            if (property == null)
                throw new ArgumentNullException("property");

            return property.Attributes.OfType<IConcreteClassProvider>().FirstOrDefault();
        }

        public static AttributeUsageAttribute GetAttributeUsage(this Type attributeType) {
            if (attributeType == null)
                throw new ArgumentNullException("attributeType");

            Require.SubclassOf("attributeType", attributeType, typeof(Attribute));
            return (AttributeUsageAttribute) Attribute.GetCustomAttribute(attributeType, typeof(AttributeUsageAttribute))
                ?? new AttributeUsageAttribute(AttributeTargets.All);
        }

        public static StreamingSourceUsageAttribute GetStreamingSourceUsage(this Type streamingSourceType) {
            if (streamingSourceType == null)
                throw new ArgumentNullException("streamingSourceType");

            Require.SubclassOf("streamingSourceType", streamingSourceType, typeof(StreamingSource));
            return (StreamingSourceUsageAttribute) streamingSourceType.GetAttribute<StreamingSourceUsageAttribute>();
        }

        public static ConstructorInfo GetActivationConstructor(this Type type) {
            if (type == null)
                throw new ArgumentNullException("type"); // $NON-NLS-1

            ConstructorInfo[] ci = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (ci.Length == 0)
                return null;
            else
                return ci.FirstOrDefault(IsActivationConstructor) ?? ci[0];
        }

        public static IActivationProvider[] GetActivationProviders(this PropertyDescriptor property) {
            if (property == null)
                throw new ArgumentNullException("property"); // $NON-NLS-1

            return property.Attributes.OfType<IActivationProvider>().ToArray();
        }

        public static IActivationProvider[] GetActivationProviders(this Type type) {
            if (type == null)
                throw new ArgumentNullException("type");

            // Each interface is considered to see if it has an activation provider; the type's
            // attributes are also considered to see if they are IActivationProvider or define
            // an activation provider
            List<IActivationProvider> results = new List<IActivationProvider>();
            HashSet<Type> e = new HashSet<Type>(
                type.GetInterfaces()
                .Select(i => i.GetActivationProviderType())
                .WhereNotNull());

            foreach (var f in type.GetCustomAttributes(true)) {
                IActivationProvider ap = f as IActivationProvider;
                if (ap != null) {
                    results.Add(ap);
                } else
                    e.AddIfNotNull(f.GetType().GetActivationProviderType());
            }
            results.AddRange(activationProviderCache.GetAll(e));

            return results.ToArray();
        }

        public static SharedRuntimeOptionsAttribute GetSharedRuntimeOptions(
            this Assembly assembly) {

            if (assembly == null)
                throw new ArgumentNullException("assembly");

            var attr = assembly.GetAttribute<SharedRuntimeOptionsAttribute>();
            if (attr == null) {

                // Optimizations for system assemblies
                if (Utility.IsScannableAssembly(assembly))
                    return SharedRuntimeOptionsAttribute.Default;
                else
                    return SharedRuntimeOptionsAttribute.Optimized;

            } else {
                return attr;
            }
        }

        public static IEnumerable<Type> FilterTypes(
            this AppDomain appDomain, Func<Type, bool> predicate) {

            if (appDomain == null)
                throw new ArgumentNullException("appDomain");

            return DescribeTypes(
                appDomain, a => (predicate(a) ? new Type[] { a } : null));
        }

        public static IEnumerable<Type> GetTypesByNamespaceUri(
            this Assembly assembly, NamespaceUri namespaceUri) {

            if (assembly == null)
                throw new ArgumentNullException("assembly");
            if (namespaceUri == null)
                throw new ArgumentNullException("namespaceUri");

            return assembly.GetTypesHelper().Where(t => t.GetQualifiedName().Namespace == namespaceUri);
        }

        public static IEnumerable<string> FilterNamespaces(
            this Assembly assembly, string namespacePattern) {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            return Utility.FilterNamespaces(assembly.GetNamespaces(), namespacePattern);
        }

        public static IEnumerable<Assembly> FilterAssemblies(
            this AppDomain appDomain, Func<Assembly, bool> predicate) {

            if (appDomain == null)
                throw new ArgumentNullException("appDomain");

            return DescribeAssemblies(
                appDomain, a => (predicate(a) ? new Assembly[] { a } : null));
        }

        // TODO Actually consider the appdomain in use
        public static IEnumerable<Assembly> DescribeAssemblies(
            this AppDomain appDomain) {

            if (appDomain == null)
                throw new ArgumentNullException("appDomain");

            return new Buffer<Assembly>(AssemblyBuffer.Instance);
        }

        public static IEnumerable<TValue> DescribeAssemblies<TValue>(
            this AppDomain appDomain, Func<Assembly, IEnumerable<TValue>> selector) {

            if (appDomain == null)
                throw new ArgumentNullException("appDomain");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return new Buffer<TValue>(
                AssemblyBuffer.Instance.SelectMany(t => selector(t) ?? Empty<TValue>.Array));
        }

        public static IDictionary<TKey, TValue> DescribeAssemblies<TKey, TValue>(
            this AppDomain appDomain, Func<Assembly, IEnumerable<KeyValuePair<TKey, TValue>>> selector)
        {
            if (appDomain == null)
                throw new ArgumentNullException("appDomain");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return LazyDescriptionDictionary<TKey, TValue>.Create(selector);
        }

        public static IEnumerable<TValue> DescribeTypes<TValue>(
            this AppDomain appDomain, Func<Type, IEnumerable<TValue>> selector)
        {
            if (appDomain == null)
                throw new ArgumentNullException("appDomain");
            if (selector == null)
                throw new ArgumentNullException("selector");

            var s = AssemblyThunk(selector);
            return new Buffer<TValue>(AssemblyBuffer.Instance.SelectMany(s));
        }

        public static IDictionary<TKey, TValue> DescribeTypes<TKey, TValue>(
            this AppDomain appDomain, Func<Type, IEnumerable<KeyValuePair<TKey, TValue>>> selector)
        {
            if (selector == null)
                throw new ArgumentNullException("selector");

            return LazyDescriptionDictionary<TKey, TValue>.Create(AssemblyThunk(selector));
        }

        static Func<Assembly, IEnumerable<TValue>> AssemblyThunk<TValue>(Func<Type, IEnumerable<TValue>> selector) {
            return (a) => (a.GetTypesHelper().SelectMany(selector) ?? Empty<TValue>.Array);
        }

        public static bool IsDefined<TAttribute>(this MemberInfo source, bool inherit = false)
            where TAttribute : Attribute
        {
            return source.GetAttribute<TAttribute>(inherit) != null;
        }

        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this MemberInfo source, bool inherit = false)
            where TAttribute : Attribute
        {
            // TODO Also obtain annotation attributes
            AttributeCompleteAppDomain(typeof(TAttribute));
            return Attribute.GetCustomAttributes(source, typeof(TAttribute), inherit).Cast<TAttribute>();
        }

        public static TAttribute GetAttribute<TAttribute>(this MemberInfo source, bool inherit = false)
            where TAttribute : Attribute
        {
            AttributeCompleteAppDomain(typeof(TAttribute));
            return Attribute.GetCustomAttribute(source, typeof(TAttribute), inherit) as TAttribute;
        }

        public static TAttribute GetAttribute<TAttribute>(this Assembly source)
            where TAttribute : Attribute
        {
            AttributeCompleteAppDomain(typeof(TAttribute));
            return Attribute.GetCustomAttribute(source, typeof(TAttribute), false) as TAttribute;
        }

        public static IEnumerable<MethodInfo> GetImplicitFilterMethods(this PropertyDescriptor property,
                                                                       Type attributeType) {
            if (property == null)
                throw new ArgumentNullException("property"); // $NON-NLS-1

            if (attributeType == null)
                throw new ArgumentNullException("attributeType"); // $NON-NLS-1

            // For instance, LocalizableAtrribute/Title ==>  GetLocalizableTitle
            string nakedName = Utility.GetImpliedName(attributeType, "Attribute");
            string methodName = string.Concat("Get", nakedName, property.Name);
            return property.ComponentType.GetMethods().Where(mi => mi.Name == methodName);
        }

        public static AssemblyName GetExtensionAssembly(this AssemblyName assemblyName) {
            return GetExtensionAssembly(assemblyName, null);
        }

        public static AssemblyName GetExtensionAssembly(this AssemblyName assemblyName, string extensionName) {
            if (assemblyName == null)
                throw new ArgumentNullException("assemblyName"); // $NON-NLS-1
            if (string.IsNullOrEmpty(extensionName))
                extensionName = "Extensions";

            // Carbonfrost.Commons.Membership.Extensions.dll
            ComponentNameBuilder builder = new ComponentNameBuilder(ComponentName.FromAssemblyName(assemblyName));
            Version v = assemblyName.Version;

            builder.Name += ("." + extensionName);
            builder.Culture = null;
            builder.Version = new Version(v.Major, v.Minor);

            return builder.Build().ToAssemblyName();
        }

        public static Assembly GetDeveloperAssembly(this Assembly assembly) {
            if (assembly == null)
                throw new ArgumentNullException("assembly"); // $NON-NLS-1

            return Assembly.Load(GetExtensionAssembly(assembly.GetName(), DEVELOPER));
        }

        public static Assembly GetExtensionAssembly(this Assembly assembly) {
            if (assembly == null)
                throw new ArgumentNullException("assembly"); // $NON-NLS-1

            return Assembly.Load(GetExtensionAssembly(assembly.GetName()));
        }

        public static Assembly GetExtensionAssembly(this Assembly assembly, string extensionName) {
            if (assembly == null)
                throw new ArgumentNullException("assembly"); // $NON-NLS-1

            return Assembly.Load(GetExtensionAssembly(assembly.GetName(), extensionName));
        }

        public static object GetInheritanceParent(this object instance) {
            if (instance == null)
                throw new ArgumentNullException("instance");

            IHierarchyObject ho = Adaptable.TryAdapt<IHierarchyObject>(instance);
            if (ho != null)
                return ho.ParentObject;

            PropertyInfo pi = instance.GetType().GetProperty("InheritanceParent");
            if (pi != null)
                return pi.GetValue(instance, null);

            return null;
        }

        public static object GetInheritanceAncestor(this object instance, Type ancestorType) {
            if (instance == null)
                throw new ArgumentNullException("instance");
            Require.ReferenceType("ancestorType", ancestorType);

            object result = instance.GetInheritanceParent();
            if (result == null)
                return null;

            if (ancestorType.IsInstanceOfType(instance))
                return result;

            return GetInheritanceAncestor(result, ancestorType);
        }

        public static T GetInheritanceAncestor<T>(this object instance, T defaultValue = null)
            where T : class {
            if (instance == null)
                throw new ArgumentNullException("instance");

            object result = instance.GetInheritanceParent();
            if (result == null)
                return defaultValue;

            T t = result as T;
            if (t != null)
                return t;

            return GetInheritanceAncestor<T>(result);
        }

        public static TypeReference GetExtensionImplementationType(
            this Type sourceType, string feature) {
            if (sourceType == null)
                throw new ArgumentNullException("sourceType"); // $NON-NLS-1

            if (string.IsNullOrWhiteSpace(feature))
                throw Failure.AllWhitespace("sourceType");

            return TypeReference.Parse(GetExtensionImplementation(sourceType, feature));
        }

        public static object Adapt(this object source, string adapterRoleName, IServiceProvider serviceProvider = null) {
            object result = TryAdapt(source, adapterRoleName, serviceProvider);
            if (result == null)
                throw Failure.NotAdaptableTo("source", source, adapterRoleName);
            else
                return result;
        }

        public static object Adapt(this object source, Type adapterType, IServiceProvider serviceProvider = null) {
            object result = TryAdapt(source, adapterType, serviceProvider);
            if (result == null)
                throw Failure.NotAdaptableTo("source", source, adapterType);
            else
                return result;
        }

        public static T Adapt<T>(this object source, IServiceProvider serviceProvider = null)
            where T: class
        {
            return (T) Adapt(source, typeof(T), serviceProvider);
        }

        public static object TryAdapt(this object source, string adapterRoleName, IServiceProvider serviceProvider = null) {
            if (source == null)
                throw new ArgumentNullException("source"); // $NON-NLS-1
            Require.NotNullOrEmptyString("adapterRoleName", adapterRoleName); // $NON-NLS-1

            object result = null;
            var dict = Adaptable.GetAdapterRoles(source.GetType(), true);
            Type[] candidates;
            if (dict.TryGetValue(adapterRoleName, out candidates)
                && candidates.Length > 0) {
                var pms = Properties.FromArray(source);
                result = Activation.CreateInstance(candidates[0], pms);
            }

            return result;
        }

        public static object TryAdapt(this object source, Type adapterType, IServiceProvider serviceProvider = null) {
            if (source == null)
                throw new ArgumentNullException("source"); // $NON-NLS-1
            if (adapterType == null)
                throw new ArgumentNullException("adapterType"); // $NON-NLS-1

            if (adapterType.IsInstanceOfType(source))
                return source;

            object result = null;
            IAdaptable a = source as IAdaptable;
            if (a != null) result = a.GetAdapter(adapterType);

            return result;
        }

        public static T TryAdapt<T>(this object source, IServiceProvider serviceProvider = null)
            where T: class
        {
            return (T) TryAdapt(source, typeof(T), serviceProvider);
        }

        public static Uri GetComponentUri(this Assembly assembly) {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            Uri uriIfAny = null;
            if (assembly.CodeBase != null)
                uriIfAny = new Uri(assembly.CodeBase);

            return Components.Component.Assembly(assembly.GetName(), uriIfAny).ToUri();
        }

        public static string[] GetAdapterRoleNames(this Type adapteeType) {
            if (adapteeType == null)
                throw new ArgumentNullException("adapteeType"); // $NON-NLS-1

            return GetAdapterRoleNames(adapteeType, true);
        }

        public static string[] GetAdapterRoleNames(this Type adapteeType, bool inherit) {
            if (adapteeType == null)
                throw new ArgumentNullException("adapteeType");

            return GetAdapterRoles(adapteeType, inherit).Keys.ToArray();
        }

        public static IDictionary<string, Type[]> GetAdapterRoles(this Type adapteeType) {
            return GetAdapterRoles(adapteeType, true);
        }

        public static IDictionary<string, Type[]> GetAdapterRoles(this Type adapteeType, bool inherit) {
            if (adapteeType == null)
                throw new ArgumentNullException("adapteeType");

            AdapterAttribute[] items = (AdapterAttribute[]) adapteeType.GetCustomAttributes(typeof(AdapterAttribute), inherit);
            var lookup = items.ToLookup(t => t.Role, t => t.AdapterType);
            return lookup.ToDictionary(t => t.Key, t => t.ToArray());
        }

        public static Type GetAdapterType(this Type adapteeType, string adapterRoleName) {
            return _GetAdapterTypes(adapteeType, adapterRoleName, false).FirstOrDefault();
        }

        public static Type GetAdapterType(this Type adapteeType, string adapterRoleName, bool inherit) {
            return _GetAdapterTypes(adapteeType, adapterRoleName, inherit).FirstOrDefault();
        }

        public static Type[] GetAdapterTypes(this Type adapteeType, string adapterRoleName) {
            return _GetAdapterTypes(adapteeType, adapterRoleName, false).ToArray();
        }

        public static Type[] GetAdapterTypes(this Type adapteeType, string adapterRoleName, bool inherit) {
            return _GetAdapterTypes(adapteeType, adapterRoleName, inherit).ToArray();
        }

        public static IEnumerable<string> GetComponentTypes(this AppDomain appDomain) {
            if (appDomain == null)
                throw new ArgumentNullException("appDomain");

            return appDomain.GetProviderNames(typeof(IRuntimeComponent))
                .Select(t => t.LocalName);
        }

        public static Type GetImplicitAdapterType(this Type adapteeType, string adapterRoleName) {
            if (adapteeType == null)
                throw new ArgumentNullException("adapteeType"); // $NON-NLS-1
            Require.NotNullOrAllWhitespace("adapterRoleName", adapterRoleName);

            Type result = adapteeType.Assembly.GetType(
                adapteeType.FullName + adapterRoleName);

            if (result != null && IsValidAdapter(result, adapterRoleName))
                return result;
            else
                return null;
        }

        public static Type GetImplicitBuilderType(this Type adapteeType) {
            if (adapteeType == null)
                throw new ArgumentNullException("adapteeType"); // $NON-NLS-1

            return GetImplicitAdapterType(adapteeType, AdapterRole.Builder);
        }

        public static Type GetImplicitStreamingSourceType(this Type adapteeType) {
            if (adapteeType == null)
                throw new ArgumentNullException("adapteeType"); // $NON-NLS-1

            return GetImplicitAdapterType(adapteeType, AdapterRole.StreamingSource);
        }

        // N.B. These are needed because C# cannot infer Func<..> in most cases
        public static Func<TResult> CreateAdapterFunction<TResult>(this object instance,
                                                                   string name,
                                                                   Expression<Func<TResult>> signature) {
            return CreateAdapterFunction<Func<TResult>>(instance, name, signature);
        }

        public static Func<T, TResult> CreateAdapterFunction<T, TResult>(this object instance,
                                                                         string name,
                                                                         Expression<Func<T, TResult>> signature) {
            return CreateAdapterFunction<Func<T, TResult>>(instance, name, signature);
        }

        public static Func<T1, T2, TResult> CreateAdapterFunction<T1, T2, TResult>(this object instance,
                                                                                   string name,
                                                                                   Expression<Func<T1, T2, TResult>> signature) {
            return CreateAdapterFunction<Func<T1, T2, TResult>>(instance, name, signature);
        }

        public static Func<T1, T2, T3, TResult> CreateAdapterFunction<T1, T2, T3, TResult>(this object instance,
                                                                                           string name,
                                                                                           Expression<Func<T1, T2, T3, TResult>> signature) {
            return CreateAdapterFunction<Func<T1, T2, T3, TResult>>(instance, name, signature);
        }

        public static TDelegate CreateAdapterFunction<TDelegate>(this object instance,
                                                                 string methodName,
                                                                 Expression<TDelegate> signature)
            where TDelegate : class
        {
            if (instance == null)
                throw new ArgumentNullException("instance"); // $NON-NLS-1

            MethodInfo mi = GetMethodBySignature<TDelegate>(instance.GetType(), methodName, signature);
            if (mi == null)
                return null;

            return (TDelegate) ((object) Delegate.CreateDelegate(typeof(TDelegate), instance, mi));
        }

        public static MethodInfo GetMethodBySignature<TDelegate>(this Type instanceType, string name, Expression<TDelegate> signature)
            where TDelegate : class
        {
            return GetMethodBySignatureCore<TDelegate>(
                instanceType,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                name,
                signature);
        }

        public static MethodInfo GetStaticMethodBySignature<TDelegate>(this Type instanceType, string name, Expression<TDelegate> signature)
            where TDelegate : class
        {
            return GetMethodBySignatureCore<TDelegate>(
                instanceType,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly,
                name,
                signature);
        }

        static MethodInfo GetMethodBySignatureCore<TDelegate>(Type instanceType,
                                                              BindingFlags flags,
                                                              string name,
                                                              Expression<TDelegate> signature)
            where TDelegate : class
        {
            if (instanceType == null)
                throw new ArgumentNullException("instanceType"); // $NON-NLS-1
            if (name == null)
                throw new ArgumentNullException("name"); // $NON-NLS-1

            if (name.Length == 0)
                throw Failure.EmptyString("name");

            Type[] argTypes = signature.Parameters.Select(p => p.Type).ToArray();
            MethodInfo mi = instanceType.GetMethod(name, flags, null, argTypes, null);
            if (mi == null)
                return null;
            if (signature.ReturnType == null)
                return mi.ReturnType == null ? mi : null;
            if (signature.ReturnType.IsAssignableFrom(mi.ReturnType))
                return mi;
            else
                return null;
        }

        public static Type GetBuilderType(this Type adapteeType) {
            if (adapteeType == null)
                throw new ArgumentNullException("adapteeType"); // $NON-NLS-1

            return Adaptable.GetAdapterType(adapteeType, AdapterRole.Builder);
        }

        public static Type GetStreamingSourceType(this Type adapteeType) {
            if (adapteeType == null)
                throw new ArgumentNullException("adapteeType"); // $NON-NLS-1

            return Adaptable.GetAdapterType(adapteeType, AdapterRole.StreamingSource);
        }

        public static Type GetActivationProviderType(this Type adapteeType) {
            if (adapteeType == null)
                throw new ArgumentNullException("adapteeType"); // $NON-NLS-1

            if (typeof(IActivationProvider).IsAssignableFrom(adapteeType))
                return adapteeType;

            return GetAdapterType(adapteeType, AdapterRole.ActivationProvider);
        }

        public static IEnumerable<Uri> GetStandards(this MemberDescriptor source) {
            if (source == null)
                throw new ArgumentNullException("source"); // $NON-NLS-1

            var attrs = source.Attributes.OfType<StandardsCompliantAttribute>();
            return SelectUris(attrs, GetAssemblyContext(source));
        }

        public static IEnumerable<Uri> GetStandards(this MemberInfo source) {
            if (source == null)
                throw new ArgumentNullException("source"); // $NON-NLS-1

            return GetStandardsCore(source, source.ReflectedType.Assembly);
        }

        public static IEnumerable<Uri> GetStandards(this Module source) {
            if (source == null)
                throw new ArgumentNullException("source"); // $NON-NLS-1

            return GetStandardsCore(source, source.Assembly);
        }

        public static IEnumerable<Uri> GetStandards(this Assembly source) {
            if (source == null)
                throw new ArgumentNullException("source"); // $NON-NLS-1

            return GetStandardsCore(source, source);
        }

        public static IEnumerable<Uri> GetStandards(this ParameterInfo source) {
            if (source == null)
                throw new ArgumentNullException("source"); // $NON-NLS-1

            var attrs = source.GetCustomAttributes(typeof(StandardsCompliantAttribute), false)
                .OfType<StandardsCompliantAttribute>();

            return SelectUris(attrs, source.Member.ReflectedType.Assembly);
        }

        public static bool IsServiceType(this Type type) {
            if (type == null)
                throw new ArgumentNullException("type");

            bool isStatic = type.IsSealed && type.IsAbstract;
            return !(type.IsPrimitive || type.IsEnum || isStatic);
        }

        public static bool IsReusable(this Type type) {
            if (type == null)
                throw new ArgumentNullException("type");

            return type.IsDefined<ReusableAttribute>();
        }

        public static bool IsProcessIsolated(this Type type) {
            if (type == null)
                throw new ArgumentNullException("type");

            return type.IsDefined<ProcessIsolatedAttribute>();
        }

        public static bool IsAppDomainIsolated(this Type type) {
            if (type == null)
                throw new ArgumentNullException("type");

            return type.IsDefined<AppDomainIsolatedAttribute>();
        }

        public static bool IsValidAdapter(this Type adapterType,
                                          string adapterRole) {
            if (adapterType == null)
                throw new ArgumentNullException("adapterType");

            return RequireAdapterRole(adapterRole).IsValidAdapter(adapterType);
        }

        public static bool IsValidAdapterMethod(this MethodInfo method,
                                                string adapterRole) {
            if (method == null)
                throw new ArgumentNullException("method");

            // return RequireAdapterRole(adapterRole).FindAdapterMethod(adapterRole);
            // TODO IsValidAdapter
            // Need the adaptee type?
            throw new NotImplementedException();
        }

        public static bool IsValidBuilder(this Type type) {
            if (type == null)
                throw new ArgumentNullException("type"); // $NON-NLS-1

            return IsValidAdapter(type, AdapterRole.Builder);
        }

        public static bool IsValidBuilderMethod(this MethodInfo method) {
            if (method == null)
                throw new ArgumentNullException("method"); // $NON-NLS-1

            return IsValidAdapterMethod(method, AdapterRole.Builder);
        }

        public static bool IsValidStreamingSource(this Type type) {
            if (type == null)
                throw new ArgumentNullException("type"); // $NON-NLS-1

            return IsValidAdapter(type, AdapterRole.StreamingSource);
        }

        public static bool IsValidStreamingSourceMethod(this MethodInfo method) {
            if (method == null)
                throw new ArgumentNullException("method"); // $NON-NLS-1

            return IsValidAdapterMethod(method, AdapterRole.StreamingSource);
        }

        internal static object InvokeBuilder(
            object instance, out MethodInfo buildMethod, IServiceProvider serviceProvider) {

            if (instance == null)
                throw new ArgumentNullException("instance");

            Type componentType = instance.GetType();
            // Invoke the builder
            Type[] argtypes = { typeof(IServiceProvider) };

            buildMethod = componentType.GetMethod("Build",
                                                  BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                                  null,
                                                  argtypes,
                                                  null);
            if (buildMethod != null) {
                object[] arguments = { serviceProvider };
                return buildMethod.Invoke(instance, arguments);
            }

            // Check for the parameterless implementation
            buildMethod = componentType.GetMethod("Build",
                                                  BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                                  null,
                                                  Type.EmptyTypes, // argtypes
                                                  null);

            if (buildMethod != null)
                return buildMethod.Invoke(instance, null);

            return null;
        }

        public static object InvokeBuilder(
            this object instance, IServiceProvider serviceProvider) {

            MethodInfo info;
            return InvokeBuilder(instance, out info, serviceProvider);
        }

        static IEnumerable<Type> _GetAdapterTypes(Type adapteeType, string adapterRoleName, bool inherit) {
            if (adapteeType == null)
                throw new ArgumentNullException("adapteeType"); // $NON-NLS-1
            Require.NotNullOrAllWhitespace("adapterRoleName", adapterRoleName);
            if (null == AdapterRoleData.FromName(adapterRoleName))
                return Empty<Type>.Array;

            IEnumerable<Type> result;
            IEnumerable<Type> explicitAdapters = ((AdapterAttribute[]) adapteeType.GetCustomAttributes(typeof(AdapterAttribute), inherit))
                .Where(a => a.Role == adapterRoleName)
                .Select(a => a.AdapterType);

            Type implicitAdapter = GetImplicitAdapterType(adapteeType, adapterRoleName);
            if (implicitAdapter == null)
                result = explicitAdapters;
            else {
                Type[] other = { implicitAdapter };
                result = explicitAdapters.Concat(other);
            }

            result = result.Concat(DefineAdapterAttribute.GetAdapterTypes(adapteeType, adapterRoleName, inherit));
            return result.Where(t => t.IsValidAdapter(adapterRoleName));
        }

        public static string GetExtensionImplementationName(this Type type) {
            if (type == null)
                throw new ArgumentNullException("type");

            const string IMPL = "Implementation.";
            int index = type.Namespace.IndexOf(IMPL);
            if (index < 0)
                return string.Empty;

            else
                return type.Namespace.Substring(index + IMPL.Length);
        }

        public static string GetImplementerName(this Type type) {
            if (type == null)
                throw new ArgumentNullException("type"); // $NON-NLS-1

            ImplementationAttribute cca = Attribute.GetCustomAttribute(type, typeof(ImplementationAttribute)) as ImplementationAttribute;
            if (cca != null)
                return cca.Implementation;

            // Check assembly
            cca = Attribute.GetCustomAttribute(type.Assembly, typeof(ImplementationAttribute)) as ImplementationAttribute;
            if (cca == null)
                return null;
            else
                return cca.Implementation;
        }

        public static QualifiedName GetQualifiedName(this Type type) {
            if (type == null)
                throw new ArgumentNullException("type");
            if (type.IsGenericParameter || (type.IsGenericType && !type.IsGenericTypeDefinition))
                throw RuntimeFailure.QualifiedNameCannotBeGeneratedFromConstructed("type");

            AssemblyInfo ai = AssemblyInfo.GetAssemblyInfo(type.Assembly);
            NamespaceUri xmlns = ai.GetXmlNamespace(type.Namespace);
            return xmlns + QualName(type);
        }

        static string QualName(Type type) {
            string name = type.Name.Replace("`", "-");
            if (type.IsNested)
                return string.Concat(QualName(type.DeclaringType), '.', name);
            else
                return name;
        }

        public static Type GetTypeByQualifiedName(this AppDomain appDomain, QualifiedName name, bool throwOnError) {
            if (appDomain == null)
                throw new ArgumentNullException("appDomain");
            if (name == null)
                throw new ArgumentNullException("name"); // $NON-NLS-1

            string cleanName = name.LocalName.Replace('.', '+').Replace('-', '`');
            foreach (var a in appDomain.GetAssemblies()) {
                AssemblyInfo ai = AssemblyInfo.GetAssemblyInfo(a);
                foreach (string clrns in ai.GetClrNamespaces(name.Namespace)) {
                    Type result = a.GetType(CombinedTypeName(clrns, cleanName));
                    if (result != null)
                        return result;
                }
            }

            if (throwOnError)
                throw RuntimeFailure.TypeMissingFromQualifiedName(name);

            return null;
        }

        public static Type GetTypeByQualifiedName(this AppDomain appDomain, QualifiedName name) {
            return GetTypeByQualifiedName(appDomain, name, false);
        }

        static string CombinedTypeName(string clrns, string name) {
            if (clrns.Length == 0)
                return name;
            else
                return string.Concat(clrns, ".", name);
        }

        static string GetExtensionImplementation(
            Type sourceType, string implementation) {

            // Carbonfrost.Commons.Membership.Implementation.Entity.EntityMembershipDataSource, Carbonfrost.Commons.Membership.Entity

            string pkt = Utility.BytesToHex(sourceType.Assembly.GetName().GetPublicKeyToken(), lowercase: true);
            AssemblyName name = sourceType.Assembly.GetName();
            string typeName = string.Format(
                "{0}.Implementation.{2}.{2}{3}, {0}.{2}, Version={1}, PublicKeyToken={4}",
                name.Name, name.Version.ToString(2), implementation, sourceType.Name, pkt);
            return typeName;
        }

        static IEnumerable<Uri> SelectUris(IEnumerable<RelationshipAttribute> attrs,
                                           Assembly assemblyContext) {

            Uri baseUri = null;
            if (assemblyContext != null)
                baseUri = AssemblyInfo.GetAssemblyInfo(assemblyContext).Base;

            if (baseUri == null || !baseUri.IsAbsoluteUri)
                return attrs.Select(t => t.Uri);
            else
                return attrs.Select(t => new Uri(baseUri, t.Uri));
        }

        static IEnumerable<Uri> GetStandardsCore(this ICustomAttributeProvider source,
                                                 Assembly assembly) {
            var attrs = source.GetCustomAttributes(typeof(StandardsCompliantAttribute), false)
                .OfType<StandardsCompliantAttribute>();
            return SelectUris(attrs, assembly);
        }

        static Assembly GetAssemblyContext(MemberDescriptor md) {
            EventDescriptor e = md as EventDescriptor;
            if (e != null)
                return e.ComponentType.Assembly;

            PropertyDescriptor p = md as PropertyDescriptor;
            if (p != null)
                return p.ComponentType.Assembly;

            return null;
        }

        static AdapterRoleData RequireAdapterRole(string adapterRole) {
            if (adapterRole == null)
                throw new ArgumentNullException("adapterRole");
            if (adapterRole.Length == 0)
                throw Failure.EmptyString("adapterRole");

            AdapterRoleData a = AdapterRoleData.FromName(adapterRole);
            if (a == null)
                throw RuntimeFailure.UnknownAdapterRole("adapterRole", adapterRole);
            return a;
        }

        static void AttributeCompleteAppDomain(Type attributeType) {
            if (typeof(IAssemblyInfoFilter).IsAssignableFrom(attributeType)) {
                AssemblyInfo.CompleteAppDomain();
            }
        }

        static IActivationProvider MakeActivationProvider(Type type) {
            return (IActivationProvider) Activator.CreateInstance(type);
        }

        static bool IsActivationConstructor(ConstructorInfo t) {
            return t.IsDefined(typeof(ActivationConstructorAttribute), false);
        }

    }
}
