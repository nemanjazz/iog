class window.WorkspaceFacade
  constructor: (@commitDataService, @workspaceStateProvider, @subscriptionManagerService = null, @snapshotsService) ->


  Commit: (workspaceId, changeSet, type) ->
    if(not @workspaceStateProvider.IsWorkspaceExpired(workspaceId))
      result = @commitDataService.AcceptCommit(changeSet)
      @workspaceStateProvider.UpdateWorspace(workspaceId, result.ResultSnapshotId)
      #subscriptionManagerService.InvokeEvents(workspaceId, result) subscription is not implemented
      return result
    else
      throw "Workspace timeout has elapsed"

  OpenWorkspace: (workspaceId, snapshotId, isolationLevel, timeout) ->
    @workspaceStateProvider.AddWorkspace(workspaceId, snapshotId, isolationLevel, timeout)
    return

  UpdateWorkspace: (workspaceId, snapshotId) ->
    @workspaceStateProvider.UpdateWorspace(workspaceId, snapshotId)
    return

  CloseWorkspace: (workspaceId) ->
    @workspaceStateProvider.RemoveWorkspace(workspaceId)
    return

  ChangesBetween: (oldSnapshotId, newSnapshotId) ->
    return @commitDataService.ChangesBetween(oldSnapshotId, newSnapshotId)

  CreateSubscription: (workspaceId, instanceId, notifyChangesFromSameWorkspace, del, propertyName) ->
    # TODO NOT IMPLEMENTED
    throw "Not implemented!"

  RemoveSubscription: (subscription) ->
    # TODO NOT IMPLEMENTED
    throw "Not implemented!"

  GetRootObjectId: (snapshotId) ->
    return @snapshotsService.GetRootObjectId(snapshotId)

  LastSnapshotId: () ->
    return @snapshotsService.GetLatestSnapshotId()