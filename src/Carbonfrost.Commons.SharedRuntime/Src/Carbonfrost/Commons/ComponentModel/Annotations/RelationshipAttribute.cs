//
// - RelationshipAttribute.cs -
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
using System.Globalization;
using Carbonfrost.Commons.Shared;
using Carbonfrost.Commons.Shared.Runtime;

namespace Carbonfrost.Commons.ComponentModel.Annotations {

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class RelationshipAttribute : Attribute, IAssemblyInfoFilter {

        public virtual string ContentType { get; set; }
        public virtual string Name { get; set; }
        public virtual string Role { get; set; }
        public virtual string Media { get; set; }
        public virtual string Culture { get; set; }

        public QualifiedName QualifiedName {
            get {
                if (string.IsNullOrEmpty(this.Name))
                    return Adaptable.GetQualifiedName(GetType());
                else
                    return QualifiedName.Parse(this.Name);
            }
        }

        public Uri Uri { get; private set; }

        public RelationshipAttribute(string uri) {
            this.Uri = new Uri(uri);
        }

        protected virtual void ApplyToAssembly(AssemblyInfo info) {
            if (string.IsNullOrEmpty(Name))
                throw Failure.PropertyMustBeSet("Name");

            CultureInfo ci = CultureInfo.InvariantCulture;
            if (!string.IsNullOrEmpty(this.Culture)) {
                ci = CultureInfo.GetCultureInfo(this.Culture);
            }

            QualifiedName name = QualifiedName.Parse(Name);
            Relationship rel = new Relationship(
                name, this.Uri, this.Role, this.Media, ci);

            info.Properties.Push(name.ToString(), rel);
        }

        void IAssemblyInfoFilter.ApplyToAssembly(AssemblyInfo info) {
            ApplyToAssembly(info);
        }
    }

}
