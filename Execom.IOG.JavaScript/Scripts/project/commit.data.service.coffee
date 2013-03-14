
class CommitDataService
  constructor: (@nodes, @typesService, @snapshotsService, @mutableParentProvider, @immutableParentProvider, @changeSetProvider,@nodeMergeExecutor, @collectedNodesProvider) ->

  AcceptCommit: (isolatedChangeSet) ->
    latestSnapshot = @snapshotsService.GetLatestSnapshotId()
    boolSnapIdEqualLatestSnap = exports.UTILS.equals(latestSnapshot, 
      isolatedChangeSet.sourceSnapshotId)

    if(boolSnapIdEqualLatestSnap)
      appendableChangeSet = CreateAppendableChangeSet(isolatedChangeSet.sourceSnapshotId, Guid.Create(), isolatedChangeSet)
      # Commit is directly on the last snapshot
      this.CommitDirect(appendableChangeSet)
      return new CommitResult(appendableChangeSet.destinationSnapshotId, appendableChangeSet.mapping)
    else
      intermediateChanges = this.ChangesBetween(isolatedChangeSet.sourceSnapshotId, latestSnapshot)
      subTree = this.IsolateSubTree(latestSnapshot, isolatedChangeSet, intermediateChanges)
      mergedChangeSet = this.CreateMergedChangeSet(latestSnapshot, subTree, isolatedChangeSet, intermediateChanges)

      # When complete perform the CommitDirect of the new changeset
      this.CommitDirect(mergedChangeSet)

      mapping = mergedChangeSet.mapping

      for key in intermediateChanges.Keys()
        item = intermediateChanges.Get(key)
        this.AddChangeItem(mapping, key, item)

      return new CommitResult(mergedChangeSet.destinationSnapshotId, mapping)

  CreateMergedChangeSet: (latestSnapshot, subTree, changeSet, intermediateChanges) ->

  # <return>added - - Dictionary, removed - - Dictionary</return>
  FindTreeAddedElements: ( nodeId, parameters) ->
    addedElements = new Dictionary()
    removedElements = new Dictionary()

    originalNodeId = Guid.EMPTY

    for key in parameters.intermediateChanges.Keys()
      item = parameters.intermediateChanges.Get(key)
      if(exports.UTILS.equals(item, nodeId))
        originalNodeId = key
        break

    if (exports.UTILS.equals(originalNodeId, Guid.EMPTY))
      originalNodeId = nodeId

    enumerator = BPlusTreeOperations.GetEnumerator(parameters.sourceProvider, originalNodeId, EdgeType.ListItem)

    while(enumerator.MoveNext())
      edge = enumerator.Current()
      foundEdge = null

      rez = BPlusTreeOperations.TryFindEdge(@nodes, nodeId, edge.data)
      if(not rez.result)
        addedElements.Add(edge.data, edge)

    enumerator = BPlusTreeOperations.GetEnumerator(@nodes, originalNodeId, EdgeType.ListItem)

    while (enumerator.MoveNext())
      edge = enumerator.Current()
      foundEdge = null
      rez = BPlusTreeOperations.TryFindEdge(parameters.sourceProvider, originalNodeId, edge.data)

      if(not rez.result)
        removedElements.Add(edge.data, edge)

  InsertRecursive: (nodeId, parameters) ->
    if(parameters.visitedNodes.Contains(nodeId))
      return

    parameters.visitedNodes.Add(nodeId, null)
    node = null
    if(parameters.changeSet.nodes.Contains(nodeId))
      node = parameters.changeSet.nodes.GetNode(nodeId, NodeAccess.Read)

    else
      node = nodes.GetNode(nodeId, NodeAccess.Read)

    for key in node.edges.Keys()
      item = node.edges.Get(key)

      newReferenceId = Guid.EMPTY
      newReferenceId = parameters.intermediateChanges.Get(item.toNodeId)
      if( not UTILS.IsPermanentEdge(item.Value) and newReferenceId? )
        node.edges.Get(key).toNodeId = newReferenceId

      referenceState = NodeState.None
      referenceState = parameters.changeSet.nodeStates.Get(item.toNodeId)
      if(referenceState? )
        if(referenceState == NodeState.Created)
          this.InsertRecursive(item.toNodeId, parameters)


    parameters.destinationProvider.SetNode(nodeId, node)

  IsolateSubTree: (snapshotId, changeSet, intermediateChanges) ->

    result = new Dictionary()

    changes = []

    for key in changeSet.Nodes.EnumerateNodes()
      if(changeSet.nodeStates.Get(item) == NodeState.Modified)
        nodeId = Guid.EMPTY
        oldNodeId = Guid.EMPTY

        # Try finding the latest id of the changed item
        if((nodeId = intermediateChanges.Get(item))? )
          oldNodeId = item
        else
          # It was not changed in the meantime, so the original ID should be still in use
          nodeId = item

        result.Add(nodeId, oldNOdeId)
        changes.Add(nodeId)

    for item in changes
      this.AddSubTreeParentsRecursive(snapshotId, item, result)

    return result

  AddSubTreeParentsRecursive: (snapshotId, nodeId, table) ->
    parents = @mutableParentProvider.ParentEdges(snapshotId, nodeId)

    if(parents == null)
      throw "Changed object is no longer reachable in the last snapshot"
    else
      while(parents.MoveNext())
        if(not table.Contains(parents.Current().toNodeId))
          table.Add(parents.Current().toNodeId, Guid.EMPTY)
          this.AddSubTreeParentsRecursive(snapshotId, parents.Current().toNodeId, table)

  ChangesBetween: (source, destination) ->
    snapshots = @snapshotsService.SnapshotsBetweenReverse(source, destination)
    changes = new Dictionary()

    for snapshotId in snapshots
      if(snapshotId != source)

        if(changeSetProvider.ContainsSnapshot(snapshotId))
          change = changeSetProvider.GetChangeSetEdges(snapshotId)
          this.AppendChange(change, changes)

        else
          throw "There is no commit history for given snapshot"

    return changes


  AppendChange: (change, changes) ->

    while (change.MoveNext())
      key = change.Current().data.data
      value = change.Current().toNodeId

      this.AppendChangeItem(changes, key, value)

  @AppendChangeItem: (mapping, source, destination) ->
    if(mapping.Contains(destination))
      existingValue = mapping.Get(destination)
      mapping.Remove(destination)
      mapping.Add(source, existingValue)

    else
      if(not mapping.Contains(source))
        mapping.Add(source, destination);
      else
        throw "Key already in map"

  @AddChangeItem: (mapping, source, destination) ->
    if(mapping.Contains(destination))
      existingValue = mapping.Get(destination)
      mapping.Add(source, existingValue)
    else
      if(not mapping.Contains(source))
        mapping.Add(source, destination);
      else
        throw "Key already in map"

  CreateAppendableChangeSet: (baseSnapshotId, newSnapshotId, changeSet) ->
    nodeMapping = new Dictionary()
    nodeStates = new Dictionary()
    reusedNodes = new Dictionary()
    tree = this.CreateAppendableChangeSetTree(baseSnapshotId, newSnapshotId, new MemoryStorage(), nodeMapping, nodeStates, changeSet, reusedNodes)
    return new AppendableChangeSet(baseSnapshotId, newSnapshotId, tree, nodeMapping, nodeStates, reusedNodes)

  CreateAppendableChangeSetTree: (baseSnapshotId, newSnapshotId, storage, nodeMapping, nodeStates, changeSet, reusedNodes) ->
    delta = new DirectNodeProvider(storage, storage is IForceUpdateStorage)
    nodeMapping.Add(baseSnapshotId, newSnapshotId)
    snapshotNode = new Node(NodeType.Snapshot, null)
    snapshotNode.previous = baseSnapshotId
    snapshotNode.AddEdge(new Edge(snapshotsService.GetRootObjectId(baseSnapshotId), new EdgeData(EdgeType.RootObject, null)))
    delta.SetNode(newSnapshotId, snapshotNode)

    for nodeId in changeSet.nodes.EnumerateNodes()
      this.AddNodeToAppendableTreeRecursive(delta, nodeId, nodeMapping, nodeStates, changeSet)

    references = new Dictionary()

    for nodeId in delta.EnumerateNodes()
      references.Add(nodeId, [])

    # Resolve edges based on mapping
    for nodeId in delta.EnumerateNodes()
      node = delta.GetNode(nodeId, NodeAccess.ReadWrite)

      for key in node.edges.values.Keys()
        edge = node.edges.values.Get(key)
        if(not UTILS.IsPermanentEdge(edge))
          # Reroute based on node mapping
          if (nodeMapping.Contains(edge.toNodeId))
            edge.toNodeId = nodeMapping.Get(edge.toNodeId)
          else
            if (not reusedNodes.Contains(edge.toNodeId))
              reusedNodes.Add(edge.toNodeId, null)

          if(edge.data.data? and not exports.UTILS.equals(Guid.tryParse(edge.data.data), Guid.EMPTY) )
            if(nodeMapping.Contains(edge.data.data))
              edge.data = new EdgeData(edge.data.semantic, nodeMapping.Get(edge.data.data))

        else
          if(not reusedNodes.Contains(edge.toNodeId))
            reusedNodes.Add(edge.toNodeId, null)

        if(references.Contains(edge.toNodeId))
          references.Get(edge.toNodeId).Add(nodeId)

    removed = false

    while(removed)
      removed = false
      for key in delta.EnumerateNodes()
        if((key != newSnapshotId) and (references.Get(key).Count() == 0))

          delta.Remove(key)

          for otherKey in references.Keys()
            references.Get(otherKey).Remove(key)

          for mappingKey in nodeMapping.Keys()
            if (nodeMapping.Get(mappingKey) == key)
              nodeMapping.Remove(mappingKey);
              break

          removed = true
          break

    changeSet.nodes.Clear();
    changeSet.nodeStates.Clear();

    return delta


  AddNodeToAppendableTreeRecursive: (delta, nodeId, nodeMapping, nodeStates, changeSet) ->
    if (not nodeMapping.Contains(nodeId))
      newId = Guid.Create()
      nodeState = NodeState.None

      nodeState = changeSet.NodeStates.Get(nodeId)

      switch nodeState
        when NodeState.None
          nodeMapping.Add(nodeId, newId)
          node = @nodes.GetNode(nodeId, NodeAccess.Read)
          newNode = this.CloneNode(node)
          newNode.commited = true
          newNode.previous = nodeId
          delta.SetNode(newId, newNode)
          nodeStates.Add(newId, NodeState.Modified)

          enumerator = mutableParentProvider.ParentEdges(changeSet.sourceSnapshotId, nodeId)

          if(enumerator != null)
            while(enumerator.MoveNext())
              this.AddNodeToAppendableTreeRecursive(delta, enumerator.Current().toNodeId, nodeMapping, nodeStates, changeSet)
        when NodeState.Created
          node = changeSet.nodes.GetNode(nodeId, NodeAccess.ReadWrite)
          node.commited = true
          node.previous = Guid.EMPTY
          delta.SetNode(nodeId, node)
          nodeStates.Add(nodeId, NodeState.Created)

        when NodeState.Modified
          nodeMapping.Add(nodeId, newId)
          node = changeSet.nodes.GetNode(nodeId, NodeAccess.ReadWrite)
          node.commited = true
          node.previous = nodeId
          delta.SetNode(newId, node)
          nodeStates.Add(newId, NodeState.Modified)

          enumerator = mutableParentProvider.ParentEdges(changeSet.sourceSnapshotId, nodeId)

          while(enumerator.MoveNext())
            this.AddNodeToAppendableTreeRecursive(delta, enumerator.Current().toNodeId, nodeMapping, nodeStates, changeSet)

        when NodeState.Removed
          throw "This is not implemented!"
        else
          throw "NodeState is " + nodeState

  CloneNode: (node) ->
    edgeList = new Dictionary()
    for key in node.edges.values.Keys()
      edge = node.edges.values.Get(key)
      edgeList.Add(edge.data, new Edge(edge.toNodeId, new EdgeData(edge.data.semantic, edge.data.flags, edge.data.data)))

    valueList = new Dictionary()

    for key in node.values.Keys()
      valueItem = node.values.Get(key)
      valueList.Add(valueItem.key, valueItem.value)

    newNode = new Node(node.nodeType, node.data, edgeList, valueList)
    return newNode

  CommitDirect: (changeSet) ->
    for nodeId in changeSet.nodes.EnumerateNodes()
      node = changeSet.nodes.GetNode(nodeId, NodeAccess.Read)
      @nodes.SetNode(nodeId, node)

    @changeSetProvider.SetChangeSet(changeSet)
    @collectedNodesProvider.StoreChangeset(changeSet, @mutableParentProvider, @immutableParentProvider)
    @mutableParentProvider.UpdateParents(changeSet, @collectedNodesProvider)
    @immutableParentProvider.UpdateParents(changeSet, @collectedNodesProvider)
    @snapshotsService.AddSnapshot(changeSet.destinationSnapshotId)
    # TODO pretty printing is missing
