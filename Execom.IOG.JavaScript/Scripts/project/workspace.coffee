namespace execom.iog.name, (exports) ->
  class exports.Workspace

    # snapshotId - - UUID
    # timeout - - Timespan
    # nodeProvider - - DirectNodeProvider
    # commitTarget - -  WorkspaceFacade
    # proxyCreatorService - -  ProxyCreatorService
    # typesService - - TypesService
    # isolationLevel - - IsolationLevel
    # immutableProxyMap - - LimitedProxyMap
    constructor: (@snapshotId, @timeout, @nodeProvider, @workspaceFacade,  @proxyCreatorService, @typesService, @isolationLevel, @immutableProxyMap) ->#, @type) ->
      @workspaceId = exports.Guid.Create()
      @mutableProxyMap = new exports.LimitedProxyMap()
      # in js there is no need for type, or this need to be fixed
      #if(not type.isInterface)
      #  throw "Interface type expected!"

      @workspaceFacade.OpenWorkspace(@workspaceId, @snapshotId, 
        @isolationLevel, @timeout)

      if(@isolationLevel == exports.IsolationLevel.ReadOnly)
        @objectInstancesService = new exports.ObjectInstancesService(
          @nodeProvider, @typesService)
        @immutableInstancesService = new exports.ObjectInstancesService(
          @nodeProvider, @typesService)
        @collectionInstancesService = new exports.CollectionInstancesService(
          @nodeProvider, @typesService)
        @dictionaryInstancesService = new exports.DictionaryInstancesService(
          @nodeProvider, @typesService)
      else
        isolatedStorage = new exports.DirectNodeProvider(
          new exports.MemoryStorage())
        @isolatedProvider =  new exports.IsolatedNodeProvider(
          @nodeProvider, isolatedStorage)
        @objectInstancesService = new exports.ObjectInstancesService(
          @isolatedProvider, @typesService)
        @immutableInstancesService = new exports.ObjectInstancesService(
          @isolatedProvider, @typesService)
        @collectionInstancesService = new exports.CollectionInstancesService(
          @isolatedProvider, @typesService)
        @dictionaryInstancesService = new exports.DictionaryInstancesService(
          @isolatedProvider, @typesService)

      @runtimeProxyFacade = new exports.RuntimeProxyFacade(@typesService, 
        @objectInstancesService, @immutableInstancesService, 
        @collectionInstancesService, 
        new exports.CollectionInstancesService(@nodeProvider, @typesService), 
        @dictionaryInstancesService, 
        new exports.DictionaryInstancesService(@nodeProvider, @typesService), 
        @mutableProxyMap, immutableProxyMap, @proxyCreatorService)
      rootObjectId = @workspaceFacade.GetRootObjectId(snapshotId)
      rootType = @workspaceFacade.GetRootType()
      @rootProxy = proxyCreatorService.NewObject(@runtimeProxyFacade, 
        rootObjectId, isolationLevel == exports.IsolationLevel.ReadOnly,
        rootType, null);

      if (@isolationLevel == exports.IsolationLevel.ReadOnly)
        @immutableProxyMap.AddProxy(rootObjectId, @rootProxy)
      else
        @mutableProxyMap.AddProxy(rootObjectId, @rootProxy)

    Data: () ->
      return @rootProxy

    SnapshotId: () ->
      return @snapshotId

    New: (type, genericTypes = []) ->
      typeId = @typesService.GetTypeIdCached(type)

      if(exports.UTILS.equals(typeId, exports.Guid.EMPTY))
        throw "Type not registered:#{type.name}"

      instanceId = exports.Guid.EMPTY

      if(@typesService.IsDictionaryType(typeId))
        instanceId = @dictionaryInstancesService.NewInstance(typeId)
      else
        if(@typesService.IsCollectionType(typeId))
          instanceId = @collectionInstancesService.NewInstance(typeId)
        else
          instanceId = @objectInstancesService.NewInstance(typeId)

      proxy = @proxyCreatorService.NewObject(@runtimeProxyFacade, 
        instanceId, false, type)
      @mutableProxyMap.AddProxy(instanceId, proxy)
      return proxy

    Spawn: (revisionId) ->
      throw "Revision is not implemented!"

    SpawnImmutable: (revisionId, type) ->
      throw "Revision is not implemented!"

    ImmutableView: (instance, type) ->
      id = exports.UTILS.GetItemId(instance)

      if (@isolatedProvider.GetNodeState(id) == exports.NodeState.Created)
        throw "Operation not allowed for uncommited instance"

      proxy = null

      result = @immutableProxyMap.TryGetProxy(id)

      if (not result["result"])
        proxy = proxyCreatorService.NewObject(runtimeProxyFacade, id, true, type)
        immutableProxyMap.AddProxy(id, proxy);

      return proxy

    InstanceRevisionId: (instance) ->
      return exports.UTILS.GetItemId(instance)

    SetImmutable: (instance, propertyName) ->
      instanceId = exports.UTILS.GetItemId(instance)
      @objectInstancesService.SetImmutable(instanceId, 
        @typesService.GetTypeMemberId(@typesService.
        GetInstanceTypeId(instanceId), propertyName))
      return

    Commit: () ->
      if (@isolationLevel == exports.IsolationLevel.ReadOnly)
        throw "Invalid commit operation in Read only isolation"

      isolatedChanges = @isolatedProvider.GetChanges(@snapshotId)

      changeSet = @workspaceFacade.Commit(@workspaceId, isolatedChanges)

      @mutableProxyMap.UpgradeProxies(changeSet.mapping)

      @isolatedProvider.Clear()

      @snapshotId = changeSet.resultSnapshotId

      return @snapshotId

    Rollback: () ->
      if(@isolationLevel == exports.IsolationLevel.ReadOnly)
        throw"Invalid rollback operation in Read only isolation"

      newInstances = new exports.Dictionary()

      for item in @isolatedProvider.EnumerateChanges()
        if (@isolatedProvider.GetNodeState(item)== exports.NodeState.Created)
          newInstances.Add(exports.UTILS.GetItemId(item), item)

      @mutableProxyMap.InvalidateProxies(newInstances)

      @isolatedProvider.Clear()

    Update: (newSnapshotId) ->
      if(newSnapshotId?)
        mapping = @workspaceFacade.ChangesBetween(@snapshotId, newSnapshotId)

        if (@isolatedProvider != null)
          for changedNodeId in @isolatedProvider.EnumerateChanges()
            if (mapping.Contains(changedNodeId))

              typeId = exports.Guid.Empty;

              try
                typeId = @typesService.GetInstanceTypeId(changedNodeId)
              catch error
                throw "ConcurrentModificationException()"

              throw "ConcurrentModificationException()"

        @mutableProxyMap.UpgradeProxies(mapping)

        @snapshotId = newSnapshotId

        @workspaceFacade.UpdateWorkspace(@workspaceId, @snapshotId)
        return

      else
        newSnapshotId = @workspaceFacade.LastSnapshotId()
        this.Update(newSnapshotId)

    TryUpdate: (newSnapshotId = null) ->
      if(newSnapshotId?)
        mapping = @workspaceFacade.ChangesBetween(snapshotId, newSnapshotId)

        if (@isolatedProvider != null)
          for changedNodeId in isolatedProvider.EnumerateChanges()
            if(mapping.Contains(changedNodeId))
              return false

        @mutableProxyMap.UpgradeProxies(mapping);

        @snapshotId = newSnapshotId;

        @workspaceFacade.UpdateWorkspace(@workspaceId, @snapshotId);

        return true

      else
        newSnapshotId = @workspaceFacade.LastSnapshotId();

        return @TryUpdate(newSnapshotId)


    CreateSubscription: (instance, del, propertyName = null, 
        notifyChangesFromSameWorkspace = false) ->
      return @workspaceFacade.CreateSubscription(@workspaceId, 
        exports.UTILS.GetItemId(instance), propertyName, notifyChangesFromSameWorkspace, del)

    RemoveSubscription: (subscription) ->
      @workspaceFacade.RemoveSubscription(subscription.subscriptionId, 
        subscription.workspaceId)

    SubWorkspace: (del) ->
      throw "Not implemented"
    
    ClearWorkspace: () ->
      @Rollback()
      @Update()
    
    CloseWorkspace: () ->
      @workspaceFacade.CloseWorkspace(@workspaceId)
      @mutableProxyMap.InvalidateProxies()
      # Make root proxy unusable
      exports.UTILS.SetItemId(@rootProxy, exports.Guid.EMPTY);
      #this is making rootProxy null
      @rootProxy = null

    ChangeIsolationLevel: (newIsolationLevel) ->
      if (isolationLevel == exports.IsolationLevel.ReadOnly)
          throw new ArgumentException(
            "Cannot change isolation level for read only workspace.")
      @isolationLevel = newIsolationLevel
      @workspaceFacade.ChangeWorkspaceIsolationLevel(workspaceId, newIsolationLevel) 



