class CollectedNodesProvider
  constructor: (@nodes = new DirectNodeProvider(), @dataNodes = new DirectNodeProvider()) ->
    @TreeOrder = 30

  StoreChangeset: (changeSet, mutableParentMap, immutableParentMap) ->
    snapshotId = changeSet.sourceSnapshotId
    snapshotRoot = BPlusTreeOperations.CreateRootNode(NodeType.Collection, snapshotId)
    @nodes.SetNode(snapshotId, snapshotRoot)

    collectedNodes = new Dictionary()

    this.GetCollectedNodesRecursive(changeSet.sourceSnapshotId, changeSet, mutableParentMap, immutableParentMap, collectedNodes, new Dictionary())

    for nodeId in collectedNodes.Keys()
      BPlusTreeOperations.InsertEdge(@nodes, snapshotId, new Edge(nodeId, new EdgeData(EdgeType.ListItem, nodeId)), @TreeOrder)


  GetEdges: (snapshotId) ->
    return BPlusTreeOperations.GetEnumerator(@nodes, snapshotId, EdgeType.ListItem)

  Cleanup: (snapshotId) ->
    this.DeleteTree(snapshotId)

  GetCollectedNodesRecursive: (nodeId, changeSet, mutableParentMap, immutableParentMap, collectedNodes, visitedNodes) ->
    if(visitedNodes.Contains(nodeId))
      return

    visitedNodes.Add(nodeId, null)

    if(changeSet.reusedNodes.Contains(nodeId))
      return

    if(this.HasReusedParent(nodeId, changeSet, mutableParentMap, immutableParentMap, collectedNodes, new Dictionary()))
      return

    collectedNodes.Add(nodeId, null)

    node = dataNodes.GetNode(nodeId, NodeAccess.Read)

    for key in node.edges.Keys()
      edge = node.edges.Get(key)

      if(edge.data.semantic != EdgeType.OfType and edge.toNodeId != Constants.NullReferenceNodeId)
        this.GetCollectedNodesRecursive(edge.toNodeId, changeSet, mutableParentMap, immutableParentMap, collectedNodes, visitedNodes)

  HasReusedParent: (nodeId, changeSet, mutableParentMap, immutableParentMap, collectedNodes, visitedNodes) ->
    if(visitedNodes.Contains(nodeId))
      return visitedNodes.Get(nodeId)

    visitedNodes.Add(nodeId, false)

    if(changeSet.reusedNodes.Contains(nodeId))
      return true

    if(collectedNodes.Contains(nodeId))
      return false

    mutableParentEnumerator = mutableParentMap.ParentEdges(changeSet.SourceSnapshotId, nodeId)

    if(mutableParentEnumerator?)
      while(mutableParentEnumerator.MoveNext())
        if (HasReusedParent(mutableParentEnumerator.Current().toNodeId, changeSet, mutableParentMap, immutableParentMap, collectedNodes, visitedNodes))
          return true

    immutableParentEnumerator = immutableParentMap.ParentEdges(changeSet.sourceSnapshotId, nodeId)

    if (immutableParentEnumerator != null)
      while (immutableParentEnumerator.MoveNext())
        if (this.HasReusedParent(immutableParentEnumerator.Current().toNodeId, changeSet, mutableParentMap, immutableParentMap, collectedNodes, visitedNodes))
          return true

    return false

  DeleteTree: (nodeId) ->
    node = @nodes.GetNode(nodeId, NodeAccess.Read)

    if( exports.UTILS.equals(node.data, BPlusTreeOperations.InternalNodeData))
      for key in node.edges.Keys()
        edge = node.edges.Get(key)
        this.DeleteTree(edge.toNodeId)

    @nodes.Remove(nodeId)
