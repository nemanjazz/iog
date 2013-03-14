namespace execom.iog.name, (exports) ->
  class exports.TypesService
  # at the mommetn DirectNodeProvider is only class that can be used for this but later on there will be probably many more
    @IOG_TYPE = "iogType"

    constructor: (@provider = new exports.DirectNodeProvider()) ->
      @supportedScalarTypes = []
      @scalarTypesTable = new exports.Dictionary()
      @typeToIdMapping = new exports.Dictionary()
      @typeIdToTypeMapping = new exports.Dictionary()
      @collectionTypesTable = new exports.Dictionary()
      @dictionaryTypesTable = new exports.Dictionary()

    AddType: (type) ->
      this.ValidateType(type)

      typeId = this.GetTypeId(type)
      # Type not found?
      if(exports.UTILS.equals(typeId, exports.Guid.EMPTY))
        typeId = exports.Guid.Create()
        node = new exports.Node(exports.NodeType.Type, this.GetTypeName(type))
        typesNode = @provider.GetNode(exports.Constants.TypesNodeId, exports.NodeAccess.ReadWrite)
        tempEdgeData = new exports.EdgeData(exports.EdgeType.Contains, typeId)
        tempEdge = new exports.Edge(typeId, tempEdgeData)

        typesNode.AddEdge(tempEdge)
        @provider.SetNode(exports.Constants.TypesNodeId, typesNode)
        @provider.SetNode(typeId, node)

        if(type.isInterface)
          properties = []
          exports.UTILS.ExtractProperties(type, properties)

          for p in properties
            isPermanent = p.GetCustomAttributes("ImmutableAttribute")
            isPrimaryKey = p.GetCustomAttributes("PrimaryKeyAttribute")

            this.AddType(p.GetPropertyType())

            memberId = this.AddTypeMember(p.name, p.GetPropertyType(), 
              isPrimaryKey)

            node.AddEdge(new exports.Edge(memberId, new exports.EdgeData(
              exports.EdgeType.Property, memberId,  
              if isPermanent then exports.EdgeFlags.Permanent else exports.EdgeFlags.None)))

          collectionType=null;
          dictionaryType= null;

          if( ( (collectionType = exports.UTILS.IsCollectionType(type))?) and ( (dictionaryType = exports.UTILS.IsDictionaryType(type))?))
            for baseType in type.interfaces
              id = this.AddType(baseType)
              node.AddEdge(new exports.Edge(
                id, new exports.EdgeData(exports.EdgeType.OfType, id)))

            @provider.SetNode(typeId, node);

          else
            for baseType in type.genericArguments
              this.AddType(baseType)

          values = this.GetConstantValues(type)

          if(values.length > 0)
            for value in values
              valueId = Guid.Create()
              valueNode = new exports.Node(exports.NodeType.Scalar,
                value.toString());
              @provider.SetNode(valueId, valueNode);
              node.AddEdge(new exports.Edge(valueId, new exports.EdgeData(
                exports.EdgeType.Contains, value.toString())));

          @provider.SetNode(typeId, node);

      return typeId

    GetConstantValues: (type) ->
      list = []

      if(type.isEnum and type.customAttributes.indexOf("FlagsAttribute") == -1)
        for item in type.enumValues
          list.push(item)

      if(type.name == "Boolean")
        list.push(true)
        list.push(false)


      return list

    EnsureBasicScalarTypes: () ->
      this.AddType(element) for element in @supportedScalarTypes

    IsScalarType: (typeId) ->
      @scalarTypesTable.Contains(typeId);

    IsCollectionType: (typeId) ->
      @collectionTypesTable.Contains(typeId);

    IsDictionaryType: (typeId) ->
      @dictionaryTypesTable.Contains(typeId)

    GetTypeId: (type) ->
      name = this.GetTypeName(type)
      if(not @provider.GetNode(exports.Constants.TypesNodeId, exports.NodeAccess.Read)?)
        return exports.Guid.EMPTY

      for edge in @provider.GetNode(exports.Constants.TypesNodeId, exports.NodeAccess.Read).edges.Array()
        if(edge.data.semantic == exports.EdgeType.Contains)
          candidateNodeId = edge.toNodeId
          node = @provider.GetNode(candidateNodeId, exports.NodeAccess.Read)

          if(exports.UTILS.equals(node.data.name, name))
            return candidateNodeId

      return exports.Guid.EMPTY

    GetTypeIdCached: (type) ->
      result = exports.Guid.EMPTY

      if( (result = @typeToIdMapping.Get(type))?)
        return result
      else
        return exports.Guid.EMPTY

    GetTypeName: (type) ->
      if(type? and type.name?)
        type.name
      else
        throw "Type is null"
    #depending on AddType
    AddTypeMember: (name, type, isPrimaryKey) ->
      memberId = exports.Guid.Create()
      node = new exports.Node(exports.NodeType.TypeMember, name)

      if(isPrimaryKey)
        node.values.Add(exports.Constants.TypeMemberPrimaryKeyId, null)

      memberTypeId = this.GetTypeId(type)

      if(exports.UTILS.equals(memberTypeId, exports.Guid.EMPTY))
        memberTypeId = this.AddType(type)

      node.AddEdge(new exports.Edge(memberTypeId, 
        new exports.EdgeData(exports.EdgeType.OfType, null)));

      @provider.SetNode(memberId, node);

      return memberId;

    GetTypeMemberId: (typeId, propertyName ) ->

      for edge in @provider.GetNode(typeId, exports.NodeAccess.Read).edges.Array()
        if(edge.data.semantic == exports.EdgeType.Property)
          candidateNode = @provider.GetNode(edge.toNodeId,
            exports.NodeAccess.Read)

          if(exports.UTILS.equals(propertyName, candidateNode.data) )
            return edge.toNodeId

      return exports.Guid.EMPTY

    GetInstanceTypeId: (instanceID) ->
      return @provider.GetNode(instanceId, exports.NodeAccess.Read).FindEdge(
        new exports.EdgeData(exports.EdgeType.OfType, null)).toNodeId;

    ###
      Returns type identifier for given member
    ###
    GetMemberTypeId: ( memberId) ->
        return @provider.GetNode(memberId, exports.NodeAccess.Read).FindEdge(
          new exports.EdgeData(exports.EdgeType.OfType, null)).toNodeId



    InitializeTypeSystem: (types) ->
      @typeToIdMapping = new exports.Dictionary()
      @typeIdToTypeMapping = new exports.Dictionary()
      @supportedScalarTypes = [exports.BooleanType, exports.StringType, 
        exports.Int32Type, exports.Int64Type, exports.DoubleType, 
        exports.DateTimeType, exports.GuidType, exports.TimeSpanType, 
        exports.ByteType, exports.CharType]
      if @provider.Contains(exports.Constants.TypesNodeId)
        #TODO (nsabo) Type and data upgrade procedure,
        # this case is not finished in core library

      else
        @provider.SetNode(exports.Constants.TypesNodeId, 
          new exports.Node(exports.NodeType.TypesRoot, null));

        this.EnsureBasicScalarTypes()

        this.AddType(type) for type in types

      for typeId in this.GetTypes()
        type = this.GetTypeFromId(typeId)
        @typeToIdMapping.Add(type, typeId);
        @typeIdToTypeMapping.Add(typeId, type);

        collectionType = null;

        if (collectionType = exports.UTILS.IsCollectionType(type))?
          @collectionTypesTable.Add(typeId, type)

        if (collectionType = exports.UTILS.IsDictionaryType(type))?
          @dictionaryTypesTable.Add(typeId, type)

      this.CacheScalarTypes()

      return @typeToIdMapping

    GetTypes: () ->
      list = []
      node = @provider.GetNode(exports.Constants.TypesNodeId,
        exports.NodeAccess.Read)
      for edge in node.edges.Array()

        if(edge.data.semantic == exports.EdgeType.Contains)
          list.push(edge.toNodeId)

      list

    CacheScalarTypes: () ->
      for edge in @provider.GetNode(exports.Constants.TypesNodeId, exports.NodeAccess.Read).edges.Array()
        if(edge.data.semantic == exports.EdgeType.Contains)
          typeId = edge.toNodeId
          type = this.GetTypeFromId(typeId)

          if (this.IsSupportedScalarType(type))
            @scalarTypesTable.Add(typeId, type);

          if(type.isEnum)
            @scalarTypesTable.Add(typeId, type);

    IsSupportedScalarType: (type) ->

      for element in @supportedScalarTypes
        if exports.UTILS.equals(element, type)
          return true

      return false

    #this will be implemented later
    ValidateType: (type) ->



    CheckPrimaryKeyField: (type) ->
      count = 0
      properties = []

      exports.UTILS.ExtractProperties(type, properties)

      for prop in properties
        if(prop.GetCustomAttributes("PrimaryKeyAttribute"))
          return true;

      return false;

    GetTypeEdges: (typeId) ->
      @provider.GetNode(typeId, exports.NodeAccess.Read).edges.Array();

    GetDefaultPropertyValue: (typeId) ->
      if(not @IsScalarType(typeId))
        throw "Scalar type expected"

      t = @typeIdToTypeMapping.Get(typeId);

      val = null;

      if(exports.UTILS.equals(t, exports.BooleanType))
        val = new exports.IOGBoolean(false)
      else
        if(exports.UTILS.equals(t, exports.DateTimeType))
          val = DateTime.MIN_DATE
          val[TypesService.IOG_TYPE] = exports.DateTimeType
        else
          if(exports.UTILS.equals(t, exports.TimeSpanType))
            val = TimeSpan.ZERO
            val[TypesService.IOG_TYPE] = exports.TimeSpanType
          else
            if(exports.UTILS.equals(t, exports.Int32Type))
              val = new exports.Int32(0)
            else
              if(exports.UTILS.equals(t, exports.Int64Type))
                val = new exports.Int64(0)
              else
                if(exports.UTILS.equals(t, exports.DoubleType))
                  val = new exports.Double(0)
                else
                  if(exports.UTILS.equals(t, exports.ByteType))
                    val = new exports.Byte(0)
                  else
                    if(exports.UTILS.equals(t, exports.StringType))
                      val = new exports.IOGString("")
                    else
                      if(exports.UTILS.equals(t, exports.GuidType))
                        val = exports.Guid.EMPTY
                      else
                        if(exports.UTILS.equals(t, exports.CharType))
                          val = new exports.Char('a');
                        else
                          if(t.isEnum)
                            val = t.enumValues[0]
                          else
                            throw "Default undefined for type " + t.name

      return val


    #Type.GetType is changed with UTILS.GetType
    GetTypeFromId: (typeId) ->
      node = @provider.GetNode(typeId, exports.NodeAccess.Read)
      #all types need to be globaly avilable
      return exports.UTILS.GetType(node.data);

    GetTypeFromIdCached: (typeId) ->
      return @typeIdToTypeMapping.Get(typeId);

    IsSealedType: (typeId) ->

      if(typeId.name == "Type")
        typeId = this.GetTypeId(type)

      if(this.IsScalarType(typeId))
        return true

      for candidate in this.GetTypes()
        if(candidate != typeId)
          for edge in this.GetTypeEdges(candidate)
            if(edge.data.semantic == exports.EdgeType.OfType)
              if(edge.toNodeId == typeId)
                return false

      return true

    GetTypePrimaryKeyMemberId: (typeId) ->
      for edge in this.GetTypeEdges(typeId)
        if edge.data.semantic == exports.EdgeType.Property
          memberNode = @provider.GetNode(edge.toNodeId, exports.NodeAccess.Read)
          if memberNode.values.Contains(exports.Constants.TypeMemberPrimaryKeyId)
            return edge.toNodeId

      return exports.Guid.EMPTY

    GetMemberName: (typeId, memberId) ->
      @provider.GetNode(memberId, exports.NodeAccess.Read).data;

    GetRegisteredTypes: () ->
      typeIdToTypeMapping.keys