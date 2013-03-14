// -----------------------------------------------------------------------
// <copyright file="IChangeSetProvider.cs" company="Execom">
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
    /// Provider of change set data history
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal interface IChangeSetProvider<TIdentifier, TNodeData, TEdgeData> where TEdgeData : IComparable<TEdgeData>
    {
        /// <summary>
        /// Provides change set edges for given snapshot
        /// </summary>
        /// <param name="snapshotId">Snapshot ID</param>
        /// <returns>Change set edge enumerator</returns>
        IEnumerator<Edge<Guid, EdgeData>> GetChangeSetEdges(Guid snapshotId);

        /// <summary>
        /// Stores the change set mapping
        /// </summary>
        /// <param name="changeSet">Change set to store</param>
        void SetChangeSet(AppendableChangeSet<TIdentifier, TNodeData, TEdgeData> changeSet);

        /// <summary>
        /// Determines if there is change set information for given snapshot
        /// </summary>
        /// <param name="snapshotId">Snapshot Id to query</param>
        /// <returns>True if data exists</returns>
        bool ContainsSnapshot(Guid snapshotId);

        /// <summary>
        /// Removes snapshot information
        /// </summary>
        /// <param name="snapshotId">Snapshot ID</param>
        void RemoveChangeSet(Guid snapshotId);
    }
}
