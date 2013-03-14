// -----------------------------------------------------------------------
// <copyright file="Node.cs" company="Execom">
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
    using System.Collections.ObjectModel;

    [Serializable]
    public enum NodeType : byte
    {
        TypesRoot,
        SnapshotsRoot,
        Snapshot,
        Type,
        Scalar,
        Object,
        Collection,
        Dictionary,
        TypeMember,
        TreeInternal,
        TreeLeaf
    }

    /// <summary>
    /// Defines a node in a graph
    /// </summary>
    /// <typeparam name="TIdentifier">Type of identifier</typeparam>
    /// <typeparam name="TNodeData">Type of node data</typeparam>
    /// <typeparam name="TEdgeData">Type of edge data</typeparam>
    /// <author>Nenad Sabo</author>
    [Serializable]
    public class Node<TIdentifier, TNodeData, TEdgeData> where TEdgeData : IComparable<TEdgeData>
    {        
        /// <summary>
        /// List of edges sorted by edge data
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification="Due to performance reasons this remains a field")]
        public readonly SortedList<TEdgeData, Edge<TIdentifier, TEdgeData>> Edges;

        /// <summary>
        /// List of scalar values sorted by an identifier
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Due to performance reasons this remains a field")]
        public readonly Dictionary<TIdentifier, object> Values;       

        /// <summary>
        /// Node data
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Due to performance reasons this remains a field")]
        public TNodeData Data;

        /// <summary>
        /// Identifier of previous node
        /// </summary>
        public TIdentifier Previous;

        /// <summary>
        /// Determines node semantic type
        /// </summary>
        public NodeType NodeType;

        /// <summary>
        /// Defines if node was commited
        /// </summary>
        public bool Commited;

        /// <summary>
        /// Initializes a new instance of the Node class
        /// </summary>
        /// <param name="nodeType">Node semantic type</param>
        /// <param name="data">Node data</param>
        public Node(NodeType nodeType, TNodeData data)
        {
            NodeType = nodeType;
            Data = data;
            Edges = new SortedList<TEdgeData, Edge<TIdentifier, TEdgeData>>();
            Values = new Dictionary<TIdentifier, object>();
        }

        public Node(NodeType nodeType, TNodeData data, IDictionary<TEdgeData, Edge<TIdentifier, TEdgeData>> edgeList)
        {
            NodeType = nodeType;
            Data = data;
            Edges = new SortedList<TEdgeData, Edge<TIdentifier, TEdgeData>>(edgeList);
            Values = new Dictionary<TIdentifier, object>();
        }

        public Node(NodeType nodeType, TNodeData data, IDictionary<TEdgeData, Edge<TIdentifier, TEdgeData>> edgeList, IDictionary<TIdentifier, object> valueList)
        {
            NodeType = nodeType;
            Data = data;
            Edges = new SortedList<TEdgeData, Edge<TIdentifier, TEdgeData>>(edgeList);
            Values = new Dictionary<TIdentifier, object>(valueList);            
        }         

        /// <summary>
        /// Returns edge by given edge data
        /// </summary>
        /// <param name="edgeData">Edge data to find</param>
        /// <returns>Edge</returns>
        public Edge<TIdentifier, TEdgeData> FindEdge(TEdgeData edgeData)
        {
            return Edges[edgeData];
        }

        /// <summary>
        /// Changes node data
        /// </summary>
        /// <param name="data">New data</param>
        public void SetData(TNodeData data)
        {            
            Data = data;
        }

        /// <summary>
        /// Changes node type
        /// </summary>
        /// <param name="nodeType"></param>
        public void SetType(NodeType nodeType)
        {
            NodeType = nodeType;
        }

        /// <summary>
        /// Adds new edge
        /// </summary>
        /// <param name="edge">New edge</param>
        public void AddEdge(Edge<TIdentifier, TEdgeData> edge)
        {
           Edges.Add(edge.Data, edge);
        }        

        /// <summary>
        /// Sets new target node for an edge
        /// </summary>
        /// <param name="data">Edge to set</param>
        /// <param name="toNodeId">New target node</param>
        public void SetEdgeToNode(TEdgeData data, TIdentifier toNodeId)
        {
            Edges[data].ToNodeId = toNodeId;
        }
    }
}
