namespace execom.iog.name, (exports) ->
  class exports.IsolatedNodeProvider
    constructor: (@parentProvider, @isolatedProvider) ->
      @nodeStates = new exports.Dictionary()

    SetNode: (identifier, node) ->
      @isolatedProvider.SetNode(identifier, node)
      if(not @nodeStates.Contains(identifier))
        @nodeStates.Add(identifier, exports.NodeState.Created)

    GetNode: (nodeId, access) ->
      if(access == exports.NodeAccess.ReadWrite)
        this.EnsureNode(nodeId)
        return @isolatedProvider.GetNode(nodeId, access)
      else
        if (@isolatedProvider.Contains(nodeId))
          return @isolatedProvider.GetNode(nodeId, access)
        else
          return @parentProvider.GetNode(nodeId, access)

    Contains: (identifier) ->
      return @isolatedProvider.Contains(identifier) or 
        @parentProvider.Contains(identifier)

    Remove: (identifier) ->
      this.EnsureNode(identifier);
      @nodeStates.Set(identifier, NodeState.Removed)

    EnumerateNodes: () ->
      return @parentProvider.EnumerateNodes()

    GetChanges: (snapshotId) ->
      return new exports.IsolatedChangeSet(snapshotId, @isolatedProvider,
        @nodeStates);

    EnumerateChanges: () ->
      return @isolatedProvider.EnumerateNodes()


    GetNodeState: (identifier) ->
      if(@nodeStates.Contains(identifier))
        return @nodeStates.Get(identifier)
      else
        return exports.NodeState.None

    Clear: () ->
      @isolatedProvider.Clear();
      @nodeStates.Clear();

    EnsureNode: (nodeId) ->
      if(not @isolatedProvider.Contains(nodeId))
        node = @parentProvider.GetNode(nodeId, exports.NodeAccess.Read)
        edgeList = new exports.Dictionary()

        for key in node.edges.Keys()
          edge = node.edges.Get(key)
          edgeList.Add(edge.data, new exports.Edge(edge.toNodeId, 
            new exports.EdgeData(edge.data.semantic, edge.data.data, 
            edge.data.flags)))

        valueList = new exports.Dictionary()

        for key in node.values.Keys()
          value = node.values.Get(key)
          valueList.Add(key, value)

        newNode = new exports.Node(node.nodeType, node.data, edgeList,
          valueList)
        newNode.commited = false
        @isolatedProvider.SetNode(nodeId, newNode)
        @nodeStates.Add(nodeId, exports.NodeState.Modified)