// -----------------------------------------------------------------------
// <copyright file="DirectNodeProvider.cs" company="Execom">
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
    /// Defines node provider which relies on given key-value storage.
    /// Can be used by multiple threads.
    /// </summary>
    /// <typeparam name="TIdentifier">Type of identifier</typeparam>
    /// <typeparam name="TNodeData">Type of node data</typeparam>
    /// <typeparam name="TEdgeData">Type of edge data</typeparam>
    /// <author>Nenad Sabo</author>
    internal class LimitedDirectNodeProviderSafe<TIdentifier, TNodeData, TEdgeData> : IEvictingProvider<TIdentifier>, INodeProvider<TIdentifier, TNodeData, TEdgeData> where TEdgeData : IComparable<TEdgeData>
    {
        /// <summary>
        /// Underlying storage
        /// </summary>
        private IKeyValueStorage<TIdentifier, object> storage;

        /// <summary>
        /// Determines if storage contains direct objects or needs to perform serialization after update
        /// </summary>
        private bool forceUpdate;

        private ReaderWriterLock rwLock;
         
        /// <summary>
        /// Creates new instance of DirectStorageNodeProvider class
        /// </summary>
        /// <param name="storage">Underlying storage for node objects</param>
        /// <param name="forceUpdate">Defines if updates should be forced after modification</param>
        public LimitedDirectNodeProviderSafe(IKeyValueStorage<TIdentifier, object> storage, bool forceUpdate)
        {
            if (!(storage is IEvictingStorage<TIdentifier>))
            {
                throw new ArgumentException("Evicting storage expected");
            }

            this.storage = storage;
            this.forceUpdate = forceUpdate;
            this.rwLock = new ReaderWriterLock();
        }

        /// <summary>
        /// Adds node to collection
        /// </summary>
        /// <param name="identifier">Node identifier</param>
        /// <param name="node">Node to add</param>
        public void SetNode(TIdentifier identifier, Node<TIdentifier, TNodeData, TEdgeData> node)
        {
            rwLock.AcquireWriterLock(-1);
            try
            {
                storage.AddOrUpdate(identifier, node);
            }
            finally
            {
                rwLock.ReleaseLock();
            }
        }

        /// <summary>
        /// Gets node from collection by an identifier
        /// </summary>
        /// <param name="nodeId">Node identifier</param>
        /// <returns>Node object</returns>
        public Node<TIdentifier, TNodeData, TEdgeData> GetNode(TIdentifier nodeId, NodeAccess access)
        {
            rwLock.AcquireReaderLock(-1);
            try
            {
                return (Node<TIdentifier, TNodeData, TEdgeData>)storage.Value(nodeId);
            }
            finally
            {
                rwLock.ReleaseLock();
            }
        }

        /// <summary>
        /// Determines if collection contains a node with given identifier
        /// </summary>
        /// <param name="identifier">Identifier to check</param>
        /// <returns>True if contains</returns>
        public bool Contains(TIdentifier identifier)
        {
            rwLock.AcquireReaderLock(-1);
            try
            {
                return storage.Contains(identifier);
            }
            finally
            {
                rwLock.ReleaseLock();
            }
        }

        /// <summary>
        /// Removes node from collection by an identifier
        /// </summary>
        /// <param name="identifier">Node identifier</param>
        public void Remove(TIdentifier identifier)
        {
            rwLock.AcquireWriterLock(-1);
            try
            {
                storage.Remove(identifier);
            }
            finally
            {
                rwLock.ReleaseLock();
            }      
        }        

        /// <summary>
        /// Returns node enumeration containing all nodes in the storage
        /// </summary>
        /// <returns>Nodes identifiers</returns>
        public IEnumerable EnumerateNodes()
        {
            rwLock.AcquireReaderLock(-1);
            try
            {
                return storage.ListKeys();
            }
            finally
            {
                rwLock.ReleaseLock();
            }
        }

        /// <summary>
        /// Clears nodes from the storage
        /// </summary>
        public void Clear()
        {
            rwLock.AcquireWriterLock(-1);
            try
            {
                storage.Clear();
            }
            finally
            {
                rwLock.ReleaseLock();
            }
        }

        public event EventHandler<KeysEvictedEventArgs<TIdentifier>> OnBeforeKeysEvicted
        {
            add
            {
                (storage as IEvictingStorage<TIdentifier>).OnBeforeKeysEvicted += value;
            }

            remove
            {
                (storage as IEvictingStorage<TIdentifier>).OnBeforeKeysEvicted -= value;
            }
        }
    }
}
