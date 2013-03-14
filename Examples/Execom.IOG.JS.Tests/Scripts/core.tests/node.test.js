// Generated by CoffeeScript 1.3.3
(function() {

  module("Node");

  test("SetType test", function () {
    var iog = window.execom.iog;
    var node;
    node = new iog.Node(iog.NodeState.None, "1");
    node.SetType(iog.NodeState.Created);
    return ok(node.nodeType === iog.NodeState.Created, "Method SetType is working");
  });

  test("SetData test", function() {
    var iog = window.execom.iog;
    var node;
    node = new iog.Node(iog.NodeState.None, "1");
    node.SetData("b");
    return ok(node.data === "b", "Method SetData is working");
  });

  test("AddEdge test", function() {
    var iog = window.execom.iog;
    var ed, node, w10, w5, y6;
    node = new iog.Node(iog.NodeState.None, "1");
    w5 = new iog.Edge("5", "w");
    node.AddEdge(w5);
    ok(node.edges.Length() === 1, "Method AddEdge is working");
    y6 = new iog.Edge("6", "y");
    node.AddEdge(y6);
    ok(node.edges.Length() === 2, "Method AddEdge is working");
    ed = node.FindEdge("y");
    ok(ed === y6, "Method FindEdge is working");
    ed = node.FindEdge("l");
    ok(!(ed != null), "Method FindEdge is returning null when there is no edge");
    node.SetEdgeToNode("w", "10");
    w5.toNodeId = 10;
    w10 = node.FindEdge("w");
    ok(w10 === w5, "Method SetEdgeToNode is working for existing value");
    node.SetEdgeToNode("4", "p");
    return ok(!(node.FindEdge("p") != null), "Method cannot set edge that doesn't exist");
  });

}).call(this);
