namespace execom.iog.name, (exports) ->
  #for this I need BPlusTreeOperations
  class exports.CollectionInstancesService

  # provider - - DirectNodeProvider
  # typesService - - TypeService
    constructor: (@provider, @typesService) ->
      @bplusTreeOrder = 100

    # Initializes new collection instance
    # typeId - Type ID of collection - UUID
    NewInstance: (typeId) ->
      id = exports.Guid.Create()
      node = exports.BPlusTreeOperations.CreateRootNode(
        exports.NodeType.Collection, id)
      @provider.SetNode(id, node)
      exports.BPlusTreeOperations.InsertEdge(@provider, id, new exports.Edge(typeId, new exports.EdgeData(
        exports.EdgeType.OfType, null)), @bplusTreeOrder)
      @provider.SetNode(id, node)
      return id

    AddScalar: (instanceId, itemTypeId, value, key = "") ->
      id = exports.Guid.Create()
      node = new exports.Node(exports.NodeType.Scalar, value)
      @provider.SetNode(id, node)

      if(key == "")
        data = exports.Guid.Create()
      else
        data = key

      exports.BPlusTreeOperations.InsertEdge(@provider, instanceId, 
        new exports.Edge(id,
        new exports.EdgeData(exports.EdgeType.ListItem, data)), @bplusTreeOrder)

    AddReference: ( instanceId, referenceId, key = "") ->
      if(key == "")
        data = exports.Guid.Create()
      else
        data = key
      exports.BPlusTreeOperations.InsertEdge(@provider, instanceId, 
        new exports.Edge(referenceId, 
        new exports.EdgeData(exports.EdgeType.ListItem, data)), 
        @bPlusTreeOrder)

    Clear: ( instanceId) ->
      removalKeys = []
      enumerator = this.GetEnumerator(instanceId)

      while(enumerator.MoveNext())
        removalKeys.push(enumerator.Current().data)

      for key in removalKeys
        exports.BPlusTreeOperations.RemoveEdge(@provider, instanceId, key,
          @bPlusTreeOrder)

    ContainsScalar: (instanceId, value, key = "") ->
      if(key == "")
        enumerator = this.GetEnumerator(instanceId)
        while (enumerator.MoveNext())
          node = @provider.GetNode(enumerator.Current().toNodeId, 
            exports.NodeAccess.Read)
          if node.data.equlas(value)
            return true

        return false
      else
        return exports.BPlusTreeOperations.TryFindEdge(@provider, instanceId, 
          new exports.EdgeData(exports.EdgeType.ListItem, key)).result

    ContainsReference: (instanceId, referenceId, key = "") ->
      if (key == "")
        enumerator = this.GetEnumerator(instanceId)

        while (enumerator.MoveNext())
          if (exports.UTILS.equals(enumerator.Current().toNodeId, referenceId))
            return true

        return false
      else
        return exports.BPlusTreeOperations.TryFindEdge(@provider, instanceId, 
          new exports.EdgeData(exports.EdgeType.ListItem, key)).result

    Count: (instanceId) ->
      return exports.BPlusTreeOperations.Count(@provider, instanceId, 
        exports.EdgeType.ListItem)

    MaxOrderedIdentifier: (instanceId) ->
      if (this.Count(instanceId) == 0)
        return 0
      else
        return exports.BPlusTreeOperations.RightEdge(@provider, 
          @provider.GetNode(instanceId, exports.NodeAccess.Read)).data.data

    RemoveScalar: (instanceId, value, key = "") ->
      if (key == "")
        enumerator = this.GetEnumerator(instanceId)

        while(enumerator.MoveNext())
          node = @provider.GetNode(enumerator.Current().toNodeId, 
            exports.NodeAccess.Read)
          if (exports.UTILS.equals(node.data, value))
            return exports.BPlusTreeOperations.RemoveEdge(@provider, instanceId,
              enumerator.Current().data, @bplusTreeOrder)

        return false
      else
        return exports.BPlusTreeOperations.RemoveEdge(@provider, instanceId, 
          new exports.EdgeData(exports.EdgeType.ListItem, key), @bplusTreeOrder)

    RemoveReference: (instanceId, referenceId, key = "") ->
      if key == ""
        enumerator = this.GetEnumerator(instanceId)

        while(enumerator.MoveNext())
          if (exports.UTILS.equals(enumerator.Current().toNodeId, referenceId))
            return exports.BPlusTreeOperations.RemoveEdge(@provider, instanceId, 
              enumerator.Current().data, @bplusTreeOrder)

        return false
      else
        return exports.BPlusTreeOperations.RemoveEdge(@provider, instanceId, 
          new exports.EdgeData(exports.EdgeType.ListItem, key), @bplusTreeOrder)

    IsCollectionInstance: (referenceId) ->
      data = @provider.GetNode(referenceId, exports.NodeAccess.Read).data
      if(data == null)
        data = exports.Guid.EMPTY
      isEmptyGuid = exports.UTILS.equals(data, exports.Guid.EMPTY)
      isInternalNode = exports.UTILS.equals(data, 
        exports.BPlusTreeOperations.InternalNodeData)
      isLeafNode = exports.UTILS.equals(data,
        exports.BPlusTreeOperations.LeafNodeData)
      return not isEmptyGuid and isInternalNode or isLeafNode

    GetInstanceTypeId: (referenceId) ->
      rez = exports.BPlusTreeOperations.TryFindEdge(@provider, referenceId, 
        new exports.EdgeData(exports.EdgeType.OfType, null))
      if rez.result
        return rez.value.toNodeId
      else
        return exports.Guid.EMPTY

    GetEnumerator: (instanceId) ->
      return exports.BPlusTreeOperations.GetEnumerator(@provider, instanceId, 
        exports.EdgeType.ListItem)

    #
    #<return>resutl - boolean, value - UUID</retunr> -
    TryFindReferenceByKey: (instanceId, key) ->
      rez = exports.BPlusTreeOperations.TryFindEdge(@provider, instanceId, 
        new exports.EdgeData(exports.EdgeType.ListItem, key))
      if rez.result
        return {
        "result": true
        "value": rez.value
        }
      else
        return {
        "result": false
        "value": exports.Guid.EMPTY
        }
