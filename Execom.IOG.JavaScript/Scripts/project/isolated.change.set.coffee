namespace execom.iog.name, (exports) ->
  class exports.IsolatedChangeSet
    constructor: (@sourceSnapshotId, @nodes = new exports.DirectNodeProvider(), @nodeStates = new exports.Dictionary()) ->
