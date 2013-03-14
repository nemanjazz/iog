class window.SnapshotsService
  constructor: (@provider = new DirectNodeProvider()) ->

  GetRootObjectId: () ->
    return @provider.GetNode(snapshotId, NodeAccess.Read).FindEdge(new EdgeData(EdgeType.RootObject, null)).toNodeId

  GetLatestSnapshotId: () ->
    node = @provider.GetNode(Constants.SnapshotsNodeId, NodeAccess.Read)
    edge = node.FindEdge(new EdgeData(EdgeType.Contains, null))
    return edge.toNodeId

  InitializeSnapshots: () ->
    if(not @provider.Contains(Constants.SnapshotsNodeId))
      snapshotsNode = new Node(NodeType.SnapshotsRoot, null)
      @provider.SetNode(Constants.SnapshotsNodeId, snapshotsNode)
      return true

    return false

  AddSnapshot: (snapshotId) ->
    node = new Node(NodeType.SnapshotsRoot, null)
    node.AddEdge(new Edge(snapshotId, new EdgeData(EdgeType.Contains, null)))
    @provider.SetNode(Constants.SnapshotsNodeId, node)

  SnapshotsBetweenReverse: (source, destination) ->
    res = []

    current = destination

    while(not exports.UTILS.equals(current, source))
      res.push(current)
      node = @provider.GetNode(current, NodeAccess.Read)
      current = node.previous

      if(exports.UTILS.equals(current, Guid.EMPTY) )
        throw "Destination snapshot is expected to be after the source snapshot"

    res.push(current)
    return res

  ListSnapshots: () ->
    res = []
    current = this.GetLatestSnapshotId()

    while(not exports.UTILS.equals(current, Guid.EMPTY))
      res.push(current)
      node = @provider.GetNode(current, NodeAccess.Read)
      current = node.previous

    return res

  RemoveUnusedSnapshots: (usedSnapshots) ->
    result = []
    current = this.GetLatestSnapshotId()
    lastUsedNodeId = Guid.EMPTY

    while(not exports.UTILS.equals(current, Guid.EMPTY) )
      if(usedSnapshots.indexOf(current) != -1 )
        #result = null
        #delete result #TODO check how to do delete
        result = []
        lastUsedNodeId = current
      else
        result.push(current)

      node = @provider.GetNode(current, NodeAccess.Read)
      current = node.previous

    lastUsedNode = @provider.GetNode(lastUsedNodeId, NodeAccess.ReadWrite)
    lastUsedNode.previous = Guid.EMPTY
    @provider.SetNode(lastUsedNodeId, lastUsedNode)

    return result

  ResetSnapshots: () ->
    lastUsedNodeId = this.GetLatestSnapshotId()
    lastUsedNode = @provider.GetNode(lastUsedNodeId, NodeAccess.ReadWrite)
    lastUsedNode.previous = Guid.EMPTY
    @provider.SetNode(lastUsedNodeId, lastUsedNode)
    return

