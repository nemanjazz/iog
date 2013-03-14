// -----------------------------------------------------------------------
// <copyright file="IsolatedNodeProvider.cs" company="Execom">
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

namespace Execom.IOG.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Execom.IOG.Graph;
    using Execom.IOG.Storage;
    using System.Threading;
    using System.Collections;    

    /// <summary>
    /// Node provider which isolates changes in a change set. 
    /// Accessed by single thread only.
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal class IsolatedNodeProvider: INodeProvider<Guid, object, EdgeData>
    {        
        /// <summary>
        /// Underlying storage
        /// </summary>
        private INodeProvider<Guid, object, EdgeData> parentProvider;

        /// <summary>
        /// Isolated storage
        /// </summary>
        private INodeProvider<Guid, object, EdgeData> isolatedProvider;

        /// <summary>
        /// Thread which is allowed to use this provider
        /// </summary>
        private Thread thread;

        /// <summary>
        /// States of isolated nodes
        /// </summary>
        private Dictionary<Guid, NodeState> nodeStates = new Dictionary<Guid, NodeState>();

        /// <summary>
        /// Creates new instance of IsolatedStorageNodeProvider class
        /// </summary>
        /// <param name="parentProvider">Underlying provider for node objects</param>
        /// <param name="isolatedProvider">Provider for isolated changes</param>
        /// <param name="thread">Thread which is allowed to use this provider</param>
        public IsolatedNodeProvider(INodeProvider<Guid, object, EdgeData> parentProvider, INodeProvider<Guid, object, EdgeData> isolatedProvider, Thread thread)
        {
            this.thread = thread;
            this.parentProvider = parentProvider;
            this.isolatedProvider = isolatedProvider;
        }

        /// <summary>
        /// Adds node to collection
        /// </summary>
        /// <param name="identifier">Node identifier</param>
        /// <param name="node">Node to add</param>
        public void SetNode(Guid identifier, Node<Guid, object, EdgeData> node)
        {
            CheckThread();
            isolatedProvider.SetNode(identifier, node);
            if (!nodeStates.ContainsKey(identifier))
            {
                nodeStates.Add(identifier, NodeState.Created);
            }
        }

        /// <summary>
        /// Gets node from collection by an identifier
        /// </summary>
        /// <param name="nodeId">Node identifier</param>
        /// <returns>Node object</returns>
        public Node<Guid, object, EdgeData> GetNode(Guid nodeId, NodeAccess access)
        {
            CheckThread();

            if (access == NodeAccess.ReadWrite)
            {
                EnsureNode(nodeId);
                return isolatedProvider.GetNode(nodeId, access);
            }
            else
            {
                if (isolatedProvider.Contains(nodeId))
                {
                    return isolatedProvider.GetNode(nodeId, access);
                }
                else
                {
                    return parentProvider.GetNode(nodeId, access);
                }
            }
        }

        /// <summary>
        /// Determines if collection contains a node with given identifier
        /// </summary>
        /// <param name="identifier">Identifier to check</param>
        /// <returns>True if contains</returns>
        public bool Contains(Guid identifier)
        {
            CheckThread();
            return isolatedProvider.Contains(identifier) || parentProvider.Contains(identifier);
        }

        /// <summary>
        /// Removes node from collection by an identifier
        /// </summary>
        /// <param name="identifier">Node identifier</param>
        public void Remove(Guid identifier)
        {
            CheckThread();
            EnsureNode(identifier);
            nodeStates[identifier] = NodeState.Removed;
        }        

        /// <summary>
        /// Returns node enumeration containing all nodes in the provider
        /// </summary>
        /// <returns>Nodes identifiers</returns>
        public IEnumerable EnumerateNodes()
        {
            CheckThread();
            return parentProvider.EnumerateNodes();
        }

        public IsolatedChangeSet<Guid, object, EdgeData> GetChanges(Guid snapshotId)
        {
            return new IsolatedChangeSet<Guid, object, EdgeData>(snapshotId, isolatedProvider, nodeStates);
        }


        /// <summary>
        /// Returns node enumeration containing changed nodes
        /// </summary>
        /// <returns>Nodes identifiers</returns>
        public IEnumerable EnumerateChanges()
        {
            CheckThread();
            return isolatedProvider.EnumerateNodes();
        }

        /// <summary>
        /// Returns state of node change
        /// </summary>
        /// <param name="identifier">Node identifier</param>
        /// <returns>State of node change</returns>
        public NodeState GetNodeState(Guid identifier)
        {
            CheckThread();
            if (nodeStates.ContainsKey(identifier))
            {
                return nodeStates[identifier];
            }
            else
            {
                return NodeState.None;
            }
        }

        /// <summary>
        /// Clears nodes from the provider
        /// </summary>
        public void Clear()
        {
            CheckThread();
            isolatedProvider.Clear();
            nodeStates.Clear();
        }

        /// <summary>
        /// Copies node from parent provider to isolated provider if it doesnt exist
        /// </summary>
        /// <param name="nodeId">Node identifier</param>
        private void EnsureNode(Guid nodeId)
        {
            if (!isolatedProvider.Contains(nodeId))
            {
                var node = parentProvider.GetNode(nodeId, NodeAccess.Read);                

                Dictionary<EdgeData, Edge<Guid, EdgeData>> edgeList = new Dictionary<EdgeData,Edge<Guid,EdgeData>>();

                foreach (var edge in node.Edges.Values)
                {
                    edgeList.Add(edge.Data, new Edge<Guid, EdgeData>(edge.ToNodeId, new EdgeData(edge.Data.Semantic, edge.Data.Flags, edge.Data.Data)));
                }

                Dictionary<Guid, object> valueList = new Dictionary<Guid,object>();

                foreach (var value in node.Values)
                {
                    valueList.Add(value.Key, value.Value);
                }

                var newNode = new Node<Guid, object, EdgeData>(node.NodeType, node.Data, edgeList, valueList);
                newNode.Commited = false;

                isolatedProvider.SetNode(nodeId, newNode);
                nodeStates.Add(nodeId, NodeState.Modified);
            }
        }

        /// <summary>
        /// Ensures that calling thread is the thread which has access to the isolated provider
        /// </summary>
        private void CheckThread()
        {
            if (Thread.CurrentThread != this.thread)
            {
                throw new InvalidOperationException("Call was made from the thread which is not allowed by the provider");
            }
        }
    }
}
