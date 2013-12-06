//
// - Platform.cs -
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
using System.IO;
using System.Reflection;

using Carbonfrost.Commons.Shared.Runtime.Components;

namespace Carbonfrost.Commons.Shared.Runtime {

    public sealed class Platform {

        private PlatformBase platform;
        private static readonly Platform _current = new Platform();

        public static Platform Current {
            get { return _current; }
        }

        public bool FileSystemCaseSensitive {
            get {
                switch (Environment.OSVersion.Platform) {
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                    case PlatformID.WinCE:
                        return false;

                    case PlatformID.Unix:
                    case PlatformID.Xbox:
                    case PlatformID.MacOSX:
                    default:
                        return true;
                }
            }
        }

        public bool IsMono { get; private set; }

        private Platform() {
            this.platform = Init();
        }

        private PlatformBase Init() {
            IsMono = Type.GetType("Mono.Runtime") != null;

            switch (Environment.OSVersion.Platform) {
                case PlatformID.MacOSX:
                    platform = new MacOSXPlatform();
                    break;

                case PlatformID.Unix:
                    platform = new UnixPlatform();
                    break;

                default:
                    platform = new WindowsPlatform();
                    break;
            }

            return platform;
        }

        abstract class PlatformBase {
        }

        class WindowsPlatform : PlatformBase {
        }

        class MacOSXPlatform : PlatformBase {
        }

        class UnixPlatform : PlatformBase {
        }

    }
}
