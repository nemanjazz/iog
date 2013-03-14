// -----------------------------------------------------------------------
// <copyright file="ICollectedNodesProvider.cs" company="Execom">
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
    /// Provider which keeps nodes which may be collected for given snapshot
    /// </summary>
    /// <typeparam name="TIdentifier">Type of identifier</typeparam>
    /// <typeparam name="TNodeData">Type of node data</typeparam>
    /// <typeparam name="TEdgeData">Type of edge data</typeparam>
    /// <author>Nenad Sabo</author>
    internal interface ICollectedNodesProvider<TIdentifier, TNodeData, TEdgeData> where TEdgeData : IComparable<TEdgeData>
    {
        /// <summary>
        /// Stores collectable nodes for a change set
        /// </summary>
        /// <param name="changeSet">Change set</param>
        /// <param name="mutableParentMap">Parent map of mutable data</param>
        /// <param name="immutableParentMap">Parent map of immutable data</param>
        void StoreChangeset(AppendableChangeSet<TIdentifier, TNodeData, TEdgeData> changeSet, IParentMapProvider<TIdentifier, TNodeData, TEdgeData> mutableParentMap, IParentMapProvider<TIdentifier, TNodeData, TEdgeData> immutableParentMap);

        /// <summary>
        /// Returns edges to collectable nodes for a snapshot
        /// </summary>
        /// <param name="snapshotId">Snapshot identifier</param>
        /// <returns>Enumerator of edges towards collectable nodes</returns>
        IEnumerator<Edge<TIdentifier, TEdgeData>> GetEdges(Guid snapshotId);

        /// <summary>
        /// Removes collectable node data for a snapshot
        /// </summary>
        /// <param name="snapshotId">Snapshot identifier</param>
        void Cleanup(Guid snapshotId);
    }
}
