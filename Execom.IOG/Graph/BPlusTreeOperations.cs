// -----------------------------------------------------------------------
// <copyright file="BPlusTreeOperations.cs" company="Execom">
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
    using System.Collections.ObjectModel;

    /// <summary>
    /// Class defines static methods for B+ tree constructed in sub-graph.   
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal static class BPlusTreeOperations
    {
        /// <summary>
        /// Defines enumerator through edges which are residing in the tree.
        /// Edges are filtered by the edge type.
        /// </summary>
        /// <author>Nenad Sabo</author>
        public class BPlusTreeEnumerator : IEnumerator<Edge<Guid, EdgeData>>
        {
            /// <summary>
            /// Node provider of the tree
            /// </summary>
            private INodeProvider<Guid, object, EdgeData> nodes;

            /// <summary>
            /// Root node id of the tree
            /// </summary>
            private Guid rootNodeId;

            /// <summary>
            /// Enumerator of the current leaf
            /// </summary>
            private IEnumerator<Edge<Guid, EdgeData>> currentLeafEnumerator;

            /// <summary>
            /// Edge type which represents the enumerator filter
            /// </summary>
            private EdgeType edgeType;

            /// <summary>
            /// Creates new instance of BPlusTreeEnumerator type
            /// </summary>
            /// <param name="nodes">Node provider which contains tree nodes</param>
            /// <param name="rootNodeId">Root item ID</param>
            /// <param name="edgeType">Edge type to use as a filter for enumeration</param>
            public BPlusTreeEnumerator(INodeProvider<Guid, object, EdgeData> nodes, Guid rootNodeId, EdgeType edgeType)
            {
                this.nodes = nodes;
                this.rootNodeId = rootNodeId;
                this.edgeType = edgeType;
                var currentLeaf = BPlusTreeOperations.LeftLeaf(nodes, nodes.GetNode(rootNodeId, NodeAccess.Read));
                this.currentLeafEnumerator = currentLeaf.Edges.Values.GetEnumerator();
            }

            /// <summary>
            /// Current edge
            /// </summary>
            public Edge<Guid, EdgeData> Current
            {
                get
                {
                    return currentLeafEnumerator.Current;
                }
            }

            /// <summary>
            /// Disposes the enumerator
            /// </summary>
            public void Dispose()
            {
                if (currentLeafEnumerator != null)
                {
                    currentLeafEnumerator.Dispose();
                    currentLeafEnumerator = null;
                }

            }

            /// <summary>
            /// Current item
            /// </summary>
            object System.Collections.IEnumerator.Current
            {
                get { return currentLeafEnumerator.Current; }
            }

            /// <summary>
            /// Moves to next item in sequence
            /// </summary>
            /// <returns>True if next item was found</returns>
            public bool MoveNext()
            {
                bool res = false;
                do
                {
                    res = MoveNextInternal();
                }
                while (res && Current.Data.Semantic != edgeType);

                return res;
            }

            /// <summary>
            /// Moves to next item without regards to edge type
            /// </summary>
            /// <returns>True if next item was found</returns>
            private bool MoveNextInternal()
            {
                if (currentLeafEnumerator == null)
                {
                    return false;
                }

                var current = Current;

                EdgeData sampleData = null;

                if (current != null)
                {
                    sampleData = current.Data;
                }

                bool res = currentLeafEnumerator.MoveNext();

                if (res)
                {
                    return true;
                }
                else
                {
                    this.currentLeafEnumerator.Dispose();
                    this.currentLeafEnumerator = null;

                    var currentLeaf = BPlusTreeOperations.NextLeaf(nodes, rootNodeId, sampleData);

                    if (currentLeaf == null)
                    {
                        return false;
                    }
                    else
                    {
                        this.currentLeafEnumerator = currentLeaf.Edges.Values.GetEnumerator();
                        return currentLeafEnumerator.MoveNext();
                    }
                }
            }

            /// <summary>
            /// Resets the enumerator
            /// </summary>
            public void Reset()
            {
                if (currentLeafEnumerator != null)
                {
                    currentLeafEnumerator.Dispose();
                }

                var currentLeaf = BPlusTreeOperations.LeftLeaf(nodes, nodes.GetNode(rootNodeId, NodeAccess.Read));
                this.currentLeafEnumerator = currentLeaf.Edges.Values.GetEnumerator();
            }
        }

        /// <summary>
        /// Determines the split result
        /// </summary>
        private class SplitResult
        {
            public readonly Guid CreatedNodeId;
            public readonly Node<Guid, object, EdgeData> CreatedNode;
            public readonly EdgeData RightKey;

            /// <summary>
            /// Creates new instance of SplitResult type
            /// </summary>
            /// <param name="createdNodeId">New node ID which was created</param>
            /// <param name="createdNode">Node which was created</param>
            /// <param name="rightKey">Minimum key value of the right sibling node of the new node</param>
            public SplitResult(Guid createdNodeId, Node<Guid, object, EdgeData> createdNode, EdgeData rightKey)
            {
                this.CreatedNodeId = createdNodeId;
                this.CreatedNode = createdNode;
                this.RightKey = rightKey;
            }
        }

        /// <summary>
        /// Determines node find result
        /// </summary>
        private class FindResult
        {
            public readonly Guid NodeId;
            public readonly Node<Guid, object, EdgeData> Node;

            public FindResult(Guid nodeId, Node<Guid, object, EdgeData> node)
            {
                this.NodeId = nodeId;
                this.Node = node;
            }
        }

        /// <summary>
        /// Determines result of removal operation
        /// </summary>
        private class RemovalResult
        {
            public readonly int RemainingCount;
            public readonly bool WasRemoved;
            public readonly bool WasMerged;

            public RemovalResult(int remainingCount, bool wasRemoved, bool wasMerged)
            {
                this.WasRemoved = wasRemoved;
                this.WasMerged = wasMerged;
                this.RemainingCount = remainingCount;
            }
        }        

        /// <summary>
        /// Data for internal node
        /// </summary>
        public static Guid InternalNodeData = new Guid("{46F41F60-F781-445A-A416-B35E0CA940B4}");

        /// <summary>
        /// Data for leaf node
        /// </summary>
        public static Guid LeafNodeData = new Guid("{6DEFCDC7-4C59-4120-87C6-C72363751BE7}");

        /// <summary>
        /// Creates new root node
        /// </summary>
        /// <param name="rootNodeId">Node identifier</param>
        /// <returns>New node</returns>
        public static Node<Guid, object, EdgeData> CreateRootNode(NodeType nodeType, Guid rootNodeId)
        {
            var node = new Node<Guid, object, EdgeData>(nodeType, LeafNodeData);
            return node;
        }

        /// <summary>
        /// Adds new edge to the tree.
        /// </summary>
        /// <param name="nodes">Node provider which hosts the nodes</param>
        /// <param name="rootNodeId">Tree root node ID</param>
        /// <param name="edge">Edge to add</param>
        public static void InsertEdge(INodeProvider<Guid, object, EdgeData> nodes, Guid rootNodeId, Edge<Guid, EdgeData> edge, int treeOrder)
        {
            if (treeOrder < 6)
            {
                throw new ArgumentException();
            }

            var result = FindLeafNode(nodes, rootNodeId, edge.Data);
            if (result.Node.Edges.Count == treeOrder)
            {
                SplitTree(nodes, rootNodeId, true, edge.Data, treeOrder);
                result = FindLeafNode(nodes, rootNodeId, edge.Data);
            }

            if (!result.Node.Data.Equals(LeafNodeData))
            {
                throw new InvalidOperationException("Leaf node expected");
            }

            var node = nodes.GetNode(result.NodeId, NodeAccess.ReadWrite);
            node.AddEdge(edge);
            nodes.SetNode(result.NodeId, node);
        }

        /// <summary>
        /// Finds the edge in tree
        /// </summary>
        /// <param name="nodes">Node provider which hosts the nodes</param>
        /// <param name="rootNodeId">Tree root node ID</param>
        /// <param name="data">Edge to find</param>
        /// <param name="value">Edge result</param>
        /// <returns>True if edge was found</returns>
        public static bool TryFindEdge(INodeProvider<Guid, object, EdgeData> nodes, Guid rootNodeId, EdgeData data, out Edge<Guid, EdgeData> value)
        {
            var result = FindLeafNode(nodes, rootNodeId, data);

            if (result == null)
            {
                throw new KeyNotFoundException();
            }
            else
            {
                if (!result.Node.Data.Equals(LeafNodeData))
                {
                    throw new InvalidOperationException("Leaf node expected");
                }

                return result.Node.Edges.TryGetValue(data, out value);
            }
        }

        /// <summary>
        /// Finds the edge in tree
        /// </summary>
        /// <param name="nodes">Node provider which hosts the nodes</param>
        /// <param name="rootNodeId">Tree root node ID</param>
        /// <param name="data">Edge to find</param>
        /// <param name="toNodeId">New to node ID</param>
        /// <returns>True if edge was found</returns>
        public static bool TrySetEdgeToNode(INodeProvider<Guid, object, EdgeData> nodes, Guid rootNodeId, EdgeData data, Guid toNodeId)
        {
            var result = FindLeafNode(nodes, rootNodeId, data);

            if (result == null)
            {
                throw new KeyNotFoundException();
            }
            else
            {
                if (!result.Node.Data.Equals(LeafNodeData))
                {
                    throw new InvalidOperationException("Leaf node expected");
                }

                result.Node.SetEdgeToNode(data, toNodeId);
                nodes.SetNode(result.NodeId, result.Node);

                return true;
            }
        }

        /// <summary>
        /// Removes the edge from the tree
        /// </summary>
        /// <param name="nodes">Node provider which hosts the nodes</param>
        /// <param name="rootNodeId">Tree root node ID</param>
        /// <param name="data">Edge to find</param>
        /// <returns>True if edge was removed</returns>
        public static bool RemoveEdge(INodeProvider<Guid, object, EdgeData> nodes, Guid rootNodeId, EdgeData data, int treeOrder)
        {
            var result = RemoveEdgeRecursive(nodes, rootNodeId, data, treeOrder);
            return result.WasRemoved;
        }        

        /// <summary>
        /// Returns number of edges in the tree
        /// </summary>
        /// <param name="nodes">Node provider which hosts the nodes</param>
        /// <param name="rootNodeId">Tree root node ID</param>
        /// <param name="data">Edge to find</param>
        /// <returns></returns>
        public static int Count(INodeProvider<Guid, object, EdgeData> nodes, Guid rootNodeId, EdgeType data)
        {
            var node = nodes.GetNode(rootNodeId, NodeAccess.Read);
            if (node.Data.Equals(LeafNodeData))
            {
                int sum = 0;
                foreach (var edge in node.Edges.Values)
                {
                    if (edge.Data.Semantic.Equals(data))
                    {
                        sum++;
                    }
                }
                return sum;
            }
            else
            {
                int sum = 0;
                foreach (var edge in node.Edges.Values)
                {
                    sum += Count(nodes, edge.ToNodeId, data);
                }
                return sum;
            }
        }

        /// <summary>
        /// Creates new enumerator through tree edges
        /// </summary>
        /// <param name="nodes">Node provider which contains tree nodes</param>
        /// <param name="rootNodeId">Tree root node ID</param>
        /// <param name="edgeType">Edge type to use as a filter</param>
        /// <returns>New enumerator</returns>
        public static IEnumerator<Edge<Guid, EdgeData>> GetEnumerator(INodeProvider<Guid, object, EdgeData> nodes, Guid rootNodeId, EdgeType edgeType)
        {
            return new BPlusTreeEnumerator(nodes, rootNodeId, edgeType);
        }

        /// <summary>
        /// Creates clone of existing tree under different ID
        /// </summary>
        /// <param name="nodes">Node provider which stores the nodes</param>
        /// <param name="sourceId">ID of source tree root</param>
        /// <param name="destinationId">ID of destination tree root</param>
        public static void Clone(INodeProvider<Guid, object, EdgeData> nodes, Guid sourceId, Guid destinationId)
        {
            var node = nodes.GetNode(sourceId, NodeAccess.Read);

            if (node.Data.Equals(InternalNodeData))
            {
                Node<Guid, object, EdgeData> newNode = new Node<Guid, object, EdgeData>(node.NodeType, node.Data);
                foreach (var edge in node.Edges.Values)
                {
                    Guid subId = Guid.NewGuid();
                    Clone(nodes, edge.ToNodeId, subId);
                    newNode.AddEdge(new Edge<Guid, EdgeData>(subId, edge.Data));
                }

                nodes.SetNode(destinationId, newNode);
            }
            else
                if (node.Data.Equals(LeafNodeData))
                {
                    Node<Guid, object, EdgeData> newNode = new Node<Guid, object, EdgeData>(node.NodeType, node.Data, node.Edges);
                    nodes.SetNode(destinationId, newNode);
                }
                else
                {
                    throw new InvalidOperationException();
                }
        }

        /// <summary>
        /// Prints tree nodes and edges
        /// </summary>
        /// <param name="nodes">Node provider which contains tree nodes</param>
        /// <param name="rootNodeId">Tree root node ID</param>
        /// <returns>String representation of the tree</returns>
        public static string PrintTree(INodeProvider<Guid, object, EdgeData> nodes, Guid rootNodeId)
        {
            StringBuilder sb = new StringBuilder();

            PrintTreeRecursive(nodes, rootNodeId, sb, 0);

            return sb.ToString();
        }

        /// <summary>
        /// Runs a recursive pass and prints tree nodes
        /// </summary>
        /// <param name="nodes">Node provider which contains tree nodes</param>
        /// <param name="nodeId">Node to print</param>
        /// <param name="sb">String builder which contains the string</param>
        /// <param name="level">Current indentation level</param>
        private static void PrintTreeRecursive(INodeProvider<Guid, object, EdgeData> nodes, Guid nodeId, StringBuilder sb, int level)
        {
            var node = nodes.GetNode(nodeId, NodeAccess.Read);
            AppendTabs(sb, level);

            if (node.Data.Equals(InternalNodeData))
            {
                sb.AppendFormat("Internal\n");
                foreach (var edge in node.Edges.Values)
                {
                    AppendTabs(sb, level);
                    sb.AppendFormat("MAX[{0}]\n", edge.Data.Equals(EdgeData.MaxValue) ? "*" : edge.Data.Data.ToString());
                    PrintTreeRecursive(nodes, edge.ToNodeId, sb, level + 1);
                }

            }
            else
            {
                sb.AppendFormat("Leaf\n");
                foreach (var edge in node.Edges.Values)
                {
                    AppendTabs(sb, level);
                    sb.AppendFormat("TO[{0}]\n", edge.ToNodeId.ToString());
                }
            }
        }

        /// <summary>
        /// Appends number of tabs to string builder
        /// </summary>
        /// <param name="sb">String builder</param>
        /// <param name="level">Number of tabs</param>
        private static void AppendTabs(StringBuilder sb, int level)
        {
            for (int i = 0; i < level; i++)
            {
                sb.Append("\t");
            }
        }

        /// <summary>
        /// Looks for leaf tree node which is supposed to have an edge with given hash
        /// </summary>
        /// <param name="nodes">Node provider</param>
        /// <param name="rootNodeId">Root node ID to start the search from</param>
        /// <param name="edgeHash">Hash to find</param>
        /// <returns>Leaf node which is supposed to contain the edge</returns>
        private static FindResult FindLeafNode(INodeProvider<Guid, object, EdgeData> nodes, Guid rootNodeId, EdgeData data)
        {
            var node = nodes.GetNode(rootNodeId, NodeAccess.Read);
            Guid id = rootNodeId;

            while (node.Data.Equals(InternalNodeData))
            {
                var leadingEdge = FindLeadingEdge(data, node);

                // Advance to next internal node
                node = nodes.GetNode(leadingEdge.ToNodeId, NodeAccess.Read);
                id = leadingEdge.ToNodeId;
            }

            return new FindResult(id, node);
        }

        /// <summary>
        /// Finds an edge which leads to given hash
        /// </summary>
        /// <param name="data">Data to find</param>
        /// <param name="node">Internal node</param>
        /// <returns>Edge which leads to hash</returns>
        private static Edge<Guid, EdgeData> FindLeadingEdge(EdgeData data, Node<Guid, object, EdgeData> node)
        {
            return node.Edges[node.Edges.Keys[FindLeadingEdgeIndex(data, node)]];
        }

        /// <summary>
        /// Finds an edge index which leads to given hash
        /// </summary>
        /// <param name="data">Data to find</param>
        /// <param name="node">Internal node</param>
        /// <returns>Edge index which leads to hash</returns>
        private static int FindLeadingEdgeIndex(EdgeData data, Node<Guid, object, EdgeData> node)
        {
            if (node.Data.Equals(InternalNodeData))
            {
                // Find tree edge which leads to the hash                
                var keys = node.Edges.Keys;
                int i = 0;
                int step = keys.Count;

                while (true)
                {
                    if (step > 1)
                    {
                        step = step / 2;
                    }                    

                    if (data.CompareTo(keys[i]) < 0)
                    {
                        // Data is smaller, try going left
                        if (i == 0)
                        {
                            // There is no place to go
                            return i;
                        }

                        if (data.CompareTo(keys[i - 1]) >= 0)
                        {
                            // Going left would lead us into the region of greater
                            return i;
                        }
                        else
                        {
                            i = i - step; 
                        }
                    }
                    else
                    {
                        // Data is greater, try going right
                        if (i == keys.Count - 1)
                        {
                            // There is no place to go
                            return i;
                        }
                        else
                        {
                            i = i + step;
                        }
                    }

                    //while (i < keys.Count && data.CompareTo(keys[i]) >= 0)
                    //{
                    //    i++;
                    //}

                }

                //return i;
            }
            else
            {
                throw new ArgumentException("Internal node expected");
            }
        }

        /// <summary>
        /// Makes room in the tree for new data
        /// </summary>
        /// <param name="nodes">Node provider which contains tree nodes</param>
        /// <param name="nodeId">Current node ID</param>
        /// <param name="isRoot">Determines if current node is root node</param>
        /// <param name="data">Data to make room for</param>
        /// <returns>Result of the operation</returns>
        private static SplitResult SplitTree(INodeProvider<Guid, object, EdgeData> nodes, Guid nodeId, bool isRoot, EdgeData data, int treeOrder)
        {
            var currentNode = nodes.GetNode(nodeId, NodeAccess.ReadWrite);

            if (isRoot)
            {
                if (currentNode.Data.Equals(InternalNodeData))
                {
                    #region Is root internal ?

                    var leadingEdge = FindLeadingEdge(data, currentNode);
                    var result = SplitTree(nodes, leadingEdge.ToNodeId, false, data, treeOrder);

                    if (result != null)
                    {
                        currentNode.AddEdge(new Edge<Guid, EdgeData>(result.CreatedNodeId, result.RightKey));

                        if (currentNode.Edges.Count == (treeOrder+1))
                        {
                            // Split root to 2 internal nodes
                            Guid leftNodeId = Guid.NewGuid();
                            Guid rightNodeId = Guid.NewGuid();
                            Node<Guid, object, EdgeData> leftNode = new Node<Guid, object, EdgeData>(NodeType.TreeInternal, InternalNodeData);
                            Node<Guid, object, EdgeData> rightNode = new Node<Guid, object, EdgeData>(NodeType.TreeInternal, InternalNodeData);

                            var keys = currentNode.Edges.Keys;
                            for (int i = 0; i < keys.Count; i++)
                            {
                                if (i < keys.Count / 2)
                                {
                                    leftNode.AddEdge(currentNode.Edges[keys[i]]);
                                }
                                else
                                {
                                    rightNode.AddEdge(currentNode.Edges[keys[i]]);
                                }
                            }

                            //Set last edges
                            SetLastInternalKey(leftNode);
                            SetLastInternalKey(rightNode);

                            currentNode.Edges.Clear();
                            currentNode.AddEdge(new Edge<Guid, EdgeData>(leftNodeId, LeftEdge(nodes, rightNode).Data));
                            currentNode.AddEdge(new Edge<Guid, EdgeData>(rightNodeId, EdgeData.MaxValue));


                            nodes.SetNode(leftNodeId, leftNode);
                            nodes.SetNode(rightNodeId, rightNode);
                        }

                        nodes.SetNode(nodeId, currentNode);
                    }
                    else
                    {
                        return null;
                    }
                    #endregion
                }
                else
                {
                    #region Root is a leaf
                    if (currentNode.Edges.Count == treeOrder)
                    {
                        // Split root to internal nodes
                        Guid leftNodeId = Guid.NewGuid();
                        Guid rightNodeId = Guid.NewGuid();
                        Node<Guid, object, EdgeData> leftNode = new Node<Guid, object, EdgeData>(NodeType.TreeLeaf, LeafNodeData);
                        Node<Guid, object, EdgeData> rightNode = new Node<Guid, object, EdgeData>(NodeType.TreeLeaf, LeafNodeData);

                        var keys = currentNode.Edges.Keys;
                        for (int i = 0; i < keys.Count; i++)
                        {
                            if (i < keys.Count / 2)
                            {
                                leftNode.AddEdge(currentNode.Edges[keys[i]]);
                            }
                            else
                            {
                                rightNode.AddEdge(currentNode.Edges[keys[i]]);
                            }
                        }

                        currentNode.Edges.Clear();
                        currentNode.AddEdge(new Edge<Guid, EdgeData>(leftNodeId, LeftEdge(nodes, rightNode).Data));
                        currentNode.AddEdge(new Edge<Guid, EdgeData>(rightNodeId, EdgeData.MaxValue));
                        currentNode.SetData(InternalNodeData);


                        nodes.SetNode(leftNodeId, leftNode);
                        nodes.SetNode(rightNodeId, rightNode);
                        nodes.SetNode(nodeId, currentNode);
                    }
                    #endregion
                }

                return null;
            }
            else
            {
                if (currentNode.Data.Equals(InternalNodeData))
                {
                    var leadingEdge = FindLeadingEdge(data, currentNode);
                    var result = SplitTree(nodes, leadingEdge.ToNodeId, false, data, treeOrder);

                    if (result != null)
                    {
                        currentNode.AddEdge(new Edge<Guid, EdgeData>(result.CreatedNodeId, result.RightKey));

                        if (currentNode.Edges.Count == treeOrder)
                        {
                            Guid newInternalId = Guid.NewGuid();
                            Node<Guid, object, EdgeData> newInternal = new Node<Guid, object, EdgeData>(NodeType.TreeInternal, InternalNodeData);
                            var keys = currentNode.Edges.Keys;

                            for (int i = 0; i < keys.Count / 2; i++)
                            {
                                newInternal.AddEdge(currentNode.Edges[keys[i]]);
                            }

                            int nrToRemove = keys.Count / 2;
                            for (int i = 0; i < nrToRemove; i++)
                            {
                                currentNode.Edges.RemoveAt(0);
                            }

                            // Set last edge of left internal node to MaxInt
                            SetLastInternalKey(newInternal);

                            nodes.SetNode(newInternalId, newInternal);
                            nodes.SetNode(nodeId, currentNode);

                            return new SplitResult(newInternalId, newInternal, LeftEdge(nodes, currentNode).Data);
                        }
                        else
                        {
                            nodes.SetNode(nodeId, currentNode);
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                    if (currentNode.Data.Equals(LeafNodeData))
                    {
                        Guid newLeafId = Guid.NewGuid();
                        Node<Guid, object, EdgeData> newLeaf = new Node<Guid, object, EdgeData>(NodeType.TreeLeaf, LeafNodeData);
                        var keys = currentNode.Edges.Keys;

                        for (int i = 0; i < keys.Count / 2; i++)
                        {
                            newLeaf.AddEdge(currentNode.Edges[keys[i]]);
                        }

                        int nrToRemove = keys.Count / 2;
                        for (int i = 0; i < nrToRemove; i++)
                        {
                            currentNode.Edges.RemoveAt(0);
                        }

                        nodes.SetNode(newLeafId, newLeaf);
                        nodes.SetNode(nodeId, currentNode);

                        return new SplitResult(newLeafId, newLeaf, LeftEdge(nodes, currentNode).Data);
                    }
                    else
                    {
                        throw new ArgumentException("Unexpected node data");
                    }
            }
        }

        /// <summary>
        /// Gets leftmost leaf in the given subtree
        /// </summary>
        /// <param name="nodes">Node provider</param>
        /// <param name="node">Node to start from</param>
        /// <returns></returns>
        private static Node<Guid, object, EdgeData> LeftLeaf(INodeProvider<Guid, object, EdgeData> nodes, Node<Guid, object, EdgeData> node)
        {
            if (node.Data.Equals(LeafNodeData))
            {
                return node;
            }
            else
            {
                return LeftLeaf(nodes, nodes.GetNode(node.Edges[node.Edges.Keys[0]].ToNodeId, NodeAccess.Read));
            }
        }

        /// <summary>
        /// Gets next leaf in tree compared to given sample data
        /// </summary>
        /// <param name="nodes">Node provider</param>
        /// <param name="rootNodeId">Root node id</param>
        /// <param name="sampleData">Sample data in the current leaf</param>
        /// <returns>Leaf node next in comparison to sample data, or null if no more leaves are found</returns>
        private static Node<Guid, object, EdgeData> NextLeaf(INodeProvider<Guid, object, EdgeData> nodes, Guid rootNodeId, EdgeData sampleData)
        {
            var node = nodes.GetNode(rootNodeId, NodeAccess.Read);
            Guid id = rootNodeId;

            Guid nextInternalParentId = Guid.Empty;

            while (node.Data.Equals(InternalNodeData))
            {
                var leadingEdgeIndex = FindLeadingEdgeIndex(sampleData, node);

                if (leadingEdgeIndex < node.Edges.Count - 1)
                {
                    nextInternalParentId = node.Edges[node.Edges.Keys[leadingEdgeIndex + 1]].ToNodeId;
                }

                // Advance to next internal node
                node = nodes.GetNode(node.Edges[node.Edges.Keys[leadingEdgeIndex]].ToNodeId, NodeAccess.Read);
            }

            if (nextInternalParentId.Equals(Guid.Empty))
            {
                return null;
            }
            else
            {
                return LeftLeaf(nodes, nodes.GetNode(nextInternalParentId, NodeAccess.Read));
            }
        }

        /// <summary>
        /// Gets leftmost edge in the given subtree
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private static Edge<Guid, EdgeData> LeftEdge(INodeProvider<Guid, object, EdgeData> nodes, Node<Guid, object, EdgeData> node)
        {
            if (node.Data.Equals(LeafNodeData))
            {
                return node.Edges[node.Edges.Keys[0]];
            }
            else
            {
                return LeftEdge(nodes, nodes.GetNode(node.Edges[node.Edges.Keys[0]].ToNodeId, NodeAccess.Read));
            }
        }

        /// <summary>
        /// Gets the rightmost edge in given subtree
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public static Edge<Guid, EdgeData> RightEdge(INodeProvider<Guid, object, EdgeData> nodes, Node<Guid, object, EdgeData> node)
        {
            if (node.Data.Equals(LeafNodeData))
            {
                if (node.Edges.Count > 0)
                {
                    return node.Edges[node.Edges.Keys[node.Edges.Count - 1]];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return RightEdge(nodes, nodes.GetNode(node.Edges[node.Edges.Keys[node.Edges.Count - 1]].ToNodeId, NodeAccess.Read));
            }
        }

        private static void SetLastInternalKey(Node<Guid, object, EdgeData> node)
        {
            var lastKey = node.Edges.Keys[node.Edges.Count - 1];
            var edge = node.Edges[lastKey];
            if (!edge.Data.Equals(EdgeData.MaxValue))
            {
                node.Edges.Remove(lastKey);
                node.AddEdge(new Edge<Guid, EdgeData>(edge.ToNodeId, EdgeData.MaxValue));
            }
        }

        private static RemovalResult RemoveEdgeRecursive(INodeProvider<Guid, object, EdgeData> nodes, Guid nodeId, EdgeData data, int treeOrder)
        {
            var node = nodes.GetNode(nodeId, NodeAccess.ReadWrite);

            try
            {
                if (node.Data.Equals(LeafNodeData))
                {
                    var removeResult = node.Edges.Remove(data);
                    return new RemovalResult(node.Edges.Count, removeResult, false);
                }
                else
                {
                    var edgeIndex = FindLeadingEdgeIndex(data, node);
                    var edge = node.Edges[node.Edges.Keys[edgeIndex]];

                    var res = RemoveEdgeRecursive(nodes, edge.ToNodeId, data, treeOrder);

                    if (!res.WasRemoved)
                    {
                        return res;
                    }

                    if (res.RemainingCount < treeOrder / 2)
                    {
                        #region Reorganize sub nodes
                        // Take data from a sibling
                        if (edgeIndex < node.Edges.Count - 1)
                        {
                            return MergeNodes(nodes, node, edgeIndex, edgeIndex + 1, treeOrder);
                        }
                        else
                        {
                            return MergeNodes(nodes, node, edgeIndex - 1, edgeIndex, treeOrder);
                        }

                        #endregion
                    }
                    else
                    {
                        return res;
                    }
                }
            }
            finally
            {
                nodes.SetNode(nodeId, node);
            }
        }

        private static RemovalResult MergeNodes(INodeProvider<Guid, object, EdgeData> nodes, Node<Guid, object, EdgeData> node, int leftIndex, int rightIndex, int treeOrder)
        {
            var leftKey = node.Edges.Keys[leftIndex];
            var leftNodeId = node.Edges[leftKey].ToNodeId;
            var leftNode = nodes.GetNode(leftNodeId, NodeAccess.ReadWrite);

            var rightKey = node.Edges.Keys[rightIndex];
            var rightNodeId = node.Edges[rightKey].ToNodeId;
            var rightNode = nodes.GetNode(rightNodeId, NodeAccess.ReadWrite);

            if (!leftNode.Data.Equals(rightNode.Data))
            {
                throw new InvalidOperationException("Both nodes must be of same type");
            }

            if (leftNode.Data.Equals(InternalNodeData))
            {
                var maxEdgeKey = leftNode.Edges.Keys[leftNode.Edges.Count - 1];
                var maxEdge = leftNode.Edges[maxEdgeKey];
                leftNode.Edges.Remove(maxEdgeKey);
                leftNode.AddEdge(new Edge<Guid, EdgeData>(maxEdge.ToNodeId, LeftEdge(nodes, rightNode).Data));
            }

            if ((leftNode.Edges.Count + rightNode.Edges.Count) <= treeOrder)
            {
                // Take all from left node 
                for (int i = 0; i < leftNode.Edges.Count; i++)
                {
                    EdgeData removalKey = leftNode.Edges.Keys[i];
                    rightNode.AddEdge(leftNode.Edges[removalKey]);
                }

                nodes.SetNode(rightNodeId, rightNode);

                nodes.Remove(leftNodeId);
                node.Edges.Remove(leftKey);

                if (node.Edges.Count == 1)
                {
                    // When no more nodes remaining copy from child
                    node.SetData(rightNode.Data);
                    node.Edges.Clear();

                    foreach (var childEdge in rightNode.Edges.Values)
                    {
                        node.AddEdge(childEdge);
                    }

                    nodes.Remove(rightNodeId);
                }

                return new RemovalResult(node.Edges.Count, true, true);
            }
            else
            {

                // Decide how many edges to take from right node
                int numberToTake = (leftNode.Edges.Count + rightNode.Edges.Count) / 2 - leftNode.Edges.Count;

                // Copy or merge?
                if (numberToTake > 0)
                {
                    // Copy edges from right -> left
                    for (int i = 0; i < numberToTake; i++)
                    {
                        EdgeData removalKey = rightNode.Edges.Keys[0];

                        leftNode.AddEdge(rightNode.Edges[removalKey]);
                        rightNode.Edges.Remove(removalKey);
                    }

                    node.Edges.Remove(leftKey);
                    node.AddEdge(new Edge<Guid, EdgeData>(leftNodeId, LeftEdge(nodes, rightNode).Data));

                    nodes.SetNode(leftNodeId, leftNode);
                    nodes.SetNode(rightNodeId, rightNode);

                    // No edges were removed
                    return new RemovalResult(node.Edges.Count, true, true);
                }
                else
                    if (numberToTake < 0)
                    {
                        // Copy edges from left -> right
                        for (int i = 0; i < Math.Abs(numberToTake); i++)
                        {
                            EdgeData removalKey = leftNode.Edges.Keys[leftNode.Edges.Count - 1];

                            rightNode.AddEdge(leftNode.Edges[removalKey]);
                            leftNode.Edges.Remove(removalKey);
                        }

                        node.Edges.Remove(leftKey);
                        node.AddEdge(new Edge<Guid, EdgeData>(leftNodeId, LeftEdge(nodes, rightNode).Data));

                        nodes.SetNode(leftNodeId, leftNode);
                        nodes.SetNode(rightNodeId, rightNode);

                        // No edges were removed
                        return new RemovalResult(node.Edges.Count, true, true);
                    }

                throw new ArgumentException("Nothing to copy");
            }
        }

        
    }
}
