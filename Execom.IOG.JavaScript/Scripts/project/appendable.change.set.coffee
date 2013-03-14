namespace execom.iog.name, (exports) ->
  class exports.AppendableChangeSet
    constructor: (@sourceSnapshotId, @destinationSnapshotId, @nodes, @mapping, @nodeStates, @reusedNodes) ->
