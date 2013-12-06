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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using Carbonfrost.Commons.ComponentModel;
using Carbonfrost.Commons.ComponentModel.Annotations;
using Carbonfrost.Commons.Shared;
using Carbonfrost.Commons.Shared.Runtime;
using Carbonfrost.Commons.Shared.Runtime.Components;

[assembly: AssemblyTitle("Carbonfrost.Commons.SharedRuntime")]
[assembly: AssemblyDescription("Carbonfrost.Commons.SharedRuntime")]
[assembly: Xmlns(Xmlns.SharedRuntime2008, Prefix = "runtime", ClrNamespace = "Carbonfrost.Commons.Shared.*, Carbonfrost.Commons.ComponentModel")]
[assembly: Xmlns(Xmlns.ShareableCodeMetadata2011, Prefix = "shareable", ClrNamespace = "Carbonfrost.Commons.ComponentModel.Annotations")]

#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration("RELEASE")]
#endif

[assembly: AssemblyCompany("Carbonfrost Systems, Inc. and contributors.")]
[assembly: AssemblyProduct("The F5 Project")]
[assembly: AssemblyCopyright("Copyright (c) 2005, 2006, 2010 Carbonfrost Systems, Inc. and contributors")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]

[assembly: InternalsVisibleTo("Carbonfrost.Commons.SharedRuntime.Test, PublicKey=00240000048000009400000006020000002400005253413100040000010001005D816AF902913A3381795785638DC3E2B9CB19D83EC2AAD8764E215F7D65CD24D35638F707D9D0AB4AD47CFE847BD19D1694782F5F547F69A6FD02EC5358BBB1D2BDE36688C923A3E32CC1EACF196C8F4A49554F180B6F9F600F9CAD688DEAA8572482E7263D20318222FA3A79740A495451A0C74F5BEE14B4BFD43B3D2928C9")]
[assembly: InternalsVisibleTo("Carbonfrost.Commons.Instrumentation, PublicKey=00240000048000009400000006020000002400005253413100040000010001005D816AF902913A3381795785638DC3E2B9CB19D83EC2AAD8764E215F7D65CD24D35638F707D9D0AB4AD47CFE847BD19D1694782F5F547F69A6FD02EC5358BBB1D2BDE36688C923A3E32CC1EACF196C8F4A49554F180B6F9F600F9CAD688DEAA8572482E7263D20318222FA3A79740A495451A0C74F5BEE14B4BFD43B3D2928C9")]
[assembly: InternalsVisibleTo("Carbonfrost.Commons.UniversalConfiguration, PublicKey=00240000048000009400000006020000002400005253413100040000010001005D816AF902913A3381795785638DC3E2B9CB19D83EC2AAD8764E215F7D65CD24D35638F707D9D0AB4AD47CFE847BD19D1694782F5F547F69A6FD02EC5358BBB1D2BDE36688C923A3E32CC1EACF196C8F4A49554F180B6F9F600F9CAD688DEAA8572482E7263D20318222FA3A79740A495451A0C74F5BEE14B4BFD43B3D2928C9")]

[assembly: Provides(typeof(ComponentStore))]
[assembly: Provides(typeof(IAdapterFactory))]
[assembly: Provides(typeof(IActivationFactory))]
[assembly: Provides(typeof(RuntimeComponentLoader))]
[assembly: Provides(typeof(IAssemblyInfoFilter))]
[assembly: Provides(typeof(IService))]
[assembly: Provides(typeof(StreamingSource))]
[assembly: Provides(typeof(IPopulateComponentCallback))]

[assembly: SharedRuntimeOptions(Optimizations = SharedRuntimeOptimizations.DisableTemplateScanning)]
