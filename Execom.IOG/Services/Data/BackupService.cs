// -----------------------------------------------------------------------
// <copyright file="BackupService.cs" company="Execom">
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
    using System.Collections;

    /// <summary>
    /// Class which performs backup operation
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal class BackupService
    {
        /// <summary>
        /// Perform backup
        /// </summary>
        /// <param name="snapshotId">Snapshot Id to backup</param>
        /// <param name="source">Source node provider</param>
        /// <param name="destination">Destination node provider</param>
        public void Backup(Guid snapshotId, INodeProvider<Guid, object, EdgeData> source, INodeProvider<Guid, object, EdgeData> destination)
        {
            foreach (var item in destination.EnumerateNodes())
            {
                throw new ArgumentException("Destination provider must be empty");
            }

            var visitedNodes = new Hashtable();

            // Create types
            AddNodesRecursive(Constants.TypesNodeId, source, destination, visitedNodes);

            // Copy snapshot
            AddNodesRecursive(snapshotId, source, destination, visitedNodes);

            // Create snapshots root
            var snapshotsNode = new Node<Guid, object, EdgeData>(NodeType.SnapshotsRoot, null);
            // Create link to the snapshot
            snapshotsNode.AddEdge(new Edge<Guid, EdgeData>(snapshotId, new EdgeData(EdgeType.Contains, null)));
            // Store the snapshots root as the last operation of backup
            destination.SetNode(Constants.SnapshotsNodeId, snapshotsNode);
        }

        private void AddNodesRecursive(Guid nodeId, INodeProvider<Guid, object, EdgeData> source, INodeProvider<Guid, object, EdgeData> destination, Hashtable visitedNodes)
        {
            if (visitedNodes.ContainsKey(nodeId))
            {
                return;
            }

            visitedNodes.Add(nodeId, null);

            var node = source.GetNode(nodeId, NodeAccess.Read);
            var newNode = new Node<Guid, object, EdgeData>(node.NodeType, node.Data, node.Edges, node.Values);
            destination.SetNode(nodeId, newNode);

            foreach (var edge in node.Edges.Values)
            {
                if (edge.ToNodeId != Constants.NullReferenceNodeId)
                {
                    AddNodesRecursive(edge.ToNodeId, source, destination, visitedNodes);
                }
            }
        }

        internal void Archive(Guid revisionId, INodeProvider<Guid, object, EdgeData> source, INodeProvider<Guid, object, EdgeData> destination)
        {
            foreach (var item in destination.EnumerateNodes())
            {
                throw new ArgumentException("Destination provider must be empty");
            }

            var visitedNodes = new Hashtable();

            // Create types
            AddNodesRecursive(Constants.TypesNodeId, source, destination, visitedNodes);

            // Copy snapshot
            AddNodesRecursive(revisionId, source, destination, visitedNodes);

            // Create snapshots root
            var snapshotsNode = new Node<Guid, object, EdgeData>(NodeType.SnapshotsRoot, null);
            // Create link to the snapshot
            snapshotsNode.AddEdge(new Edge<Guid, EdgeData>(revisionId, new EdgeData(EdgeType.Contains, null)));
            // Store the snapshots root as the last operation of backup
            destination.SetNode(Constants.SnapshotsNodeId, snapshotsNode);
        }
    }
}
