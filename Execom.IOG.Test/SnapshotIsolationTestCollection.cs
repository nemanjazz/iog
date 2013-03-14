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
    public class SnapshotIsolationTestCollection
    {
        public SnapshotIsolationTestCollection()
        {
            Properties.Settings.Default["SnapshotIsolationEnabled"] = true;            
        }

        public interface ICar
        {
            string Model { get; set; }
            DateTime ManufactureDate { get; set; }
        }
       
        [Concurrent]
        public interface IPerson
        {
            [PrimaryKey]
            int Id { get; set; }

            ICar Car { get; set; }

            string Name { get; set; }
            string Address { get; set; }
        }

        [Concurrent]
        public interface IDatabase
        {
            IIndexedCollection<IPerson> PersonCollection { get; set; }
        }

        [TestMethod]
        public void TestProperty()
        {
            Context ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                ws.Data.PersonCollection = ws.New<IIndexedCollection<IPerson>>();

                for (int i = 0; i < 10; i++)
                {
                    var person = ws.New<IPerson>();
                    person.Id = i;
                    person.Name = "Person" + i;
                    ws.Data.PersonCollection.Add(person);
                }

                ws.Commit();
            }

            var ws1 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws1.Data.PersonCollection.FindByPrimaryKey(0).Name = "Changed";
            ws1.Data.PersonCollection.FindByPrimaryKey(9).Name = "Changed";

            var ws2 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws2.Data.PersonCollection.FindByPrimaryKey(1).Name = "Changed";
            ws2.Data.PersonCollection.FindByPrimaryKey(9).Address = "Changed";

            ws1.Commit();

            //Assert.AreEqual("Name", ws1.Data.Person.Name);

            ws2.Commit();

            //Assert.AreEqual("Name", ws2.Data.Person.Name);
            //Assert.AreEqual("Address", ws2.Data.Person.Address);

            ws1.Dispose();
            ws2.Dispose();

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                Assert.AreEqual("Changed", ws.Data.PersonCollection.FindByPrimaryKey(0).Name);
                Assert.AreEqual("Changed", ws.Data.PersonCollection.FindByPrimaryKey(1).Name);
                Assert.AreEqual("Changed", ws.Data.PersonCollection.FindByPrimaryKey(9).Name);
                Assert.AreEqual("Changed", ws.Data.PersonCollection.FindByPrimaryKey(9).Address);
            }
        }

        [TestMethod]
        public void TestPropertyReference()
        {
            Context ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                ws.Data.PersonCollection = ws.New<IIndexedCollection<IPerson>>();

                for (int i = 0; i < 10; i++)
                {
                    var person = ws.New<IPerson>();
                    person.Id = i;
                    person.Name = "Person" + i;
                    ws.Data.PersonCollection.Add(person);
                }

                ws.Commit();
            }

            var ws1 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws1.Data.PersonCollection.FindByPrimaryKey(0).Name = "Changed";
            ws1.Data.PersonCollection.FindByPrimaryKey(9).Name = "Changed";
            ws1.Data.PersonCollection.FindByPrimaryKey(5).Car = ws1.New<ICar>();
            ws1.Data.PersonCollection.FindByPrimaryKey(5).Car.Model = "Model1";

            var ws2 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws2.Data.PersonCollection.FindByPrimaryKey(1).Name = "Changed";
            ws2.Data.PersonCollection.FindByPrimaryKey(9).Address = "Changed";
            ws2.Data.PersonCollection.FindByPrimaryKey(6).Car = ws2.New<ICar>();
            ws2.Data.PersonCollection.FindByPrimaryKey(6).Car.Model = "Model2";

            ws1.Commit();

            //Assert.AreEqual("Name", ws1.Data.Person.Name);

            ws2.Commit();

            //Assert.AreEqual("Name", ws2.Data.Person.Name);
            //Assert.AreEqual("Address", ws2.Data.Person.Address);

            ws1.Dispose();
            ws2.Dispose();

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                Assert.AreEqual("Changed", ws.Data.PersonCollection.FindByPrimaryKey(0).Name);
                Assert.AreEqual("Changed", ws.Data.PersonCollection.FindByPrimaryKey(1).Name);
                Assert.AreEqual("Model1", ws.Data.PersonCollection.FindByPrimaryKey(5).Car.Model);
                Assert.AreEqual("Model2", ws.Data.PersonCollection.FindByPrimaryKey(6).Car.Model);
                Assert.AreEqual("Changed", ws.Data.PersonCollection.FindByPrimaryKey(9).Name);
                Assert.AreEqual("Changed", ws.Data.PersonCollection.FindByPrimaryKey(9).Address);
            }
        }

         [TestMethod]
        public void TestPropertyReferenceModified()
        {
            Context ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                ws.Data.PersonCollection = ws.New<IIndexedCollection<IPerson>>();

                for (int i = 0; i < 10; i++)
                {
                    var person = ws.New<IPerson>();
                    person.Id = i;
                    person.Name = "Person" + i;
                    person.Car = ws.New<ICar>();
                    ws.Data.PersonCollection.Add(person);
                }

                ws.Commit();
            }

            var ws1 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws1.Data.PersonCollection.FindByPrimaryKey(0).Car.Model = "Changed";

            var ws2 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws2.Data.PersonCollection.FindByPrimaryKey(1).Car.Model = "Changed";

            ws1.Commit();

            //Assert.AreEqual("Name", ws1.Data.Person.Name);

            ws2.Commit();

            //Assert.AreEqual("Name", ws2.Data.Person.Name);
            //Assert.AreEqual("Address", ws2.Data.Person.Address);

            ws1.Dispose();
            ws2.Dispose();

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                Assert.AreEqual("Changed", ws.Data.PersonCollection.FindByPrimaryKey(0).Car.Model);
                Assert.AreEqual("Changed", ws.Data.PersonCollection.FindByPrimaryKey(1).Car.Model);
            }
        }

         [TestMethod]
         public void TestPropertyReferenceAdded()
         {
             Context ctx = new Context(typeof(IDatabase));

             using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
             {
                 ws.Data.PersonCollection = ws.New<IIndexedCollection<IPerson>>();

                 for (int i = 0; i < 10; i++)
                 {
                     var person = ws.New<IPerson>();
                     person.Id = i;
                     person.Name = "Person" + i;
                     ws.Data.PersonCollection.Add(person);
                 }

                 ws.Commit();
             }

             var ws1 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
             var person10 = ws1.New<IPerson>();
             person10.Id = 10;
             person10.Name = "Person10";
             person10.Car = ws1.New<ICar>();
             ws1.Data.PersonCollection.Add(person10);

             var ws2 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
             var person11 = ws2.New<IPerson>();
             person11.Id = 11;
             person11.Name = "Person11";
             person11.Car = ws2.New<ICar>();
             ws2.Data.PersonCollection.Add(person11);

             ws1.Commit();

             //Assert.AreEqual("Name", ws1.Data.Person.Name);

             ws2.Commit();

             //Assert.AreEqual("Name", ws2.Data.Person.Name);
             //Assert.AreEqual("Address", ws2.Data.Person.Address);

             ws1.Dispose();
             ws2.Dispose();

             using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
             {
                 Assert.AreEqual(12, ws.Data.PersonCollection.Count);
                 Assert.IsTrue(ws.Data.PersonCollection.ContainsPrimaryKey(10));
                 Assert.IsTrue(ws.Data.PersonCollection.ContainsPrimaryKey(11));

                 Assert.AreNotEqual(null, ws.Data.PersonCollection.FindByPrimaryKey(10).Car);
                 Assert.AreNotEqual(null, ws.Data.PersonCollection.FindByPrimaryKey(11).Car);
             }
         }     
    }
}
