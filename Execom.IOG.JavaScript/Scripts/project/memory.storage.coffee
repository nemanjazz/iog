namespace execom.iog.name, (exports) ->
  class exports.MemoryStorage
    constructor: (@dictionary = new exports.Dictionary()) ->

    Remove: (key) ->
      return @dictionary.Remove(key)

    Contains: (key) ->
      return @dictionary.Contains(key);

    Value: (key) ->
      @dictionary.Get(key)

    AddOrUpdate: (key, value) ->
      if(@dictionary.Contains(key))
        @dictionary.Set(key, value)
      else
        @dictionary.Add(key, value)

      return true

    ListKeys: () ->
      @dictionary.Keys()

    Values: () ->
      return @dictionary.array.map( (element) ->
        element.value
      )

    Clear: () ->
      @dictionary.Clear()
      return

    Length: () ->
      return @dictionary.Length()