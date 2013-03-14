using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Attributes;

namespace Execom.IOG.Test.UseCases
{
    [TestClass]
    public class PlainDatamodelTest
    {
        private Context ctx = null;

        public interface ICity
        {
            [Immutable]
            string Name { get; set; }
        }

        public interface ICar
        {
            string Model { get; set; }
        }

        public interface IAddress
        {
            string Name { get; set; }
            int Number { get; set; }
        }

        public interface IPerson
        {
            string Name { get; set; }
            ICar Car { get; set; }
            ICity City { get; set; }
            IAddress Address { get; set; }
        }

        public interface IEvent
        {
            [Immutable]
            IPerson Organizator { get; set; }
            ICity City { get; set; }
            IAddress Address { get; set; }
        }

        public interface IDatabase
        {
            [Immutable]
            IPerson PermanentPerson { get; set; }
            IEvent Event { get; set; }
            IPerson PlainPerson { get; set; }
        }

        [TestInitialize]
        public void SetUp()
        {
            ctx = new Context(typeof(IDatabase));
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                var person = ws.New<IPerson>();
                person.Name = "John Connor";
                person.Address = ws.New<IAddress>();
                person.Address.Name = "1st Boulevard";
               
                person.City = ws.New<ICity>();
                person.City.Name = "San Francisco";
                person.Car = ws.New<ICar>();
                person.Car.Model = "Ford Mustang";

                var newEvent = ws.New<IEvent>();
                newEvent.City = person.City;
                newEvent.Organizator = person;
                newEvent.Address = ws.New<IAddress>();
                newEvent.Address.Name = "12th Boulevard";
                newEvent.Address.Number = 10;

                database.PermanentPerson = person;
                database.PlainPerson = person;
                database.Event = newEvent;

                ws.Commit();
            }
        }

        [TestMethod]
        public void TestReadData()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                Assert.AreEqual("John Connor", database.PlainPerson.Name);
                Assert.AreEqual("John Connor", database.PermanentPerson.Name);

                Assert.AreEqual(database.PlainPerson, database.Event.Organizator);
                Assert.AreEqual(database.PermanentPerson, database.Event.Organizator);

                Assert.AreEqual(database.PlainPerson.City, database.Event.City);
                Assert.AreEqual(database.PermanentPerson.City, database.Event.City);

                Assert.AreEqual("San Francisco", database.PermanentPerson.City.Name);
                Assert.AreNotEqual("Los Angeles", database.Event.City.Name);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                Assert.AreEqual("John Connor", database.PlainPerson.Name);
                Assert.AreEqual("John Connor", database.PermanentPerson.Name);

                Assert.AreEqual(database.PlainPerson, database.Event.Organizator);
                Assert.AreEqual(database.PermanentPerson, database.Event.Organizator);

                Assert.AreEqual(database.PlainPerson.City, database.Event.City);
                Assert.AreEqual(database.PermanentPerson.City, database.Event.City);

                Assert.AreEqual("San Francisco", database.PermanentPerson.City.Name);
                Assert.AreEqual("San Francisco", database.PlainPerson.City.Name);
            }
        }

        [TestMethod]
        public void TestModificationData()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                Assert.AreNotEqual("Volkswagen Golf", database.PlainPerson.Car.Model);
                Assert.AreEqual("Ford Mustang", database.PlainPerson.Car.Model);

                Assert.AreNotEqual(16, database.Event.Address.Number);
                Assert.AreEqual(10, database.Event.Address.Number);

                Assert.AreNotEqual("Seatle", database.PlainPerson.City.Name);
                Assert.AreEqual("San Francisco", database.PlainPerson.City.Name);

                database.PlainPerson.Car.Model = "Volkswagen Golf";
                database.Event.Address.Number = 16;
                database.PlainPerson.City.Name = "Seatle";

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                Assert.AreNotEqual("Ford Mustang", database.PlainPerson.Car.Model);
                Assert.AreEqual("Volkswagen Golf", database.PlainPerson.Car.Model);

                Assert.AreNotEqual(10, database.Event.Address.Number);
                Assert.AreEqual(16, database.Event.Address.Number);

                Assert.AreEqual("Seatle", database.PlainPerson.City.Name);
                Assert.AreEqual(database.Event.City.Name, database.PlainPerson.City.Name);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.InvalidOperationException))]
        public void TestModificationOfImmutable()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                Assert.AreEqual("San Francisco", database.PermanentPerson.City.Name);
                database.PermanentPerson.City.Name = "New York City";
                Assert.AreEqual("San Francisco", database.PermanentPerson.City.Name);

                Assert.AreEqual(database.PlainPerson, database.Event.Organizator);
                database.Event.Organizator = ws.New<IPerson>();
                Assert.AreEqual(database.PlainPerson, database.Event.Organizator);

                ws.Commit();
            }
        }

        [TestMethod]
        public void TestModifAndRollback()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                Assert.AreNotEqual("Volkswagen Golf", database.PlainPerson.Car.Model);
                Assert.AreEqual("Ford Mustang", database.PlainPerson.Car.Model);

                Assert.AreNotEqual("Seatle", database.PlainPerson.City.Name);
                Assert.AreEqual("San Francisco", database.PlainPerson.City.Name);

                database.PlainPerson.Car.Model = "Volkswagen Golf";
                database.PlainPerson.City.Name = "Seatle";

                Assert.AreNotEqual("Ford Mustang", database.PlainPerson.Car.Model);
                Assert.AreEqual("Volkswagen Golf", database.PlainPerson.Car.Model);

                Assert.AreEqual("Seatle", database.PlainPerson.City.Name);
                Assert.AreEqual(database.Event.City.Name, database.PlainPerson.City.Name);

                ws.Rollback();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                Assert.AreNotEqual("Volkswagen Golf", database.PlainPerson.Car.Model);
                Assert.AreEqual("Ford Mustang", database.PlainPerson.Car.Model);

                Assert.AreNotEqual("Seatle", database.PlainPerson.City.Name);
                Assert.AreEqual("San Francisco", database.PlainPerson.City.Name);
            }
        }
    }
}
