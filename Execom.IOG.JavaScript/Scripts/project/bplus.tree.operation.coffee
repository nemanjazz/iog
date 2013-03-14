namespace execom.iog.name, (exports) ->
  class exports.SplitResult

  # Creates new instance of SplitResult type
  # createdNodeId - New node ID which was created - Guid
  # createdNode - Node which was created - Node
  # rightKey - Minimum key value of the right sibling node of the new node - EdgeData
    constructor: (@createdNodeId, @createdNode, @rightKey) ->

  class exports.FindResult
  # nodeId - Guid
  # node - Node
    constructor: (@nodeId, @node) ->

  class exports.RemovalResult

  # remainingCount - int
  # wasRemoved - boolean
  # wasMerged - boolean
    constructor: (@remainingCount, @wasRemoved, @wasMerged) ->


  class BPlusTreeOperationsClass
    constructor: () ->
      this.InitFields()

    InitFields: () ->
      @InternalNodeData = new exports.Guid(
        "46F41F60-F781-445A-A416-B35E0CA940B4")
      @LeafNodeData = new exports.Guid("6DEFCDC7-4C59-4120-87C6-C72363751BE7")

    # Creates new root node
    # rootNodeId - Node identifier - Guid
    # <returns>New node</returns> - Node
    CreateRootNode: (nodeType, rootNodeId) ->
      node = new exports.Node(nodeType, this.LeafNodeData);
      return node;

    # Adds new edge to the tree.
    # nodes - Node provider which hosts the nodes - INodeProvider
    # rootNodeId - Tree root node ID - Guid
    # edge - Edge to add - Edge
    # treeOrder - - int
    InsertEdge: (nodes, rootNodeId, edge, treeOrder) ->
      if treeOrder < 6
        throw "treeOrder is smaller then 6"

      result = this.FindLeafNode(nodes, rootNodeId, edge.data)
      if result.node.edges.Length() == treeOrder
        this.SplitTree(nodes, rootNodeId, true, edge.data, treeOrder)
        result = this.FindLeafNode(nodes, rootNodeId, edge.data)

      if not exports.UTILS.equals(result.node.data, this.LeafNodeData)
        throw "Leaf node expected"

      node = nodes.GetNode(result.nodeId, exports.NodeAccess.ReadWrite)
      node.AddEdge(edge)
      nodes.SetNode(result.nodeId, node);

    # Finds the edge in tree
    # nodes - Node provider which hosts the nodes - INodeProvider
    # rootNodeId - Tree root node ID - Guid
    # data - Edge to find
    # <return>object with boolean result and Edge value</return>
    TryFindEdge: (nodes, rootNodeId, data ) ->
      result = this.FindLeafNode(nodes, rootNodeId, data);
      if result == null or result.node == null
        return {
        "result": false
        "value": null
        }
      else
        if not exports.UTILS.equals(result.node.data, this.LeafNodeData)
          return {
          "result": false
          "value": null
          }

        value = result.node.edges.Get(data);
        return {
        "result": value?
        "value": value
        }


    # Finds the edge in tree
    # nodes - Node provider which hosts the nodes - INodeProvider
    # rootNodeId - Tree root node ID - Guid
    # data - Edge to find - EdgeData
    # toNodeId - New to node ID - Guid
    TrySetEdgeToNode: ( nodes, rootNodeId, data, toNodeId) ->
      result = this.FindLeafNode(nodes, rootNodeId, data);

      if (result == null)
        throw "Key not found"
      else

        if result.node.data != this.LeafNodeData
          throw "Leaf node expected"

        result.node.SetEdgeToNode(data, toNodeId);
        nodes.SetNode(result.nodeId, result.node);

        return true

    # Removes the edge from the tree
    # nodes - Node provider which hosts the nodes - INodeProvider
    # rootNodeId - Tree root node ID - Guid
    # data - Edge to find - EdgeData
    # treeOrder -  - int
    # <returns>True if edge was removed</returns> - boolean
    RemoveEdge: (nodes, rootNodeId, data, treeOrder) ->
      result = this.RemoveEdgeRecursive(nodes, rootNodeId, data, treeOrder);
      return result.wasRemoved;

    # Returns number of edges in the tree
    # nodes - Node provider which hosts the nodes - INodeProvider
    # rootNodeId - Tree root node ID - Guid
    # data - Edge to find - EdgeType
    # <return>int</return>
    Count: (nodes, rootNodeId, data) ->
      node = nodes.GetNode(rootNodeId, exports.NodeAccess.Read)

      if exports.UTILS.equals(node.data, this.LeafNodeData)
        sum = 0
        for edge in node.edges.Array()
          if edge.data.semantic == data
            sum = sum + 1

        return sum
      else
        sum = 0
        for edge in node.edges.Array()
          sum = sum + this.Count(nodes, edge.toNodeId, data)

        return sum


    # Creates new enumerator through tree edges
    # nodes - Node provider which contains tree nodes - INodeProvider
    # rootNodeId - Tree root node ID - Guid
    # edgeType - Edge type to use as a filter - EdgeType
    # <returns>New enumerator</returns>
    GetEnumerator: ( nodes, rootNodeId,edgeType) ->
      return new exports.BPlusTreeEnumerator(nodes, rootNodeId, edgeType)

    # Creates clone of existing tree under different ID
    # nodes - Node provider which stores the nodes - INodeProvider
    # sourceId - ID of source tree root - Guid
    # destinationId - ID of destination tree root - Guid
    Clone: (nodes, sourceId, destinationId) ->
      node = nodes.GetNode(sourceId, exports.NodeAccess.Read)

      if exports.UTILS.equals(node.data, this.InternalNodeData)
        newNode = new exports.Node(node.nodeType, node.data)
        for edge in node.edges.values
          subId = exports.Guid.Create()
          this.Clone(nodes, edge.toNodeId, subId);
          newNode.AddEdge(new exports.Edge(subId, edge.data))

        nodes.SetNode(destinationId, newNode)
      else
        if exports.UTILS.equals(node.data, this.LeafNodeData)
          newNode = new exports.Node(node.nodeType, node.data, node.edges)
          nodes.SetNode(destinationId, newNode)
        else
          throw "Invalid operation"

    # Looks for leaf tree node which is supposed to have an edge with given hash
    # nodes - Node provider - INodeProvider
    # rootNodeId - Root node ID to start the search from - Guid
    # edgeHash - Hash to find - EdgeData
    # <returns>Leaf node which is supposed to contain the edge</returns> - FindResult
    FindLeafNode: (nodes, rootNodeId, data) ->
      node = nodes.GetNode(rootNodeId, exports.NodeAccess.Read)
      id = rootNodeId
      if not node?
        return new FindResult(null, null)

      while exports.UTILS.equals(node.data, this.InternalNodeData)
        leadingEdge = this.FindLeadingEdge(data, node)
        node = nodes.GetNode(leadingEdge.toNodeId, exports.NodeAccess.Read)
        id = leadingEdge.toNodeId

      return new exports.FindResult(id, node)

    # Finds an edge which leads to given hash
    # data - Data to find - EdgeData
    # node - Internal node - Node
    # <returns>Edge which leads to hash</returns> - Edge
    FindLeadingEdge: (data, node) ->
      return node.edges.Get(node.edges.Keys()[this.FindLeadingEdgeIndex(data, node)]);

    # Finds an edge index which leads to given hash
    # data - Data to find - EdgeData
    # node - Internal node - Node
    # <returns>Edge which leads to hash</returns> - Edge
    FindLeadingEdgeIndex: (data, node) ->
      if exports.UTILS.equals(node.data, this.InternalNodeData)
        keys = node.edges.Keys()
        i = 0
        step = keys.length

        while(true)
          if step > 1
            step = Math.floor(step/2)#step / 2

          if data.compareTo(keys[i]) < 0
            if i == 0
              return i

            if data.compareTo(keys[i - 1]) >= 0
              return i
            else
              i = i - step
          else
            if i == keys.length - 1
              return i
            else
              i = i + step
      else
        throw "Internal node expected"

    # Makes room in the tree for new data
    # nodes - Node provider which contains tree nodes - INodeProvider
    # nodeId - Current node ID - Guid
    # isRoot - Determines if current node is root node - boolean
    # data - Data to make room for - EdgeData
    # <returns>Result of the operation</returns> - SplitResult
    SplitTree: (nodes, nodeId, isRoot, data, treeOrder) ->
      currentNode = nodes.GetNode(nodeId, exports.NodeAccess.ReadWrite)

      if isRoot

        if exports.UTILS.equals(currentNode.data, this.InternalNodeData)
          leadingEdge = this.FindLeadingEdge(data, currentNode)
          result = this.SplitTree(nodes, leadingEdge.toNodeId, false, data, treeOrder)

          if result != null
            currentNode.AddEdge(new exports.Edge(result.createdNodeId,
              result.rightKey))

            if currentNode.edges.Length() == (treeOrder+1)
              leftNodeId = exports.Guid.Create()
              rightNodeId = exports.Guid.Create()
              leftNode = new exports.Node(exports.NodeType.TreeInternal, 
                this.InternalNodeData)
              rightNode = new exports.Node(exports.NodeType.TreeInternal, 
                this.InternalNodeData)

              keys = currentNode.edges.Keys();
              for eleme, i in keys
                if (i < Math.floor(keys.length/2)) #keys.Length() / 2)
                  leftNode.AddEdge(currentNode.edges.Get(keys[i]))
                else
                  rightNode.AddEdge(currentNode.edges.Get(keys[i]))

              this.SetLastInternalKey(leftNode)
              this.SetLastInternalKey(rightNode)

              currentNode.edges.Clear()
              currentNode.AddEdge(new exports.Edge(leftNodeId, 
                this.LeftEdge(nodes, rightNode).data))
              currentNode.AddEdge(new exports.Edge(rightNodeId, 
                exports.EdgeDataSingleton.MAX_VALUE))

              nodes.SetNode(leftNodeId, leftNode)
              nodes.SetNode(rightNodeId, rightNode)

            nodes.SetNode(nodeId, currentNode)
          else
            return null
        else
          if currentNode.edges.Length() == treeOrder
            leftNodeId = exports.Guid.Create()
            rightNodeId = exports.Guid.Create()

            leftNode = new exports.Node(exports.NodeType.TreeLeaf, 
              this.LeafNodeData)
            rightNode = new exports.Node(exports.NodeType.TreeLeaf, 
              this.LeafNodeData)

            keys = currentNode.edges.Keys()

            for key, i in keys
              if i < Math.floor(keys.length/2)#keys.Length()/2
                leftNode.AddEdge(currentNode.edges.Get(keys[i]))
              else
                rightNode.AddEdge(currentNode.edges.Get(keys[i]))

            currentNode.edges.Clear();
            currentNode.AddEdge(new exports.Edge(leftNodeId, 
              this.LeftEdge(nodes, rightNode).data))
            currentNode.AddEdge(new exports.Edge(rightNodeId, 
              exports.EdgeDataSingleton.MAX_VALUE))
            currentNode.SetData(this.InternalNodeData);


            nodes.SetNode(leftNodeId, leftNode);
            nodes.SetNode(rightNodeId, rightNode);
            nodes.SetNode(nodeId, currentNode);

        return null
      else
        if exports.UTILS.equals(currentNode.data, this.InternalNodeData)
          leadingEdge = this.FindLeadingEdge(data, currentNode)
          result = this.SplitTree(nodes, leadingEdge.toNodeId, false, data,
            treeOrder)

          if (result != null)
            currentNode.AddEdge(new exports.Edge(result.createdNodeId,
              result.rightKey))

            if currentNode.edges.Length() == treeOrder
              newInternalId = exports.Guid.Create()
              newInternal = new exports.Node(exports.NodeType.TreeInternal, 
                this.InternalNodeData)
              keys = currentNode.edges.Keys()

              for i, key of keys
                if (i >= Math.floor(keys.length/2)) #keys.Length()/2)
                  break
                newInternal.AddEdge(currentNode.edges.Get(key))

              nrToRemove = Math.floor(keys.length/2) #keys.Length() / 2

              for j in [0..nrToRemove]
                #TODO didn't finished
                currentNode.edges.Remove(keys[j])

              this.SetLastInternalKey(newInternal)

              nodes.SetNode(newInternalId, newInternal)
              nodes.SetNode(nodeId, currentNode)

              return new exports.SplitResult(newInternalId, newInternal,
                this.LeftEdge(nodes, currentNode).data)
            else
              nodes.SetNode(nodeId, currentNode)
              return null
          else
            return null
        else
          if exports.UTILS.equals(currentNode.data, this.LeafNodeData)
            newLeafId = exports.Guid.Create()

            newLeaf = new exports.Node(exports.NodeType.TreeLeaf, 
              this.LeafNodeData)
            keys = currentNode.edges.Keys();

            for i in [0..Math.floor(keys.length/2)]#keys.Length()/2]
              newLeaf.AddEdge(currentNode.edges.Get(keys[i]))

            nrToRemove = keys.length/2 #keys.Length() / 2

            for i in [0..nrToRemove]
              currentNode.edges.Remove(keys[i])

            nodes.SetNode(newLeafId, newLeaf);
            nodes.SetNode(nodeId, currentNode);

            return new exports.SplitResult(newLeafId, newLeaf, 
              this.LeftEdge(nodes, currentNode).data)
          else
            throw "Unexpected node data"

    # Gets leftmost leaf in the given subtree
    # nodes - Node provider - INodeProvider
    # node - Node to start from - Node
    # <returns></returns> - Node
    LeftLeaf: (nodes, node) ->
      if exports.UTILS.equals(node.data, this.LeafNodeData)
        return node
      else
        return this.LeftLeaf(nodes, 
          nodes.GetNode(node.edges.Get(node.edges.Keys()[0]).toNodeId, 
          exports.NodeAccess.Read))

    # Gets next leaf in tree compared to given sample data
    # nodes - Node provider - INodeProvider
    # rootNodeId - Root node id - Guid
    # sampleData - Sample data in the current leaf - EdgeData
    # <returns>Leaf node next in comparison to sample data, or null if no more leaves are found</returns> - Node
    NextLeaf: (nodes, rootNodeId, sampleData) ->
      node = nodes.GetNode(rootNodeId, exports.NodeAccess.Read)
      id = rootNodeId
      nextInternalParentId = exports.Guid.EMPTY

      while ( exports.UTILS.equals(node.data, this.InternalNodeData) )
        leadingEdgeIndex = this.FindLeadingEdgeIndex(sampleData, node)

        if leadingEdgeIndex < node.edges.Length() - 1
          nextInternalParentId = 
            node.edges[node.edges.Keys()[leadingEdgeIndex + 1]].toNodeId

        node = nodes.GetNode(node.edges.Get(
          node.edges.Keys()[leadingEdgeIndex]).toNodeId, exports.NodeAccess.Read)

      if exports.UTILS.equals(nextInternalParentId, exports.Guid.EMPTY)
        return null
      else
        return this.LeftLeaf(nodes, nodes.GetNode(nextInternalParentId, 
          exports.NodeAccess.Read))

    # Gets leftmost edge in the given subtree
    # <returns></returns> - Edge
    LeftEdge: (nodes, node) ->
      if exports.UTILS.equals(node.data, this.LeafNodeData)
        return node.edges.Get(node.edges.Keys()[0])
      else
        return this.LeftEdge(nodes, nodes.GetNode(node.
          edges.Get(node.edges.Keys()[0]).toNodeId, exports.NodeAccess.Read))

    # Gets the rightmost edge in given subtree
    # <returns></returns> - Edge
    RightEdge: (nodes, node) ->
      if exports.UTILS.equals(node.data, this.LeafNodeData)
        if node.edges.Length() > 0
          return node.edges.Get(node.edges.Keys()[node.edges.Length() - 1])
        else
          return null

    # <returns></returns> - void
    SetLastInternalKey: (node) ->
      lastKey = node.edges.Keys()[node.edges.Length() - 1]
      edge = node.edges.Get(lastKey)

      if not exports.UTILS.equals(edge.data, exports.EdgeDataSingleton.MAX_VALUE)
        node.edges.Remove(lastKey);
        node.AddEdge(new exports.Edge(edge.toNodeId, 
          exports.EdgeDataSingleton.MAX_VALUE))

    # <returns></returns> - RemovalResult
    RemoveEdgeRecursive: (nodes, nodeId, data, treeOrder) ->
      node = nodes.GetNode(nodeId, exports.NodeAccess.ReadWrite)
      #TODO there was expresion in c# code in finally block, I don't see why we need that expresion
      if exports.UTILS.equals(node.data, this.LeafNodeData)
        removeResult = node.edges.Remove(data)
        res = new exports.RemovalResult(node.edges.Length(), removeResult, false)
        return res
      else
        edgeIndex = this.FindLeadingEdgeIndex(data, node)
        edge = node.edges.Get(node.edges.Keys()[edgeIndex])

        res = this.RemoveEdgeRecursive(nodes, edge.toNodeId, data, treeOrder)

        if not res.wasRemoved
          return res

        if res.remainingCount < Math.floor(treeOrder/2)#treeOrder / 2

          if edgeIndex < node.edges.Length() - 1
            res = this.MergeNodes(nodes, node, edgeIndex, edgeIndex + 1, 
              treeOrder)
            return res
          else
            res = this.MergeNodes(nodes, node, edgeIndex - 1, edgeIndex,
              treeOrder)
            return res
        else
          return res

    # <returns></returns> - RemovalResult
    MergeNodes: (nodes, node, leftIndex, rightIndex, treeOrder) ->
      leftKey = node.edges.Keys()[leftIndex]
      leftNodeId = node.edges.Get(leftKey).toNodeId
      leftNode = nodes.GetNode(leftNodeId, exports.NodeAccess.ReadWrite)

      rightKey = node.edges.Keys()[rightIndex]
      rightNodeId = node.edges.Get(rightKey).toNodeId
      rightNode = nodes.GetNode(rightNodeId, exports.NodeAccess.ReadWrite)

      if not exports.UTILS.equals(leftNode.data, rightNode.data)
        throw "Both nodes must be of same type"

      if exports.UTILS.equals(leftNode.data, this.InternalNodeData)
        maxEdgeKey = leftNode.edges.Keys()[leftNode.edges.Length() - 1];
        maxEdge = leftNode.edges.Get(maxEdgeKey)
        leftNode.edges.Remove(maxEdgeKey);
        leftNode.AddEdge(new exports.Edge(maxEdge.toNodeId, 
          this.LeftEdge(nodes, rightNode).data))

      if (leftNode.edges.Length() + rightNode.edges.Length()) <= treeOrder
        for i in [0..(leftNode.edges.Length() - 1)]
          removalKey = leftNode.edges.Keys()[i]
          rightNode.AddEdge(leftNode.edges.Get(removalKey))

        nodes.SetNode(rightNodeId, rightNode)
        nodes.Remove(leftNodeId)
        node.edges.Remove(leftKey)

        if node.edges.Length() == 1
          node.SetData(rightNode.data)
          node.edges.Clear()

          for childEdge in rightNode.edges.Array()
            node.AddEdge(childEdge)

          nodes.Remove(rightNodeId)

        return new exports.RemovalResult(node.edges.Length(), true, true)
      else
        numberToTake = Math.floor((leftNode.edges.Length() + 
          rightNode.edges.Length())/ 2) - leftNode.edges.Length()

        if numberToTake > 0
          for i in [0..numberToTake]
            removalKey = rightNode.edges.Keys()[0];

            leftNode.AddEdge(rightNode.edges.Get(removalKey));
            rightNode.edges.Remove(removalKey)

          node.edges.Remove(leftKey)
          node.AddEdge(new exports.Edge(leftNodeId, 
            this.LeftEdge(nodes, rightNode).data))

          nodes.SetNode(leftNodeId, leftNode)
          nodes.SetNode(rightNodeId, rightNode)

          return new exports.RemovalResult(node.edges.Length(), true, true)
        else
          if numberToTake < 0
            for i in [0..Math.abs(numberToTake)]
              removalKey = leftNode.edges.Keys()[leftNode.edges.Length() - 1]
              rightNode.AddEdge(leftNode.edges.Get(removalKey))
              leftNode.edges.Remove(removalKey)

            node.edges.Remove(leftKey);
            node.AddEdge(new exports.Edge(leftNodeId, 
              this.LeftEdge(nodes, rightNode).data))

            nodes.SetNode(leftNodeId, leftNode)
            nodes.SetNode(rightNodeId, rightNode)

            #No edges were removed
            return new exports.RemovalResult(node.edges.Length(), true, true)
          throw "Nothing to copy"

  exports.BPlusTreeOperations = new BPlusTreeOperationsClass()