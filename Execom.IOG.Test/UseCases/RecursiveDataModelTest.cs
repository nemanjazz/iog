using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Attributes;

namespace Execom.IOG.Test.UseCases
{
    [TestClass]
    public class RecursiveDataModelTest
    {
        private Context ctx = null;

        public interface IPerson
        {
            String Name { get; set; }
            int Age { get; set; }
            [Immutable]
            IPerson Father { get; set; }
            [Immutable]
            IPerson Mother { get; set; }
        }

        public interface IDepartment
        {
            String Name { get; set; }
            IDepartment SubDepartment { get; set; }
        }

        public interface IEmployee
        {
            String Name { get; set; }
            IEmployee Manager { get; set; }
        }

        public interface IDatabase
        {
            IDepartment Department { get; set; }
            IEmployee Employee { get; set; }
            IPerson Person { get; set; }
        }

        [TestInitialize]
        public void SetUp()
        {
            ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                IPerson customer = ws.New<IPerson>();
                customer.Age = 20;
                customer.Name = "Indiana Jones Jr.";
                IPerson custFather = ws.New<IPerson>();
                custFather.Age = 50;
                custFather.Name = "Indiana Jones";
                IPerson custMother = ws.New<IPerson>();
                custMother.Age = 45;
                custMother.Name = "Anna Mary Jones";
                customer.Father = custFather;
                customer.Mother = custMother;
                database.Person = customer;

                IDepartment historyDepartment = ws.New<IDepartment>();
                historyDepartment.Name = "History";
                IDepartment archaeologyDepartment = ws.New<IDepartment>();
                archaeologyDepartment.Name = "Archaeology";
                historyDepartment.SubDepartment = archaeologyDepartment;
                database.Department = historyDepartment;

                IEmployee manager = ws.New<IEmployee>();
                manager.Name = "Manager 1";
                IEmployee worker = ws.New<IEmployee>();
                worker.Name = "Worker 1";
                worker.Manager = manager;
                database.Employee = worker;

                ws.Commit();
            }
        }


        [TestMethod]
        public void TestReadData()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                Assert.AreEqual("Indiana Jones Jr.", database.Person.Name);
                Assert.IsInstanceOfType(database.Person.Father, typeof(IPerson));
                Assert.AreEqual("Indiana Jones", database.Person.Father.Name);
                Assert.AreNotEqual("Amy", database.Person.Mother.Name);

                Assert.IsInstanceOfType(database.Department.SubDepartment, typeof(IDepartment));
                Assert.AreEqual("Archaeology", database.Department.SubDepartment.Name);

                Assert.AreNotEqual("Worker", database.Employee.Name);
                Assert.IsNotNull(database.Employee.Manager);
                Assert.AreEqual("Manager 1", database.Employee.Manager.Name);

                ws.Commit();
            }
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                Assert.AreEqual("Indiana Jones Jr.", database.Person.Name);
                Assert.IsInstanceOfType(database.Person.Father, typeof(IPerson));
                Assert.AreEqual("Indiana Jones", database.Person.Father.Name);
                Assert.AreNotEqual("Amy", database.Person.Mother.Name);

                Assert.IsInstanceOfType(database.Department.SubDepartment, typeof(IDepartment));
                Assert.AreEqual("Archaeology", database.Department.SubDepartment.Name);

                Assert.AreNotEqual("Worker", database.Employee.Name);
                Assert.IsNotNull(database.Employee.Manager);
                Assert.AreEqual("Manager 1", database.Employee.Manager.Name);
            }
        }

        [TestMethod]
        public void TestModificationData()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                Assert.IsInstanceOfType(database.Person.Father, typeof(IPerson));
                Assert.AreEqual("Indiana Jones", database.Person.Father.Name);
                Assert.AreNotEqual("Amy", database.Person.Mother.Name);

                Assert.AreNotEqual("Worker", database.Employee.Name);
                Assert.IsNotNull(database.Employee.Manager);
                Assert.AreEqual("Manager 1", database.Employee.Manager.Name);

                database.Person.Name = "Indiana Jones 2nd";
                database.Department.Name = "Computer science";
                database.Employee.Manager.Name = "Manager 2";

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                Assert.AreNotEqual("Indiana Jones Jr.", database.Person.Name);
                Assert.AreEqual("Indiana Jones 2nd", database.Person.Name);

                Assert.AreNotEqual("Archaeology", database.Department.Name);
                Assert.AreEqual("Computer science", database.Department.Name);

                Assert.AreNotEqual("Manager 1", database.Department.Name);
                Assert.AreEqual("Manager 2", database.Employee.Manager.Name);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.InvalidOperationException))]
        public void TestModificationOfImmutable()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                Assert.AreNotEqual("Sherlock Holmes", database.Person.Father.Name);
                Assert.AreEqual("Indiana Jones",database.Person.Father.Name);

                database.Person.Father.Name = "Sherlock Holmes";
                
                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                Assert.AreNotEqual("Sherlock Holmes", database.Person.Father.Name);
                Assert.AreEqual("Indiana Jones", database.Person.Father.Name);
            }
        }

        [TestMethod]
        public void TestModifAndRollback()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                Assert.AreEqual("Archaeology", database.Department.SubDepartment.Name);
                database.Department.SubDepartment.Name = "Computer Science";

                Assert.AreEqual("Manager 1", database.Employee.Manager.Name);
                database.Department.SubDepartment.Name = "Manager 2";

                ws.Rollback();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                Assert.AreNotEqual("Computer Science", database.Department.SubDepartment.Name);
                Assert.AreEqual("Archaeology", database.Department.SubDepartment.Name);

                Assert.AreNotEqual("Manager 2", database.Employee.Manager.Name);
                Assert.AreEqual("Manager 1", database.Employee.Manager.Name);
            }
        }

    }
}
