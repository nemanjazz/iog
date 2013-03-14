// -----------------------------------------------------------------------
// <copyright file="IParentMapProvider.cs" company="Execom">
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
    /// Describes interface which provides parents of nodes within given snapshot
    /// </summary>
    /// <typeparam name="TIdentifier">Type of node identifier</typeparam>
    internal interface IParentMapProvider<TIdentifier, TNodeData, TEdgeData> where TEdgeData : IComparable<TEdgeData>
    {
        /// <summary>
        /// Returns list of parents for given node witin a snapshot
        /// </summary>
        /// <param name="snapshotId">Snapshot within parents are requested</param>
        /// <param name="nodeId">Node which parents are requested</param>
        /// <returns>List of parent node identifiers for each node in dictionary format</returns>
        IEnumerator<Edge<TIdentifier, TEdgeData>> ParentEdges(TIdentifier snapshotId, TIdentifier nodeId);

        /// <summary>
        /// Updates parent information based on change set
        /// </summary>
        /// <param name="changeSet">Data change description</param>
        void UpdateParents(AppendableChangeSet<TIdentifier, TNodeData, TEdgeData> changeSet, ICollectedNodesProvider<TIdentifier, TNodeData, TEdgeData> collectedNodesProvider);
    }
}
