using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Attributes;

namespace Execom.IOG.Test
{
    
    [TestClass]
    public class PermanentTest
    {
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
            [Immutable]
            IPerson PermanentPerson { get; set; }

            IPerson PlainPerson { get; set; }
        }

        [TestMethod]
        public void TestSimple()
        {
            Context ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                var person = ws.New<IPerson>();
                
                person.Name = "John Connor";

                database.PermanentPerson = person;
                database.PlainPerson = person;

                Assert.AreEqual("John Connor", database.PlainPerson.Name);
                Assert.AreEqual("John Connor", database.PermanentPerson.Name);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                database.PlainPerson.Name = "Changed!";
                Assert.AreEqual("Changed!", database.PlainPerson.Name);
                Assert.AreEqual("John Connor", database.PermanentPerson.Name);
                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                Assert.AreEqual("Changed!", database.PlainPerson.Name);
                Assert.AreEqual("John Connor", database.PermanentPerson.Name);
            }
        }

        [TestMethod]
        public void TestDeep()
        {
            Context ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                var car = ws.New<ICar>();
                car.Model = "Model1";

                var person = ws.New<IPerson>();                
                person.Name = "John Connor";
                person.Car = car;

                database.PermanentPerson = person;
                database.PlainPerson = person;

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                database.PlainPerson.Car.Model = "Changed!";
                Assert.AreEqual("Changed!", database.PlainPerson.Car.Model);
                Assert.AreEqual("Model1", database.PermanentPerson.Car.Model);
                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                Assert.AreEqual("Changed!", database.PlainPerson.Car.Model);
                Assert.AreEqual("Model1", database.PermanentPerson.Car.Model);
            }
        }
    }
}
