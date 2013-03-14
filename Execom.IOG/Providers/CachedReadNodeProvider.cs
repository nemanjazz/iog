// -----------------------------------------------------------------------
// <copyright file="CachedReadNodeProvider.cs" company="Execom">
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
    using System.Collections;

    /// <summary>
    /// Defines node provider with read caching. It assumes that underlying providers have multi-read excluive-write capability.
    /// </summary>
    /// <typeparam name="TIdentifier">Type of identifier</typeparam>
    /// <typeparam name="TNodeData">Type of node data</typeparam>
    /// <typeparam name="TEdgeData">Type of edge data</typeparam>
    /// <author>Nenad Sabo</author>
    public class CachedReadNodeProvider<TIdentifier, TNodeData, TEdgeData> : INodeProvider<TIdentifier, TNodeData, TEdgeData> where TEdgeData : IComparable<TEdgeData>
    {
        /// <summary>
        /// Underlying node provider
        /// </summary>
        private INodeProvider<TIdentifier, TNodeData, TEdgeData> parentProvider;

        /// <summary>
        /// Data cache provider
        /// </summary>
        private INodeProvider<TIdentifier, TNodeData, TEdgeData> cacheProvider;

        private ReaderWriterLock rwLock;

        /// <summary>
        /// Creates new instance of CachedReadNodeProvider type
        /// </summary>
        /// <param name="parentProvider"></param>
        /// <param name="cacheProvider"></param>
        public CachedReadNodeProvider(INodeProvider<TIdentifier, TNodeData, TEdgeData> parentProvider, INodeProvider<TIdentifier, TNodeData, TEdgeData> cacheProvider)
        {
            this.parentProvider = parentProvider;
            this.cacheProvider = cacheProvider;
            rwLock = new ReaderWriterLock();
        }

        /// <summary>
        /// Adds node to collection and cache
        /// </summary>
        /// <param name="identifier">Node identifier</param>
        /// <param name="node">Node to add</param>
        public void SetNode(TIdentifier identifier, Node<TIdentifier, TNodeData, TEdgeData> node)
        {
            rwLock.AcquireWriterLock(-1);
            try
            {
                cacheProvider.SetNode(identifier, node);
                parentProvider.SetNode(identifier, node);
            }
            finally
            {
                rwLock.ReleaseLock();
            }
        }

        /// <summary>
        /// Gets node from collection by an identifier. First attempt to read from cache.
        /// </summary>
        /// <param name="nodeId">Node identifier</param>
        /// <returns>Node object</returns>
        public Node<TIdentifier, TNodeData, TEdgeData> GetNode(TIdentifier identifier, NodeAccess access)
        {
            rwLock.AcquireReaderLock(-1);
            try
            {
                // Read from cache
                var node = cacheProvider.GetNode(identifier, access);

                if (node != null)
                {
                    return node;
                }
                else
                {
                    // Read from parent
                    node = parentProvider.GetNode(identifier, access);
                    cacheProvider.SetNode(identifier, node);
                    return node;
                }
            }
            finally
            {
                rwLock.ReleaseLock();
            }
        }

        /// <summary>
        /// Determines if collection contains a node with given identifier. Check is done on cache first.
        /// </summary>
        /// <param name="identifier">Identifier to check</param>
        /// <returns>True if contains</returns>
        public bool Contains(TIdentifier identifier)
        {
            rwLock.AcquireReaderLock(-1);
            try
            {
                if (cacheProvider.Contains(identifier))
                {
                    return true;
                }
                return parentProvider.Contains(identifier);
            }
            finally
            {
                rwLock.ReleaseLock();
            }
        }

        /// <summary>
        /// Removes node from collection by an identifier. It is also removed from cache.
        /// </summary>
        /// <param name="identifier">Node identifier</param>
        public void Remove(TIdentifier identifier)
        {
            rwLock.AcquireWriterLock(-1);
            try
            {
                cacheProvider.Remove(identifier);
                parentProvider.Remove(identifier);
            }
            finally
            {
                rwLock.ReleaseLock();
            }
        }

        /// <summary>
        /// Returns node enumeration containing all nodes in the parent provider
        /// </summary>
        /// <returns>Nodes identifiers</returns>
        public IEnumerable EnumerateNodes()
        {
            rwLock.AcquireReaderLock(-1);
            try
            {
                return parentProvider.EnumerateNodes();
            }
            finally
            {
                rwLock.ReleaseLock();
            }
        }

        /// <summary>
        /// Clears nodes from the parent provider and cache
        /// </summary>
        public void Clear()
        {
            rwLock.AcquireWriterLock(-1);
            try
            {
                parentProvider.Clear();
                cacheProvider.Clear();
            }
            finally
            {
                rwLock.ReleaseLock();
            }
        }
    }
}
