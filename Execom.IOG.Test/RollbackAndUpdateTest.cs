using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Exceptions;

namespace Execom.IOG.Test
{
    [TestClass]
    public class RollbackAndUpdateTest
    {
        public RollbackAndUpdateTest()
        {
            Properties.Settings.Default["SnapshotIsolationEnabled"] = true;            
        }

        public interface ICar
        {
            string Model { get; set; }
        }

        public interface IPerson
        {
            string Name { get; set; }
            ICar Car { get; set; }
        }

        public interface IDatabase
        {
            IPerson Person { get; set; }
        }

        [TestMethod]
        public void TestRollback()
        {

            Context ctx = new Context(typeof(IDatabase));

            Guid initialPersonId = Guid.Empty;

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                var person = ws.New<IPerson>();
                var car = ws.New<ICar>();
                car.Model = "model";

                person.Name = "John Connor";
                person.Car = car;

                database.Person = person;

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                var newInstance = ws.New<IPerson>();
                newInstance.Name = "new";
                ws.Data.Person = newInstance;

                ws.Rollback();

                bool exception = false;

                try
                {
                    newInstance.Name = "";
                }
                catch(ArgumentException)
                {
                    exception = true;
                }

                Assert.IsTrue(exception);
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                var person = ws.Data.Person;
                person.Name = "changed";

                ws.Rollback();

                Assert.AreEqual("John Connor", person.Name);

                person.Name = "changed";

                Assert.AreEqual("changed", person.Name);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                Assert.AreEqual("changed", ws.Data.Person.Name);
            }
        }

        [TestMethod]
        public void TestRollbackAndUpdate()
        {

            Context ctx = new Context(typeof(IDatabase));

            Guid initialPersonId = Guid.Empty;

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                var person = ws.New<IPerson>();
                var car = ws.New<ICar>();
                car.Model = "model";

                person.Name = "John Connor";
                person.Car = car;

                database.Person = person;

                ws.Commit();
            }

            var ws1 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            var ws2 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);

            ws1.Data.Person.Name = "Changed";
            ws1.Commit();

            Assert.AreEqual("John Connor", ws2.Data.Person.Name);

            ws2.Data.Person.Car.Model = "Changed";
            ws2.Rollback();
            ws2.Update(ws1.SnapshotId);

            Assert.AreEqual("Changed", ws2.Data.Person.Name);
            Assert.AreEqual("model", ws2.Data.Person.Car.Model);
        }

        [TestMethod]
        public void TestRollbackAndUpdateConflict()
        {

            Context ctx = new Context(typeof(IDatabase));

            Guid initialPersonId = Guid.Empty;

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                var person = ws.New<IPerson>();
                var car = ws.New<ICar>();
                car.Model = "model";

                person.Name = "John Connor";
                person.Car = car;

                database.Person = person;

                ws.Commit();
            }

            var ws1 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            var ws2 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);

            ws1.Data.Person.Name = "Changed";
            ws1.Commit();

            Assert.AreEqual("John Connor", ws2.Data.Person.Name);

            ws2.Data.Person.Name = "Conflict";
            ws2.Data.Person.Car.Model = "Changed";
            ws2.Rollback();
            ws2.Update(ws1.SnapshotId);

            Assert.AreEqual("Changed", ws2.Data.Person.Name);
            Assert.AreEqual("model", ws2.Data.Person.Car.Model);
        }

        [TestMethod]
        public void TestUpdate()
        {

            Context ctx = new Context(typeof(IDatabase));

            Guid initialPersonId = Guid.Empty;

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                var person = ws.New<IPerson>();
                var car = ws.New<ICar>();
                car.Model = "model";

                person.Name = "John Connor";
                person.Car = car;

                database.Person = person;

                ws.Commit();
            }

            var ws1 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            var ws2 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);

            ws1.Data.Person.Name = "Changed";
            ws1.Commit();

            Assert.AreEqual("John Connor", ws2.Data.Person.Name);

            ws2.Data.Person.Car.Model = "Changed";
            ws2.Update();

            Assert.AreEqual("Changed", ws2.Data.Person.Name);
            Assert.AreEqual("Changed", ws2.Data.Person.Car.Model);
        }

        [TestMethod]
        [ExpectedException(typeof(ConcurrentModificationException))]
        public void TestUpdateConflict()
        {

            Context ctx = new Context(typeof(IDatabase));

            Guid initialPersonId = Guid.Empty;

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                var person = ws.New<IPerson>();
                var car = ws.New<ICar>();
                car.Model = "model";

                person.Name = "John Connor";
                person.Car = car;

                database.Person = person;

                ws.Commit();
            }

            var ws1 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            var ws2 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);

            ws1.Data.Person.Name = "Changed";
            ws1.Commit();

            Assert.AreEqual("John Connor", ws2.Data.Person.Name);

            ws2.Data.Person.Name = "Conflict";
            ws2.Data.Person.Car.Model = "Changed";
            ws2.Update();

            Assert.AreEqual("Changed", ws2.Data.Person.Name);
            Assert.AreEqual("model", ws2.Data.Person.Car.Model);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestUpdateToOld()
        {

            Context ctx = new Context(typeof(IDatabase));

            Guid oldSnapshotId = Guid.Empty;

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                var person = ws.New<IPerson>();
                var car = ws.New<ICar>();
                car.Model = "model";

                person.Name = "John Connor";
                person.Car = car;

                database.Person = person;

                oldSnapshotId = ws.SnapshotId;

                ws.Commit();                
            }



            var ws1 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            var ws2 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);

            ws1.Data.Person.Name = "Changed";
            ws1.Commit();

            Assert.AreEqual("John Connor", ws2.Data.Person.Name);

            ws2.Data.Person.Car.Model = "Changed";
            ws2.Update(oldSnapshotId);

            Assert.AreEqual("Changed", ws2.Data.Person.Name);
            Assert.AreEqual("Changed", ws2.Data.Person.Car.Model);
        }
    }
}
