namespace execom.iog.name, (exports) ->
  class exports.SortedList
    constructor: (@array = []) ->

    Add: (key, value) ->
      index = -1
      for kv, i in @array
        if(exports.UTILS.equals(kv.key, key))
          throw 'Key already exists!'
        else
          if(kv.key.compareTo(key) >= 0)
            index = i
            break;

      item = new exports.KeyValuePair(key, value)
      if(index != -1)
        @array.splice( index,0, item)
      else
        @array.push(item)
      return

    Set: (key, value) ->
      for kv, i in @array
        if(exports.UTILS.equals(kv.key, key))
          @array[index].value = value

      return

    Remove: (key) ->
      for kv, i in @array
        if(exports.UTILS.equals(kv.key, key))
          if i == 0
            @array = @array.slice(1, @array.length)
            return true
          else
            if i == @array.length - 1
              @array = @array.slice(0, @array.length - 1)
              return true
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

      #TODO maybe throw exception
      return null

    Array: () ->
      rez = []
      for kv, i in @array
        rez.push(kv.value)

      return rez

    Length: () ->
      @array.length

    Clear: () ->
      #should check if this way I have memory leak
      @array = []
      return

    Keys: () ->
      rez = []
      for kv, i in @array
        rez.push(kv.key)

      return rez
