class ParentMapProvider
  constructor: (@nodes, @dataNodeProvider, @fallbackParentMapProvider, @filterMutable) ->
    if(@nodes.equals(@dataNodeProvider))
      throw "Argument exception"

    @ParentsTreeOrder = 100
    @createdNodes = false
    @lastSnapshotId = Guid.EMPTY

  ParentEdges: (snapshotId, nodeId) ->
    if (not @createdNodes)
      @createdNodes = true;
      this.AddParentsRecursive(snapshotId, new Dictionary(), @nodes);
      @lastSnapshotId = snapshotId

    if (snapshotId != @lastSnapshotId)

    # Try going for fallback provider
      if (@fallbackParentMapProvider != null)
        return @fallbackParentMapProvider.parentEdges(snapshotId, nodeId);
      else
        # If not, generate asked snapshot
        @nodes.Clear();
        this.AddParentsRecursive(snapshotId, new Dictionary(), @nodes);
        @lastSnapshotId = snapshotId

    # Find edge leading to holder node for given ID
    if (nodes.Contains(nodeId))
      return BPlusTreeOperations.GetEnumerator(@nodes, nodeId, EdgeType.Contains);
    else
      return null


  UpdateParents: (changeSet, collectedNodesProvider) ->
    if (changeSet.sourceSnapshotId != @lastSnapshotId)
      # If the last snapshot is not in the memory, clear and return
      @nodes.Clear()
      @createdNodes = false
      return

    enumerator = collectedNodesProvider.GetEdges(changeSet.sourceSnapshotId)

    if(enumerator?)
      while (enumerator.MoveNext())
        if (@nodes.Contains(enumerator.Current().toNodeId))
          this.DeleteTree(enumerator.Current().toNodeId)

        node = @dataNodeProvider.GetNode(enumerator.Current().toNodeId, NodeAccess.Read)

        for key in node.edges.values.Keys()
          elem = node.edges.values.Get(key)
          if(this.EdgeFilter(elem))
            if (nodes.Contains(edge.ToNodeId))
              BPlusTreeOperations.RemoveEdge(@nodes, edge.toNodeId, new EdgeData(EdgeType.Contains, enumerator.Current().toNodeId), @ParentsTreeOrder)

    for nodeId in changeSet.nodes.EnumerateNodes()
      holderNode = BPlusTreeOperations.CreateRootNode(NodeType.Collection, nodeId)
      nodes.SetNode(nodeId, holderNode)

    for nodeId in changeSet.reusedNodes.Keys()
      if(not nodes.Contains(nodeId))
        holderNode = BPlusTreeOperations.CreateRootNode(NodeType.Collection, nodeId)
        @nodes.SetNode(nodeId, holderNode)

    for nodeId in changeSet.nodes.EnumerateNodes()
      node = changeSet.nodes.GetNode(nodeId, NodeAccess.Read)

      for key in node.edges.values.Keys()
        elem = node.edges.values.Get(key)
        if(this.EdgeFilter(elem))
          edgeData = new EdgeData(EdgeType.Contains, nodeId)
          existingEdge = null
          rez = BPlusTreeOperations.TryFindEdge(@nodes, elem.toNodeId, edgeData)
          if(not rez.result)
            BPlusTreeOperations.InsertEdge(@nodes, elem.toNodeId, new Edge(nodeId, edgeData), @ParentsTreeOrder)

    lastSnapshotId = changeSet.destinationSnapshotId

  EdgeFilter: (edge) ->
    if(@filterMutable)
      return not UTILS.IsPermanentEdge(edge)
    else
      return ((edge.data.flags ==  EdgeFlags.Permanent) == EdgeFlags.Permanent) and not(edge.toNodeId == Constants.NullReferenceNodeId) and not (edge.data.semantic == EdgeType.OfType)

  AddParentsRecursive: (nodeId, visitedNodes, nodes) ->
    if(nodeId == Constants.NullReferenceNodeId)
      return

    if(visitedNodes.Contains(nodeId))
      return
    else
      visitedNodes.Add(nodeId, null)

    for key in @dataNodeProvider.GetNode(nodeId, NodeAccess.Read).edges.Keys()
      edge = @dataNodeProvider.GetNode(nodeId, NodeAccess.Read).edges.Get(key)

      if(this.EdgeFilter(edge))

        if (not nodes.Contains(edge.toNodeId))
          holderNode = BPlusTreeOperations.CreateRootNode(NodeType.Collection, edge.toNodeId)
          nodes.SetNode(edge.toNodeId, holderNode)

        edgeData = new EdgeData(EdgeType.Contains, nodeId)
        existingEdge = null
        rez = BPlusTreeOperations.TryFindEdge(nodes, edge.toNodeId, edgeData)
        if(not rez.result)
          BPlusTreeOperations.InsertEdge(nodes, edge.toNodeId, new Edge(nodeId, edgeData), @ParentsTreeOrder)

    for key in @dataNodeProvider.GetNode(nodeId, NodeAccess.Read).edges.Keys()
      edge = @dataNodeProvider.GetNode(nodeId, NodeAccess.Read).edges.Get(key)
      this.AddParentsRecursive(edge.toNodeId, visitedNodes, nodes)

  DeleteTree: (nodeId) ->
    node = @nodes.GetNode(nodeId, NodeAccess.Read)
    if(node.data.equals(BPlusTreeOperations.InternalNodeData))
      for key in node.edges.Keys()
        edge = node.edges.Get(key)
        this.DeleteTree(edge.toNodeId)