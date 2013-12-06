//
// - InterfaceUsageInfo.cs -
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Text;

using Carbonfrost.Commons.Shared;
using Carbonfrost.Commons.Shared.Runtime;
using Carbonfrost.Commons.SharedRuntime.Resources;

namespace Carbonfrost.Commons.ComponentModel {

    [Serializable]
    public class InterfaceUsageInfo : IStatus {

        private readonly string message;
        private readonly InterfaceUsage usage;
        private readonly Uri helpContext;
        private readonly bool isError;
        private static readonly InterfaceUsageInfo unspecified = new InterfaceUsageInfo(InterfaceUsage.Unspecified);

        public static InterfaceUsageInfo Unspecified { get { return unspecified; } }

        [SelfDescribingPriority(PriorityLevel.Low)]
        public Uri HelpUri { get { return helpContext; } }

        [SelfDescribingPriority(PriorityLevel.High)]
        public bool IsError { get { return isError; } }

        [SelfDescribingPriority(PriorityLevel.High)]
        public string Message { get { return message; } }

        [SelfDescribingPriority(PriorityLevel.High)]
        public InterfaceUsage Usage { get { return usage; } }

        // Constructors.

        public InterfaceUsageInfo(InterfaceUsage usage) {
            this.usage = usage;
        }

        public InterfaceUsageInfo(
            InterfaceUsage usage, string message, Uri helpContext, bool isError) {
            this.message = message;
            this.usage = usage;
            this.helpContext = helpContext;
            this.isError = isError;
        }

        public static InterfaceUsageInfo FromMember(MemberDescriptor member) {
            if (member == null)
                return new InterfaceUsageInfo(InterfaceUsage.Missing);

            ObsoleteAttribute oa = (ObsoleteAttribute) member.Attributes[typeof(ObsoleteAttribute)];
            if (oa != null && !oa.IsDefaultAttribute())
                return new InterfaceUsageInfo(
                    InterfaceUsage.Obsolete, oa.Message, null, oa.IsError);

            PreliminaryAttribute pa = (PreliminaryAttribute) member.Attributes[typeof(PreliminaryAttribute)];
            if (pa != null && !pa.IsDefaultAttribute())
                return new InterfaceUsageInfo(
                    InterfaceUsage.Preliminary,
                    pa.Message,
                    (pa.HelpUri == null) ? null : new Uri(pa.HelpUri),
                    pa.IsError);

            return InterfaceUsageInfo.Unspecified;
        }

        public static InterfaceUsageInfo FromMember(ICustomAttributeProvider member) {
            if (member == null)
                return new InterfaceUsageInfo(InterfaceUsage.Missing);

            ObsoleteAttribute[] obs = (ObsoleteAttribute[]) member.GetCustomAttributes(typeof(ObsoleteAttribute), true);
            if (obs.Length > 0)
                return new InterfaceUsageInfo(
                    InterfaceUsage.Obsolete,
                    obs[0].Message, null,
                    obs[0].IsError);

            PreliminaryAttribute[] prelim = (PreliminaryAttribute[]) member.GetCustomAttributes(typeof(ObsoleteAttribute), true);
            if (prelim.Length > 0)
                return new InterfaceUsageInfo(
                    InterfaceUsage.Preliminary,
                    prelim[0].Message,
                    (prelim[0].HelpUri == null) ? null : new Uri(prelim[0].HelpUri),
                    prelim[0].IsError);

            return InterfaceUsageInfo.Unspecified;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.Usage);

            bool hasMessage = !string.IsNullOrEmpty(this.Message);
            // Preliminary (Message Text [Error] [Help Link])
            if (hasMessage || this.IsError || this.HelpUri != null) {
                sb.Append(' ');

                if (hasMessage) {
                    sb.Append('(');
                    sb.Append(this.Message);
                }

                if (this.IsError)
                    sb.Append(hasMessage ? " " : string.Empty).Append(SR.Error());

                if (this.HelpUri != null) {
                    if (this.IsError)
                        sb.Append(' ');

                    sb.Append('[');
                    sb.Append(this.HelpUri);
                    sb.Append(']');
                }

                if (hasMessage)
                    sb.Append(')');
            }

            return sb.ToString();
        }

        Exception IStatus.Exception {
            get {
                return null;
            }
        }

        FileLocation IStatus.FileLocation {
            get {
                throw new NotImplementedException();
            }
        }

        Severity IStatus.Level {
            get {
                throw new NotImplementedException();
            }
        }

        int IStatus.ErrorCode {
            get {
                return 1;
            }
        }

        ReadOnlyCollection<IStatus> IStatus.Children {
            get {
                return Empty<IStatus>.ReadOnly;
            }
        }

        Carbonfrost.Commons.Shared.Runtime.Components.Component IStatus.Component {
            get {
                throw new NotImplementedException();
            }
        }

        bool IEquatable<IStatus>.Equals(IStatus other) {
            throw new NotImplementedException();
        }
    }
}
