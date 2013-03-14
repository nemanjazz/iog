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
    /// Can NOT be used by multiple threads.
    /// </summary>
    /// <typeparam name="TIdentifier">Type of identifier</typeparam>
    /// <typeparam name="TNodeData">Type of node data</typeparam>
    /// <typeparam name="TEdgeData">Type of edge data</typeparam>
    /// <author>Nenad Sabo</author>
    [Serializable]
    internal class DirectNodeProviderUnsafe<TIdentifier, TNodeData, TEdgeData> : INodeProvider<TIdentifier, TNodeData, TEdgeData> where TEdgeData : IComparable<TEdgeData>
    {
        /// <summary>
        /// Underlying storage
        /// </summary>
        private IKeyValueStorage<TIdentifier, object> storage;

        /// <summary>
        /// Determines if storage contains direct objects or needs to perform serialization after update
        /// </summary>
        private bool forceUpdate;

        /// <summary>
        /// Creates new instance of DirectStorageNodeProvider class
        /// </summary>
        /// <param name="storage">Underlying storage for node objects</param>
        /// <param name="forceUpdate">Defines if updates should be forced after modification</param>
        public DirectNodeProviderUnsafe(IKeyValueStorage<TIdentifier, object> storage, bool forceUpdate)
        {
            this.storage = storage;
            this.forceUpdate = forceUpdate;
        }

        /// <summary>
        /// Adds node to collection
        /// </summary>
        /// <param name="identifier">Node identifier</param>
        /// <param name="node">Node to add</param>
        public void SetNode(TIdentifier identifier, Node<TIdentifier, TNodeData, TEdgeData> node)
        {
            storage.AddOrUpdate(identifier, node);
        }

        /// <summary>
        /// Gets node from collection by an identifier
        /// </summary>
        /// <param name="nodeId">Node identifier</param>
        /// <returns>Node object</returns>
        public Node<TIdentifier, TNodeData, TEdgeData> GetNode(TIdentifier nodeId, NodeAccess access)
        {
            return (Node<TIdentifier, TNodeData, TEdgeData>)storage.Value(nodeId);
        }

        /// <summary>
        /// Determines if collection contains a node with given identifier
        /// </summary>
        /// <param name="identifier">Identifier to check</param>
        /// <returns>True if contains</returns>
        public bool Contains(TIdentifier identifier)
        {
            return storage.Contains(identifier);
        }

        /// <summary>
        /// Removes node from collection by an identifier
        /// </summary>
        /// <param name="identifier">Node identifier</param>
        public void Remove(TIdentifier identifier)
        {
            storage.Remove(identifier);
        }

        /// <summary>
        /// Returns node enumeration containing all nodes in the storage
        /// </summary>
        /// <returns>Nodes identifiers</returns>
        public IEnumerable EnumerateNodes()
        {
            return storage.ListKeys();
        }

        /// <summary>
        /// Clears nodes from the storage
        /// </summary>
        public void Clear()
        {
            storage.Clear();
        }
    }
}
