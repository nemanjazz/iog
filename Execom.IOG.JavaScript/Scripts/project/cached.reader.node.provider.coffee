namespace execom.iog.name, (exports) ->
  class exports.CachedReadNodeProvider
    constructor: (@parentProvider, @cacheProvider) ->

    SetNode: (identifier, node) ->
      @parentProvider.SetNode(identifier, node)
      @cacheProvider.SetNode(identifier, node)
      return

    GetNode: (identifier, access) ->
      node = @cacheProvider.GetNode(identifier, access)

      if(node?)
        return node
      else
        node = @parentProvider.GetNode(identifier, access);
        @cacheProvider.SetNode(identifier, node);
        return node

    Contains: (identifier) ->
      if(@cacheProvider.Contains(identifier))
        return true
      else
        return @parentProvider.Contains(identifier)

    Remove: (identifier) ->
      @cacheProvider.Remove(identifier);
      @parentProvider.Remove(identifier);
      return

    EnumerateNodes: () ->
      return @parentProvider.EnumerateNodes()

    Clear: () ->
      @parentProvider.Clear();
      @cacheProvider.Clear();
      return
