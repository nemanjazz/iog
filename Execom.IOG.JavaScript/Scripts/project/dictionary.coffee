namespace execom.iog.name, (exports) ->
  class exports.Dictionary
    constructor: (@array = []) ->

    Add: (key, value) ->
      for kv, i in @array
        if(exports.UTILS.equals(kv.key, key))
          throw 'Key already exists!'
      item = new exports.KeyValuePair(key, value)
      @array.push(item)
      return

    Set: (key, value) ->
      for kv, i in @array
        if(exports.UTILS.equals(kv.key, key))
          @array[i].value = value
      return

    Remove: (key) ->
      for kv, i in @array
        if(exports.UTILS.equals(kv.key, key))
          if i == 0
            @array = @array.slice(1, @array.length)
          else
            if i == @array.length - 1
              @array = @array.slice(0, @array.length - 1)
            else
              tempArray = @array.slice(0, i)
              @array = @array.slice(i + 1, @array.length)
              @array = tempArray.concat(@array)
          return true

      return false

    Contains: (key) ->
      index = -1
      for kv, i in @array
        if exports.UTILS.equals(kv.key, key)
          return true

      return false

    IndexOf: (key) ->
      index = -1
      for kv, i in @array
        if exports.UTILS.equals(kv.key, key)
          index = i
          return index

      return index

    Get: (key) ->
      for kv, i in @array
        if(exports.UTILS.equals(kv.key, key))
          return @array[i].value

      return null

    Array: () ->
      rez = []
      for kv in @array
        rez.push(kv.value)

      return rez

    Length: () ->
      @array.length

    Clear: () ->
      @array = []
      return

    Keys: () ->
      rez = []
      for kv in @array
        rez.push(kv.key)

      return rez