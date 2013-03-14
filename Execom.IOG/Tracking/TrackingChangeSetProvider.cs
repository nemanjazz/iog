// -----------------------------------------------------------------------
// <copyright file="TrackingChangeSetProvider.cs" company="Execom">
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

namespace Execom.IOG.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Execom.IOG.Graph;
    using Execom.IOG.Services.Data;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Provider of change set history which keeps data in 2 layered B+ tree:
    /// - 1st layer contains snapshots, all within a single root with Id = Constants.SnapshotsNodeId
    /// - 2nd layer is withing one snapshot and contains mapping of changes
    /// Mapping is stored as old->new Id. Old Id is tree key, new Id is where tree edge is pointing to.
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal class TrackingChangeSetProvider : IChangeSetProvider<Guid, object, EdgeData>
    {
        /// <summary>
        /// Node storage
        /// </summary>
        private INodeProvider<Guid, object, EdgeData> nodes;

        /// <summary>
        /// Tree order
        /// </summary>
        private const int TreeOrder = 100;

        private object sync = new object();

        /// <summary>
        /// Creates new instance of TrackingChangeSetProvider type
        /// </summary>
        /// <param name="nodes">Node provider which will store the tracking information</param>
        public TrackingChangeSetProvider(INodeProvider<Guid, object, EdgeData> nodes)
        {
            this.nodes = nodes;

            InitializeRoot();
        }

        /// <summary>
        /// Initialize global root node if not found
        /// </summary>
        private void InitializeRoot()
        {
            if (!nodes.Contains(Constants.SnapshotsNodeId))
            {
                var node = BPlusTreeOperations.CreateRootNode(NodeType.Collection, Constants.SnapshotsNodeId);
                nodes.SetNode(Constants.SnapshotsNodeId, node);
            }
        }


        /// <summary>
        /// Stores the change set
        /// </summary>
        /// <param name="changeSet">Change set to store</param>
        public void SetChangeSet(AppendableChangeSet<Guid, object, EdgeData> changeSet)
        {
            lock (sync)
            {
                var snapshotNode = BPlusTreeOperations.CreateRootNode(NodeType.Dictionary, changeSet.DestinationSnapshotId);

                nodes.SetNode(changeSet.DestinationSnapshotId, snapshotNode);

                // Add all changes from changeset to snapshot tree. 
                foreach (var item in changeSet.Mapping)
                {
                    BPlusTreeOperations.InsertEdge(nodes, changeSet.DestinationSnapshotId, new Edge<Guid, EdgeData>(item.Value, new EdgeData(EdgeType.ListItem, item.Key)), TreeOrder);
                }

                // Add snapshot to collection
                BPlusTreeOperations.InsertEdge(nodes, Constants.SnapshotsNodeId, new Edge<Guid, EdgeData>(changeSet.DestinationSnapshotId, new EdgeData(EdgeType.ListItem, changeSet.DestinationSnapshotId)), TreeOrder);
            }
        }

        /// <summary>
        /// Provides change set edges for given snapshot
        /// </summary>
        /// <param name="snapshotId">Snapshot ID</param>
        /// <returns>Change set edge enumerator</returns>
        public IEnumerator<Edge<Guid, EdgeData>> GetChangeSetEdges(Guid snapshotId)
        {
            lock (sync)
            {
                Edge<Guid, EdgeData> edge = null;
                if (BPlusTreeOperations.TryFindEdge(nodes, Constants.SnapshotsNodeId, new EdgeData(EdgeType.ListItem, snapshotId), out edge))
                {
                    return BPlusTreeOperations.GetEnumerator(nodes, snapshotId, EdgeType.ListItem);
                }
                else
                { 
                    // Returtn empty enumerator
                    return new Collection<Edge<Guid, EdgeData>>().GetEnumerator(); 
                }
            }
        }

        /// <summary>
        /// Determines if there is change set information for given snapshot
        /// </summary>
        /// <param name="snapshotId">Snapshot Id to query</param>
        /// <returns>True if data exists</returns>
        public bool ContainsSnapshot(Guid snapshotId)
        {
            lock (sync)
            {
                Edge<Guid, EdgeData> edge = null;
                return BPlusTreeOperations.TryFindEdge(nodes, Constants.SnapshotsNodeId, new EdgeData(EdgeType.ListItem, snapshotId), out edge);
            }
        }

        /// <summary>
        /// Removes information for a snapshot
        /// </summary>
        /// <param name="snapshotId">Snapshot ID to remove</param>
        public void RemoveChangeSet(Guid snapshotId)
        {
            lock (sync)
            {
                // Remove from snapshots list
                if (BPlusTreeOperations.RemoveEdge(nodes, Constants.SnapshotsNodeId, new EdgeData(EdgeType.ListItem, snapshotId), TreeOrder))
                {
                    DeleteTree(snapshotId);
                }
            }
        }

        private void DeleteTree(Guid nodeId)
        {
            var node = nodes.GetNode(nodeId, NodeAccess.Read);

            if (node.Data.Equals(BPlusTreeOperations.InternalNodeData))
            {
                foreach (var edge in node.Edges)
                {
                    DeleteTree(edge.Value.ToNodeId);
                }                
            }

            nodes.Remove(nodeId);
        }
    }
}
