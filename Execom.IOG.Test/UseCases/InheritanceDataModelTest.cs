using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Attributes;

namespace Execom.IOG.Test.UseCases
{
    [TestClass]
    public class InheritanceDataModelTest
    {
        private Context ctx = null;

        public interface IPerson
        {
            String Name { get; set; }
            int Age { get; set; }
        }

        public interface IDepartment
        {
            String Name { get; set; }
            IDepartment SubDepartment { get; set; }
        }

        public interface IStudent : IPerson
        {
            IDepartment Department { get; set; }
            [Immutable]
            int StudentID { get; set; }
        }

        public interface IInstitution
        {
            String Name { get; set; }
        }

        public interface IUniversity : IInstitution
        {
            [Immutable]
            ICollection<IDepartment> Departments { get; set; }
        }

        public interface IDatabase
        {
            ICollection<IStudent> Students { get; set; }
            ICollection<IDepartment> Departments { get; set; }
            IUniversity University { get; set; }
        }

        [TestInitialize]
        public void SetUp()
        {
            ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                database.Departments = ws.New<ICollection<IDepartment>>();
                database.Students = ws.New<ICollection<IStudent>>();
                database.University = ws.New<IUniversity>();

                database.University.Name = "1st University";

                IDepartment historyDepartment = ws.New<IDepartment>();
                historyDepartment.Name = "History";
                IDepartment archaeologyDepartment = ws.New<IDepartment>();
                archaeologyDepartment.Name = "Archaeology";
                archaeologyDepartment.SubDepartment = ws.New<IDepartment>();
                
                historyDepartment.SubDepartment = archaeologyDepartment;

                database.Departments.Add(historyDepartment);

                ICollection<IDepartment> tempDepartments= ws.New<ICollection<IDepartment>>();
                tempDepartments.Add(historyDepartment);

                database.University.Departments = tempDepartments;

                IStudent student = ws.New<IStudent>();
                student.StudentID = 12345;
                student.Name = "John Connor";
                student.Department = archaeologyDepartment;

                database.Students.Add(student);

                ws.Commit();
            }
        }

        [TestMethod]
        public void TestReadData()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                var student = database.Students.Single(s => s.Name.Equals("John Connor"));
                Assert.IsNotNull(student);
                Assert.IsInstanceOfType(student, typeof(IStudent));
                Assert.IsInstanceOfType(student, typeof(IPerson));
                Assert.IsInstanceOfType(student.Department.SubDepartment, typeof(IDepartment));
                
                IPerson person = student;
                Assert.AreEqual(person.Name, student.Name);

                Assert.IsInstanceOfType(database.University, typeof(IUniversity));
                Assert.IsInstanceOfType(database.University, typeof(IInstitution));

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                var student = database.Students.Single(s => s.Name.Equals("John Connor"));
                Assert.IsNotNull(student);
                Assert.IsInstanceOfType(student, typeof(IStudent));
                Assert.IsInstanceOfType(student, typeof(IPerson));
                Assert.IsInstanceOfType(student.Department.SubDepartment, typeof(IDepartment));

                Assert.IsInstanceOfType(database.University, typeof(IUniversity));
                Assert.IsInstanceOfType(database.University, typeof(IInstitution));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.InvalidOperationException))]
        public void TestModificationData()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                
                var student = database.Students.Single(s => s.Name.Equals("John Connor"));
                Assert.IsNotNull(student);
                student.Name = "Indiana Jones";

                Assert.AreEqual("1st University", database.University.Name);
                database.University.Name = "2nd University";

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                var student = database.Students.Single(s => s.Name.Equals("John Connor"));
                Assert.IsNull(student);
                Assert.AreEqual("Indiana Jones",student.Name);

                Assert.AreNotEqual("1st University", database.University.Name);
                Assert.AreEqual("2st University", database.University.Name);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.InvalidOperationException))]
        public void TestModifAndRollback()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                var student = database.Students.Single(s => s.Name.Equals("John Connor"));
                Assert.IsNotNull(student);
                student.Name = "Indiana Jones";

                Assert.AreEqual("1st University", database.University.Name);
                database.University.Name = "2nd University";

                ws.Rollback();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                var student = database.Students.Single(s => s.Name.Equals("Indiana Jones"));
                Assert.IsNull(student);
                student = database.Students.Single(s => s.Name.Equals("John Connor"));
                Assert.IsNotNull(student);

                Assert.AreNotEqual("2st University", database.University.Name);
                Assert.AreEqual("1st University", database.University.Name);
            }
        }
    }
}
