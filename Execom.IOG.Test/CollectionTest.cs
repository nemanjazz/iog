using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Attributes;
using Execom.IOG.Types;

namespace Execom.IOG.Test
{    
    [TestClass]
    public class CollectionTest
    {

        public CollectionTest()
        {
            Properties.Settings.Default["SnapshotIsolationEnabled"] = true;            
        }

        public interface IPerson
        {
            [PrimaryKey]
            string Name { get; set; }
            int Age { get; set; }
        }

        public interface IDatabase
        {
            ICollection<int> IntCollection { get; set; }
            ICollection<IPerson> PersonCollection { get; set; }
            IDictionary<int, IPerson> PersonDictionary { get; set; }
            IOrderedCollection<int> OrderedIntCollection { get; set; }
            IOrderedCollection<IPerson> OrderedPersonCollection { get; set; }
        }

        [TestMethod]
        public void TestCreate()
        {
            Context ctx = new Context(typeof(IDatabase));
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                ws.Data.IntCollection = ws.New<ICollection<int>>();
                ws.Data.IntCollection.Add(0);                
                ws.Commit();
            }
        }

        [TestMethod]
        public void TestCreateReference()
        {
            Context ctx = new Context(typeof(IDatabase));
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                ws.Data.PersonCollection = ws.New<ICollection<IPerson>>();
                ws.Commit();
            }
        }

        [TestMethod]
        public void TestAddReference()
        {
            Context ctx = new Context(typeof(IDatabase));
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                ws.Data.PersonCollection = ws.New<ICollection<IPerson>>();
                ws.Data.PersonCollection.Add(ws.New<IPerson>());

                ws.Data.PersonDictionary = ws.New<IDictionary<int, IPerson>>();
                ws.Data.PersonDictionary.Add(0, ws.New<IPerson>());
                IPerson per = ws.Data.PersonDictionary[0];
                per.Name = "I'm from dictionary";

                ws.Commit();
            }
        }

        [TestMethod]
        public void TestContainsReference()
        {
            Context ctx = new Context(typeof(IDatabase));
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IPerson person = ws.New<IPerson>();
                ws.Data.PersonCollection = ws.New<ICollection<IPerson>>();
                ws.Data.PersonCollection.Add(person);
                Assert.IsTrue(ws.Data.PersonCollection.Contains(person));
                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                foreach (var person in ws.Data.PersonCollection)
                {
                    Assert.IsTrue(ws.Data.PersonCollection.Contains(person));
                }
            }
        }

        [TestMethod]
        public void TestLinqReference()
        {
            int n = 1000;
            Context ctx = new Context(typeof(IDatabase));
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                ws.Data.PersonCollection = ws.New<ICollection<IPerson>>();

                for (int i = 0; i < n; i++)
                {
                    IPerson person = ws.New<IPerson>();
                    person.Name = "Person" + i;
                    ws.Data.PersonCollection.Add(person);
                }

                for (int i = 0; i < n; i++)
                {
                    Assert.AreEqual(1, ws.Data.PersonCollection.Count(p => p.Name.Equals("Person" + i)));
                }

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                for (int i = 0; i < n; i++)
                {
                    Assert.AreEqual(1, ws.Data.PersonCollection.Count(p => p.Name.Equals("Person" + i)));
                }
            }
        }

        [TestMethod]
        public void TestRemoveReference()
        {
            int n = 300;
            Context ctx = new Context(typeof(IDatabase));
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                ws.Data.PersonCollection = ws.New<ICollection<IPerson>>();

                for (int i = 0; i < n; i++)
                {
                    IPerson person = ws.New<IPerson>();
                    person.Name = "Person" + i;
                    person.Age=i;
                    ws.Data.PersonCollection.Add(person);
                }

                while (ws.Data.PersonCollection.FirstOrDefault(p => p.Age % 2 == 1) != null)
                {
                    ws.Data.PersonCollection.Remove(ws.Data.PersonCollection.FirstOrDefault(p => p.Age % 2 == 1));
                }

                Assert.AreEqual(n / 2, ws.Data.PersonCollection.Count);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                foreach (var item in ws.Data.PersonCollection)
                {
                    Assert.IsTrue(item.Age % 2 == 0);
                }

                Assert.AreEqual(n / 2, ws.Data.PersonCollection.Count);
            }
        }

        [TestMethod]
        public void TestOrderedReference()
        {
            int n = 300;
            Context ctx = new Context(typeof(IDatabase));
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                ws.Data.OrderedPersonCollection = ws.New<IOrderedCollection<IPerson>>();

                for (int i = 0; i < n; i++)
                {
                    IPerson person = ws.New<IPerson>();
                    person.Name = "Person" + i;
                    person.Age = i;
                    ws.Data.OrderedPersonCollection.Add(person);
                }

                Assert.AreEqual(n, ws.Data.OrderedPersonCollection.Count);
                Assert.AreEqual(n - 1, ws.Data.OrderedPersonCollection.Last().Age);
                Assert.AreEqual(0, ws.Data.OrderedPersonCollection.First().Age);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                Assert.AreEqual(n, ws.Data.OrderedPersonCollection.Count);
                Assert.AreEqual(n - 1, ws.Data.OrderedPersonCollection.Last().Age);
                Assert.AreEqual(0, ws.Data.OrderedPersonCollection.First().Age);
            }
        }

        [TestMethod]
        public void TestOrderedScalar()
        {
            int n = 300;
            Context ctx = new Context(typeof(IDatabase));
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                ws.Data.OrderedIntCollection = ws.New<IOrderedCollection<int>>();

                for (int i = 0; i < n; i++)
                {
                    ws.Data.OrderedIntCollection.Add(i);
                }

                Assert.AreEqual(n, ws.Data.OrderedIntCollection.Count);
                Assert.AreEqual(n - 1, ws.Data.OrderedIntCollection.Last());
                Assert.AreEqual(0, ws.Data.OrderedIntCollection.First());

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                Assert.AreEqual(n, ws.Data.OrderedIntCollection.Count);
                Assert.AreEqual(n - 1, ws.Data.OrderedIntCollection.Last());
                Assert.AreEqual(0, ws.Data.OrderedIntCollection.First());
            }
        }

        [TestMethod]
        public void TestDictinaryAddReference()
        {
            int n = 5;
            Context ctx = new Context(typeof(IDatabase));
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                ws.Data.PersonCollection = ws.New<ICollection<IPerson>>();
                ws.Data.PersonDictionary = ws.New<IDictionary<int, IPerson>>();

                for (int i = 0; i < n; i++)
                {
                    IPerson person = ws.New<IPerson>();
                    person.Name = "Person" + i;
                    person.Age = i;
                    ws.Data.PersonCollection.Add(person);
                }

                ws.Data.PersonDictionary.Add(0, ws.Data.PersonCollection.First());

                ws.Commit();
            }

            var ws1 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws1.Data.IntCollection = ws1.New<ICollection<int>>();

            var ws2 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws2.Data.PersonDictionary.Add(1, ws2.Data.PersonCollection.Last());
            ws2.Data.PersonCollection.First().Name = "Changed";
            ws2.Data.PersonCollection.Last().Name = "Changed";

            ws1.Commit();
            ws2.Commit();

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                Assert.AreEqual(2, ws.Data.PersonDictionary.Count);
                Assert.AreEqual("Changed", ws.Data.PersonDictionary[0].Name);
                Assert.AreEqual("Changed", ws.Data.PersonDictionary[1].Name);
            }
        }

        [TestMethod]
        public void TestDictinaryAddRemoveReference()
        {
            int n = 5;
            Context ctx = new Context(typeof(IDatabase));
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                ws.Data.PersonCollection = ws.New<ICollection<IPerson>>();
                ws.Data.PersonDictionary = ws.New<IDictionary<int, IPerson>>();

                for (int i = 0; i < n; i++)
                {
                    IPerson person = ws.New<IPerson>();
                    person.Name = "Person" + i;
                    person.Age = i;
                    ws.Data.PersonCollection.Add(person);
                }

                ws.Data.PersonDictionary.Add(0, ws.Data.PersonCollection.First());

                ws.Commit();
            }

            var ws1 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws1.Data.IntCollection = ws1.New<ICollection<int>>();

            var ws2 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws2.Data.PersonDictionary.Add(1, ws2.Data.PersonCollection.Last());
            ws2.Data.PersonCollection.First().Name = "Changed";
            ws2.Data.PersonCollection.Last().Name = "Changed";
            ws2.Data.PersonCollection.Remove(ws2.Data.PersonCollection.Single(o => o.Name == "Person2"));

            ws1.Commit();
            ws2.Commit();

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                Assert.AreEqual(2, ws.Data.PersonDictionary.Count);
                Assert.AreEqual("Changed", ws.Data.PersonDictionary[0].Name);
                Assert.AreEqual("Changed", ws.Data.PersonDictionary[1].Name);
                Assert.AreEqual(n - 1, ws.Data.PersonCollection.Count);
            }
        }      
    }
}
