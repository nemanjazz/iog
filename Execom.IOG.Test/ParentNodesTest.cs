using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Attributes;
using Execom.IOG.Types;
using System.IO;
using Execom.IOG.Storage;

namespace Execom.IOG.Test
{
    [TestClass]
    public class ParentNodesTest
    {

        public interface ICar
        {
            string Model { get; set; }
        }

        public interface IComputer
        {
            string Model { get; set; }
        }

        public interface IPerson
        {
            string Name { get; set; }
            ICar Car { get; set; }
            [StoreParentNodes]
            ICar CarWithParent { get; set; }
            ICollection<IComputer> Computers { get; set; }
            [StoreParentNodes]
            ICollection<IComputer> ComputersWithParent { get; set; }
        }

        public interface IDatabase
        {
            IPerson Person { get; set; }
        }

        public interface IPersonDictionary
        {
            string Name { get; set; }
            ICar Car { get; set; }
            IDictionary<String, IComputer> Computers { get; set; }
            [StoreParentNodes]
            IDictionary<String, IComputer> ComputersWithParent { get; set; }
        }

        public interface IDatabaseDictionary
        {
            IPersonDictionary Person { get; set; }
        }

        [TestMethod]
        public void TestParentNodesProperty()
        {
            Context ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                var car = ws.New<ICar>();
                car.Model = "Renault";

                var carWithParent = ws.New<ICar>();
                carWithParent.Model = "Renault with parent";

                var person = ws.New<IPerson>();
                person.Name = "John Connor";
                person.Car = car;
                person.CarWithParent = carWithParent;

                database.Person = person;

                Assert.AreEqual("John Connor", database.Person.Name);
                Assert.AreEqual("Renault", database.Person.Car.Model);
                Assert.AreEqual("Renault with parent", database.Person.CarWithParent.Model);
                Assert.AreEqual(0, ws.ParentNodes(database.Person.Car).Count);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                Assert.AreEqual("John Connor", database.Person.Name);
                Assert.AreEqual("Renault", database.Person.Car.Model);
                Assert.AreEqual("Renault with parent", database.Person.CarWithParent.Model);
                Assert.AreEqual(0, ws.ParentNodes(database.Person.Car).Count);
                Assert.AreEqual(1, ws.ParentNodes(database.Person.CarWithParent).Count);
                Assert.AreEqual("John Connor", (ws.ParentNodes(database.Person.CarWithParent).ElementAt(0) as IPerson).Name);
            }
        }

        [TestMethod]
        public void TestParentNodesCollection()
        {
            Context ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                var person = ws.New<IPerson>();
                person.Name = "John Connor";
                person.Computers = ws.New<ICollection<IComputer>>();
                person.ComputersWithParent = ws.New<ICollection<IComputer>>();

                IComputer computer = ws.New<IComputer>();
                computer.Model = "PC1";
                person.Computers.Add(computer);
                computer = ws.New<IComputer>();
                computer.Model = "PC2";
                person.Computers.Add(computer);

                computer = ws.New<IComputer>();
                computer.Model = "PC1 with parent";
                person.ComputersWithParent.Add(computer);
                computer = ws.New<IComputer>();
                computer.Model = "PC2 with parent";
                person.ComputersWithParent.Add(computer);

                database.Person = person;

                Assert.AreEqual("John Connor", database.Person.Name);
                Assert.AreEqual(2, database.Person.Computers.Count);
                Assert.IsTrue(database.Person.Computers.Any(comp => comp.Model.Equals("PC1")));
                Assert.IsTrue(database.Person.Computers.Any(comp => comp.Model.Equals("PC2")));
                Assert.AreEqual(0, ws.ParentNodes(database.Person.Computers.ElementAt(0)).Count);
                Assert.AreEqual(0, ws.ParentNodes(database.Person.Computers.ElementAt(1)).Count);
                Assert.AreEqual(2, database.Person.ComputersWithParent.Count);
                Assert.IsTrue(database.Person.ComputersWithParent.Any(comp => comp.Model.Equals("PC1 with parent")));
                Assert.IsTrue(database.Person.ComputersWithParent.Any(comp => comp.Model.Equals("PC2 with parent")));
                Assert.AreEqual(0, ws.ParentNodes(database.Person.ComputersWithParent.ElementAt(0)).Count);
                Assert.AreEqual(0, ws.ParentNodes(database.Person.ComputersWithParent.ElementAt(1)).Count);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                Assert.AreEqual("John Connor", database.Person.Name);
                Assert.AreEqual(2, database.Person.Computers.Count);
                Assert.IsTrue(database.Person.Computers.Any(comp => comp.Model.Equals("PC1")));
                Assert.IsTrue(database.Person.Computers.Any(comp => comp.Model.Equals("PC2")));
                Assert.AreEqual(0, ws.ParentNodes(database.Person.Computers.ElementAt(0)).Count);
                Assert.AreEqual(0, ws.ParentNodes(database.Person.Computers.ElementAt(1)).Count);
                Assert.AreEqual(2, database.Person.ComputersWithParent.Count);
                Assert.IsTrue(database.Person.ComputersWithParent.Any(comp => comp.Model.Equals("PC1 with parent")));
                Assert.IsTrue(database.Person.ComputersWithParent.Any(comp => comp.Model.Equals("PC2 with parent")));
                ICollection<object> parentNodes = ws.ParentNodes(database.Person.ComputersWithParent.ElementAt(0));
                Assert.AreEqual(1, parentNodes.Count);
                Assert.IsTrue(parentNodes.ElementAt(0) is IPerson);
                parentNodes = ws.ParentNodes(database.Person.ComputersWithParent.ElementAt(1));
                Assert.AreEqual(1, parentNodes.Count);
                Assert.IsTrue(parentNodes.ElementAt(0) is IPerson);
            }
        }

        [TestMethod]
        public void TestParentNodesDictionary()
        {
            Context ctx = new Context(typeof(IDatabaseDictionary));

            using (var ws = ctx.OpenWorkspace<IDatabaseDictionary>(IsolationLevel.Exclusive))
            {
                IDatabaseDictionary database = ws.Data;

                var person = ws.New<IPersonDictionary>();
                person.Name = "John Connor";
                person.Computers = ws.New<IDictionary<String, IComputer>>();
                person.ComputersWithParent = ws.New<IDictionary<String, IComputer>>();

                IComputer computer = ws.New<IComputer>();
                computer.Model = "PC1";
                person.Computers.Add(computer.Model, computer);
                computer = ws.New<IComputer>();
                computer.Model = "PC2";
                person.Computers.Add(computer.Model, computer);

                computer = ws.New<IComputer>();
                computer.Model = "PC1 with parent";
                person.ComputersWithParent.Add(computer.Model, computer);
                computer = ws.New<IComputer>();
                computer.Model = "PC2 with parent";
                person.ComputersWithParent.Add(computer.Model, computer);

                database.Person = person;

                Assert.AreEqual("John Connor", database.Person.Name);
                Assert.AreEqual(2, database.Person.Computers.Count);
                Assert.IsTrue(database.Person.Computers.Values.Any(comp => comp.Model.Equals("PC1")));
                Assert.IsTrue(database.Person.Computers.Values.Any(comp => comp.Model.Equals("PC2")));
                Assert.AreEqual(0, ws.ParentNodes(database.Person.Computers.Values.ElementAt(0)).Count);
                Assert.AreEqual(0, ws.ParentNodes(database.Person.Computers.Values.ElementAt(1)).Count);
                Assert.AreEqual(2, database.Person.ComputersWithParent.Count);
                Assert.IsTrue(database.Person.ComputersWithParent.Values.Any(comp => comp.Model.Equals("PC1 with parent")));
                Assert.IsTrue(database.Person.ComputersWithParent.Values.Any(comp => comp.Model.Equals("PC2 with parent")));
                Assert.AreEqual(0, ws.ParentNodes(database.Person.ComputersWithParent.Values.ElementAt(0)).Count);
                Assert.AreEqual(0, ws.ParentNodes(database.Person.ComputersWithParent.Values.ElementAt(1)).Count);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabaseDictionary>(IsolationLevel.Exclusive))
            {
                IDatabaseDictionary database = ws.Data;

                Assert.AreEqual("John Connor", database.Person.Name);
                Assert.AreEqual(2, database.Person.Computers.Count);
                Assert.IsTrue(database.Person.Computers.Values.Any(comp => comp.Model.Equals("PC1")));
                Assert.IsTrue(database.Person.Computers.Values.Any(comp => comp.Model.Equals("PC2")));
                Assert.AreEqual(0, ws.ParentNodes(database.Person.Computers.Values.ElementAt(0)).Count);
                Assert.AreEqual(0, ws.ParentNodes(database.Person.Computers.Values.ElementAt(1)).Count);
                Assert.AreEqual(2, database.Person.ComputersWithParent.Count);
                Assert.IsTrue(database.Person.ComputersWithParent.Values.Any(comp => comp.Model.Equals("PC1 with parent")));
                Assert.IsTrue(database.Person.ComputersWithParent.Values.Any(comp => comp.Model.Equals("PC2 with parent")));
                ICollection<object> parentNodes = ws.ParentNodes(database.Person.ComputersWithParent.Values.ElementAt(0));
                Assert.AreEqual(1, parentNodes.Count);
                Assert.IsTrue(parentNodes.ElementAt(0) is IPersonDictionary);
                parentNodes = ws.ParentNodes(database.Person.ComputersWithParent.Values.ElementAt(1));
                Assert.AreEqual(1, parentNodes.Count);
                Assert.IsTrue(parentNodes.ElementAt(0) is IPersonDictionary);
            }
        }

        [TestMethod]
        public void TestParentNodesUpdate()
        {
            Context ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                

                var person = ws.New<IPerson>();
                person.Name = "John Connor";

                var car = ws.New<ICar>();
                car.Model = "Renault";
                person.Car = car;

                car = ws.New<ICar>();
                car.Model = "Renault with parent";
                person.CarWithParent = car;

                database.Person = person;

                Assert.AreEqual("John Connor", database.Person.Name);
                Assert.AreEqual("Renault", database.Person.Car.Model);
                Assert.AreEqual("Renault with parent", database.Person.CarWithParent.Model);
                Assert.AreEqual(0, ws.ParentNodes(database.Person.Car).Count);
                Assert.AreEqual(0, ws.ParentNodes(database.Person.CarWithParent).Count);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                database.Person.Name = "John Connor Junior";
                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                Assert.AreEqual("John Connor Junior", database.Person.Name);
                Assert.AreEqual("Renault", database.Person.Car.Model);
                Assert.AreEqual(0, ws.ParentNodes(database.Person.Car).Count);
                Assert.AreEqual(1, ws.ParentNodes(database.Person.CarWithParent).Count);
                Assert.AreEqual("John Connor Junior", (ws.ParentNodes(database.Person.CarWithParent).ElementAt(0) as IPerson).Name);
            }
        }

        [TestMethod]
        public void TestParentNodesIndexStorage()
        {
            var file = new FileStream("data.dat", FileMode.Create);
            var storage = new IndexedFileStorage(file, 256, true);
            Context ctx = new Context(typeof(IDatabase), null, storage);

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                var person = ws.New<IPerson>();
                person.Name = "John Connor";

                var car = ws.New<ICar>();
                car.Model = "Renault";
                person.Car = car;

                car = ws.New<ICar>();
                car.Model = "Renault with parent";
                person.CarWithParent = car;

                database.Person = person;

                Assert.AreEqual("John Connor", database.Person.Name);
                Assert.AreEqual("Renault", database.Person.Car.Model);
                Assert.AreEqual("Renault with parent", database.Person.CarWithParent.Model);
                Assert.AreEqual(0, ws.ParentNodes(database.Person.Car).Count);
                Assert.AreEqual(0, ws.ParentNodes(database.Person.CarWithParent).Count);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                Assert.AreEqual("John Connor", database.Person.Name);
                Assert.AreEqual("Renault", database.Person.Car.Model);
                Assert.AreEqual("Renault with parent", database.Person.CarWithParent.Model);
                Assert.AreEqual(0, ws.ParentNodes(database.Person.Car).Count);
                Assert.AreEqual(1, ws.ParentNodes(database.Person.CarWithParent).Count);
                Assert.AreEqual("John Connor", (ws.ParentNodes(database.Person.CarWithParent).ElementAt(0) as IPerson).Name);
            }


            ctx.Dispose();
            storage.Dispose();
            file.Close();
            file = new FileStream("data.dat", FileMode.Open);
            storage = new IndexedFileStorage(file, 256, true);
            ctx = new Context(typeof(IDatabase), null, storage);
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                Assert.AreEqual("John Connor", database.Person.Name);
                Assert.AreEqual("Renault", database.Person.Car.Model);
                Assert.AreEqual("Renault with parent", database.Person.CarWithParent.Model);
                Assert.AreEqual(0, ws.ParentNodes(database.Person.Car).Count);
                Assert.AreEqual(1, ws.ParentNodes(database.Person.CarWithParent).Count);
                Assert.AreEqual("John Connor", (ws.ParentNodes(database.Person.CarWithParent).ElementAt(0) as IPerson).Name);
            }
        }
    }
}