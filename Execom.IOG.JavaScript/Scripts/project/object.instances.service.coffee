namespace execom.iog.name, (exports) ->
  #Service with operations for management of plain instances (simple objects with properties)
  class exports.ObjectInstancesService
  # provider should be of DirectNodeProvider
  # typesService is of TypesService
    constructor: (@provider, @typesService) ->

      # Add instance of type
      # typeId Type id we wish to instanciate - Guid
      # <returns>Instance ID</returns> - Guid
    NewInstance: (typeId) ->
      id = exports.Guid.Create()

      node = new exports.Node(exports.NodeType.Object, null);
      node.commited = false;

      node.AddEdge(new exports.Edge(typeId, 
        new exports.EdgeData(exports.EdgeType.OfType, null)));

      this.InitializeInstance(node, typeId);

      @provider.SetNode(id, node);

      return id

    # Returns scalar value for instance member
    # instanceId Instance ID - Guid
    # memberId Member ID - Guid
    # <returns>Scalar value</returns> - object
    GetScalarInstanceMember: (instanceId, memberId) ->
      return @provider.GetNode(instanceId, exports.NodeAccess.Read).
        values.Get(memberId)

    # <returns>Scalar value</returns> - object
    GetScalarInstanceValue: (valueInstanceId) ->
      return @provider.GetNode(valueInstanceId, NodeAccess.Read).data;

    #return object with guid, and isPermanet boolean value
    GetReferenceInstanceMember: (instanceId, memberId) ->
      node = @provider.GetNode(instanceId, exports.NodeAccess.Read);
      edge = node.FindEdge(new exports.EdgeData(
        exports.EdgeType.Property, memberId));
      isPermanent = (edge.data.flags == 
        exports.EdgeFlags.Permanent) and node.commited;
      rezObject =
        "guid": edge.toNodeId
        "isPermanet": isPermanent
      return  rezObject

    # Sets scalar value for instance member
    # instanceId Instance ID - UUID
    # memberId Member ID - UUID
    # value Scalar value
    SetScalarInstanceMember: (instanceId, memberId, value) ->
      node = @provider.GetNode(instanceId, exports.NodeAccess.ReadWrite)

      if(node.values.Contains(memberId))
        node.values.Set(memberId, value) #[memberId] = value;
      else
        node.values.Add(memberId, value)

      @provider.SetNode(instanceId, node)
      return

    SetReferenceInstanceMember: (instanceId, memberId, referenceId) ->
      node = @provider.GetNode(instanceId, exports.NodeAccess.ReadWrite)
      node.SetEdgeToNode(new exports.EdgeData(exports.EdgeType.Property,
        memberId), referenceId)
      @provider.SetNode(instanceId, node);
      return


    # Initializes instance properties to default values
    # instanceId Instance ID to initialize - Node
    # typeId Type ID - GUID
    InitializeInstance: (instance, typeId) ->
      for edge in @typesService.GetTypeEdges(typeId)
        if(edge.data.semantic == exports.EdgeType.Property)
          memberId = edge.toNodeId;
          memberTypeId = @typesService.GetMemberTypeId(memberId);
          if(@typesService.IsScalarType(memberTypeId))
            value = @typesService.GetDefaultPropertyValue(memberTypeId)
            instance.values.Add(memberId, value);
          else
            isPermanentEdge = (edge.data.flags == exports.EdgeFlags.Permanent)
            instance.AddEdge(new exports.Edge(
              exports.Constants.NullReferenceNodeId, 
              new exports.EdgeData(exports.EdgeType.Property, 
              memberId, if isPermanentEdge then exports.EdgeFlags.Permanent else exports.EdgeFlags.None)));

      return

    # Returns type ID of given instance ID
    # instanceId Instance ID - Guid
    # return Type ID - GUID
    GetInstanceTypeId: (instanceId) ->
      return @provider.GetNode(instanceId, exports.NodeAccess.Read).
        FindEdge(new exports.EdgeData(exports.EdgeType.OfType, null)).toNodeId

    # Sets reference of instance as immutable
    # instanceId Instance ID - Guid
    # memberId Member ID to set as immutable - Guid
    SetImmutable: (instanceId, memberId) ->
      node = @provider.GetNode(instanceId, exports.NodeAccess.ReadWrite)
      edge = node.FindEdge(new exports.EdgeData(
        exports.EdgeType.Property, memberId))
      node.Edges.Remove(edge.data)
      newEdge = new exports.Edge(edge.toNodeId, 
        new exports.EdgeData(edge.data.semantic, edge.data.Data,
        exports.EdgeFlags.Permanent))
      node.AddEdge(newEdge)
      @provider.SetNode(instanceId, node)
      return