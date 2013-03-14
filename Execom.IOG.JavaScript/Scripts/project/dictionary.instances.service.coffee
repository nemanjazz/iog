namespace execom.iog.name, (exports) ->
  class exports.DictionaryInstancesService

  # Service for collection instance manipulations
  # provider - - DirectNodeProvider
  # typesService - - TypeService
    constructor: (@provider, @typesService) ->
      @bplusTreeOrder = 100

    # Initializes new dictionary instance
    # typeId - Type ID of dictionary - UUID
    # <returns>Instance ID</returns> - UUID
    NewInstance: (typeId) ->
      id = exports.Guid.Create()
      node = exports.BPlusTreeOperations.CreateRootNode(
        exports.NodeType.Dictionary, id)
      @provider.SetNode(id, node)
      exports.BPlusTreeOperations.InsertEdge(@provider, id, 
        new exports.Edge(typeId, new exports.EdgeData(
          exports.EdgeType.OfType, null)), @bplusTreeOrder);
      @provider.SetNode(id, node)
      return id

    AddScalar: (instanceId, itemTypeId, key, value) ->
      id = exports.Guid.Create()
      node = new exports.Node(exports.NodeType.Scalar, value)
      @provider.SetNode(id, node)
      exports.BPlusTreeOperations.InsertEdge(@provider, instanceId, 
        new exports.Edge(id, new exports.EdgeData(
          exports.EdgeType.ListItem, key)), @bplusTreeOrder)

    AddReference: (instanceId, key, referenceId) ->
      exports.BPlusTreeOperations.InsertEdge(@provider, instanceId, 
        new exports.Edge(referenceId, new exports.EdgeData(
          exports.EdgeType.ListItem, key)), @bplusTreeOrder)

    ContainsKey: (instanceId, key) ->
      return exports.BPlusTreeOperations.TryFindEdge(@provider, instanceId, 
        new exports.EdgeData(exports.EdgeType.ListItem, key)).result

    GetEnumerator: (instanceId) ->
      return exports.BPlusTreeOperations.GetEnumerator(
        @provider, instanceId, exports.EdgeType.ListItem)

    Remove: (instanceId, key) ->
      return BPlusTreeOperations.RemoveEdge(@provider, instanceId, 
        new exports.EdgeData(exports.EdgeType.ListItem, key), @bplusTreeOrder)

    TryGetScalar: (instanceId, key, value) ->
      rez = BPlusTreeOperations.TryFindEdge(@provider, instanceId,
        new exports.EdgeData(exports.EdgeType.ListItem, key))
      if rez.result
        value = @provider.GetNode(rez.value.toNodeId, 
          exports.NodeAccess.Read).data
        return {
        "result": true
        "value": value
        }
      else
        return {
        "result": false
        "value": null
        }

    TryGetReference: (instanceId, key) ->
      rez = exports.BPlusTreeOperations.TryFindEdge(@provider, instanceId, 
        new exports.EdgeData(exports.EdgeType.ListItem, key))
      if rez.result
        return {
        "result": rez.result
        "value": rez.value.toNodeId
        }
      else
        return {
        "result": false
        "value": exports.Guid.EMPTY
        }

    SetScalar: (instanceId, itemTypeId, key, value) ->
      id = exports.Guid.Create()
      node = new exports.Node(exports.NodeType.Scalar, value)
      @provider.SetNode(id, node)

      if not exports.BPlusTreeOperations.TrySetEdgeToNode(@provider, instanceId, new exports.EdgeData(EdgeType.ListItem, key), id)
        throw "Item not found with the specified key"

    SetReference: (instanceId, key, referenceId) ->
      if not exports.BPlusTreeOperations.TrySetEdgeToNode(@provider, instanceId, new exports.EdgeData(exports.EdgeType.ListItem, key), referenceId)
        throw "Item not found with the specified key"

    Clear: (instanceId) ->
      removalKeys = []
      enumerator = this.GetEnumerator(instanceId)

      while (enumerator.MoveNext())
        removalKeys.Add(enumerator.Current().data)

      for key in removalKeys
        exports.BPlusTreeOperations.RemoveEdge(
          @provider, instanceId, key, @bplusTreeOrder)

    Count: (instanceId) ->
      return exports.BPlusTreeOperations.Count(@provider, instanceId, 
        exports.EdgeType.ListItem)
