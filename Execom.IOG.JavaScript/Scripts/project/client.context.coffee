namespace execom.iog.name, (exports) ->
  class exports.ClientContext
    VALUE = 'value'

    constructor: (@hostname, @serviceName, @localStorage = new exports.DirectNodeProvider()) ->
      @snapshotIsolationEnabled = null
      tempSnapshotIsolationEnabled = null
      @serverContext = new exports.ServerContext(@hostname, @serviceName)

      @snapshotIsolationEnabled = @serverContext.SnapshotIsolationEnabled()

      @defaultWorkspaceTimeout = null
      tempDefaultWorkspaceTimeout = null
      @defaultWorkspaceTimeout = @serverContext.DefaultWorkspaceTimeout()

      @immutableProxyMap = new exports.LimitedProxyMap(
        exports.IOGSettings.ObjectCacheMinimumCount,
        exports.IOGSettings.ObjectCacheMaximumCount)
      @provider = new exports.CachedReadNodeProvider(@serverContext, 
        @localStorage)
      @generationService = null
      this.InitializeServices()

    InitializeServices: () ->
      @typesService = new exports.TypesService(@provider)
      exports.types = []
      @serverContext.EntityTypes()

      interfaceToTypeIdMapping = @typesService.InitializeTypeSystem(window.types)
      completeTypesList = interfaceToTypeIdMapping.Keys()
      @generationService = new exports.GenerationService(@typesService)
      interfaceToGeneratedMapping = @generationService.GenerateProxyTypes(
        completeTypesList)
      @proxyCreatorService = new exports.ProxyCreatorService(completeTypesList,
        interfaceToTypeIdMapping, interfaceToGeneratedMapping)
      exports.StaticProxyFacade.Initialize(@typesService)
      return

    Backup: (storage, snapshotId = "") ->
      throw "Not implemented!"

    Cleanup: () ->
      throw "Not implemented!"

    ExpireWorkspaces: () ->
      throw "Not implemented!"

    LastSnapshotId: () ->
      return @serverContext.LastSnapshotId().value; 

    OpenWorkspace: (isolationLevel, snapshotId = null, timeout = null) ->
      defaultWorkspaceTimeout = null
      defaultSnapshotId = null

      #when we didn't set neither snapshotId or timeout
      if(not snapshotId? and not timeout?)
        defaultWorkspaceTimeout = @defaultWorkspaceTimeout
        defaultSnapshotId = @serverContext.LastSnapshotId()

      # when we set timout  but we didn't set snapshotId
      if(snapshotId? and not timeout?)
        if(!snapshotId.hasOwnProperty(exports.ClientContext.VALUE))
          snapshotId = new exports.Guid(snapshotId)

        defaultSnapshotId = snapshotId
        defaultWorkspaceTimeout = @defaultWorkspaceTimeout

      # when we set timeout  but we didn't set  snapshotId
      if(not snapshotId? and timeout?)
        defaultWorkspaceTimeout = timeout

        if (not @snapshotIsolationEnabled and isolationLevel == exports.IsolationLevel.Snapshot)
          throw "Snapshot isolation level disabled by configuration"

        if (isolationLevel == exports.IsolationLevel.Exclusive)
          @serverContext.EnterExclusiveLock()

        snapshotId = @serverContext.LastSnapshotId()
        return new exports.Workspace(snapshotId, defaultWorkspaceTimeout,
          @provider, @serverContext, @proxyCreatorService, @typesService,
          isolationLevel, @immutableProxyMap)#, rootType)

      if (not @snapshotIsolationEnabled and isolationLevel == exports.IsolationLevel.Snapshot)
        throw "Snapshot isolation level disabled by configuration"

      if(isolationLevel == exports.IsolationLevel.Exclusive)
        @serverContext.EnterExclusiveLock()
        lastSnapshotId = @serverContext.LastSnapshotId()
        if(not exports.UTILS.equals(defaultSnapshotId, lastSnapshotId) )
          throw "Snapshot other than the last snapshot cannot be opened for exclusive write."

      return new exports.Workspace(defaultSnapshotId, defaultWorkspaceTimeout, 
        @provider, @serverContext, @proxyCreatorService, @typesService, 
        isolationLevel, @immutableProxyMap)#, rootType)

    UpdateWorkspaceToExclusive: (workspace) ->
        @serverContext.EnterExclusiveLock()
        workspace.Update()
        workspace.ChangeIsolationLevel(exports.IsolationLevel.Exclusive)

    UpdateWorkspaceToSnapshot: (workspace) ->
        @serverContext.ExitExclusiveLock()
        workspace.ChangeIsolationLevel(exports.IsolationLevel.Snapshot)