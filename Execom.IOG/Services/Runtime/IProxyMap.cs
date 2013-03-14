// -----------------------------------------------------------------------
// <copyright file="IProxyMap.cs" company="Execom">
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

namespace Execom.IOG.Services.Runtime
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface with methods of the proxy map. Proxy map is storing 
    /// references to proxy objects organized by instance ID.
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal interface IProxyMap
    {
        /// <summary>
        /// Add new proxy instance to map
        /// </summary>
        /// <param name="instanceId">Instance ID</param>
        /// <param name="proxy">Proxy to add</param>
        void AddProxy(Guid instanceId, object proxy);

        /// <summary>
        /// Cleans all proxy references
        /// </summary>
        void Cleanup();

        /// <summary>
        /// Makes all proxies unusable by clearing instance ID
        /// </summary>
        void InvalidateProxies();

        /// <summary>
        /// Makes proxies unusable by clearing instance ID
        /// </summary>
        /// <param name="instances">Identifiers for instances to be cleared</param>
        void InvalidateProxies(System.Collections.ObjectModel.Collection<Guid> instances);

        /// <summary>
        /// Gets a proxy from the map if it exists
        /// </summary>
        /// <param name="instanceId">Instance ID</param>
        /// <param name="proxy">Proxy result, null if not found</param>
        /// <returns>True if proxy was found</returns>
        bool TryGetProxy(Guid instanceId, out object proxy);

        /// <summary>
        /// Upgrades identifiers for existing proxies to new instance versions from the changeset
        /// </summary>
        /// <param name="changeSet">Change set</param>
        void UpgradeProxies(Dictionary<Guid, Guid> mapping);
    }
}
