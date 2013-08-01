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
    public class ParentNodesTest
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
            IPerson Person { get; set; }
        }

        [TestMethod]
        public void TestParentNodes()
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
                Assert.AreEqual(0, ws.ParentNodes<IPerson>(database.Person.Car).Count);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                Assert.AreEqual("John Connor", database.Person.Name);
                Assert.AreEqual("Renault", database.Person.Car.Model);
                Assert.AreEqual(1, ws.ParentNodes<IPerson>(database.Person.Car).Count);
                Assert.AreEqual("John Connor", ws.ParentNodes<IPerson>(database.Person.Car).ElementAt(0).Name);
            }
        }
    }
}