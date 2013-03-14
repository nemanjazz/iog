// -----------------------------------------------------------------------
// <copyright file="IRuntimeProxyFacade.cs" company="Execom">
// Copyright 2011 EXECOM d.o.o
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
// -----------------------------------------------------------------------

namespace Execom.IOG.Services.Facade
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Interface which defines operations on proxies during runtime
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal interface IRuntimeProxyFacade : IRuntimeCollectionProxyFacade, IRuntimeDictionaryProxyFacade
    {
        /// <summary>
        /// Returns value of a property: 
        /// - If property is scalar, returns the scalar value
        /// - If property is not scalar, returns the proxy object
        /// </summary>
        /// <param name="instanceId">Instance ID</param>
        /// <param name="memberId">Member ID</param>
        /// <param name="isScalar">Determines if member is scalar</param>
        /// <param name="isReadOnly">Determines if instance is read only</param>
        /// <returns>Scalar value, or a proxy</returns>
        object GetInstanceMemberValue(Guid instanceId, Guid memberId, bool isScalar, bool isReadOnly);

        /// <summary>
        /// Sets value for a property
        /// </summary>
        /// <param name="instanceId">Instance ID</param>
        /// <param name="memberId">Member ID</param>
        /// <param name="value">New value to set</param>
        /// <param name="isScalar">Determines if member is scalar</param>
        /// <param name="isReadOnly">Determines if instance is read only</param>
        void SetInstanceMemberValue(Guid instanceId, Guid memberId, object value, bool isScalar, bool isReadOnly);

        /// <summary>
        /// Method is called when .NET GC collects a proxy
        /// </summary>
        /// <param name="instance">Proxy instance</param>
        void ProxyCollected(object instance);        
    }
}
