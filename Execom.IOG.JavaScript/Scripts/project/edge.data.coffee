namespace execom.iog.name, (exports) ->
  class exports.EdgeData extends exports.IOGObject
    constructor: (@semantic, @data, @flags = exports.EdgeFlags.None) ->

    compareTo: (other) ->

      if not other?
        console.log(other)
      if other? and not other.semantic?
        console.log(other.semantic)

      if exports.UTILS.equals(this, other)
        return 0

      if this.semantic == exports.EdgeType.Special
        if this == exports.EdgeDataSingleton.MIN_VALUE
          return -1

        if other == exports.EdgeDataSingleton.MIN_VALUE
          return 1

        if this == exports.EdgeDataSingleton.MAX_VALUE
          return 1

        if other == exports.EdgeDataSingleton.MAX_VALUE
          return -1

      if this.semantic == other.semantic
        if(this.data.hasOwnProperty('data') and this.data.hasOwnProperty('data'))
          return this.data.data.compareTo(other.data.data)
        else
          return this.data.compareTo(other.data)
      else
        if this.semantic < other.semantic
          return -1
        else
          if this.semantic > other.semantic
            return 1
          else
            return 0
            
    

namespace execom.iog.name, (exports) ->
  exports.EdgeDataSingleton =
    MAX_VALUE: new exports.EdgeData(exports.EdgeType.Special,
      new exports.Guid("53F11357-62B7-430F-B446-9EC8F9702406"), 
        exports.EdgeFlags.None)
    MIN_VALUE: new exports.EdgeData(exports.EdgeType.Special,
      new exports.Guid("76367091-B69D-4BDF-A643-779032AF3503"), 
        exports.EdgeFlags.None)
