//
// - PopulateComponentCallback.cs -
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
using System.ComponentModel;
using Carbonfrost.Commons.Shared;
using Carbonfrost.Commons.Shared.Runtime;

namespace Carbonfrost.Commons.ComponentModel {

    [Providers]
    public static class PopulateComponentCallback {

        public static readonly IPopulateComponentCallback Null = new NullPopulateComponentCallback();
        public static readonly IPopulateComponentCallback Default = new DefaultPopulateComponentCallback();

        internal static void ConversionError(this IPopulateComponentCallback callback,
                                             IServiceProvider serviceProvider,
                                             PropertyDescriptor pd,
                                             object value,
                                             Exception ex) {
            if (callback == null)
                return;

            callback.OnConversionException(pd.Name,
                                           value,
                                           ex);
        }


        internal static void Missing(this IPopulateComponentCallback callback,
                                     IServiceProvider serviceProvider,
                                     string name) {
            if (callback == null)
                return;

            callback.OnPropertyAnnotation(name, new InterfaceUsageInfo(InterfaceUsage.Missing));
        }

        sealed class NullPopulateComponentCallback : IPopulateComponentCallback {
            public void OnConversionException(string property, object value, Exception exception) {}
            public void OnPropertyAnnotation(string property, InterfaceUsageInfo usageInfo) {}
            public void OnEventAnnotation(string @event, InterfaceUsageInfo usageInfo) {}
        }

        sealed class DefaultPopulateComponentCallback : IPopulateComponentCallback {

            public void OnConversionException(string property, object value, Exception exception) {
                throw RuntimeFailure.PropertyConversionFailed(property, value, exception);
            }

            public void OnEventAnnotation(string @event, InterfaceUsageInfo usageInfo) {
                if (usageInfo.IsError) {
                    switch (usageInfo.Usage) {
                        case InterfaceUsage.Missing:
                            throw RuntimeFailure.EventMissing(@event);

                        case InterfaceUsage.Obsolete:
                            throw RuntimeFailure.EventObsolete(@event, usageInfo.Message);

                        case InterfaceUsage.Preliminary:
                            throw RuntimeFailure.EventPreliminary(@event, usageInfo.Message);

                        case InterfaceUsage.Unspecified:
                        default:
                            break;
                    }

                }
            }

            public void OnPropertyAnnotation(string property, InterfaceUsageInfo usageInfo) {
                if (usageInfo.IsError) {
                    switch (usageInfo.Usage) {
                        case InterfaceUsage.Missing:
                            throw RuntimeFailure.PropertyMissing(property);

                        case InterfaceUsage.Obsolete:
                            throw RuntimeFailure.PropertyObsolete(property, usageInfo.Message);

                        case InterfaceUsage.Preliminary:
                            throw RuntimeFailure.PropertyPreliminary(property, usageInfo.Message);

                        case InterfaceUsage.Unspecified:
                        default:
                            break;
                    }

                }
            }
        }
    }
}

