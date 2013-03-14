using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Attributes;

namespace Execom.IOG.Test
{
    [TestClass]
    public class SnapshotIsolationTest
    {        

        public SnapshotIsolationTest()
        {
            Properties.Settings.Default["SnapshotIsolationEnabled"] = true;            
        }
       
        public interface IMaufacturer
        {
            string Name { get; set; }
        }

        [Concurrent]
        public interface ICar
        {
            [Override]
            string Model { get; set; }
            DateTime ManufactureDate { get; set; }
            IMaufacturer Manufacturer { get; set; }
        }

        [Concurrent]
        public interface IPerson
        {
            string Name { get; set; }
            string Address { get; set; }

            [Override]
            ICar Car { get; set; }
        }

        [Concurrent]
        public interface IDatabase
        {
            IPerson Person { get; set; }
            ICar StockCar { get; set; }
        }

        [TestMethod]
        public void TestProperty()
        {            
            Context ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                ws.Data.Person = ws.New<IPerson>();
                ws.Commit();
            }

            var ws1 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws1.Data.Person.Name = "Name";

            var ws2 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws2.Data.Person.Address = "Address";

            ws1.Commit();

            Assert.AreEqual("Name", ws1.Data.Person.Name);

            ws2.Commit();

            Assert.AreEqual("Name", ws2.Data.Person.Name);
            Assert.AreEqual("Address", ws2.Data.Person.Address);

            ws1.Dispose();
            ws2.Dispose();

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                Assert.AreEqual("Name", ws.Data.Person.Name);
                Assert.AreEqual("Address", ws.Data.Person.Address);
            }
        }

        [TestMethod]
        public void TestReferencedProperty()
        {

            Context ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                ws.Data.Person = ws.New<IPerson>();
                ws.Data.Person.Car = ws.New<ICar>();
                ws.Commit();
            }

            var ws1 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws1.Data.Person.Car.Model = "Model";

            var ws2 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws2.Data.Person.Car.ManufactureDate = DateTime.MaxValue;

            ws1.Commit();

            Assert.AreEqual("Model", ws1.Data.Person.Car.Model);

            ws2.Commit();

            Assert.AreEqual("Model", ws2.Data.Person.Car.Model);
            Assert.AreEqual(DateTime.MaxValue, ws2.Data.Person.Car.ManufactureDate);

            ws1.Dispose();
            ws2.Dispose();

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                Assert.AreEqual("Model", ws.Data.Person.Car.Model);
                Assert.AreEqual(DateTime.MaxValue, ws.Data.Person.Car.ManufactureDate);
            }
        }

        [TestMethod]
        public void TestReferenceSetToNew()
        {
            Context ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                ws.Data.Person = ws.New<IPerson>();
                ws.Data.Person.Car = ws.New<ICar>();
                ws.Data.Person.Car.Model = "Model";
                ws.Data.StockCar = ws.Data.Person.Car;
                ws.Commit();
            }

            var ws1 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws1.Data.Person.Name = "Name";


            var ws2 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws2.Data.Person.Car = ws2.New<ICar>();
            ws2.Data.Person.Car.Model = "Model2";
            ws2.Data.Person.Car.Manufacturer = ws2.New<IMaufacturer>();
            ws2.Data.Person.Car.Manufacturer.Name = "Manufacturer2";
            ws1.Commit();

            Assert.AreEqual("Name", ws1.Data.Person.Name);

            ws2.Commit();

            Assert.AreEqual("Name", ws1.Data.Person.Name);
            Assert.AreEqual("Model2", ws2.Data.Person.Car.Model);
            Assert.AreEqual("Manufacturer2", ws2.Data.Person.Car.Manufacturer.Name);

            ws1.Dispose();
            ws2.Dispose();

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                Assert.AreEqual("Model", ws.Data.StockCar.Model);
                Assert.AreEqual("Name", ws.Data.Person.Name);
                Assert.AreEqual("Model2", ws.Data.Person.Car.Model);
                Assert.AreEqual("Manufacturer2", ws.Data.Person.Car.Manufacturer.Name);
            }
        }

        [TestMethod]
        public void TestReferenceChanged()
        {
            Context ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                ws.Data.Person = ws.New<IPerson>();
                ws.Data.StockCar = ws.New<ICar>();
                ws.Data.StockCar.Model = "Stock";
                ws.Commit();
            }

            var ws1 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws1.Data.Person.Name = "Name";

            var ws2 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws2.Data.Person.Car = ws2.Data.StockCar;

            ws1.Commit();

            Assert.AreEqual("Name", ws1.Data.Person.Name);

            ws2.Commit();

            Assert.AreEqual("Name", ws2.Data.Person.Name);
            Assert.AreEqual("Stock", ws2.Data.Person.Car.Model);

            ws1.Dispose();
            ws2.Dispose();

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                Assert.AreEqual("Name", ws.Data.Person.Name);
                Assert.AreEqual("Stock", ws.Data.Person.Car.Model);
            }
        }

        [TestMethod]
        public void TestReferenceChangedAndUpgraded()
        {
            Context ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                ws.Data.Person = ws.New<IPerson>();
                ws.Data.StockCar = ws.New<ICar>();
                ws.Data.StockCar.Model = "Stock";
                ws.Commit();
            }

            var ws1 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws1.Data.Person.Name = "Name";
            ws1.Data.StockCar.Model = "Changed";

            var ws2 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws2.Data.Person.Car = ws2.Data.StockCar;
            ws2.Data.StockCar.ManufactureDate = DateTime.MaxValue;

            ws1.Commit();

            Assert.AreEqual("Name", ws1.Data.Person.Name);

            ws2.Commit();

            Assert.AreEqual("Name", ws2.Data.Person.Name);
            Assert.AreEqual("Changed", ws2.Data.Person.Car.Model);
            Assert.AreEqual(DateTime.MaxValue, ws2.Data.Person.Car.ManufactureDate);

            ws1.Dispose();
            ws2.Dispose();

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                Assert.AreEqual("Name", ws.Data.Person.Name);
                Assert.AreEqual("Changed", ws.Data.Person.Car.Model);
                Assert.AreEqual(DateTime.MaxValue, ws.Data.Person.Car.ManufactureDate);
            }
        }

        [TestMethod]
        public void TestReferenceChangedAndUpgradedConflict()
        {
            Properties.Settings.Default["ConcurrencyAttributesEnabled"] = true;

            Context ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                ws.Data.Person = ws.New<IPerson>();
                ws.Data.StockCar = ws.New<ICar>();
                ws.Data.StockCar.Model = "Stock";
                ws.Commit();
            }

            var ws1 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws1.Data.Person.Name = "Name";
            ws1.Data.Person.Car = ws1.Data.StockCar;
            ws1.Data.StockCar.Model = "Changed";

            var ws2 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws2.Data.Person.Car = ws2.Data.StockCar;
            ws2.Data.Person.Car.Model = "Conflict";
            ws2.Data.StockCar.ManufactureDate = DateTime.MaxValue;

            ws1.Commit();

            Assert.AreEqual("Name", ws1.Data.Person.Name);

            ws2.Commit();

            Assert.AreEqual("Name", ws2.Data.Person.Name);
            Assert.AreEqual("Conflict", ws2.Data.Person.Car.Model);
            Assert.AreEqual("Conflict", ws2.Data.StockCar.Model);
            Assert.AreEqual(DateTime.MaxValue, ws2.Data.Person.Car.ManufactureDate);

            ws1.Dispose();
            ws2.Dispose();

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                Assert.AreEqual("Name", ws.Data.Person.Name);
                Assert.AreEqual("Conflict", ws.Data.Person.Car.Model);
                Assert.AreEqual("Conflict", ws.Data.StockCar.Model);
                Assert.AreEqual(DateTime.MaxValue, ws.Data.Person.Car.ManufactureDate);
            }
        }

        [TestMethod]
        public void TestReferenceChangedAndUpgradedConflictAutoOverride()
        {
            Properties.Settings.Default["ConcurrencyAttributesEnabled"] = false;
            Properties.Settings.Default["ConcurrencyAutoOverrideResolution"] = true;

            Context ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                ws.Data.Person = ws.New<IPerson>();
                ws.Data.StockCar = ws.New<ICar>();
                ws.Data.StockCar.Model = "Stock";
                ws.Commit();
            }

            var ws1 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws1.Data.Person.Name = "Name";
            ws1.Data.Person.Car = ws1.Data.StockCar;
            ws1.Data.StockCar.Model = "Changed";

            var ws2 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws2.Data.Person.Car = ws2.Data.StockCar;
            ws2.Data.Person.Car.Model = "Conflict";
            ws2.Data.StockCar.ManufactureDate = DateTime.MaxValue;

            ws1.Commit();

            Assert.AreEqual("Name", ws1.Data.Person.Name);

            ws2.Commit();

            Assert.AreEqual("Name", ws2.Data.Person.Name);
            Assert.AreEqual("Conflict", ws2.Data.Person.Car.Model);
            Assert.AreEqual("Conflict", ws2.Data.StockCar.Model);
            Assert.AreEqual(DateTime.MaxValue, ws2.Data.Person.Car.ManufactureDate);

            ws1.Dispose();
            ws2.Dispose();

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                Assert.AreEqual("Name", ws.Data.Person.Name);
                Assert.AreEqual("Conflict", ws.Data.Person.Car.Model);
                Assert.AreEqual("Conflict", ws.Data.StockCar.Model);
                Assert.AreEqual(DateTime.MaxValue, ws.Data.Person.Car.ManufactureDate);
            }
        }

        [TestMethod]
        public void TestPropertyAndReference()
        {
            Context ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                ws.Data.Person = ws.New<IPerson>();
                ws.Data.Person.Car = ws.New<ICar>();
                ws.Commit();
            }

            var ws1 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws1.Data.Person.Name = "Name";
            ws1.Data.Person.Car.Model = "Model";

            var ws2 = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            ws2.Data.Person.Address = "Address";
            ws2.Data.Person.Car.ManufactureDate = DateTime.Today;

            ws1.Commit();

            Assert.AreEqual("Name", ws1.Data.Person.Name);

            ws2.Commit();

            Assert.AreEqual("Name", ws2.Data.Person.Name);
            Assert.AreEqual("Address", ws2.Data.Person.Address);

            ws1.Dispose();
            ws2.Dispose();

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                Assert.AreEqual("Name", ws.Data.Person.Name);
                Assert.AreEqual("Address", ws.Data.Person.Address);
            }
        }
    }
}
