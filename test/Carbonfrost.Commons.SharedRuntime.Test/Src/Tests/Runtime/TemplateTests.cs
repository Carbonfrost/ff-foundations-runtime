//
// - TemplateTests.cs -
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
using System.Linq;

using Carbonfrost.Commons.ComponentModel.Annotations;
using Carbonfrost.Commons.Shared;
using Carbonfrost.Commons.Shared.Runtime;
using Carbonfrost.Commons.Shared.Runtime.Components;
using NUnit.Framework;

[assembly: Xmlns("http://ns.example.com/2012")]

namespace Tests.Runtime {

    [TemplatesAttribute]
    public static class Templates {

        public static readonly ITemplate<StatusBuilder> A = Template.Create((StatusBuilder l) => { l.ErrorCode = 4; });
        public static readonly ITemplate<StatusBuilder> B = Template.Create((StatusBuilder l) => { l.ErrorCode = 5; });

    }

    [TestFixture]
    public class TemplateTests {

        [Test]
        public void template_from_qualified_name() {
            ITemplate<StatusBuilder> a = Template.FromName<StatusBuilder>(NamespaceUri.Parse("http://ns.example.com/2012").GetName("A"));
            var sb = a.CreateInstance();

            Assert.That(sb.ErrorCode, Is.EqualTo(4));
        }

        [Test]
        public void template_get_names() {
            IEnumerable<QualifiedName> names = AppDomain.CurrentDomain.GetTemplateNames(typeof(StatusBuilder));
            Assert.That(names, Contains.Item(NamespaceUri.Parse("http://ns.example.com/2012") + "A"));
            Assert.That(names, Contains.Item(NamespaceUri.Parse("http://ns.example.com/2012") + "B"));
        }

        [Test]
        public void template_get_name_from_object() {
            var name = NamespaceUri.Parse("http://ns.example.com/2012").GetName("A");
            ITemplate<StatusBuilder> a = Template.FromName<StatusBuilder>(name);
            Assert.That(a.GetTemplateName(), Is.EqualTo(name));
        }

        [Test]
        public void template_from_local_name() {
            ITemplate<StatusBuilder> template = Template.FromName<StatusBuilder>("A");
            var sb = template.CreateInstance();
            Assert.That(sb.ErrorCode, Is.EqualTo(4));
        }

        [Test]
        public void initialize_from_instance_nominal() {
            var expected = Component.Assembly(ComponentName.Parse("System, Version=5.0"));
            var t = Template.Create(new StatusBuilder { Component = expected, ErrorCode = 0xdead });

            StatusBuilder sb = new StatusBuilder();
            t.Initialize(sb);

            Assert.That(sb.ErrorCode, Is.EqualTo(0xdead));
            Assert.That(sb.Component, Is.EqualTo(expected));
        }

        [Test]
        public void activation_initialize_tests() {
            var asm = Component.Assembly(ComponentName.Parse("System, Version=5.0"));
            Dictionary<string, object> values =  new Dictionary<string, object>() {
                { "Component", asm },
                { "ErrorCode", 0xdead },
            };

            StatusBuilder sb = new StatusBuilder();
            Activation.Initialize(sb, values);

            Assert.That(sb.ErrorCode, Is.EqualTo(0xdead));
            Assert.That(sb.Component, Is.EqualTo(asm));
        }

    }
}
