namespace execom.iog.name, (exports) ->
  class exports.RecursiveResolutionParameters
    constructor: (@subTree, @destinationProvider, @sourceProvider, @changeSet, @intermediateChanges, @visitedNodes) ->

# delegate void MergeRecursiveDelegate(Guid nodeId, RecursiveResolutionParameters parameters); this will not be implemented

# delegate void InsertRecursiveDelegate(Guid nodeId, RecursiveResolutionParameters parameters); this will not be implemented