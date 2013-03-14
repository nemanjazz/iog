// -----------------------------------------------------------------------
// <copyright file="CachedWriteNodeProvider.cs" company="Execom">
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

namespace Execom.IOG.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Execom.IOG.Graph;
    using Execom.IOG.Storage;
    using System.Threading;
    using System.Collections.ObjectModel;
    using System.Collections;

    /// <summary>
    /// Defines node provider with read/write caching. 
    /// Can NET be used by multiple threads.
    /// Writes are first performed to the cache, and only written to parent when evicted from the cache.
    /// It assumes that underlying providers have multi-read excluive-write capability.
    /// </summary>
    /// <typeparam name="TIdentifier">Type of identifier</typeparam>
    /// <typeparam name="TNodeData">Type of node data</typeparam>
    /// <typeparam name="TEdgeData">Type of edge data</typeparam>
    /// <author>Nenad Sabo</author>
    public class CachedWriteNodeProviderUnsafe<TIdentifier, TNodeData, TEdgeData> : IDisposable, INodeProvider<TIdentifier, TNodeData, TEdgeData> where TEdgeData : IComparable<TEdgeData>
    {
        /// <summary>
        /// Underlying node provider
        /// </summary>
        private INodeProvider<TIdentifier, TNodeData, TEdgeData> parentProvider;

        /// <summary>
        /// Data cache provider
        /// </summary>
        private INodeProvider<TIdentifier, TNodeData, TEdgeData> cacheProvider;


        /// <summary>
        /// Creates new instance of CachedWriteNodeProvider type
        /// </summary>
        /// <param name="parentProvider"></param>
        /// <param name="cacheProvider"></param>
        public CachedWriteNodeProviderUnsafe(INodeProvider<TIdentifier, TNodeData, TEdgeData> parentProvider, INodeProvider<TIdentifier, TNodeData, TEdgeData> cacheProvider)
        {
            if (!(cacheProvider is IEvictingProvider<TIdentifier>))
            {
                throw new ArgumentException("Cache provider is expected to support evicting");
            }

            this.parentProvider = parentProvider;
            this.cacheProvider = cacheProvider;

            (cacheProvider as IEvictingProvider<TIdentifier>).OnBeforeKeysEvicted += new EventHandler<KeysEvictedEventArgs<TIdentifier>>(CachedWriteNodeProvider_OnBeforeKeysEvicted);
        }

        void CachedWriteNodeProvider_OnBeforeKeysEvicted(object sender, KeysEvictedEventArgs<TIdentifier> e)
        {
            // Move data to parent provider
            foreach (var key in e.Keys)
            {
                var node = cacheProvider.GetNode(key, NodeAccess.Read);
                parentProvider.SetNode(key, node);
                cacheProvider.Remove(key);
            }
        }

        /// <summary>
        /// Adds node to collection and cache
        /// </summary>
        /// <param name="identifier">Node identifier</param>
        /// <param name="node">Node to add</param>
        public void SetNode(TIdentifier identifier, Node<TIdentifier, TNodeData, TEdgeData> node)
        {
            if (parentProvider.Contains(identifier))
            {
                parentProvider.Remove(identifier);
            }

            cacheProvider.SetNode(identifier, node);
        }

        /// <summary>
        /// Gets node from collection by an identifier. First attempt to read from cache.
        /// </summary>
        /// <param name="nodeId">Node identifier</param>
        /// <returns>Node object</returns>
        public Node<TIdentifier, TNodeData, TEdgeData> GetNode(TIdentifier identifier, NodeAccess access)
        {
            if (cacheProvider.Contains(identifier))
            {
                return cacheProvider.GetNode(identifier, access);
            }

            // Read from parent -> move to cache
            Node<TIdentifier, TNodeData, TEdgeData> node = parentProvider.GetNode(identifier, access);
            parentProvider.Remove(identifier);
            cacheProvider.SetNode(identifier, node);            
            return node;
        }

        /// <summary>
        /// Determines if collection contains a node with given identifier. Check is done on cache first.
        /// </summary>
        /// <param name="identifier">Identifier to check</param>
        /// <returns>True if contains</returns>
        public bool Contains(TIdentifier identifier)
        {
            if (cacheProvider.Contains(identifier))
            {
                return true;
            }
            return parentProvider.Contains(identifier);
        }

        /// <summary>
        /// Removes node from collection by an identifier. It is also removed from cache.
        /// </summary>
        /// <param name="identifier">Node identifier</param>
        public void Remove(TIdentifier identifier)
        {
            if (cacheProvider.Contains(identifier))
            {
                cacheProvider.Remove(identifier);
            }
            else
            {
                parentProvider.Remove(identifier);
            }
        }

        /// <summary>
        /// Returns node enumeration containing all nodes
        /// </summary>
        /// <returns>Nodes identifiers</returns>
        public IEnumerable EnumerateNodes()
        {
            Collection<TIdentifier> items = new Collection<TIdentifier>();

            foreach (TIdentifier item in cacheProvider.EnumerateNodes())
            {
                items.Add(item);
            }

            foreach (TIdentifier item in parentProvider.EnumerateNodes())
            {
                items.Add(item);
            }

            return items;
        }

        /// <summary>
        /// Clears nodes from the parent provider and cache
        /// </summary>
        public void Clear()
        {
            parentProvider.Clear();
            cacheProvider.Clear();
        }

        /// <summary>
        /// Performs clean shutdown
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            (cacheProvider as IEvictingProvider<TIdentifier>).OnBeforeKeysEvicted -= new EventHandler<KeysEvictedEventArgs<TIdentifier>>(CachedWriteNodeProvider_OnBeforeKeysEvicted);
        }
    }
}
