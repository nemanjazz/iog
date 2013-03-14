namespace execom.iog.name, (exports) ->
  class exports.TrackingWorkspaceStateProvider
    constructor: (@workspaceStates = new exports.Dictionary()) ->

    AddWorkspace: (workspaceId, snapshotId, isolationLevel, timeout) ->
      @workspaceStates.Add(workspaceId, 
        new exports.WorkspaceStateElement(snapshotId, isolationLevel, 
          DateTime.utcNow, timeout))

    IsWorkspaceExpired: (workspaceId) ->
      return not @workspaceStates.Contains(workspaceId) or @workspaceStates[workspaceId].IsExpired()

    GetWorkspaceSnapshotId: (workspaceId) ->
      return @workspaceStates[workspaceId].snapshotId

    RemoveWorkspace: (workspaceId) ->
      level = exports.IsolationLevel.ReadOnly

      level = @workspaceStates.Get(workspaceId).isolationLevel
      @workspaceStates.Remove(workspaceId)

    Cleanup: () ->

      expiredWorkspaces = []
      for key in @workspaceStates.Keys()
        item = @workspaceStates.Get(key)
        if(item.IsExpired())
          expiredWorkspaces.push(key)

      for key in expiredWorkspaces
        this.RemoveWorkspace(key)

    UsedSnapshotIds: () ->
      res = []

      for key in @workspaceStates.Keys()
        item = @workspaceStates.Get(key)
        if(item.IsExpired())
          res.push(item.snapshotId)

      return res


    UpdateWorspace: (workspaceId, snapshotId) ->
      @workspaceStates[workspaceId].lastAccessDateTime = DateTime.utcNow
      @workspaceStates[workspaceId].snapshotId = snapshotId


    # TODO WorkspaceExclusiveLockProvider for now I did not implemented somthing like that
    # on internet there is many threads about is js single threaded or not, it is difficult to find out
    # what is true

  class WorkspaceStateElement
    #  snapshotId - - UUID
    #  isolationLevel - - IsolationLevel
    #  openedDateTime - - DateTime
    #  timeout - - TimeSpan
    constructor: (@snapshotId, @isolationLevel, @lastAccessDateTime, @timeout) ->

    IsExpired: () ->
      return (@lastAccessDateTime.Add(Timeout))._millis < DateTime.utcNow.span._millis