// -----------------------------------------------------------------------
// <copyright file="AppendableChangeSet.cs" company="Execom">
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
    /// Defines a change set which contains entire change-tree which is appendable to snapshots 
    /// </summary>
    /// <typeparam name="TIdentifier">Type of identifier</typeparam>
    /// <typeparam name="TNodeData">Type of node data</typeparam>
    /// <typeparam name="TEdgeData">Type of edge data</typeparam>
    /// <author>Nenad Sabo</author>
    internal class AppendableChangeSet<TIdentifier, TNodeData, TEdgeData> where TEdgeData : IComparable<TEdgeData>
    {
        /// <summary>
        /// Snapshot ID which is considered a base
        /// </summary>
        public TIdentifier SourceSnapshotId;

        /// <summary>
        /// Snapshot ID for new version
        /// </summary>
        public TIdentifier DestinationSnapshotId;

        /// <summary>
        /// Nodes which are forming the change delta tree.
        /// </summary>
        public INodeProvider<TIdentifier, TNodeData, TEdgeData> Nodes;

        /// <summary>
        /// Mapping from old->new node id
        /// </summary>
        public Dictionary<TIdentifier, TIdentifier> Mapping;

        /// <summary>
        /// Defines actions which were performed to create the change set
        /// </summary>
        public Dictionary<TIdentifier, NodeState> NodeStates;

        /// <summary>
        /// Contains node IDs which are reused from previous snapshot
        /// </summary>
        public Hashtable ReusedNodes;

        /// <summary>
        /// Creates new instance of AppendableChangeSet type
        /// </summary>
        /// <param name="sourceSnapshotId">Snapshot ID which is considered a base</param>
        /// <param name="destinationSnapshotId">Snapshot ID for new version</param>
        /// <param name="nodes">Nodes which are forming the change delta tree</param>
        /// <param name="mapping">Mapping from old->new node id</param>
        public AppendableChangeSet(TIdentifier sourceSnapshotId, TIdentifier destinationSnapshotId, INodeProvider<TIdentifier, TNodeData, TEdgeData> nodes, Dictionary<TIdentifier, TIdentifier> mapping, Dictionary<TIdentifier, NodeState> nodeStates, Hashtable reusedNodes)
        {
            this.SourceSnapshotId = sourceSnapshotId;
            this.DestinationSnapshotId = destinationSnapshotId;
            this.Nodes = nodes;
            this.Mapping = mapping;
            this.NodeStates = nodeStates;
            this.ReusedNodes = reusedNodes;
        }
    }
}
