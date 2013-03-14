class window.ImmutableAttribute
  constructor: () ->
    @name = "ImmutableAttribute"

class window.OverrideAttribute
  constructor: () ->
    @name = "OverrideAttribute"

class window.PrimaryKeyAttribute
  constructor: () ->
    @name = "PrimaryKeyAttribute"

class window.RevisionIdAttribute
  constructor: () ->
    @name = "RevisionIdAttribute"

window.ConcurrentBehavior =
  Static: 0
  Dynamic: 1

class window.ConcurrentAttribute
  # behavior - ConcurrentBehavior
  # resolver - Type
  constructor: ( resolver, @behavior = ConcurrentBehavior.Static) ->
    @name = "ConcurrentAttribute"
    if(@behavior == ConcurrentBehavior.Dynamic)
      if(resolver.interfaces.length != 1)
        throw "User defined resolver type missing or not derived from IUserDefinedMergeResolver"

      @resolver = resolver
