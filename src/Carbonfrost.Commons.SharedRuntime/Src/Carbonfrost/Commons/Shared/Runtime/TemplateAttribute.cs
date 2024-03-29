//
// - TemplateAttribute.cs -
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
using System.ComponentModel;
using System.Linq;

namespace Carbonfrost.Commons.Shared.Runtime {

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property)]
    public sealed class TemplateAttribute : Attribute, IActivationProvider {

        // TODO TemplateAttribute

        public string TemplateName { get; private set; }

        public TemplateAttribute(string templateName) {
            if (templateName == null)
                throw new ArgumentNullException("templateName");
            if (templateName.Length == 0)
                throw Failure.EmptyString("templateName");

            this.TemplateName = templateName;
        }

        object IActivationProvider.ActivateComponent(IServiceProvider serviceProvider, object component) {
            return component;
        }

        void IActivationProvider.ActivateProperty(IServiceProvider serviceProvider, object component, PropertyDescriptor property) {
        }
    }
}

