module "Node"

test "SetType test", () ->
	node = new Node(NodeState.None, "1")

	node.SetType(NodeState.Created)

	ok node.nodeType == NodeState.Created, "Method SetType is working"

test "SetData test", () ->
	node = new Node(NodeState.None, "1")

	node.SetData("b")

	ok node.data == "b", "Method SetData is working"

test "AddEdge test", () ->
	node = new Node(NodeState.None, "1")
	w5  = new Edge("5", "w")
	node.AddEdge(w5)

	ok node.edges.Length() == 1, "Method AddEdge is working"
	
	y6 = new Edge("6", "y")
	node.AddEdge(y6)

	ok node.edges.Length() == 2, "Method AddEdge is working"

	ed = node.FindEdge("y")

	ok ed == y6, "Method FindEdge is working"

	ed = node.FindEdge("l")

	ok not ed?, "Method FindEdge is returning null when there is no edge"

	node.SetEdgeToNode("w", "10")
	w5.toNodeId = 10

	w10 = node.FindEdge("w")

	ok w10 == w5, "Method SetEdgeToNode is working for existing value"

	node.SetEdgeToNode("4", "p")

	ok not node.FindEdge("p")?, "Method cannot set edge that doesn't exist"