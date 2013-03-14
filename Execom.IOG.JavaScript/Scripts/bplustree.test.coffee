
module "B+ Tree"

test "Test fill", () ->
  nodes = new DirectNodeProvider(new MemoryStorage(), false)

  rootId = UUID.create()
  TreeOrder = 20


  node  = BPlusTreeOperations.CreateRootNode(NodeType.Collection, rootId)

  ok node?, "Root node created"

  ok node.data == BPlusTreeOperations.LeafNodeData, "Created node is Leaf"

  nodes.SetNode(rootId, node)

  references = []

  for i in [0..100]
    references.push(UUID.create())

  for reference, i in references
    #console.log(i)
    BPlusTreeOperations.InsertEdge(nodes, rootId, new Edge(reference, new EdgeData(EdgeType.ListItem, reference)), TreeOrder)

test "Test Consistency", () ->
  nodes = new DirectNodeProvider(new MemoryStorage(), false)

  rootId = UUID.create()
  node = BPlusTreeOperations.CreateRootNode(NodeType.Collection, rootId)
  nodes.SetNode(rootId, node)
  TreeOrder = 20
  references = []
  referencesAdded = []

  for i in [0..100]
    references.push(UUID.create())

  for reference, j in references
    data = new EdgeData(EdgeType.ListItem, reference)
    BPlusTreeOperations.InsertEdge(nodes, rootId, new Edge(reference, data), TreeOrder)
    referencesAdded.push(reference)

    for addedReference, i in referencesAdded
      console.log("First for " + i + " time " + j)
      dataAdded = new EdgeData(EdgeType.ListItem, addedReference)
      rez = BPlusTreeOperations.TryFindEdge(nodes, rootId, dataAdded)
      ok rez.result == true, "Edge is found"
      ok addedReference == rez.value.data.data, "Data found is expected"

  for reference, i in references
    console.log("Second for " + i)
    data = new EdgeData(EdgeType.ListItem, reference)
    rez = BPlusTreeOperations.TryFindEdge(nodes, rootId, data)
    ok rez.result == true, "Edge is found"
    ok reference == rez.value.data.data, "Data found is expected"
    ok reference == rez.value.toNodeId, "GUID found is expected"

test "Test Fill Remove", () ->
  nodes = new DirectNodeProvider(new MemoryStorage(), false)
  TreeOrder = 20
  rootId = UUID.create()
  node = BPlusTreeOperations.CreateRootNode(NodeType.Collection, rootId)
  nodes.SetNode(rootId, node)

  references = []

  for i in [0..100]
    references.push(UUID.create())

  count = 0

  for reference in references
    ok count == BPlusTreeOperations.Count(nodes, rootId, EdgeType.ListItem), "Count method is working for refrence " + reference
    BPlusTreeOperations.InsertEdge(nodes, rootId, new Edge(reference, new EdgeData(EdgeType.ListItem, reference)), TreeOrder)
    count = count + 1

  for reference in references
    numberOfElements = BPlusTreeOperations.Count(nodes, rootId, EdgeType.ListItem)
    ok count == numberOfElements, "Count method after removing is working for refrence " + reference
    BPlusTreeOperations.RemoveEdge(nodes, rootId, new EdgeData(EdgeType.ListItem, reference), TreeOrder)
    count = count - 1

test "Test Fill Remove Backwards", () ->

  nodes = new DirectNodeProvider(new MemoryStorage(), false)
  TreeOrder = 20
  rootId = UUID.create()
  node = BPlusTreeOperations.CreateRootNode(NodeType.Collection, rootId)
  nodes.SetNode(rootId, node)

  references = []
  for i in [0..100]
    references.push(UUID.create())

  count = 0

  for reference in references
    ok count == BPlusTreeOperations.Count(nodes, rootId, EdgeType.ListItem), "Count method is working for refrence " + reference
    BPlusTreeOperations.InsertEdge(nodes, rootId, new Edge(reference, new EdgeData(EdgeType.ListItem, reference)), TreeOrder)
    count = count + 1

  for reference in references.reverse()
    numberOfElements = BPlusTreeOperations.Count(nodes, rootId, EdgeType.ListItem)
    ok count == numberOfElements, "Count method after removing is working for refrence " + reference
    BPlusTreeOperations.RemoveEdge(nodes, rootId, new EdgeData(EdgeType.ListItem, reference), TreeOrder)
    count = count - 1
