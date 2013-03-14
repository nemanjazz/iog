// -----------------------------------------------------------------------
// <copyright file="SnapshotsService.cs" company="Execom">
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

namespace Execom.IOG.Services.Data
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Execom.IOG.Graph;
    using Execom.IOG.Storage;
    using System.Collections.ObjectModel;
    using System.Threading;

    /// <summary>
    /// Service with operations for snapshot management
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal class SnapshotsService
    {
        /// <summary>
        /// Node provider
        /// </summary>
        private INodeProvider<Guid, object, EdgeData> provider;

        /// <summary>
        /// Initializes a new instance of the TypesService class
        /// </summary>
        /// <param name="provider">Node provider which contains nodes</param>
        public SnapshotsService(INodeProvider<Guid, object, EdgeData> provider)
        {
            this.provider = provider;
        }

        /// <summary>
        /// Returns root object id for given snapshot
        /// </summary>
        /// <param name="snapshotId">Snapshot ID</param>
        /// <returns>Root object ID</returns>
        public Guid GetRootObjectId(Guid snapshotId)
        {
            return provider.GetNode(snapshotId, NodeAccess.Read).FindEdge(new EdgeData(EdgeType.RootObject, null)).ToNodeId;
        }

        /// <summary>
        /// Returns last commited snapshot ID
        /// </summary>
        /// <returns>Snapshot ID</returns>
        public Guid GetLatestSnapshotId()
        {
            var node = provider.GetNode(Constants.SnapshotsNodeId, NodeAccess.Read);
            var edge = node.FindEdge(new EdgeData(EdgeType.Contains, null));
            return edge.ToNodeId;
        }

        /// <summary>
        /// Initializes snapshots node
        /// </summary>
        /// <returns>True if snapshots were initialized for the first time</returns>
        public bool InitializeSnapshots()
        {
            if (!provider.Contains(Constants.SnapshotsNodeId))
            {
                var snapshotsNode = new Node<Guid, object, EdgeData>(NodeType.SnapshotsRoot, null);
                provider.SetNode(Constants.SnapshotsNodeId, snapshotsNode);
                return true;
            }

            return false;
        }

        public void AddSnapshot(Guid snapshotId)
        {
            var node = new Node<Guid, object, EdgeData>(NodeType.SnapshotsRoot, null);
            node.AddEdge(new Edge<Guid, EdgeData>(snapshotId, new EdgeData(EdgeType.Contains, null)));
            provider.SetNode(Constants.SnapshotsNodeId, node);
        }

        /// <summary>
        /// Returns list of snapshots in sequence from destination to source snapshot
        /// </summary>
        /// <param name="source">Source snapshot ID</param>
        /// <param name="destination">Destination snapshot ID</param>
        /// <returns>List of snapshots in sequence from destination to source, including the source and destination</returns>
        public List<Guid> SnapshotsBetweenReverse(Guid source, Guid destination)
        {
            List<Guid> res = new List<Guid>();

            Guid current = destination;

            while (!current.Equals(source))
            {
                res.Add(current);
                var node = provider.GetNode(current, NodeAccess.Read);
                current = node.Previous;

                if (current.Equals(Guid.Empty))
                {
                    throw new ArgumentException("Destination snapshot is expected to be after the source snapshot");
                }
            }

            res.Add(current);

            return res;
        }

        /// <summary>
        /// Returns list of snapshots
        /// </summary>
        /// <returns>List of snapshots</returns>
        public List<Guid> ListSnapshots()
        {
            List<Guid> res = new List<Guid>();

            Guid current = GetLatestSnapshotId();

            while (!current.Equals(Guid.Empty))
            {
                res.Add(current);
                var node = provider.GetNode(current, NodeAccess.Read);
                current = node.Previous;
            }

            return res;
        }

        /// <summary>
        /// Returns list of snapshots which are all before snapshots from given list.
        /// Removes snapshots which are returned.
        /// This information is used to remove unused snapshot data.
        /// </summary>
        /// <param name="usedSnapshots">List of used snapshots</param>
        /// <returns>List of snapshots</returns>
        public List<Guid> RemoveUnusedSnapshots(Collection<Guid> usedSnapshots)
        {
            List<Guid> result = new List<Guid>();
            Guid current = GetLatestSnapshotId();
            Guid lastUsedNodeId = Guid.Empty;

            while (current != Guid.Empty)
            {
                if (usedSnapshots.Contains(current))
                {
                    // Used snapshot was found, clear all from before
                    result.Clear();
                    lastUsedNodeId = current;
                }
                else
                {
                    // Unused snapshot, add to result
                    result.Add(current);
                }

                // Move to previous
                var node = provider.GetNode(current, NodeAccess.Read);
                current = node.Previous;
            }

            var lastUsedNode = provider.GetNode(lastUsedNodeId, NodeAccess.ReadWrite);
            lastUsedNode.Previous = Guid.Empty;
            provider.SetNode(lastUsedNodeId, lastUsedNode);

            return result;
        }

        /// <summary>
        /// Remove all snapshots leaving only the last one
        /// </summary>
        public void ResetSnapshots()
        {
            var lastUsedNodeId = GetLatestSnapshotId();
            var lastUsedNode = provider.GetNode(lastUsedNodeId, NodeAccess.ReadWrite);
            lastUsedNode.Previous = Guid.Empty;
            provider.SetNode(lastUsedNodeId, lastUsedNode);
        }
    }
}
