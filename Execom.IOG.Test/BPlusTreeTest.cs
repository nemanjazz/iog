using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Graph;
using Execom.IOG.Providers;
using Execom.IOG.Storage;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Execom.IOG.Test
{
    [TestClass]
    public class BPlusTreeTest
    {

        private const int TreeOrder = 20;
        [TestMethod]
        public void TestFill()
        {
            INodeProvider<Guid, object, EdgeData> nodes = new DirectNodeProviderSafe<Guid, object, EdgeData>(new MemoryStorageUnsafe<Guid, object>(), false);

            Guid rootId = Guid.NewGuid();            
            Node<Guid, object, EdgeData> node  = BPlusTreeOperations.CreateRootNode(NodeType.Collection, rootId);
            nodes.SetNode(rootId, node);

            Collection<Guid> references = new Collection<Guid>();
            for (int i = 0; i < 10000; i++)
            {
                references.Add(Guid.NewGuid());
            }

            foreach (var reference in references)
            {
                BPlusTreeOperations.InsertEdge(nodes, rootId, new Edge<Guid, EdgeData>(reference, new EdgeData(EdgeType.ListItem, reference)), TreeOrder);
            }
        }

        [TestMethod]
        public void TestConsistency()
        {
            INodeProvider<Guid, object, EdgeData> nodes = new DirectNodeProviderSafe<Guid, object, EdgeData>(new MemoryStorageUnsafe<Guid, object>(), false);

            Guid rootId = Guid.NewGuid();
            Node<Guid, object, EdgeData> node = BPlusTreeOperations.CreateRootNode(NodeType.Collection, rootId);
            nodes.SetNode(rootId, node);

            Collection<Guid> references = new Collection<Guid>();
            Collection<Guid> referencesAdded = new Collection<Guid>();

            for (int i = 0; i < 1000; i++)
            {
                references.Add(Guid.NewGuid());
            }

            foreach (var reference in references)
            {
                
                var data = new EdgeData(EdgeType.ListItem, reference);
                BPlusTreeOperations.InsertEdge(nodes, rootId, new Edge<Guid, EdgeData>(reference, data), TreeOrder);
                referencesAdded.Add(reference);

                foreach (var addedReference in referencesAdded)
                {
                    var dataAdded = new EdgeData(EdgeType.ListItem, addedReference);
                    Edge<Guid, EdgeData> edge;
                    Assert.IsTrue(BPlusTreeOperations.TryFindEdge(nodes, rootId, dataAdded, out edge));
                    Assert.AreEqual(addedReference, edge.Data.Data);
                }
            }

            foreach (var reference in references)
            {
                var data = new EdgeData(EdgeType.ListItem, reference);
                Edge<Guid, EdgeData> edge;
                Assert.IsTrue(BPlusTreeOperations.TryFindEdge(nodes, rootId, data, out edge));
                Assert.AreEqual(reference, edge.Data.Data);
                Assert.AreEqual(reference, edge.ToNodeId);
            }
        }

        [TestMethod]
        public void TestFillRemove()
        {
            INodeProvider<Guid, object, EdgeData> nodes = new DirectNodeProviderSafe<Guid, object, EdgeData>(new MemoryStorageUnsafe<Guid, object>(), false);

            Guid rootId = Guid.NewGuid();
            Node<Guid, object, EdgeData> node = BPlusTreeOperations.CreateRootNode(NodeType.Collection, rootId);
            nodes.SetNode(rootId, node);

            Collection<Guid> references = new Collection<Guid>();
            for (int i = 0; i < 1000; i++)
            {
                references.Add(Guid.NewGuid());
            }

            int count = 0;

            foreach (var reference in references)
            {
                Assert.AreEqual(count, BPlusTreeOperations.Count(nodes, rootId, EdgeType.ListItem));
                BPlusTreeOperations.InsertEdge(nodes, rootId, new Edge<Guid, EdgeData>(reference, new EdgeData(EdgeType.ListItem, reference)), TreeOrder);
                count++;
            }
                        
            foreach (var reference in references)
            {                
                Assert.AreEqual(count, BPlusTreeOperations.Count(nodes, rootId, EdgeType.ListItem));
                BPlusTreeOperations.RemoveEdge(nodes, rootId, new EdgeData(EdgeType.ListItem, reference), TreeOrder);
                count--;                
            }
        }

        [TestMethod]
        public void TestFillRemoveBackwards()
        {
            INodeProvider<Guid, object, EdgeData> nodes = new DirectNodeProviderSafe<Guid, object, EdgeData>(new MemoryStorageUnsafe<Guid, object>(), false);

            Guid rootId = Guid.NewGuid();
            Node<Guid, object, EdgeData> node = BPlusTreeOperations.CreateRootNode(NodeType.Collection, rootId);
            nodes.SetNode(rootId, node);

            Collection<Guid> references = new Collection<Guid>();
            for (int i = 0; i < 1000; i++)
            {
                references.Add(Guid.NewGuid());
            }

            int count = 0;

            foreach (var reference in references)
            {
                Assert.AreEqual(count, BPlusTreeOperations.Count(nodes, rootId, EdgeType.ListItem));
                BPlusTreeOperations.InsertEdge(nodes, rootId, new Edge<Guid, EdgeData>(reference, new EdgeData(EdgeType.ListItem, reference)), TreeOrder);
                count++;
            }

            foreach (var reference in references.Reverse())
            {
                Assert.AreEqual(count, BPlusTreeOperations.Count(nodes, rootId, EdgeType.ListItem));
                BPlusTreeOperations.RemoveEdge(nodes, rootId, new EdgeData(EdgeType.ListItem, reference), TreeOrder);
                count--;
            }
        }
    }
}
