namespace execom.iog.name, (exports) ->
  class exports.Node
    constructor: (@nodeType, @data, @edges = new exports.SortedList(), @values = new exports.Dictionary()) ->
      @previous = exports.Guid.EMPTY
      @commited = false

    FindEdge: (edgeData) ->
      @edges.Get(edgeData)

    SetType: (nodeType) ->
      @nodeType = nodeType
      return

    SetData: (data) ->
      @data = data
      return

    AddEdge: (edge) ->
      @edges.Add(edge.data, edge)
      return

    SetEdgeToNode: (data, toNodeId) ->
      edge = @edges.Get(data)
      if(edge?)
        edge.toNodeId = toNodeId
        return
