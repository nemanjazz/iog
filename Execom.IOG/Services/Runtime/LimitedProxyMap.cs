// -----------------------------------------------------------------------
// <copyright file="LimitedProxyMap.cs" company="Execom">
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
    using System.Diagnostics;

    /// <summary>
    /// Object contains map of instance id -> proxy with references.
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal class LimitedProxyMap : IProxyMap
    {
        /// <summary>
        /// Table containing weak references
        /// </summary>
        private Hashtable table = new Hashtable();

        private int minElements;

        private int maxElements;

        private class TableElement : IComparable
        {
            public object Taget;
            public long AccessCount;

            public TableElement(object target, long lastAccess)
            {
                this.Taget = target;
                this.AccessCount = lastAccess;
            }

            public int CompareTo(object obj)
            {
                return AccessCount.CompareTo(((TableElement)obj).AccessCount);
            }
        }

        /// <summary>
        /// Creates new instance of WeakProxyMap type
        /// </summary>
        public LimitedProxyMap(int minElements, int maxElements)
        {
            this.minElements = minElements;
            this.maxElements = maxElements;
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
                if (table.Count > maxElements)
                {
                    Cleanup();
                }

                if (!table.ContainsKey(instanceId))
                {
                    table.Add(instanceId, new TableElement(proxy, 0));
                }
                else
                {
                    ((TableElement)table[instanceId]).AccessCount++;
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
                    var element = ((TableElement)table[instanceId]);
                    proxy = element.Taget;
                    element.AccessCount++;
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
        /// Cleans all proxy references
        /// </summary>
        public void Cleanup()
        {
            lock (table)
            {
                if (table.Count > minElements)
                {
                    Guid[] accessIds = new Guid[table.Count];
                    TableElement[] entries = new TableElement[table.Count];

                    table.Values.CopyTo(entries, 0);
                    table.Keys.CopyTo(accessIds, 0);

                    Array.Sort(entries, accessIds);

                    int nrToRemove = table.Count - minElements;

                    foreach (var key in accessIds)
                    {
                        table.Remove(key);

                        nrToRemove--;
                        if (nrToRemove == 0)
                        {
                            break;
                        }
                    }

                    foreach (var item in table.Values)
                    {
                        ((TableElement)item).AccessCount = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Upgrades identifiers for existing proxies to new instance versions from the changeset
        /// </summary>
        /// <param name="mapping">Change set mapping</param>
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
                        TableElement element = (TableElement)table[key];
                        object reference = element.Taget;
                        Guid newKey = mapping[key];
                        Utils.SetItemId(reference, newKey);
                        table.Add(newKey, element);
                        table.Remove(key);
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
                var target = ((TableElement)table[key]).Taget;
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
                    var target = ((TableElement)table[key]).Taget;
                    if (target != null)
                    {
                        Utils.SetItemId(target, Guid.Empty);
                    }
                }
            }
        }
    }
}
