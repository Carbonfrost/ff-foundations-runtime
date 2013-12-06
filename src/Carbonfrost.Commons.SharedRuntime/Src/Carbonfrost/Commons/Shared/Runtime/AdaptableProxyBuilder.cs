//
// - AdaptableProxyBuilder.cs -
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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;

namespace Carbonfrost.Commons.Shared.Runtime {

    sealed class AdaptableProxyBuilder {

        static readonly IDictionary<Tuple<FallbackBehavior, Type, Type>, AdaptableProxyBuilder> cache
            = new Dictionary<Tuple<FallbackBehavior, Type, Type>, AdaptableProxyBuilder>();

        static readonly string GENERATOR_VERSION = typeof(AdaptableProxyBuilder).Assembly.GetName().Version.ToString();
        const string GENERATOR = "FFProxyBuilder";

        private readonly Type instanceType;
        private readonly Type baseType;
        private Type constructedType;
        private readonly AssemblyBuilder theAssembly;
        private readonly FallbackBehavior fallback;

        private AdaptableProxyBuilder(FallbackBehavior fallback,
                                      Type instanceType,
                                      Type baseType) {
            this.instanceType = instanceType;
            if (baseType.IsValueType)
                throw RuntimeFailure.ValueTypesNotSupported();

            this.fallback = fallback;
            this.baseType = baseType;
            this.theAssembly = GenerateAssembly();
        }

        public static AdaptableProxyBuilder Create(FallbackBehavior fallback,
                                                   Type instanceType,
                                                   Type baseType) {
            var key = Tuple.Create(fallback, instanceType, baseType);
            return cache.GetValueOrCache(key, () => (new AdaptableProxyBuilder(fallback, instanceType, baseType)));
        }

        AssemblyBuilder GenerateAssembly() {
            MethodInfo[] methods = EnumerateMethods().ToArray();
            MethodInfo[] pickMethods = new MethodInfo[methods.Length];
            bool failFast = FallbackBehavior.None == fallback;

            for (int i = 0; i < methods.Length; i++) {
                var method = methods[i];
                MethodInfo pickMethod = PickCompatibleMethod(method);

                if (pickMethod == null) {
                    if (failFast)
                        return null;
                }

                pickMethods[i] = pickMethod;
            }

            ConstructorInfo attributeConstructor = typeof(GeneratedCodeAttribute).GetConstructors()[0];
            CustomAttributeBuilder cab = new CustomAttributeBuilder(attributeConstructor, new [] { GENERATOR, GENERATOR_VERSION });

            string proxy = GenerateName();
            string typeName = string.Format("{0}{1}Adapter", instanceType.Name, baseType.Name);
            AssemblyName name = new AssemblyName(
                string.Format("DynamicProxy{0}, Version=0.0.0.0", proxy));

            AssemblyBuilder asm = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder mod = asm.DefineDynamicModule(name.Name, name.Name + ".dll");

            TypeBuilder typeBuilder;
            if (baseType.IsInterface) {
                typeBuilder = mod.DefineType(typeName,
                                             TypeAttributes.Class | TypeAttributes.Public,
                                             typeof(object),
                                             new Type[] { baseType });
            } else {
                typeBuilder = mod.DefineType(typeName, TypeAttributes.Class | TypeAttributes.Public, baseType );
            }

            asm.SetCustomAttribute(cab);
            FieldBuilder inner = EmitInstanceInit(typeBuilder);
            int index = 0;
            foreach (var method in methods) {
                EmitMethod(typeBuilder, method, pickMethods[index++], inner);
            }

            this.constructedType = typeBuilder.CreateType();
            return asm;
        }

        private MethodInfo PickCompatibleMethod(MethodInfo method) {
            MethodInfo pickMethod = this.instanceType.GetMethod(
                method.Name, method.GetParameters().Select(t => t.ParameterType).ToArray());

            if (pickMethod == null)
                return null;

            if (pickMethod.IsStatic)
                return null;

            if (!pickMethod.ReturnType.Equals(method.ReturnType))
                return null;

            return pickMethod;
        }

        void EmitMethod(TypeBuilder typeBuilder, MethodInfo method, MethodInfo pickMethod, FieldBuilder inner) {
            var parameters = method.GetParameters();
            var parameterTypes = parameters.Select(t => t.ParameterType).ToArray();

            var mb = typeBuilder.DefineMethod(
                method.Name,
                MethodAttributes.Virtual | MethodAttributes.Public,
                CallingConventions.HasThis,
                method.ReturnType,
                parameterTypes);

            var il = mb.GetILGenerator();

            if (pickMethod != null) {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, inner);
            }

            int position = 0;
            foreach (var p in parameters) {
                var pb = mb.DefineParameter(position++,
                                            p.Attributes, null);
                if (pickMethod != null)
                    il.Emit(OpCodes.Ldarg, position);
            }

            if (pickMethod != null)
                il.Emit(OpCodes.Call, pickMethod);

            else if (fallback == FallbackBehavior.CreateDefault) {

                if (method.ReturnType == typeof(void)) {
                    // nop

                } else if (method.ReturnType.IsPrimitive) {
                    EmitPrimitive(il, method.ReturnType);
                }
                else if (method.ReturnType.IsValueType) {
                    LocalBuilder lb = il.DeclareLocal(method.ReturnType);
                    il.Emit(OpCodes.Ldloca_S, 0);
                    il.Emit(OpCodes.Initobj, method.ReturnType);
                    il.Emit(OpCodes.Ldloc_0);
                } else
                    il.Emit(OpCodes.Ldnull);

            } else {
                il.Emit(OpCodes.Newobj, typeof(NotImplementedException).GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Throw);
                return;
            }

            il.Emit(OpCodes.Ret);
        }

        private void EmitPrimitive(ILGenerator il, Type type) {
            il.Emit(OpCodes.Ldc_I4_0);

            switch (Type.GetTypeCode(type)) {
                case TypeCode.DBNull:
                case TypeCode.Boolean:
                    break;

                case TypeCode.Char:
                case TypeCode.SByte:
                    break;

                case TypeCode.Byte:
                    break;

                case TypeCode.Int16:
                    break;

                case TypeCode.UInt16:
                    break;

                case TypeCode.Int32:
                case TypeCode.UInt32:
                    break;

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    il.Emit(OpCodes.Conv_I8);
                    break;

                case TypeCode.Single:
                    il.Emit(OpCodes.Conv_R4);
                    break;

                case TypeCode.Double:
                    il.Emit(OpCodes.Conv_R8);
                    break;

                case TypeCode.Decimal:
                    break;

                case TypeCode.DateTime:
                case TypeCode.String:
                default:
                    return;
            }
        }

        FieldBuilder EmitInstanceInit(TypeBuilder t) {
            FieldBuilder inner = t.DefineField("_inner", instanceType, FieldAttributes.InitOnly | FieldAttributes.Private);
            ConstructorBuilder cb = t.DefineConstructor(MethodAttributes.Public,
                                                        CallingConventions.Standard,
                                                        new[] { instanceType });
            var il = cb.GetILGenerator();

            // Base constructor
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));

            // Store the inner instance in the field
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, inner);
            il.Emit(OpCodes.Ret);

            return inner;
        }

        IEnumerable<MethodInfo> EnumerateMethods() {
            return baseType.GetMethods().Where(m => m.IsVirtual);
        }

        string GenerateName() {

            MemoryStream ms = new MemoryStream();

            StreamWriter w = new StreamWriter(ms);
            w.WriteLine(instanceType.AssemblyQualifiedName);
            w.WriteLine(baseType.AssemblyQualifiedName);

            return Utility.BytesToHex(MD5.Create().ComputeHash(ms.ToArray()));
        }

        public object CreateInstance(object instance) {
            if (constructedType == null)
                return null;

            return Activator.CreateInstance(constructedType, instance);
        }
    }
}
