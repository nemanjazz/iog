// -----------------------------------------------------------------------
// <copyright file="ImutableParentMapProvider.cs" company="Execom">
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
    using System.Collections;
    using Execom.IOG.Storage;
    using System.Threading;
    using Execom.IOG.Services.Runtime;

    /// <summary>
    /// Provider of parent information which stores parent data in graph for immutable portion of the graph.
    /// Data is organized 3-layers of B+ trees in following manner:
    /// - root node is stored with ID = Constants.SnapshotsNodeId
    /// - it points to parent information for snapshots in B+ tree with data (Contains, SnapshotID)
    /// - snapshot data is stored with ID = snapshotID
    /// - each snapshot node points to holder nodes in B+ tree structure with data (Contains, NodeId)
    /// - holder nodes IDs are generated new
    /// - holder stores parents of particular node in B+ tree structure with data (Contains, ParentId), pointing to ParentId
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal class ParentMapProvider : IParentMapProvider<Guid, object, EdgeData>
    {
        private INodeProvider<Guid, object, EdgeData> nodes;
        private Guid lastSnapshotId;
        private INodeProvider<Guid, object, EdgeData> dataNodeProvider;
        private bool createdNodes = false;
        private IParentMapProvider<Guid, object, EdgeData> fallbackParentMapProvider;

        private const int ParentsTreeOrder = 100;

        /// <summary>
        /// Synchronization object
        /// </summary>
        private object dataSync = new object();

        private bool filterMutable;

        /// <summary>
        /// Creates new instance of MutableParentMapProvider type
        /// </summary>
        /// <param name="nodes">Node provider which stores parent information nodes (must not be same as data nodes provider) </param>
        /// <param name="dataNodeProvider">Data nodes provider, being read only</param>
        /// <param name="fallbackParentMapProvider">Parent map provider used in case the parent information is asked for snapshot other than the last one</param>
        public ParentMapProvider(INodeProvider<Guid, object, EdgeData> nodes, INodeProvider<Guid, object, EdgeData> dataNodeProvider, IParentMapProvider<Guid, object, EdgeData> fallbackParentMapProvider, bool filterMutable)
        {
            if (nodes == dataNodeProvider)
            {
                throw new ArgumentException();
            }

            this.nodes = nodes;
            this.dataNodeProvider = dataNodeProvider;
            this.fallbackParentMapProvider = fallbackParentMapProvider;
            this.filterMutable = filterMutable;
        }        

        /// <summary>
        /// Returns list of parents for given node witin a snapshot
        /// </summary>
        /// <param name="snapshotId">Snapshot within parents are requested</param>
        /// <param name="nodeId">Node which parents are returned</param>
        /// <returns>List of parent node identifiers for each node in dictionary format, or null if there are no parent edges</returns>
        public IEnumerator<Edge<Guid, EdgeData>> ParentEdges(Guid snapshotId, Guid nodeId)
        {
            lock (dataSync)
            {
                // Generate parents if they dont exist
                if (!createdNodes)
                {
                    createdNodes = true;
                    AddParentsRecursive(snapshotId, new Hashtable(), nodes);
                    lastSnapshotId = snapshotId;
                }

                // Do we have the last snapshot in the memory
                if (snapshotId != lastSnapshotId)
                {
                    // Try going for fallback provider
                    if (fallbackParentMapProvider != null)
                    {
                        return fallbackParentMapProvider.ParentEdges(snapshotId, nodeId);
                    }
                    else
                    {
                        // If not, generate asked snapshot
                        nodes.Clear();
                        AddParentsRecursive(snapshotId, new Hashtable(), nodes);
                        lastSnapshotId = snapshotId;
                    }
                }

                // Find edge leading to holder node for given ID
                if (nodes.Contains(nodeId))
                {
                    return BPlusTreeOperations.GetEnumerator(nodes, nodeId, EdgeType.Contains);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Updates parent information based on change set
        /// </summary>
        /// <param name="changeSet">Data change description</param>
        public void UpdateParents(AppendableChangeSet<Guid, object, EdgeData> changeSet, ICollectedNodesProvider<Guid, object, EdgeData> collectedNodesProvider)
        {
            lock (dataSync)
            {
                if (changeSet.SourceSnapshotId != lastSnapshotId)
                {
                    // If the last snapshot is not in the memory, clear and return
                    nodes.Clear();
                    createdNodes = false;
                    return;
                }

                using (var enumerator = collectedNodesProvider.GetEdges(changeSet.SourceSnapshotId))
                {
                    if (enumerator != null)
                    {
                        while (enumerator.MoveNext())
                        {
                            if (nodes.Contains(enumerator.Current.ToNodeId))
                            {
                                DeleteTree(enumerator.Current.ToNodeId);
                            }

                            // Get the old node
                            var node = dataNodeProvider.GetNode(enumerator.Current.ToNodeId, NodeAccess.Read);
                            // For every edge in old node
                            foreach (var edge in node.Edges.Values)
                            {
                                if (EdgeFilter(edge))
                                {
                                    // Find holder in destination snapshot for referenced node
                                    if (nodes.Contains(edge.ToNodeId))
                                    {
                                        BPlusTreeOperations.RemoveEdge(nodes, edge.ToNodeId, new EdgeData(EdgeType.Contains, enumerator.Current.ToNodeId), ParentsTreeOrder);
                                    }
                                }
                            }
                        }
                    }
                }


                // Add new node ids to map
                foreach (Guid nodeId in changeSet.Nodes.EnumerateNodes())
                {
                    var holderNode = BPlusTreeOperations.CreateRootNode(NodeType.Collection, nodeId);
                    nodes.SetNode(nodeId, holderNode);
                }

                // Add reused nodes if needed
                foreach (Guid nodeId in changeSet.ReusedNodes.Keys)
                {
                    if (!nodes.Contains(nodeId))
                    {
                        var holderNode = BPlusTreeOperations.CreateRootNode(NodeType.Collection, nodeId);
                        nodes.SetNode(nodeId, holderNode);
                    }
                }

                // Add new node edges to map
                foreach (Guid nodeId in changeSet.Nodes.EnumerateNodes())
                {
                    var node = changeSet.Nodes.GetNode(nodeId, NodeAccess.Read);

                    // Add this id into all referenced nodes
                    foreach (var edge in node.Edges.Values)
                    {
                        if (EdgeFilter(edge))
                        {
                            var edgeData = new EdgeData(EdgeType.Contains, nodeId);
                            Edge<Guid, EdgeData> existingEdge = null;
                            if (!BPlusTreeOperations.TryFindEdge(nodes, edge.ToNodeId, edgeData, out existingEdge))
                            {
                                BPlusTreeOperations.InsertEdge(nodes, edge.ToNodeId, new Edge<Guid, EdgeData>(nodeId, edgeData), ParentsTreeOrder);
                            }
                        }
                    }
                }

                // Set last snapshot as the destination ID
                lastSnapshotId = changeSet.DestinationSnapshotId;
            }
        }

        private bool EdgeFilter(Edge<Guid, EdgeData> edge)
        {
            if (filterMutable)
            {
                return !Utils.IsPermanentEdge(edge);
            }
            else
            {
                return ((edge.Data.Flags & EdgeFlags.Permanent) == EdgeFlags.Permanent) &&
                        !(edge.ToNodeId == Constants.NullReferenceNodeId) &&
                        !(edge.Data.Semantic == EdgeType.OfType);
            }
        }                            

        /// <summary>
        /// Recursive breadth-first pass through node graph which generates parent information
        /// </summary>
        /// <param name="snapshotId">Snapshot to generate</param>
        /// <param name="nodeId">Visited node</param>
        /// <param name="visitedNodes">List of visited nodes</param>
        private void AddParentsRecursive(Guid nodeId, Hashtable visitedNodes, INodeProvider<Guid, object, EdgeData> nodes)
        {
            if (nodeId.Equals(Constants.NullReferenceNodeId))
            {
                return;
            }

            if (visitedNodes.ContainsKey(nodeId))
            {
                return;
            }
            else
            {
                visitedNodes.Add(nodeId, null);
            }

            // Add the node as parent of all child nodes
            foreach (var edge in dataNodeProvider.GetNode(nodeId, NodeAccess.Read).Edges.Values)
            {
                if (EdgeFilter(edge))
                {
                    if (!nodes.Contains(edge.ToNodeId))
                    {
                        var holderNode = BPlusTreeOperations.CreateRootNode(NodeType.Collection, edge.ToNodeId);
                        nodes.SetNode(edge.ToNodeId, holderNode);
                    }

                    var edgeData = new EdgeData(EdgeType.Contains, nodeId);
                    Edge<Guid, EdgeData> existingEdge = null;

                    if (!BPlusTreeOperations.TryFindEdge(nodes, edge.ToNodeId, edgeData, out existingEdge))
                    {
                        BPlusTreeOperations.InsertEdge(nodes, edge.ToNodeId, new Edge<Guid, EdgeData>(nodeId, edgeData), ParentsTreeOrder);
                    }
                }
            }

            foreach (var edge in dataNodeProvider.GetNode(nodeId, NodeAccess.Read).Edges.Values)
            {
                AddParentsRecursive(edge.ToNodeId, visitedNodes, nodes);
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
