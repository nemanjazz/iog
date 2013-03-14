using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Attributes;

namespace Execom.IOG.Test
{
    [TestClass]
    public class SpawningTest
    {
        public interface ICar
        {
            string Model { get; set; }
        }

        public interface IPerson
        {
            [RevisionId]
            Guid RevisionId { get; }

            string Name { get; set; }
            ICar Car { get; set; }
        }

        public interface IDatabase
        {
            IPerson Person { get; set; }
        }

        [TestMethod]
        public void TestRespawn()
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

                initialPersonId = person.RevisionId;

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                var person = database.Person;
                person.Name = "changed";
                person.Car.Model = "changed";

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                var initialPerson = ws.Spawn<IPerson>(initialPersonId);

                Assert.AreEqual("John Connor", initialPerson.Name);
                Assert.AreEqual("model", initialPerson.Car.Model);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                var person = ws.Data.Person;

                Assert.AreEqual("changed", person.Name);
                Assert.AreEqual("changed", person.Car.Model);

                ws.Commit();
            }
        }

        [TestMethod]
        public void TestRespawnImmutable()
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

                initialPersonId = person.RevisionId;

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                var person = database.Person;
                person.Name = "changed";
                person.Car.Model = "changed";

                var initialPerson = ws.SpawnImmutable<IPerson>(ws.InstanceRevisionId(person));

                Assert.AreEqual("John Connor", initialPerson.Name);
                Assert.AreEqual("model", initialPerson.Car.Model);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                var initialPerson = ws.SpawnImmutable<IPerson>(initialPersonId);

                Assert.AreEqual("John Connor", initialPerson.Name);
                Assert.AreEqual("model", initialPerson.Car.Model);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                var person = ws.Data.Person;

                Assert.AreEqual("changed", person.Name);
                Assert.AreEqual("changed", person.Car.Model);

                ws.Commit();
            }
        }
    }
}
