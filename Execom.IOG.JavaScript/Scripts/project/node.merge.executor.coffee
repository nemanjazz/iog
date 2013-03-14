class window.NodeMergeExecutor
  constructor: (@objectAttributeProvider, @typesService) ->

  MergeObjects: (nodeId, originalNode, changedNode, node, mergeRecursive, insertRecursive, parameters) ->
    typeId = @typesService.GetInstanceTypeId(originalNode)
    if(not @objectAttributeProvider.IsConcurrent(typeId))
      instanceType = @typesService.GetTypeFromId(@typesService.GetInstanceTypeId(nodeId))
      throw "Concurrent modification not allowed for entity type:" + instanceType.name

    if (@objectAttributeProvider.IsStaticConcurrency(typeId))
      this.MergeObjectsStatic(nodeId, typeId, originalNode, changedNode, node, mergeRecursive, insertRecursive, parameters)
    else
      throw "Dynamic concurrency not implemented"

  ChangeObject: (nodeId, changedNode, node, mergeRecursive,  insertRecursive, parameters) ->
    for key in changedNode.values.Keys()
      node.Add(key, changedNode.Get(key))

    for edge in changedNode.edges.Array()
      newReference = edge.toNodeId
      nodeState = NodeState.None
      nodeState = parameters.changeSet.nodeStates.Get(newReference)

      if(nodeState == null)
        nodeState = NodeState.None

      if(nodeState == NodeState.Created)
        node.SetEdgeToNode(edge.data, newReference)
        do insertRecursive(newReference, parameters)  #TODO maybe here I need to put do before function name
      else
        node.SetEdgeToNode(edge.data, newReference)
        do mergeRecursive(newReference, parameters) #TODO maybe here I need to put do before function name


  MergeObjectsStatic: (nodeId, typeId, originalNode, changedNode, node, mergeRecursive, insertRecursive, parameters) ->

    for key in originalNode.values.Keys()
      element = originalNode.values.Get(key)
      if(not changedNode.values.Get(key).equals(element))
        if(node.values.Get(key).equals(element))
          node.values.Set(key, changedNode.values.Get(key))
        else
          if(@objectAttributeProvider.IsMemberOverride(typeId, key))
            node.values.Set(key, changedNode.values.Get(key))
          else
            throw "Concurrent modification of scalar value not allowed in type:" + typesService.GetTypeFromId(typeId).name

    for edge in originalNode.edges
      isOverride = false
      if(not changedNode.edges.Get(edge.key).toNodeId == edge.value.toNodeId)
        isOverride = @objectAttributeProvider.IsMemberOverride(typeId, edge.key.data)

        if(node.edges.Get(edge.key).toNodeId == edge.value.toNodeId or isOverride)
          newReference = changedNode.edges.Get(edge.key).toNodeId
          newReferenceUpdated = Guid.EMPTY

          if( (newReferenceUpdated = parameters.intermediateChanges.Get(newReference))?)
            node.SetEdgeToNode(edge.value.data, newReferenceUpdated)
            mergeRecursive(newReferenceUpdated, parameters)
          else
            nodeState = NodeState.None
            nodeState = parameters.changeSet.nodeStates.Get(newReference)

            if (nodeState == NodeState.Created)
              node.SetEdgeToNode(edge.value.data, newReference)
              insertRecursive(newReference, parameters)
            else
              node.SetEdgeToNode(edge.value.data, newReference)
              mergeRecursive(newReference, parameters)

        else
          throw "Concurrent modification of referenced item not allowed in type:" + typesService.GetTypeFromId(typeId).name

      else
        mergeRecursive(node.edges.Get(edge.key).toNodeId, parameters)

