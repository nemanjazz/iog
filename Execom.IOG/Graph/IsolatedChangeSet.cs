// -----------------------------------------------------------------------
// <copyright file="IsolatedChangeSet.cs" company="Execom">
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

    /// <summary>
    /// Defines a change set which contains entire change-tree which is appendable to snapshots 
    /// </summary>
    /// <typeparam name="TIdentifier">Type of identifier</typeparam>
    /// <typeparam name="TNodeData">Type of node data</typeparam>
    /// <typeparam name="TEdgeData">Type of edge data</typeparam>
    /// <author>Nenad Sabo</author>
    [Serializable]
    public class IsolatedChangeSet<TIdentifier, TNodeData, TEdgeData> where TEdgeData : IComparable<TEdgeData>
    {
        /// <summary>
        /// Snapshot ID which is considered a base
        /// </summary>
        public TIdentifier SourceSnapshotId;

        /// <summary>
        /// Nodes which are forming the changes
        /// </summary>
        public INodeProvider<TIdentifier, TNodeData, TEdgeData> Nodes; 

        /// <summary>
        /// Defines actions which were performed to create the change set
        /// </summary>
        public Dictionary<TIdentifier, NodeState> NodeStates;

        /// <summary>
        /// Creates new instance of IsolatedChangeSet type
        /// </summary>
        /// <param name="sourceSnapshotId">Snapshot ID which is considered a base</param>
        /// <param name="nodes">Nodes which are forming the change delta tree</param>
        public IsolatedChangeSet(TIdentifier sourceSnapshotId, INodeProvider<TIdentifier, TNodeData, TEdgeData> nodes, Dictionary<TIdentifier, NodeState> nodeStates)
        {
            this.SourceSnapshotId = sourceSnapshotId;
            this.Nodes = nodes;
            this.NodeStates = nodeStates;
        }
    }
}
