namespace execom.iog.name, (exports) ->
  exports.events = $({})
  class exports.ServerContext

    #TYPE
    @TYPE = "Type"
    @IOG_TYPE = "iogType"

    @NODE = 'node'

    #CONTAINS service
    @IDENTIFIER = 'identifier'

    # NEED THIS FOR JSON
    @RESULT = "result"
    @R20 =  /%20/g

    #NODE ELEMENTS
    @COMMITED = "Commited"
    @NODE_TYPE = "NodeType"
    @VALUES = "Values"
    @EDGES = "Edges"
    @DATA = "Data"
    @PREVIOUS = "Previous"

    #KEY VALUE WRAPER
    @KEY = "Key"
    @VALUE = "Value"

    # EDGE DATA
    @SEMANTIC = "Semantic"
    @FLAGS = "Flags"

    # OBJECT_WRAPER
    @TYPE_NAME = "TypeName"

    # EDGE_WRAPPER
    @TO_NODE_ID = "ToNodeId"

    #GetNode Service parameters
    @NODE_ID = "nodeId"
    @ACCESS = "access"

    #WORKSPACE
    @WORKSPACE_ID = "workspaceId"
    @SNAPSHOT_ID = "snapshotId"
    @ISOLATION_LEVEL = "isolationLevel"
    @TIMEOUT = "timeout"
    @SNAPSHOT_ID = "snapshotId"

    #ChangesBetween
    @OLD_SNAPSHOT_ID = "oldSnapshotId"
    @NEW_SNAPSHOT_ID = "newSnapshotId"

    #UpdateWorkspace
    @WORKSPACE_ID = "workspaceId"
    @SNAPSHOT_ID = "snapshotId"

    #Commit
    @CHANGE_SET = "changeSet"
    @RESULT_SNAPSHOT_ID = "ResultSnapshotId"
    @MAPPING = "Mapping"

    @NAME_OF_TYPE = "NameOfType"
    @GENERIC_ARGUMENTS_TYPE_NAME = "GenericAgrumentsTypeName"

    constructor: (@hostname, @serviceName) ->
      if(@hostname?)
        @urlForService = "http://#{@hostname}/#{@serviceName}/"
      else
        @urlForService = "#{@serviceName}/"
      eventsHub = $.connection.eventsHub 
      
    @ProcessDataForSending: (data) ->

      return JSON.stringify(data);

    ServiceCall: (functionName, dataForSending, handler) ->
      $.ajax({
      type: "POST",
      contentType: "application/json; charset=utf-8",
      url: @urlForService + functionName,
      data: if dataForSending? then ServerContext.ProcessDataForSending(dataForSending) else "{}",
      async: false,
      crossDomain: true,
      dataType: "json",
      success: (msg) ->
        #alert(msg)
        handler(msg)
      error: (e) ->
        alert('Error, this is not working!' + e.getAllResponseHeaders())
      })
      
    CreateSubscription: (workspaceId, instanceId, propertyName,
      notifyChangesFromSameWorkspace, callback) ->
      msg = {}
      msg.workspaceId = workspaceId.value
      msg.instanceId = instanceId.value
      msg.propertyName = propertyName
      msg.notifyChangesFromSameWorkspace = notifyChangesFromSameWorkspace
      msg.callerId = $.connection.hub.id
      rez = null
      @ServiceCall("CreateSubscription", msg, (msg) ->
        #alert("LastSnapshotId!")
        temp = msg.d
        #TODO need to check if ther is result propertie
        resultJson = JSON.parse(temp)
        subscriptionObj = resultJson[ServerContext.RESULT]
        subscriptionId = new exports.Guid(subscriptionObj.SubscriptionId)
        workspaceId = new exports.Guid(subscriptionObj.WorkspaceId)
        subscription = new exports.Subscription(subscriptionId, workspaceId)
        
        exports.events.bind( subscriptionId.value, (args) ->
          console.log("Event happend")
          callback(args)
        );
        rez = subscription
      )
      return rez
    
    RemoveSubscription: (subscriptionId, workspaceId)->
      msg = {}
      msg.subscriptionId = subscriptionId
      msg.workspaceId = workspaceId
      msg.callerId = $.connection.hub.id
      @ServiceCall("RemoveSubscription", msg, (msg) ->
        #alert("LastSnapshotId!")
      )
    
    LastSnapshotId: () ->
      rez = null
      @ServiceCall("LastSnapshotId", null, (msg) ->
        #alert("LastSnapshotId!")
        temp = msg.d
        #TODO need to check if ther is result propertie
        resultJson = JSON.parse(temp)
        rez = new exports.Guid(resultJson[ServerContext.RESULT])
      )
      return rez

    EnterExclusiveLock: () ->
      @ServiceCall("EnterExclusiveLock", null, (msg) ->
        #alert("EnterExclusiveLock!")
      )

    EnterSharedLock: () ->
      @ServiceCall("EnterSharedLock", null, (msg) ->
        #alert("EnterSharedLock!")
      )

    SnapshotIsolationEnabled: () ->
      tempSnapshotIsolationEnabled = null
      @ServiceCall("SnapshotIsolationEnabled", null, (msg) ->
        #alert("Snapshot isolation enabled!");
        temp = msg.d
        #TODO need to check if ther is result propertie
        tempSnapshotIsolationEnabled = JSON.parse(temp)[ServerContext.RESULT]
      )
      return tempSnapshotIsolationEnabled

    DefaultWorkspaceTimeout: () ->
      tempDefaultWorkspaceTimeout = null
      @ServiceCall("DefaultWorkspaceTimeout", null, (msg) ->
        #alert("defaultWorkspaceTimeout!")
        temp = msg.d
        tempSpan = new TimeSpan()
        tempSpan._millis = JSON.parse(temp)[ServerContext.RESULT]
        tempSpan[ServerContext.IOG_TYPE] = exports.TimeSpanType
        #TODO need to check if ther is result propertie
        tempDefaultWorkspaceTimeout = tempSpan
      )
      return tempDefaultWorkspaceTimeout

    EntityTypes: () ->
      @ServiceCall("EntityTypes", null, (msg) ->
        #alert("EntityTypes!");
        temp = msg.d
        temp = JSON.parse(temp)
        for elem in temp
          newType = ServerContext.ExtractType(elem)
          exports.types.push(newType)
          genericType = newType
          genericType.ClearGenericArguments()
          exports[newType.name + ServerContext.TYPE] = genericType
          
          if(newType.isEnum)
            newEnum = new exports.IOGEnum(newType)
            exports.enums[newEnum.name] = newEnum
            if not exports.typeToEnums.Contains(newType)
              exports.typeToEnums.Add(newType, newEnum) 
          
          if(newType.isDictionaryType)
            exports.DictionaryType = genericType
          if(newType.isCollectionType)
            exports.ArrayType = genericType

          exports.IOGType.FindScalar(newType)

        ServerContext.ReinitializeConstants()
        return
      )
    ###
    When library is initialized it made some Guid that don't have correct
    data for field iogType. Correct data for that field can be found
    only after first call of service EntityTypes. This method is used
    to fix that constats that are not in correct state at the begining.
    ###  
    @ReinitializeConstants: () ->
      exports.EdgeDataSingleton.MAX_VALUE.data = 
        new exports.Guid("53F11357-62B7-430F-B446-9EC8F9702406")
      exports.EdgeDataSingleton.MIN_VALUE.data = 
        new exports.Guid("76367091-B69D-4BDF-A643-779032AF3503")

      exports.Constants.TypesNodeId = 
        new exports.Guid("22DD35BD-071B-4429-837D-4F5D2C201580")
      exports.Constants.SnapshotsNodeId = 
        new exports.Guid("52138911-0016-4C08-A685-9487617FD664")
      exports.Constants.ExclusiveWriterLockId =
        new exports.Guid("7EB5139E-72C2-4029-9EFD-1CD514775832")
      exports.Constants.NullReferenceNodeId = 
        new exports.Guid("FFCE2840-A5D7-4C1F-81F4-A8AC7FC61F92")
      exports.Constants.TypeMemberPrimaryKeyId = 
        new exports.Guid("67B21654-1E2D-4565-A4AE-33A7E1D43AF2")

      exports.BPlusTreeOperations.InitFields()

    OpenWorkspace: (workspaceId, snapshotId, isolationLevel, timeout) ->
      objectForSending = {}
      objectForSending[ServerContext.WORKSPACE_ID] = workspaceId.value
      if(snapshotId? and snapshotId.hasOwnProperty('value'))
        objectForSending[ServerContext.SNAPSHOT_ID] =  snapshotId.value
      else
        objectForSending[ServerContext.SNAPSHOT_ID] =  snapshotId
      objectForSending[ServerContext.ISOLATION_LEVEL] = isolationLevel
      objectForSending[ServerContext.TIMEOUT] = timeout._millis

      @ServiceCall("OpenWorkspace", objectForSending, (msg) ->
        #alert("OpenWorkspace!")

      )

    GetRootObjectId: (snapshotId) ->
      objectForSending = {}
      if(snapshotId? and snapshotId.hasOwnProperty('value'))
        objectForSending[ServerContext.SNAPSHOT_ID] = snapshotId.value
      else
        objectForSending[ServerContext.SNAPSHOT_ID] = snapshotId
      rez = null
      @ServiceCall("GetRootObjectId", objectForSending, (msg) ->
        #alert("GetRootObjectId!")
        temp = msg.d
        rez = new exports.Guid(JSON.parse(temp)[ServerContext.RESULT])
      )

      return rez

    EnumerateNodes: () ->
      rez = null
      @ServiceCall("EnumerateNodes", null, (msg) ->
        #alert("EnumerateNodes!")
        temp = msg.d
        newObj = JSON.parse(temp)[ServerContext.RESULT]
        rez = ServerContext.ExtractType(newObj)
      )

      return rez

    Clear: () ->
      @ServiceCall("Clear", null, (msg) ->
        alert("Clear")
      )


    GetRootType: () ->
      rez = null
      @ServiceCall("GetRootType", null, (msg) ->
        #alert("GetRootType!")
        temp = msg.d
        newObj = JSON.parse(temp)[ServerContext.RESULT]
        rez = ServerContext.ExtractType(newObj)
      )

      if(exports.RootType == null)
        exports.RootType = rez
      return rez

    Contains: (identifier) ->
      rez = null
      objectForSending = {}
      objectForSending[ServerContext.IDENTIFIER] = identifier.value
      @ServiceCall("Contains", objectForSending, (msg) ->
        #alert("Contains!")
        temp = msg.d
        rez = new exports.IOGBoolean(JSON.parse(temp)[ServerContext.RESULT])

      )
      return rez

    GetNode: (identifier, access) ->
      rez = null
      objectForSending = {}
      objectForSending[ServerContext.NODE_ID] = identifier.value
      objectForSending[ServerContext.ACCESS] = access
      @ServiceCall("GetNode", objectForSending, (msg) ->
        #alert("GetNode!")
        temp = msg.d
        rez = ServerContext.NodeParser(JSON.parse(temp)[ServerContext.RESULT])
      )
      return rez

    SetNode: (identifier, node) ->
      objectForSending = {}
      objectForSending[ServerContext.IDENTIFIER] = identifier.value
      objectForSending[ServerContext.NODE] = node
      @ServiceCall("SetNode", objectForSending, (msg) ->
        #alert("SetNode!")
      )

    ChangesBetween: (oldSnapshotId, newSnapshotId) ->
      rez = new exports.Dictionary()
      objectForSending = {}
      objectForSending[ServerContext.NEW_SNAPSHOT_ID] = newSnapshotId.value
      objectForSending[ServerContext.OLD_SNAPSHOT_ID] = oldSnapshotId.value

      @ServiceCall("ChangesBetween", objectForSending, (msg) ->
        temp = msg.d
        tempRez = JSON.parse(temp)[ServerContext.RESULT]
        for key, value of tempRez
          rez.Add(new exports.Guid(key), new exports.Guid(value))
      )

      return rez

    UpdateWorkspace: (workspaceId, snapshotId) ->
      objectForSending = {}
      objectForSending[ServerContext.WORKSPACE_ID] = workspaceId.value
      objectForSending[ServerContext.SNAPSHOT_ID] = snapshotId.value

      @ServiceCall("UpdateWorkspace", objectForSending, (msg) ->
        #alert("UpdateWorkspace!")
      )

    CloseWorkspace: (workspaceId) ->
      objectForSending = {}
      objectForSending[ServerContext.WORKSPACE_ID] = workspaceId.value

      @ServiceCall("CloseWorkspace", objectForSending, (msg) ->
        #alert("CloseWorkspace!")
      )

    Commit: (workspaceId, changeSet) ->

      objectForSending = {}
      objectForSending[ServerContext.WORKSPACE_ID] = workspaceId.value
      objectForSending[ServerContext.CHANGE_SET] = changeSet
      rez = null
      @ServiceCall("Commit", objectForSending, (msg) ->
        #alert("Commit!")
        temp = msg.d
        tempRez = JSON.parse(temp)[ServerContext.RESULT]
        if(tempRez.hasOwnProperty(ServerContext.MAPPING) and tempRez.hasOwnProperty(ServerContext.RESULT_SNAPSHOT_ID))
          resultSnapshotId = 
            new exports.Guid(tempRez[ServerContext.RESULT_SNAPSHOT_ID])
          mapping = new exports.Dictionary();
          for key, value of tempRez[ServerContext.MAPPING]
            if(typeof(value) != 'function')
              mapping.Add(new exports.Guid(key), new exports.Guid(value))

          commitResult = new exports.CommitResult(resultSnapshotId, mapping)
          rez = commitResult
      )

      return rez

    @GetValue: (object, property) ->
      if(object.hasOwnProperty(property))
        return object[property]

      return null

    @ExtractGenericArguments: (object) ->
      rez = []
      args = this.GetValue(object, exports.TypeConstants.GenericArguments)
      if(args?)
        if(args.length == 0)
          #rez.push()
          try
            type = exports.IOGType.FindType(args.Name)
          catch error
            return rez
          if(type?)
            rez.push(type)
        else
          for elem in args
            try
              type = exports.IOGType.FindType(elem)
            catch error
              continue
            if(type?)
              rez.push(type)
      return rez


    @ExtractType: (object) ->
      id = this.GetValue(object, exports.TypeConstants.ID)
      name = this.GetValue(object, exports.TypeConstants.Name)
      isCollectionType = this.GetValue(object, 
        exports.TypeConstants.IsCollectionType)
      isDictionaryType = this.GetValue(object, 
        exports.TypeConstants.IsDictionaryType)
      isInterface = this.GetValue(object, exports.TypeConstants.IsInterface)
      isEnum = this.GetValue(object, exports.TypeConstants.IsEnum)
      isGenericType = this.GetValue(object, exports.TypeConstants.IsGenericType)
      customAttributes = this.GetValue(object, 
        exports.TypeConstants.CustomAttributes)
      interfaces = this.GetValue(object, exports.TypeConstants.Interfaces)
      enumValues = this.GetValue(object, exports.TypeConstants.EnumValues)
      genericArguments = ServerContext.ExtractGenericArguments(object)
      isScalar = this.GetValue(object, exports.TypeConstants.IsScalar)
      genericType = this.GetValue(object, exports.TypeConstants.GenericType)
      objectProperties = this.GetValue(object, exports.TypeConstants.Properties)

      type = new exports.IOGType(id, name, isCollectionType, isDictionaryType, 
        isInterface, isEnum, isGenericType, genericType, customAttributes)
      if(objectProperties? and not objectProperties.length <= 0)
        properties = ServerContext.ExtractProperties(objectProperties, type)
      else
        properties = []
      #type = new IOGType(UUID.create(), "Boolean", false, false, false, false, false, null, [])
      #, properties, interfaces, enumValues, genericArguments, isScalar)
      type.properties = properties
      type.interfaces = interfaces
      type.enumValues = enumValues
      type.genericArguments = genericArguments
      type.isScalar = isScalar

      return type

    @ExtractProperties: (object, decleringTypeParam = null) ->
      result = []
      for eleme in object
        canRead = this.GetValue(eleme, exports.ProperiteConstants.CanRead)
        canWrite = this.GetValue(eleme, exports.ProperiteConstants.CanWrite)
        name = this.GetValue(eleme, exports.ProperiteConstants.Name)
        isStatic = this.GetValue(eleme, exports.ProperiteConstants.IsStatic)
        customAttributes = this.GetValue(eleme, 
          exports.ProperiteConstants.CustomAttributes)

        tempProperty = ServerContext.ExtractPropertyNameType(
          this.GetValue(eleme, exports.ProperiteConstants.PropertyType))

        propertyType = tempProperty

        tempDecleringType = ServerContext.ExtractPropertyNameType(
          this.GetValue(eleme, exports.ProperiteConstants.DeclaringType))

        decleringType = tempDecleringType
        #if(decleringTypeParam?) then decleringTypeParam else IOGType.FindType(this.GetValue(eleme, ProperiteConstants.DeclaringType).Name)
        pro = new exports.PropertyInfo(canRead, canWrite, decleringType, name,
          propertyType, isStatic, customAttributes)
        result.push(pro)

      return result

    @ExtractPropertyNameType: (object) ->

      if(object == null)
        return null
      if(not object.hasOwnProperty(ServerContext.NAME_OF_TYPE) or not object.hasOwnProperty(ServerContext.GENERIC_ARGUMENTS_TYPE_NAME))
        return null

      nameOfType = object[ServerContext.NAME_OF_TYPE]
      genericArguments = object[ServerContext.GENERIC_ARGUMENTS_TYPE_NAME]
      genericArgumentsProxies = []
      for argument in genericArguments
        genericArgumentProxy = ServerContext.ExtractPropertyNameType(argument)
        genericArgumentsProxies.push(genericArgumentProxy)
      propertyType = new exports.TypeProxy(nameOfType, genericArgumentsProxies)
      return propertyType

    @NodeParser: (object) ->
      valuesDictionary = new exports.Dictionary()
      for own key, value of object[ServerContext.VALUES]
        valueParsed = ServerContext.ObjectWrapperParse(value)
        newKey = new exports.Guid(key)
        if(exports.UTILS.equals(newKey, exports.Guid.EMPTY))
          continue
        valuesDictionary.Add(newKey, valueParsed)

      edgesSortedList = new exports.SortedList()
      for element in object[ServerContext.EDGES]
        key = element[ServerContext.KEY]
        value = element[ServerContext.VALUE]

        edgeDataKey = ServerContext.EdgeDataParse(key)
        edgeValeu = ServerContext.EdgeParse(value)


        edgesSortedList.Add(edgeDataKey, edgeValeu)
      dataObject = ServerContext.ObjectWrapperParse(object[ServerContext.DATA],
        object[ServerContext.NODE_TYPE])
      node = new exports.Node(object[ServerContext.NODE_TYPE], dataObject,
        edgesSortedList, valuesDictionary)
      node.previous = new exports.Guid(object[ServerContext.PREVIOUS])
      node.commited = object[ServerContext.COMMITED]
      return node;

    @EdgeParse: (object) ->
      if(not object? or not (object.hasOwnProperty(ServerContext.TO_NODE_ID) and object.hasOwnProperty(ServerContext.DATA)) )
        return null
      unparsedEdge = new exports.Guid(object[ServerContext.TO_NODE_ID])
      edge = new exports.Edge(unparsedEdge, 
        ServerContext.EdgeDataParse(object[ServerContext.DATA]))

      return edge

    @EdgeDataParse: (object) ->
      edgeData = new exports.EdgeData()
      edgeData.semantic = object[ServerContext.SEMANTIC]
      edgeData.flags = object[ServerContext.FLAGS]
      edgeData.data = ServerContext.ObjectWrapperParse(object[ServerContext.DATA])
      return edgeData

    @ObjectWrapperParse: (object, nodeType) ->
      if(not object? or not (object.hasOwnProperty(ServerContext.TYPE_NAME) and object.hasOwnProperty(ServerContext.DATA)) )
        return null
      if(not object[ServerContext.TYPE_NAME]? )
        return null

      if(not object[ServerContext.TYPE_NAME][ServerContext.NAME_OF_TYPE]?)
        return null

      if(nodeType == exports.NodeType.Type)
        #getting type proxy
        typeProxy = 
          ServerContext.ExtractPropertyNameType(object[ServerContext.TYPE_NAME])
        return typeProxy.GetType()

      switch object[ServerContext.TYPE_NAME][ServerContext.NAME_OF_TYPE]
        when exports.ScalarName.String
          tempRez = new exports.IOGString(object[ServerContext.DATA])
          return tempRez
        when exports.ScalarName.Guid
          tempRez = new exports.Guid(object[ServerContext.DATA])
          return tempRez
        when exports.ScalarName.Int32
          tempRez = new exports.Int32(object[ServerContext.DATA])
          return tempRez
        when exports.ScalarName.Int64
          tempRez = new exports.Int64(object[ServerContext.DATA])
          return tempRez
        when exports.ScalarName.Double
          tempRez = new exports.Double(object[ServerContext.DATA])
          return tempRez
        when exports.ScalarName.Boolean
          tempRez = new exports.IOGBoolean(object[ServerContext.DATA])
          return tempRez
        when exports.ScalarName.Byte
          tempRez = new exports.Byte(object[ServerContext.DATA])
          return tempRez
        when exports.ScalarName.Char
          tempRez = new exports.Char(object[ServerContext.DATA])
          return tempRez
        when exports.ScalarName.DateTime
          tempRez = new DateTime(object[ServerContext.DATA])
          tempRez[ServerContext.IOG_TYPE] = window.DateTimeType
          return  tempRez
        when exports.ScalarName.TimeSpan
          tempRez = new TimeSpan(object[ServerContext.DATA])
          tempRez[ServerContext.IOG_TYPE] = exports.TimeSpanType
          return  tempRez
        else
          enumeration = null
          for key, value of exports.enums 
            if(key == object[ServerContext.TYPE_NAME][ServerContext.NAME_OF_TYPE])
              enumeration = value
              break
          if(enumeration? and enumeration.type?)
            tempRez = enumeration.fromInt(object[ServerContext.DATA]) 
            return tempRez
          throw "Type is not supported!"
