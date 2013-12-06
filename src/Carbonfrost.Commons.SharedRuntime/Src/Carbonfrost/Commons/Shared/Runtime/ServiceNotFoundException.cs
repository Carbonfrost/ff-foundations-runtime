//
// - ServiceNotFoundException.cs -
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
using System.Runtime.Serialization;
using Carbonfrost.Commons.SharedRuntime.Resources;

namespace Carbonfrost.Commons.Shared {

    [Serializable]
    public class ServiceNotFoundException : InvalidOperationException {

        private readonly string serviceName;
        private readonly Type serviceType;

        public string ServiceName { get { return serviceName; } }
        public Type ServiceType { get { return serviceType; } }

        public ServiceNotFoundException(Type serviceType) : base(SR.ServiceNotFound(serviceType))
        {
            this.serviceType = serviceType;
        }

        public ServiceNotFoundException(string serviceName) : base(SR.ServiceNotFound(serviceName))
        {
            this.serviceName = serviceName;
        }

        protected ServiceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.serviceType = (Type) info.GetValue("serviceType", serviceType);
            this.serviceName = info.GetString("serviceName");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("serviceName", serviceName);
            info.AddValue("serviceType", serviceType);

            base.GetObjectData(info, context);
        }

        public ServiceNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }
}
