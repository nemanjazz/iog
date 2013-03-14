// -----------------------------------------------------------------------
// <copyright file="NodeMergeExecutor.cs" company="Execom">
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

namespace Execom.IOG.Services.Merging
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Execom.IOG.Graph;
    using Execom.IOG.Exceptions;
    using Execom.IOG.Services.Data;

    /// <summary>
    /// Implementation of merge functions on node level
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal class NodeMergeExecutor
    {
        private IMergeRuleProvider objectAttributeProvider;
        private TypesService typesService;

        public NodeMergeExecutor(IMergeRuleProvider objectAttributeProvider, TypesService typesService)
        {
            this.objectAttributeProvider = objectAttributeProvider;
            this.typesService = typesService;
        }

        public void MergeObjects(Guid nodeId, Node<Guid, object, EdgeData> originalNode, Node<Guid, object, EdgeData> changedNode, Node<Guid, object, EdgeData> node, MergeRecursiveDelegate mergeRecursive, InsertRecursiveDelegate insertRecursive, RecursiveResolutionParameters parameters)
        {
            Guid typeId = typesService.GetInstanceTypeId(originalNode);
            if (!objectAttributeProvider.IsConcurrent(typeId))
            {
                var instanceType = typesService.GetTypeFromId(typesService.GetInstanceTypeId(nodeId));
                throw new ConcurrentModificationException("Concurrent modification not allowed for entity type:" + instanceType.ToString());
            }

            if (objectAttributeProvider.IsStaticConcurrency(typeId))
            {
                MergeObjectsStatic(nodeId, typeId, originalNode, changedNode, node, mergeRecursive, insertRecursive, parameters);
            }
            else
            {
                throw new NotImplementedException("Dynamic concurrency not implemented");
            }
        }

        public void ChangeObject(Guid nodeId, Node<Guid, object, EdgeData> changedNode, Node<Guid, object, EdgeData> node, MergeRecursiveDelegate mergeRecursive, InsertRecursiveDelegate insertRecursive, RecursiveResolutionParameters parameters)
        {
            // Consolidate the values
            foreach (var item in changedNode.Values)
            {
                node.Values[item.Key] = item.Value;
            }

            // Consolidate the edges
            foreach (var edge in changedNode.Edges)
            {
                Guid newReference = edge.Value.ToNodeId;

                NodeState nodeState = NodeState.None;
                parameters.ChangeSet.NodeStates.TryGetValue(newReference, out nodeState);

                // If changed node is a new node go into the changed node recursion
                if (nodeState == NodeState.Created)
                {
                    // Set the new reference
                    node.SetEdgeToNode(edge.Value.Data, newReference);
                    // Insert the new nodes
                    insertRecursive(newReference, parameters);
                }
                else
                {
                    // Set the new reference
                    node.SetEdgeToNode(edge.Value.Data, newReference);
                    // Follow the new reference and see changes
                    mergeRecursive(newReference, parameters);
                }
            }
        }
    

        private void MergeObjectsStatic(Guid nodeId, Guid typeId, Node<Guid, object, EdgeData> originalNode, Node<Guid, object, EdgeData> changedNode, Node<Guid, object, EdgeData> node, MergeRecursiveDelegate mergeRecursive, InsertRecursiveDelegate insertRecursive, RecursiveResolutionParameters parameters)
        {
            // Consolidate the values
            foreach (var item in originalNode.Values)
            {
                // Value was changed compared to previous version
                if (!changedNode.Values[item.Key].Equals(item.Value))
                {
                    // Try setting value in the node, if it is still the same
                    if (node.Values[item.Key].Equals(item.Value))
                    {
                        node.Values[item.Key] = changedNode.Values[item.Key];
                    }
                    else
                    {
                        if (objectAttributeProvider.IsMemberOverride(typeId, item.Key))
                        {
                            // Override with changed value
                            node.Values[item.Key] = changedNode.Values[item.Key];
                        }
                        else
                        {
                            throw new ConcurrentModificationException("Concurrent modification of scalar value not allowed in type:" + typesService.GetTypeFromId(typeId).ToString() + " for member " + typesService.GetMemberName(typeId, item.Key));
                        }
                    }
                }
            }

            // Consolidate the edges
            foreach (var edge in originalNode.Edges)
            {
                // Value was changed compared to previous version
                if (!changedNode.Edges[edge.Key].ToNodeId.Equals(edge.Value.ToNodeId))
                {
                    // Was member set to override?
                    bool isOverride = objectAttributeProvider.IsMemberOverride(typeId, (Guid)edge.Key.Data);

                    // Try setting value in the node, if it is still the same or if override is set
                    if (node.Edges[edge.Key].ToNodeId.Equals(edge.Value.ToNodeId) || isOverride)
                    {
                        Guid newReference = changedNode.Edges[edge.Key].ToNodeId;
                        Guid newReferenceUpdated = Guid.Empty;

                        // Is there an intermediate change?
                        if (parameters.IntermediateChanges.TryGetValue(newReference, out newReferenceUpdated))
                        {
                            // Set edge to node from the changed node
                            node.SetEdgeToNode(edge.Value.Data, newReferenceUpdated);
                            // See about the new object
                            mergeRecursive(newReferenceUpdated, parameters);
                        }
                        else
                        {
                            NodeState nodeState = NodeState.None;
                            parameters.ChangeSet.NodeStates.TryGetValue(newReference, out nodeState);

                            // If changed node is a new node go into the changed node recursion
                            if (nodeState == NodeState.Created)
                            {
                                // Set the new reference
                                node.SetEdgeToNode(edge.Value.Data, newReference);
                                // Insert the new nodes
                                insertRecursive(newReference, parameters);
                            }
                            else
                            {
                                // Set the new reference
                                node.SetEdgeToNode(edge.Value.Data, newReference);
                                // Follow the new reference and see changes
                                mergeRecursive(newReference, parameters);
                            }
                        }
                    }
                    else
                    {
                        throw new ConcurrentModificationException("Concurrent modification of referenced item not allowed in type:" + typesService.GetTypeFromId(typeId).ToString() + " for member " + typesService.GetMemberName(typeId, (Guid)edge.Key.Data));
                    }
                }
                else
                {
                    mergeRecursive(node.Edges[edge.Key].ToNodeId, parameters);
                }
            }
        }
    }
}
