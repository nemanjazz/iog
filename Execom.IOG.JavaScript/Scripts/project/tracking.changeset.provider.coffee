class window.TrackingChangeSetProvider
  constructor: (@nodes) ->
    this.InitializeRoot()
    @TreeOrder = 100

  InitializeRoot: () ->
    if(not nodes.Contains(Constants.SnapshotsNodeId))
      node = BPlusTreeOperations.CreateRootNode(NodeType.Collection, Constants.SnapshotsNodeId)
      nodes.SetNode(Constants.SnapshotsNodeId, node)

  SetChangeSet: (changeSet) ->
    snapshotNode = BPlusTreeOperations.CreateRootNode(NodeType.Dictionary, changeSet.DestinationSnapshotId)
    @nodes.SetNode(changeSet.DestinationSnapshotId, snapshotNode)

    for key in changeSet.mapping.Keys()
      element = changeSet.mapping.Get(key)
      BPlusTreeOperations.InsertEdge(@nodes, changeSet.DestinationSnapshotId, new Edge(element.Value, new EdgeData(EdgeType.ListItem, key)), TreeOrder)

    BPlusTreeOperations.InsertEdge(@nodes, Constants.SnapshotsNodeId, new Edge(changeSet.destinationSnapshotId, new EdgeData(EdgeType.ListItem, changeSet.destinationSnapshotId)), TreeOrder)

  GetChangeSetEdges: (snapshotId) ->
    edge = null
    rez = BPlusTreeOperations.TryFindEdge(@nodes, Constants.SnapshotsNodeId, new EdgeData(EdgeType.ListItem, snapshotId))
    if(rez.result)
      return BPlusTreeOperations.GetEnumerator(@nodes, snapshotId, EdgeType.ListItem)
    else
      return new Enumerator()

  ContainsSnapshot: (snapshotId) ->
    edge = null
    return BPlusTreeOperations.TryFindEdge(@nodes, Constants.SnapshotsNodeId, new EdgeData(EdgeType.ListItem, snapshotId)).result

  RemoveChangeSet: (snapshotId) ->
    if(BPlusTreeOperations.RemoveEdge(@nodes, Constants.SnapshotsNodeId, new EdgeData(EdgeType.ListItem, snapshotId), TreeOrder))
      this.DeleteTree(snapshotId)

  DeleteTree: (nodeId) ->
    node = @nodes.GetNode(nodeId, NodeAccess.Read)

    if(node.data.equals(BPlusTreeOperations.InternalNodeData))
      for key in node.edges.Keys()
        elem = node.edges.Get(key)
        this.DeleteTree(elem.toNodeId)

    @nodes.Remove(nodeId)




