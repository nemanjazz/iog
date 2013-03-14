// -----------------------------------------------------------------------
// <copyright file="CollectableNodesProvider.cs" company="Execom">
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
    using System.Collections;
    using Execom.IOG.Services.Data;

    /// <summary>
    /// Provider which keeps nodes which may be collected for given snapshot
    /// </summary>
    /// <typeparam name="TIdentifier">Type of identifier</typeparam>
    /// <typeparam name="TNodeData">Type of node data</typeparam>
    /// <typeparam name="TEdgeData">Type of edge data</typeparam>
    /// <author>Nenad Sabo</author>
    internal class CollectedNodesProvider : ICollectedNodesProvider<Guid, object, EdgeData>
    {
        private INodeProvider<Guid, object, EdgeData> nodes;
        private INodeProvider<Guid, object, EdgeData> dataNodes;
        private const int TreeOrder = 30;

        /// <summary>
        /// Creates new instance of CollectedNodesProvider type
        /// </summary>
        /// <param name="nodes">Provider of nodes which will hold the collected node data</param>
        /// <param name="dataNodes">Provider of nodes which hold the actual object data</param>
        public CollectedNodesProvider(INodeProvider<Guid, object, EdgeData> nodes, INodeProvider<Guid, object, EdgeData> dataNodes)
        {
            this.nodes = nodes;
            this.dataNodes = dataNodes;
        }

        /// <summary>
        /// Stores collectable nodes for a change set
        /// </summary>
        /// <param name="changeSet">Change set</param>
        /// <param name="mutableParentMap">Parent map of mutable data</param>
        /// <param name="immutableParentMap">Parent map of immutable data</param>
        public void StoreChangeset(AppendableChangeSet<Guid, object, EdgeData> changeSet, IParentMapProvider<Guid, object, EdgeData> mutableParentMap, IParentMapProvider<Guid, object, EdgeData> immutableParentMap)
        {
            Guid snapshotId = changeSet.SourceSnapshotId;

            var snapshotRoot = BPlusTreeOperations.CreateRootNode(NodeType.Collection, snapshotId);
            nodes.SetNode(snapshotId, snapshotRoot);

            Hashtable collectedNodes = new Hashtable();

            GetCollectedNodesRecursive(changeSet.SourceSnapshotId, changeSet, mutableParentMap, immutableParentMap, collectedNodes, new Hashtable());

            foreach (Guid nodeId in collectedNodes.Keys)
            {
                BPlusTreeOperations.InsertEdge(nodes, snapshotId, new Edge<Guid, EdgeData>(nodeId, new EdgeData(EdgeType.ListItem, nodeId)), TreeOrder);
            }
        }

        /// <summary>
        /// Returns edges to collectable nodes for a snapshot
        /// </summary>
        /// <param name="snapshotId">Snapshot identifier</param>
        /// <returns>Enumerator of edges towards collectable nodes</returns>
        public IEnumerator<Edge<Guid, EdgeData>> GetEdges(Guid snapshotId)
        {
            return BPlusTreeOperations.GetEnumerator(nodes, snapshotId, EdgeType.ListItem);
        }

        /// <summary>
        /// Removes collectable node data for a snapshot
        /// </summary>
        /// <param name="snapshotId">Snapshot identifier</param>
        public void Cleanup(Guid snapshotId)
        {
            DeleteTree(snapshotId);
        }

        private void GetCollectedNodesRecursive(Guid nodeId, AppendableChangeSet<Guid, object, EdgeData> changeSet, IParentMapProvider<Guid, object, EdgeData> mutableParentMap, IParentMapProvider<Guid, object, EdgeData> immutableParentMap, Hashtable collectedNodes, Hashtable visitedNodes)
        {
            if (visitedNodes.ContainsKey(nodeId))
            {
                return;
            }

            visitedNodes.Add(nodeId, null);

            if (changeSet.ReusedNodes.ContainsKey(nodeId))
            {
                // Reused nodes and their children are not to be collected
                return;
            }

            if (HasReusedParent(nodeId, changeSet, mutableParentMap, immutableParentMap, collectedNodes, new Hashtable()))
            {
                return;
            }

            collectedNodes.Add(nodeId, null);

            var node = dataNodes.GetNode(nodeId, NodeAccess.Read);

            foreach (var edge in node.Edges.Values)
            {
                if (edge.Data.Semantic != EdgeType.OfType && edge.ToNodeId != Constants.NullReferenceNodeId)
                {
                    GetCollectedNodesRecursive(edge.ToNodeId, changeSet, mutableParentMap, immutableParentMap, collectedNodes, visitedNodes);
                }
            }
        }

        private bool HasReusedParent(Guid nodeId, AppendableChangeSet<Guid, object, EdgeData> changeSet, IParentMapProvider<Guid, object, EdgeData> mutableParentMap, IParentMapProvider<Guid, object, EdgeData> immutableParentMap, Hashtable collectedNodes, Hashtable visitedNodes)
        {
            if (visitedNodes.ContainsKey(nodeId))
            {
                return (bool)visitedNodes[nodeId];
            }

            visitedNodes.Add(nodeId, false);

            if (changeSet.ReusedNodes.ContainsKey(nodeId))
            {
                return true;
            }

            if (collectedNodes.ContainsKey(nodeId))
            {
                return false;
            }

            using (var mutableParentEnumerator = mutableParentMap.ParentEdges(changeSet.SourceSnapshotId, nodeId))
            {
                if (mutableParentEnumerator != null)
                {
                    while (mutableParentEnumerator.MoveNext())
                    {
                        if (HasReusedParent(mutableParentEnumerator.Current.ToNodeId, changeSet, mutableParentMap, immutableParentMap, collectedNodes, visitedNodes))
                        {
                            return true;
                        }
                    }
                }
            }

            using (var immutableParentEnumerator = immutableParentMap.ParentEdges(changeSet.SourceSnapshotId, nodeId))
            {
                if (immutableParentEnumerator != null)
                {
                    while (immutableParentEnumerator.MoveNext())
                    {
                        if (HasReusedParent(immutableParentEnumerator.Current.ToNodeId, changeSet, mutableParentMap, immutableParentMap, collectedNodes, visitedNodes))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
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
