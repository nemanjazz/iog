// -----------------------------------------------------------------------
// <copyright file="WeakProxyMap.cs" company="Execom">
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
    using System.Text;
    using System.Collections;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Object contains map of instance id -> proxy with weak references
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal class WeakProxyMap : IProxyMap
    {
        /// <summary>
        /// Table containing weak references
        /// </summary>
        private Hashtable table = new Hashtable();

        /// <summary>
        /// Creates new instance of WeakProxyMap type
        /// </summary>
        public WeakProxyMap()
        {
        }

        /// <summary>
        /// Add new proxy instance to map
        /// </summary>
        /// <param name="instanceId">Instance ID</param>
        /// <param name="proxy">Proxy to add</param>
        public void AddProxy(Guid instanceId, object proxy)
        {
            if (proxy == null)
            {
                throw new ArgumentNullException("proxy");
            }

            lock (table)
            {
                if (!table.ContainsKey(instanceId))
                {
                    table.Add(instanceId, new WeakReference(proxy));
                }
                else
                {
                    if (((WeakReference)table[instanceId]).IsAlive)
                    {
                        throw new NotSupportedException("Key already exists");
                    }

                    table[instanceId] = new WeakReference(proxy);
                }
            }
        }

        /// <summary>
        /// Gets a proxy from the map if it exists
        /// </summary>
        /// <param name="instanceId">Instance ID</param>
        /// <param name="proxy">Proxy result, null if not found</param>
        /// <returns>True if proxy was found</returns>
        public bool TryGetProxy(Guid instanceId, out object proxy)
        {
            lock (table)
            {
                if (table.ContainsKey(instanceId))
                {
                    proxy = ((WeakReference)table[instanceId]).Target;
                    return proxy != null;
                }
                else
                {
                    proxy = null;
                    return false;
                }
            }
        }        

        /// <summary>
        /// Cleans all proxy references collected by .NET GC
        /// </summary>
        public void Cleanup()
        {
            lock (table)
            {
                Collection<Guid> toRemove = new Collection<Guid>();

                foreach (Guid key in table.Keys)
                {
                    if (!((WeakReference)table[key]).IsAlive)
                    {
                        toRemove.Add(key);
                    }
                }

                foreach (Guid key in toRemove)
                {
                    table.Remove(key);
                }
            }
        }

        /// <summary>
        /// Upgrades identifiers for existing proxies to new instance versions from the changeset
        /// </summary>
        /// <param name="mapping">Change set</param>
        public void UpgradeProxies(Dictionary<Guid, Guid> mapping)
        {
            lock (table)
            {
                Collection<Guid> keys = new Collection<Guid>();

                // Copy the keys
                foreach (Guid key in table.Keys)
                {
                    keys.Add(key);
                }

                foreach (Guid key in keys)
                {
                    // Is there a new version
                    if (mapping.ContainsKey(key))
                    {
                        if (table[key] != null)
                        {
                            WeakReference weakReference = (WeakReference)table[key];
                            object reference = weakReference.Target;

                            if (reference != null)
                            {
                                Guid newKey = mapping[key];
                                Utils.SetItemId(reference, newKey);
                                table.Add(newKey, weakReference);
                                table.Remove(key);
                            }
                        }
                    }
                }

                Cleanup();
            }
        }

        /// <summary>
        /// Makes all proxies unusable by clearing instance ID
        /// </summary>
        public void InvalidateProxies()
        {
            foreach (Guid key in table.Keys)
            {
                var target = ((WeakReference)table[key]).Target;
                if (target != null)
                {
                    Utils.SetItemId(target, Guid.Empty);
                }
            }
        }

        /// <summary>
        /// Makes proxies unusable by clearing instance ID
        /// </summary>
        /// <param name="instances">Identifiers for instances to be cleared</param>
        public void InvalidateProxies(Collection<Guid> instances)
        {
            foreach (Guid key in table.Keys)
            {
                if (instances.Contains(key))
                {
                    var target = ((WeakReference)table[key]).Target;
                    if (target != null)
                    {
                        Utils.SetItemId(target, Guid.Empty);
                    }
                }
            }
        }
    }
}
