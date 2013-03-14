// -----------------------------------------------------------------------
// <copyright file="INodeProvider.cs" company="Execom">
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

namespace Execom.IOG.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Collections;

    /// <summary>
    /// Defines node access options
    /// </summary>
    public enum NodeAccess
    {
        /// <summary>
        /// Node can only be read from
        /// </summary>
        Read,
        /// <summary>
        /// Node can be used for reading and writting
        /// </summary>
        ReadWrite
    }

    /// <summary>
    /// Defines a node provider interface. It is able to store and retreive nodes by an identifier.
    /// </summary>
    /// <typeparam name="TIdentifier">Type of identifier</typeparam>
    /// <typeparam name="TNodeData">Type of node data</typeparam>
    /// <typeparam name="TEdgeData">Type of edge data</typeparam>
    /// <author>Nenad Sabo</author>
    public interface INodeProvider<TIdentifier, TNodeData, TEdgeData> where TEdgeData : IComparable<TEdgeData>
    {
        /// <summary>
        /// Adds or updates node in the collection
        /// </summary>
        /// <param name="identifier">Node identifier</param>
        /// <param name="node">Node to add or update</param>
        void SetNode(TIdentifier identifier, Node<TIdentifier, TNodeData, TEdgeData> node);

        /// <summary>
        /// Gets node from collection by an identifier
        /// </summary>
        /// <param name="nodeId">Node identifier</param>
        /// <param name="access">Node access option</param>
        /// <returns>Node object</returns>
        Node<TIdentifier, TNodeData, TEdgeData> GetNode(TIdentifier nodeId, NodeAccess access);

        /// <summary>
        /// Determines if collection contains a node with given identifier
        /// </summary>
        /// <param name="identifier">Identifier to check</param>
        /// <returns>True if contains</returns>
        bool Contains(TIdentifier identifier);

        /// <summary>
        /// Removes node from collection by an identifier
        /// </summary>
        /// <param name="identifier">Node identifier</param>
        void Remove(TIdentifier identifier);        

        /// <summary>
        /// Returns node enumeration containing all nodes in the provider
        /// </summary>
        /// <returns>Nodes identifiers</returns>
        IEnumerable EnumerateNodes();

        /// <summary>
        /// Clears nodes from the provider
        /// </summary>
        void Clear();
    }
}
