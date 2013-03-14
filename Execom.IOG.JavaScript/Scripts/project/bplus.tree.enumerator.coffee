namespace execom.iog.name, (exports) ->
  class exports.BPlusTreeEnumerator
    constructor: (@nodes, @rootNodeId, @edgeType, @currentLeaf) ->
      @currentLeaf = exports.BPlusTreeOperations.LeftLeaf(nodes, 
        nodes.GetNode(rootNodeId, exports.NodeAccess.Read))
      @currentLeafEnumerator = @currentLeaf.edges.Array().GetEnumerator()

    # Current edge
    # <return></return> - Edge
    Current: () ->
      return @currentLeafEnumerator.Current()

    # Current item
    # <return></return> - object
    CurrentItem: () ->
      return @currentLeafEnumerator.Current()

    # Moves to next item in sequence
    # <returns>True if next item was found</returns> - boolean
    MoveNext: () ->
      res = false
      res = this.MoveNextInternal()

      while(res and this.Current().data.semantic != @edgeType)
        res =  this.MoveNextInternal()

      return res

    # Moves to next item without regards to edge type
    # <returns>True if next item was found</returns> - boolean
    MoveNextInternal: () ->
      if not @currentLeafEnumerator?
        return false

      current = this.Current()
      sampleData = null

      if current?
        sampleData = current.data

      res = @currentLeafEnumerator.MoveNext();

      if res
        return true
      else
        @currentLeafEnumerator = null;
        currentLeaf = exports.BPlusTreeOperations.NextLeaf(@nodes, 
          @rootNodeId, sampleData);

        if  currentLeaf == null
          return false
        else
          @currentLeafEnumerator = currentLeaf.edges.values.GetEnumerator()
          return @currentLeafEnumerator.MoveNext();

    # Resets the enumerator
    # <result></result> - void
    Reset: () ->
      if @currentLeafEnumerator?
        @currentLeafEnumerator.Reset()

      currentLeaf = exports.BPlusTreeOperations.LeftLeaf(@nodes, 
        @nodes.GetNode(@rootNodeId, exports.NodeAccess.Read))
      @currentLeafEnumerator = currentLeaf.edges.Array().GetEnumerator();

      return
