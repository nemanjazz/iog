namespace execom.iog.name, (exports) ->
  class exports.DirectNodeProvider
    constructor: (@storage = new exports.MemoryStorage(), @forceUpdate = false) ->

    SetNode: (identifier, node) ->
      @storage.AddOrUpdate(identifier, node);

    GetNode: (nodeId, access) ->
      @storage.Value(nodeId);

    Contains: (identifier) ->
      @storage.Contains(identifier);

    Remove: (identifier) ->
      @storage.Remove(identifier);
      return

    EnumerateNodes: () ->
      return @storage.ListKeys();

    Clear: () ->
      @storage.Clear();
      return

    Length: () ->
      return @storage.Length()