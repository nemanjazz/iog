// -----------------------------------------------------------------------
// <copyright file="CommitDataService.cs" company="Execom">
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
    using Execom.IOG.Providers;
    using Execom.IOG.Storage;
    using Execom.IOG.Services.Runtime;
    using System.Threading;
    using System.Collections.ObjectModel;
    using Execom.IOG.Services.Merging;
    using Execom.IOG.Exceptions;
    using System.Diagnostics;

    /// <summary>
    /// Service which performs data commit on a node provider
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal class CommitDataService
    {
        /// <summary>
        /// Nodes which will accept data changes
        /// </summary>
        private INodeProvider<Guid, object, EdgeData> nodes;

        /// <summary>
        /// Service which handles snapshots
        /// </summary>
        private SnapshotsService snapshotsService;

        /// <summary>
        /// Provider of node parent information
        /// </summary>
        private IParentMapProvider<Guid, object, EdgeData> mutableParentProvider;

        /// <summary>
        /// Provider of node parent information
        /// </summary>
        private IParentMapProvider<Guid, object, EdgeData> immutableParentProvider;

        private ICollectedNodesProvider<Guid, object, EdgeData> collectedNodesProvider;

        /// <summary>
        /// Provider of previous change sets
        /// </summary>
        private IChangeSetProvider<Guid, object, EdgeData> changeSetProvider;

        private object commitSync = new object();

        private TypesService typesService;

        private NodeMergeExecutor nodeMergeExecutor;

        /// <summary>
        /// Creates new instance of CommitDataService type
        /// </summary>
        /// <param name="nodes">Nodes which will accept data changes</param>
        /// <param name="snapshotsService">Service which handles snapshots</param>
        /// <param name="mutableParentProvider">Provider of node parent information</param>
        public CommitDataService(INodeProvider<Guid, object, EdgeData> nodes, TypesService typesService, SnapshotsService snapshotsService, IParentMapProvider<Guid, object, EdgeData> mutableParentProvider, IParentMapProvider<Guid, object, EdgeData> immutableParentProvider, IChangeSetProvider<Guid, object, EdgeData> changeSetProvider, NodeMergeExecutor nodeMergeExecutor, ICollectedNodesProvider<Guid, object, EdgeData> collectedNodesProvider)
        {
            this.nodes = nodes;
            this.snapshotsService = snapshotsService;
            this.mutableParentProvider = mutableParentProvider;
            this.immutableParentProvider = immutableParentProvider;
            this.changeSetProvider = changeSetProvider;
            this.nodeMergeExecutor = nodeMergeExecutor;
            this.typesService = typesService;
            this.collectedNodesProvider = collectedNodesProvider;
        }

        /// <summary>
        /// Accepts incoming isolated commit
        /// </summary>
        /// <param name="isolatedChangeSet">Isolated changeset</param>
        /// <returns>Appended changes</returns>
        public CommitResult<Guid> AcceptCommit(IsolatedChangeSet<Guid, object, EdgeData> isolatedChangeSet)
        {
            lock (commitSync)
            {
                var latestSnapshot = snapshotsService.GetLatestSnapshotId();
                if (latestSnapshot.Equals(isolatedChangeSet.SourceSnapshotId))
                {
                    AppendableChangeSet<Guid, object, EdgeData> appendableChangeSet = CreateAppendableChangeSet(isolatedChangeSet.SourceSnapshotId, Guid.NewGuid(), isolatedChangeSet);
                    // Commit is directly on the last snapshot
                    CommitDirect(appendableChangeSet);
                    return new CommitResult<Guid>(appendableChangeSet.DestinationSnapshotId, appendableChangeSet.Mapping);
                }
                else
                {
                    // There are snapshots in between
#if DEBUG
                    Debug.WriteLine("Source snapshot = " + isolatedChangeSet.SourceSnapshotId);
#endif
                    // Calculate changes between last snapshot and source snapshot
                    var intermediateChanges = ChangesBetween(isolatedChangeSet.SourceSnapshotId, latestSnapshot);

                    // Isolate portion of the last snapshot which is relevant for the change set
                    var subTree = IsolateSubTree(latestSnapshot, isolatedChangeSet, intermediateChanges);

                    // Create a brand new changeset which is compatible with last snapshot
                    // Do this by recursive walk through the last snapshot and perform the "compatible" operations done in the incomming changeset

                    var mergedChangeSet = CreateMergedChangeSet(latestSnapshot, subTree, isolatedChangeSet, intermediateChanges);

                    // When complete perform the CommitDirect of the new changeset
                    CommitDirect(mergedChangeSet);

                    // Start with created change mapping
                    var mapping = mergedChangeSet.Mapping;

                    // Merge intermediate changes items to it
                    foreach(var item in intermediateChanges)
                    {
                        AddChangeItem(mapping, item.Key, item.Value);
                    }

                    return new CommitResult<Guid>(mergedChangeSet.DestinationSnapshotId, mapping);
                }
            }
        }        

        #region Methods which create merged changeset        
        private AppendableChangeSet<Guid, object, EdgeData> CreateMergedChangeSet(Guid latestSnapshot, Hashtable subTree, IsolatedChangeSet<Guid, object, EdgeData> changeSet, Dictionary<Guid, Guid> intermediateChanges)
        {            
#if DEBUG
            Debug.WriteLine("Merging over intermediate changes");
#endif
            // We create node provider which will host changes to the last snapshot made by the merge process
            var isolatedNodes = new DirectNodeProviderUnsafe<Guid,object, EdgeData>(new MemoryStorageUnsafe<Guid, object>(), false);
            // Perform merging on the current thread
            IsolatedNodeProvider destinationProvider = new IsolatedNodeProvider(nodes, isolatedNodes, Thread.CurrentThread);
            IsolatedNodeProvider sourceProvider = new IsolatedNodeProvider(nodes, changeSet.Nodes, Thread.CurrentThread);

            // Make changes from incoming changes within a subtree
            MergeRecursive(latestSnapshot, new RecursiveResolutionParameters(subTree, destinationProvider, sourceProvider, changeSet, intermediateChanges, new Hashtable()));
            
            // We create an appendable change set from changes made to last snapshot, defining a new snapshot
            return CreateAppendableChangeSet(latestSnapshot, Guid.NewGuid(), destinationProvider.GetChanges(latestSnapshot));
        }

        private void MergeRecursive(Guid nodeId, RecursiveResolutionParameters parameters)
        {
            if (parameters.VisitedNodes.ContainsKey(nodeId))
            {
                return;
            }

            parameters.VisitedNodes.Add(nodeId, null);

            // Are we within the last snapshot affected sub tree
            if (parameters.SubTree.ContainsKey(nodeId))
            {
                // Create node which we will change
                var node = parameters.DestinationProvider.GetNode(nodeId, NodeAccess.ReadWrite);                

                // Perform the change depending on node type
                switch (node.NodeType)
                {
                    case NodeType.Snapshot:
                        foreach (var edge in node.Edges.Values)
                        {
                            MergeRecursive(edge.ToNodeId, parameters);
                        }
                        break;
                    case NodeType.Scalar:
                        throw new NotImplementedException("TODO");
                    case NodeType.Object:
                        {
                            // Is there a change in the changeset with conflicting change?
                            // TODO (nsabo) Optimize searching by new ID value
                            Guid changeNodeId = (Guid)parameters.SubTree[nodeId];

                            if (changeNodeId != Guid.Empty)
                            {
                                var changedNode = parameters.ChangeSet.Nodes.GetNode(changeNodeId, NodeAccess.Read);
                                var originalNode = nodes.GetNode(changeNodeId, NodeAccess.Read);

                                nodeMergeExecutor.MergeObjects(nodeId, originalNode, changedNode, node, new MergeRecursiveDelegate(MergeRecursive), new InsertRecursiveDelegate(InsertRecursive), parameters);                                
                            }
                            else
                            {
                                var changedNode = parameters.ChangeSet.Nodes.GetNode(nodeId, NodeAccess.Read);
                                if (changedNode != null)
                                {
                                    nodeMergeExecutor.ChangeObject(nodeId, changedNode, node, new MergeRecursiveDelegate(MergeRecursive), new InsertRecursiveDelegate(InsertRecursive), parameters);
                                }
                                else
                                {
                                    // Follow edges
                                    foreach (var item in node.Edges.Values)
                                    {
                                        MergeRecursive(item.ToNodeId, parameters);
                                    }
                                }
                            }

                            break;
                        }
                    case NodeType.Collection:
                    case NodeType.Dictionary:
                        {
                            var treeOrder = node.NodeType == NodeType.Collection ? CollectionInstancesService.BPlusTreeOrder : DictionaryInstancesService.BPlusTreeOrder;

                            // Search the changeset for new added elements
                            Dictionary<EdgeData, Edge<Guid, EdgeData>> addedEdges = null;
                            Dictionary<EdgeData, Edge<Guid, EdgeData>> removedEdges = null;
                            FindTreeAddedElements(nodeId, parameters, out addedEdges, out removedEdges);

                            foreach (var addedEdge in addedEdges.Values)
                            {
                                // Add node in the isolated provider
                                InsertRecursive(addedEdge.ToNodeId, parameters);
                                // Create edge from the current collection
                                BPlusTreeOperations.InsertEdge(parameters.DestinationProvider, nodeId, addedEdge, treeOrder);
                            }

                            foreach (var removedEdge in removedEdges.Values)
                            {
                                BPlusTreeOperations.RemoveEdge(parameters.DestinationProvider, nodeId, removedEdge.Data, treeOrder);
                            }

                            // Enumerate through existing elements and look for their changes
                            using (var enumerator = BPlusTreeOperations.GetEnumerator(nodes, nodeId, EdgeType.ListItem))
                            {
                                while (enumerator.MoveNext())
                                {
                                    if (!addedEdges.ContainsKey(enumerator.Current.Data) && !removedEdges.ContainsKey(enumerator.Current.Data))
                                    {
                                        MergeRecursive(enumerator.Current.ToNodeId, parameters);
                                    }
                                }
                            }

                            break;
                        }
                    default:
                        throw new ArgumentException("Unexpected node type in tree :" + node.NodeType);
                }

                parameters.DestinationProvider.SetNode(nodeId, node);
            }                       
        }

        private void FindTreeAddedElements(Guid nodeId, RecursiveResolutionParameters parameters, out Dictionary<EdgeData, Edge<Guid, EdgeData>> addedElements, out Dictionary<EdgeData, Edge<Guid, EdgeData>> removedElements)
        {
            addedElements = new Dictionary<EdgeData, Edge<Guid, EdgeData>>();
            removedElements = new Dictionary<EdgeData, Edge<Guid, EdgeData>>();

            // TODO (nsabo) Optimize searching by new ID value
            Guid originalNodeId = Guid.Empty;
            foreach (var item in parameters.IntermediateChanges)
            {
                if (item.Value.Equals(nodeId))
                {
                    originalNodeId = item.Key;
                    break;
                }
            }

            if (originalNodeId == Guid.Empty)
            {
                originalNodeId = nodeId;
            }

            // Go through changed list and see if some item has not been included in the last version
            using (var enumerator = BPlusTreeOperations.GetEnumerator(parameters.SourceProvider, originalNodeId, EdgeType.ListItem))
            {
                while (enumerator.MoveNext())
                {
                    var edge = enumerator.Current;
                    Edge<Guid, EdgeData> foundEdge = null;

                    // Try finding the element in last version
                    if (!BPlusTreeOperations.TryFindEdge(nodes, originalNodeId, edge.Data, out foundEdge))
                    {
                        addedElements.Add(edge.Data, edge);
                    }
                }
            }

            // Go through last version and see if there are elements which are not found in changed list
            using (var enumerator = BPlusTreeOperations.GetEnumerator(nodes, originalNodeId, EdgeType.ListItem))
            {
                while (enumerator.MoveNext())
                {
                    var edge = enumerator.Current;
                    Edge<Guid, EdgeData> foundEdge = null;

                    // Try finding the element in last version
                    if (!BPlusTreeOperations.TryFindEdge(parameters.SourceProvider, originalNodeId, edge.Data, out foundEdge))
                    {
                        removedElements.Add(edge.Data, edge);
                    }
                }
            }
        }

        private void InsertRecursive(Guid nodeId, RecursiveResolutionParameters parameters)
        {
            if (parameters.VisitedNodes.ContainsKey(nodeId))
            {
                return;
            }

            parameters.VisitedNodes.Add(nodeId, null);

            Node<Guid, object, EdgeData> node = null;

            // Get new node from changeset
            if (parameters.ChangeSet.Nodes.Contains(nodeId))
            {
                node = parameters.ChangeSet.Nodes.GetNode(nodeId, NodeAccess.Read);
            }
            else
            {
                node = nodes.GetNode(nodeId, NodeAccess.Read);
            }

            // Process edges
            foreach (var item in node.Edges)
            { 
                // Upgrade reference to existing object when changed
                Guid newReferenceId = Guid.Empty;
                if (!Utils.IsPermanentEdge(item.Value) && // Not permanent edge
                    parameters.IntermediateChanges.TryGetValue(item.Value.ToNodeId, out newReferenceId)) // There is newer version
                {
                    // Update new reference
                    node.Edges[item.Key].ToNodeId = newReferenceId;
                }

                // Follow reference to new object
                NodeState referenceState = NodeState.None;
                if (parameters.ChangeSet.NodeStates.TryGetValue(item.Value.ToNodeId, out referenceState))
                {
                    if (referenceState == NodeState.Created)
                    {
                        InsertRecursive(item.Value.ToNodeId, parameters);
                    }
                }
            }

            parameters.DestinationProvider.SetNode(nodeId, node);
        }        

        /// <summary>
        /// Creates map of node ids in the last snapshot which are affected by changes in the change set.
        /// </summary>
        /// <param name="snapshotId">Last snapshot</param>
        /// <param name="changeSet">Change set which is created</param>
        /// <param name="intermediateChanges">Intermediate changes</param>
        /// <returns>Table of IDs for nodes which are modified in the last snapshot</returns>
        private Hashtable IsolateSubTree(Guid snapshotId, IsolatedChangeSet<Guid, object, EdgeData> changeSet, Dictionary<Guid, Guid> intermediateChanges)
        {
            Hashtable result = new Hashtable();

            Collection<Guid> changes = new Collection<Guid>();

            foreach (Guid item in changeSet.Nodes.EnumerateNodes())
            {
                // Test if item is new or modified
                if (changeSet.NodeStates[item] == NodeState.Modified)
                {
                    Guid nodeId = Guid.Empty;
                    Guid oldNodeId = Guid.Empty;

                    // Try finding the latest id of the changed item
                    if (intermediateChanges.TryGetValue(item, out nodeId))
                    {
                        oldNodeId = item;
                    }
                    else 
                    {
                        // It was not changed in the meantime, so the original ID should be still in use
                        nodeId = item;
                    }

                    // Remember new node ID with old node ID as value
                    result.Add(nodeId, oldNodeId);    
                
                    // Changes 
                    changes.Add(nodeId);
                }
            }

            foreach (var item in changes)
            {
                AddSubTreeParentsRecursive(snapshotId, item, result);
            }

            return result;
        }

        private void AddSubTreeParentsRecursive(Guid snapshotId, Guid nodeId, Hashtable table)
        {            
            var parents = mutableParentProvider.ParentEdges(snapshotId, nodeId);

            if (parents == null)
            {
                // For example long transaction is changing an element, but the intermediate has excluded it from the list
                // Maybe this should be an option somewhere?
                // throw new ConcurrentModificationException("Changed object is no longer reachable in the last snapshot");
                return;
            }
            else
            {
                using (parents)
                {
                    while (parents.MoveNext())
                    {
                        if (!table.ContainsKey(parents.Current.ToNodeId))
                        {
                            table.Add(parents.Current.ToNodeId, Guid.Empty);
                            AddSubTreeParentsRecursive(snapshotId, parents.Current.ToNodeId, table);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates map of changes from old ID to new ID given the two snapshot IDs
        /// </summary>
        /// <param name="source">Source snapshot ID</param>
        /// <param name="destination">Destination snapshot ID</param>
        /// <returns>Dictionary mapping (old Id -> new Id)</returns>
        public Dictionary<Guid, Guid> ChangesBetween(Guid source, Guid destination)
        {
            var snapshots = snapshotsService.SnapshotsBetweenReverse(source, destination);

            Dictionary<Guid, Guid> changes = new Dictionary<Guid, Guid>();

            foreach (var snapshotId in snapshots)
            {
                // Skip changes that lead to the source
                if (snapshotId != source)
                {
                    if (changeSetProvider.ContainsSnapshot(snapshotId))
                    {
//#if DEBUG
//                        Debug.WriteLine("Changes for snapshot " + snapshotId);
//                        using (var change = changeSetProvider.GetChangeSetEdges(snapshotId))
//                        {
//                            while (change.MoveNext())
//                            {
//                                Debug.WriteLine(change.Current.Data.Data.ToString() + " -> " + change.Current.ToNodeId);
//                            }
//                        }
//#endif

                        using (var change = changeSetProvider.GetChangeSetEdges(snapshotId))
                        {
                            AppendChange(change, changes);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("There is no commit history for given snapshot");
                    }
                }
            }

            return changes;
        }

        /// <summary>
        /// Adds changes from given changeset to changes map
        /// </summary>
        /// <param name="change">Change set to add</param>
        /// <param name="changes">Existing changes map</param>
        private void AppendChange(IEnumerator<Edge<Guid, EdgeData>> change, Dictionary<Guid, Guid> changes)
        {
            while (change.MoveNext())
            {
                var key = (Guid)change.Current.Data.Data;
                var value = change.Current.ToNodeId;

                AppendChangeItem(changes, key, value);
            }
        }

        /// <summary>
        /// Appends a single change item to the existing change map given by source -> destination 
        /// </summary>
        /// <param name="mapping">Change map</param>
        /// <param name="source">Source Id</param>
        /// <param name="destination">Destination Id</param>
        private static void AppendChangeItem(Dictionary<Guid, Guid> mapping, Guid source, Guid destination)
        {
            if (mapping.ContainsKey(destination))
            {
                var existingValue = mapping[destination];
                mapping.Remove(destination);
                mapping.Add(source, existingValue);
            }
            else
            {
                if (!mapping.ContainsKey(source))
                {
                    mapping.Add(source, destination);
                }
                else
                {
                    throw new ArgumentException("Key already in map");
                }
            }
        }

        /// <summary>
        /// Adds a single change item to the existing change map given by source -> destination 
        /// </summary>
        /// <param name="mapping">Change map</param>
        /// <param name="source">Source Id</param>
        /// <param name="destination">Destination Id</param>
        private static void AddChangeItem(Dictionary<Guid, Guid> mapping, Guid source, Guid destination)
        {
            if (mapping.ContainsKey(destination))
            {
                var existingValue = mapping[destination];
                mapping.Add(source, existingValue);
            }
            else
            {
                if (!mapping.ContainsKey(source))
                {
                    mapping.Add(source, destination);
                }
                else
                {
                    throw new ArgumentException("Key already in map");
                }
            }
        }
    
        #endregion

        #region Methods which create change set from isolated changes
        /// <summary>
        /// Creates a changeset from isolated changes
        /// </summary>
        /// <param name="baseSnapshotId">Base snapshot of the change set</param>
        /// <returns>Change set object</returns>        
        private AppendableChangeSet<Guid, object, EdgeData> CreateAppendableChangeSet(Guid baseSnapshotId, Guid newSnapshotId, IsolatedChangeSet<Guid, object, EdgeData> changeSet)
        {
            Dictionary<Guid, Guid> nodeMapping = new Dictionary<Guid, Guid>(); // Mapping old->new node id
            Dictionary<Guid, NodeState> nodeStates = new Dictionary<Guid, NodeState>();
            Hashtable reusedNodes = new Hashtable();
            var tree = CreateAppendableChangeSetTree(baseSnapshotId, newSnapshotId, new MemoryStorageUnsafe<Guid, object>(), nodeMapping, nodeStates, changeSet, reusedNodes);
            return new AppendableChangeSet<Guid, object, EdgeData>(baseSnapshotId, newSnapshotId, tree, nodeMapping, nodeStates, reusedNodes);
        }

        /// <summary>
        /// Creates new delta tree describing the new snapshot
        /// </summary>
        /// <param name="baseSnapshotId">Base snapshot of the change set</param>
        /// <param name="newSnapshotId">New snapshot of the change set</param>
        /// <returns>Delta tree nodes</returns>
        private INodeProvider<Guid, object, EdgeData> CreateAppendableChangeSetTree(Guid baseSnapshotId, Guid newSnapshotId, IKeyValueStorage<Guid, object> storage, Dictionary<Guid, Guid> nodeMapping, Dictionary<Guid, NodeState> nodeStates, IsolatedChangeSet<Guid, object, EdgeData> changeSet, Hashtable reusedNodes)
        {
            DirectNodeProviderUnsafe<Guid, object, EdgeData> delta = new DirectNodeProviderUnsafe<Guid, object, EdgeData>(storage, storage is IForceUpdateStorage);

            // Create snapshot in delta
            nodeMapping.Add(baseSnapshotId, newSnapshotId);
            var snapshotNode = new Node<Guid, object, EdgeData>(NodeType.Snapshot, null);
            snapshotNode.Previous = baseSnapshotId;
            // Link to root object
            snapshotNode.AddEdge(new Edge<Guid, EdgeData>(snapshotsService.GetRootObjectId(baseSnapshotId), new EdgeData(EdgeType.RootObject, null)));
            // Set node to provider
            delta.SetNode(newSnapshotId, snapshotNode);

            // Add all changes and parent nodes
            foreach (Guid nodeId in changeSet.Nodes.EnumerateNodes())
            {
                AddNodeToAppendableTreeRecursive(delta, nodeId, nodeMapping, nodeStates, changeSet);
            }

            // Prepare list of unreferenced node IDs
            Dictionary<Guid, Collection<Guid>> references = new Dictionary<Guid, Collection<Guid>>();

            // Resolve edges based on mapping
            foreach (Guid nodeId in delta.EnumerateNodes())
            {
                references.Add(nodeId, new Collection<Guid>());
            }

            // Resolve edges based on mapping
            foreach (Guid nodeId in delta.EnumerateNodes())
            {
                var node = delta.GetNode(nodeId, NodeAccess.ReadWrite);                

                foreach (var edge in node.Edges.Values)
                {
                    // Permanent edges should not be touched
                    if (!Utils.IsPermanentEdge(edge))
                    {
                        // Reroute based on node mapping
                        if (nodeMapping.ContainsKey(edge.ToNodeId))
                        {
                            edge.ToNodeId = nodeMapping[edge.ToNodeId];
                        }
                        else
                        {
                            if (!reusedNodes.ContainsKey(edge.ToNodeId))
                            {
                                reusedNodes.Add(edge.ToNodeId, null);
                            }
                        }

                        // Change edge data if it points to changed node
                        if (edge.Data.Data != null && edge.Data.Data is Guid)
                        {
                            if (nodeMapping.ContainsKey((Guid)edge.Data.Data))
                            {
                                edge.Data = new EdgeData(edge.Data.Semantic, nodeMapping[(Guid)edge.Data.Data]);
                            }
                        }
                    }
                    else
                    {
                        if (!reusedNodes.ContainsKey(edge.ToNodeId))
                        {
                            reusedNodes.Add(edge.ToNodeId, null);
                        }
                    }

                    if (references.ContainsKey(edge.ToNodeId))
                    {
                        references[edge.ToNodeId].Add(nodeId);
                    }
                }
            }

            // Remove remaining unreferenced nodes
            bool removed = false;

            do
            {
                removed = false;

                foreach (Guid key in delta.EnumerateNodes())
                {
                    // There are no references to the key
                    if ((key!=newSnapshotId) && (references[key].Count == 0))
                    {
#if DEBUG
                        Debug.WriteLine("Removed unreferenced key " + key);
#endif
                        delta.Remove(key);

                        reusedNodes.Remove(key);

                        nodeStates.Remove(key);

                        foreach (Guid otherKey in references.Keys)
                        {
                            references[otherKey].Remove(key);
                        }

                        foreach (var mappingKey in nodeMapping.Keys)
                        {
                            if (nodeMapping[mappingKey] == key)
                            {
                                nodeMapping.Remove(mappingKey);
                                break;
                            }
                        }

                        removed = true;
                        break;
                    }
                }
            }
            while (removed);

            // Isolated provider nodes have been corrupted, perform clear
            changeSet.Nodes.Clear();
            changeSet.NodeStates.Clear();

            return delta;
        }

        /// <summary>
        /// Adds node to delta with recursive pass through node parents
        /// </summary>
        /// <param name="delta">Provider which will accept new nodes</param>
        /// <param name="nodeId">Node ID to add</param>
        /// <param name="nodeMapping">Mapping between (old ID)->(new ID)</param>
        private void AddNodeToAppendableTreeRecursive(DirectNodeProviderUnsafe<Guid, object, EdgeData> delta, Guid nodeId, Dictionary<Guid, Guid> nodeMapping, Dictionary<Guid, NodeState> nodeStates, IsolatedChangeSet<Guid, object, EdgeData> changeSet)
        {
            // Skip already added nodes
            if (!nodeMapping.ContainsKey(nodeId))
            {
                // Generate new ID
                Guid newId = Guid.NewGuid();
                // Check node state
                NodeState nodeState = NodeState.None;

                changeSet.NodeStates.TryGetValue(nodeId, out nodeState);

                switch (nodeState)
                {
                    case NodeState.None:
                        {
                            // Register in the mapping
                            nodeMapping.Add(nodeId, newId);
                            // Get undelrying node
                            var node = nodes.GetNode(nodeId, NodeAccess.Read);

                            var newNode = CloneNode(node);

                            // Set node to commited
                            newNode.Commited = true;

                            // Create edge to previous node
                            newNode.Previous = nodeId;

                            // New node is created which is copied from underlying provider
                            delta.SetNode(newId, newNode);

                            // This change is defined as modification
                            nodeStates.Add(newId, NodeState.Modified);

                            // Add node parents from the current snapshot
                            using (var enumerator = mutableParentProvider.ParentEdges(changeSet.SourceSnapshotId, nodeId))
                            {
                                if (enumerator != null)
                                {
                                    while (enumerator.MoveNext())
                                    {
                                        AddNodeToAppendableTreeRecursive(delta, enumerator.Current.ToNodeId, nodeMapping, nodeStates, changeSet);
                                    }
                                }
                            }

                            // Update parent nodes
                            UpdateParentNodes(newId, newNode, nodeState, delta, changeSet);
                        }
                        break;
                    case NodeState.Created:
                        {
                            // Read the node
                            var node = changeSet.Nodes.GetNode(nodeId, NodeAccess.ReadWrite);
                            // Set node to commited
                            node.Commited = true;
                            // There is no previous
                            node.Previous = Guid.Empty;
                            // Store to delta
                            delta.SetNode(nodeId, node);
                            // This change is defined as creation
                            nodeStates.Add(nodeId, NodeState.Created);

                            // Update parent nodes
                            UpdateParentNodes(nodeId, node, nodeState, delta, changeSet);
                        }
                        break;
                    case NodeState.Modified:
                        {
                            // Register in the mapping
                            nodeMapping.Add(nodeId, newId);
                            // Read the node
                            var node = changeSet.Nodes.GetNode(nodeId, NodeAccess.ReadWrite);
                            // Set node to commited
                            node.Commited = true;
                            // Set the previous
                            node.Previous = nodeId;
                            // Store to delta
                            delta.SetNode(newId, node);
                            // This change is defined as modification
                            nodeStates.Add(newId, NodeState.Modified);

                            // Add node parents from the current snapshot
                            using (var enumerator = mutableParentProvider.ParentEdges(changeSet.SourceSnapshotId, nodeId))
                            {
                                while (enumerator.MoveNext())
                                {
                                    AddNodeToAppendableTreeRecursive(delta, enumerator.Current.ToNodeId, nodeMapping, nodeStates, changeSet);
                                }
                            }

                            // Update parent nodes
                            UpdateParentNodes(newId, node, nodeState, delta, changeSet);
                        }
                        break;

                    case NodeState.Removed:
                        {
                            // Update parent nodes
                            var node = GetNode(nodeId, changeSet);
                            UpdateParentNodes(nodeId, node, nodeState, delta, changeSet);
                        }
                        break;
                    default:
                        throw new ArgumentException(nodeState.ToString());
                }
            }
        }

        /// <summary>
        /// For the given nod, goes through all the child nodes and adds that node to the parent node list of the child nodes.
        /// </summary>
        /// <param name="newId"></param>
        /// <param name="parentNode"></param>
        /// <param name="nodeState"></param>
        /// <param name="delta">Node provider which contains nodes which will be saved at the end of the commit process</param>
        private void UpdateParentNodes(Guid newId, Node<Guid, object, EdgeData> parentNode, NodeState nodeState, DirectNodeProviderUnsafe<Guid, object, EdgeData> delta, IsolatedChangeSet<Guid, object, EdgeData> changeSet)
        {
            foreach (Edge<Guid, EdgeData> edge in parentNode.Edges.Values)
            {
                if ((edge.Data.Flags & EdgeFlags.StoreParentNodes) == EdgeFlags.StoreParentNodes)
                {
                    if ((edge.Data as EdgeData).Semantic == EdgeType.Property)
                    {
                        var childNode = GetNode(edge.ToNodeId, changeSet);
                        if (childNode != null)
                        {
                            switch (childNode.NodeType)
                            {
                                case NodeType.Object:
                                    UpdateParentNode(newId, nodeState, delta, edge, childNode, parentNode);
                                    break;
                                case NodeType.Collection:
                                case NodeType.Dictionary:
                                    foreach (Edge<Guid, EdgeData> collectionEdge in childNode.Edges.Values)
                                    {
                                        if (collectionEdge.Data.Semantic.Equals(EdgeType.ListItem))
                                        {
                                            UpdateParentNode(newId, nodeState, delta, collectionEdge, GetNode(collectionEdge.ToNodeId, changeSet), parentNode);
                                        }
                                    }
                                    break;
                                default:
                                    throw new NotImplementedException("NodeType=" + childNode.NodeType);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the parent nodes list.
        /// If the node state is None, Created or Modified new node is added to the parent nodes list of all the child nodes (child nodes are nodes which are referenced by the new node as properties).
        /// If the node state is Removed new node is removed from the parent nodes list of all the child nodes.
        /// </summary>
        /// <param name="newId"></param>
        /// <param name="nodeState"></param>
        /// <param name="delta"></param>
        /// <param name="edge"></param>
        /// <param name="childNode"></param>
        /// <param name="parentNode"></param>
        private static void UpdateParentNode(Guid newId, NodeState nodeState, DirectNodeProviderUnsafe<Guid, object, EdgeData> delta, Edge<Guid, EdgeData> edge, Node<Guid, object, EdgeData> childNode, Node<Guid, object, EdgeData> parentNode)
        {
            switch (nodeState)
            {
                case NodeState.None:
                case NodeState.Created:
                    AddParentToChildNode(newId, childNode);
                    break;
                case NodeState.Modified:
                    if (!parentNode.Previous.Equals(Guid.Empty) && childNode.ParentNodes.Contains(parentNode.Previous))
                    {
                        childNode.ParentNodes.Remove(parentNode.Previous);
                    }
                    AddParentToChildNode(newId, childNode);
                    break;
                case NodeState.Removed:
                    childNode.ParentNodes.Remove(newId);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Adds parent node to child nodes parent node list if that node is not already in the parents node list.
        /// </summary>
        /// <param name="newId"></param>
        /// <param name="childNode"></param>
        private static void AddParentToChildNode(Guid newId, Node<Guid, object, EdgeData> childNode)
        {
            if (!childNode.ParentNodes.Contains(newId))
            {
                childNode.ParentNodes.Add(newId);
            }
        }

        /// <summary>
        /// Returns node from the nodes list. If node does not exist in the nodes list node from the changeSet is returned.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="changeSet"></param>
        /// <returns></returns>
        private Node<Guid, object, EdgeData> GetNode(Guid nodeId, IsolatedChangeSet<Guid, object, EdgeData> changeSet)
        {
            var childNode = nodes.GetNode(nodeId, NodeAccess.ReadWrite);
            if (childNode != null)
            {
                return childNode;
            }
            return changeSet.Nodes.GetNode(nodeId, NodeAccess.ReadWrite);
        }

        private Node<Guid, object, EdgeData> CloneNode(Node<Guid, object, EdgeData> node)
        {
            Dictionary<EdgeData, Edge<Guid, EdgeData>> edgeList = new Dictionary<EdgeData, Edge<Guid, EdgeData>>();

            // Copy existing edges except the Previous
            foreach (var edge in node.Edges.Values)
            {
                edgeList.Add(edge.Data, new Edge<Guid, EdgeData>(edge.ToNodeId, new EdgeData(edge.Data.Semantic, edge.Data.Flags, edge.Data.Data)));
            }

            Dictionary<Guid, object> valueList = new Dictionary<Guid, object>();

            foreach (var valueItem in node.Values)
            {
                valueList.Add(valueItem.Key, valueItem.Value);
            }

            var newNode = new Node<Guid, object, EdgeData>(node.NodeType, node.Data, edgeList, valueList);
            return newNode;
        }
        #endregion

        /// <summary>
        /// Direct commit of data is appended on the existing nodes
        /// </summary>
        /// <param name="changeSet">Changes</param>
        private void CommitDirect(AppendableChangeSet<Guid, object, EdgeData> changeSet)
        {
            // Add all nodes to the provider
            foreach (Guid nodeId in changeSet.Nodes.EnumerateNodes())
            {
                var node = changeSet.Nodes.GetNode(nodeId, NodeAccess.Read);
                nodes.SetNode(nodeId, node);
            }

            // Store the change set in the change set provider
            changeSetProvider.SetChangeSet(changeSet);

            // Calculate collected nodes in the source changeset
            collectedNodesProvider.StoreChangeset(changeSet, mutableParentProvider, immutableParentProvider);

            // Update parent information in the parent map provider
            mutableParentProvider.UpdateParents(changeSet, collectedNodesProvider);

            // Update parent information in the immutable parent map provider
            immutableParentProvider.UpdateParents(changeSet, collectedNodesProvider);
            
            // Add new snapshot making it visible
            snapshotsService.AddSnapshot(changeSet.DestinationSnapshotId);

#if DEBUG
            Debug.WriteLine("Created snapshot " + changeSet.DestinationSnapshotId);

            Utils.LogNodesRecursive(changeSet.DestinationSnapshotId, nodes, changeSet.Nodes, 0, new Hashtable(), typesService);
            
            Debug.WriteLine("---------------------------------------------");

            Debug.WriteLine("Collected nodes are:");

            var enumerator = collectedNodesProvider.GetEdges(changeSet.SourceSnapshotId);

            if (enumerator != null)
            {
                using (enumerator)
                {
                    while (enumerator.MoveNext())
                    {
                        Debug.WriteLine(enumerator.Current.ToNodeId);
                    }
                }
            }

            Debug.WriteLine("---------------------------------------------");
#endif
        }

        
    }
}
