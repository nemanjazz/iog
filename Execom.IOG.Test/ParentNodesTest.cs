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
            ICollection<IComputer> Computers { get; set; }
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

                var person = ws.New<IPerson>();
                person.Name = "John Connor";
                person.Car = car;

                database.Person = person;

                Assert.AreEqual("John Connor", database.Person.Name);
                Assert.AreEqual("Renault", database.Person.Car.Model);
                Assert.AreEqual(0, ws.ParentNodes(database.Person.Car).Count);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                Assert.AreEqual("John Connor", database.Person.Name);
                Assert.AreEqual("Renault", database.Person.Car.Model);
                Assert.AreEqual(1, ws.ParentNodes(database.Person.Car).Count);
                Assert.AreEqual("John Connor", (ws.ParentNodes(database.Person.Car).ElementAt(0) as IPerson).Name);
            }
        }

        [TestMethod]
        public void TestParentNodesCollection()
        {
            Context ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                IComputer pc = ws.New<IComputer>();
                pc.Model = "PC";

                IComputer laptop = ws.New<IComputer>();
                laptop.Model = "Laptop";

                var person = ws.New<IPerson>();
                person.Name = "John Connor";
                person.Computers = ws.New<ICollection<IComputer>>();
                person.Computers.Add(pc);
                person.Computers.Add(laptop);

                database.Person = person;

                Assert.AreEqual("John Connor", database.Person.Name);
                Assert.AreEqual(2, database.Person.Computers.Count);
                Assert.IsTrue(database.Person.Computers.Any(comp => comp.Model.Equals("PC")));
                Assert.IsTrue(database.Person.Computers.Any(comp => comp.Model.Equals("Laptop")));
                Assert.AreEqual(0, ws.ParentNodes(database.Person.Computers.ElementAt(0)).Count);
                Assert.AreEqual(0, ws.ParentNodes(database.Person.Computers.ElementAt(1)).Count);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                Assert.AreEqual("John Connor", database.Person.Name);
                Assert.AreEqual(2, database.Person.Computers.Count);
                Assert.IsTrue(database.Person.Computers.Any(comp => comp.Model.Equals("PC")));
                Assert.IsTrue(database.Person.Computers.Any(comp => comp.Model.Equals("Laptop")));
                ICollection<object> parentNodes = ws.ParentNodes(database.Person.Computers.ElementAt(0));
                Assert.AreEqual(1, parentNodes.Count);
                Assert.IsTrue(parentNodes.ElementAt(0) is IPerson);
                parentNodes = ws.ParentNodes(database.Person.Computers.ElementAt(1));
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

                IComputer pc = ws.New<IComputer>();
                pc.Model = "PC";

                IComputer laptop = ws.New<IComputer>();
                laptop.Model = "Laptop";

                var person = ws.New<IPersonDictionary>();
                person.Name = "John Connor";
                person.Computers = ws.New<IDictionary<String, IComputer>>();
                person.Computers.Add(pc.Model, pc);
                person.Computers.Add(laptop.Model, laptop);

                database.Person = person;

                Assert.AreEqual("John Connor", database.Person.Name);
                Assert.AreEqual(2, database.Person.Computers.Count);
                Assert.IsTrue(database.Person.Computers.Values.Any(comp => comp.Model.Equals("PC")));
                Assert.IsTrue(database.Person.Computers.Values.Any(comp => comp.Model.Equals("Laptop")));
                Assert.AreEqual(0, ws.ParentNodes(database.Person.Computers.Values.ElementAt(0)).Count);
                Assert.AreEqual(0, ws.ParentNodes(database.Person.Computers.Values.ElementAt(1)).Count);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabaseDictionary>(IsolationLevel.Exclusive))
            {
                IDatabaseDictionary database = ws.Data;

                Assert.AreEqual("John Connor", database.Person.Name);
                Assert.AreEqual(2, database.Person.Computers.Count);
                Assert.IsTrue(database.Person.Computers.Values.Any(comp => comp.Model.Equals("PC")));
                Assert.IsTrue(database.Person.Computers.Values.Any(comp => comp.Model.Equals("Laptop")));
                ICollection<object> parentNodes = ws.ParentNodes(database.Person.Computers.Values.ElementAt(0));
                Assert.AreEqual(1, parentNodes.Count);
                Assert.IsTrue(parentNodes.ElementAt(0) is IPersonDictionary);
                parentNodes = ws.ParentNodes(database.Person.Computers.Values.ElementAt(1));
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

                var car = ws.New<ICar>();
                car.Model = "Renault";

                var person = ws.New<IPerson>();
                person.Name = "John Connor";
                person.Car = car;

                database.Person = person;

                Assert.AreEqual("John Connor", database.Person.Name);
                Assert.AreEqual("Renault", database.Person.Car.Model);
                Assert.AreEqual(0, ws.ParentNodes(database.Person.Car).Count);

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
                Assert.AreEqual(1, ws.ParentNodes(database.Person.Car).Count);
                Assert.AreEqual("John Connor Junior", (ws.ParentNodes(database.Person.Car).ElementAt(0) as IPerson).Name);
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

                var car = ws.New<ICar>();
                car.Model = "Renault";

                var person = ws.New<IPerson>();
                person.Name = "John Connor";
                person.Car = car;

                database.Person = person;

                Assert.AreEqual("John Connor", database.Person.Name);
                Assert.AreEqual("Renault", database.Person.Car.Model);
                Assert.AreEqual(0, ws.ParentNodes(database.Person.Car).Count);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                Assert.AreEqual("John Connor", database.Person.Name);
                Assert.AreEqual("Renault", database.Person.Car.Model);
                Assert.AreEqual(1, ws.ParentNodes(database.Person.Car).Count);
                Assert.AreEqual("John Connor", (ws.ParentNodes(database.Person.Car).ElementAt(0) as IPerson).Name);
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
                Assert.AreEqual(1, ws.ParentNodes(database.Person.Car).Count);
                Assert.AreEqual("John Connor", (ws.ParentNodes(database.Person.Car).ElementAt(0) as IPerson).Name);
            }
        }
    }
}