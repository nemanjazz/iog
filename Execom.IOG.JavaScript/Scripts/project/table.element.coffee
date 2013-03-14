namespace execom.iog.name, (exports) ->
  class exports.TableElement
    constructor: (@target, lastAccess)  ->
      @accessCount = lastAccess

    compareTo: (other) ->
      if(not other.hasOwnProperty("accessCount"))
        throw "Other object has not field accessCount!"

      if this.accessCount < other.accessCount
        return -1
      else
        if this.accessCount > other.accessCount
          return 1
        else
          return 0