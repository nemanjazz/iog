using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Attributes;
using Execom.IOG.Services.Data;
using System.Collections;

namespace Execom.IOG.Test
{
    [TestClass]
    public class IOGDataValuesServiceTest
    {
        public IOGDataValuesServiceTest()
        {
            Properties.Settings.Default["SnapshotIsolationEnabled"] = true;
        }

        public interface IPerson
        {
            [PrimaryKey]
            string Name { get; set; }
            int Age { get; set; }
        }

        public interface IDatabase
        {
            string Name { get; set; }
            int Age { get; set; }
            ICollection<string> EmployeeNames { get; set; }
            IDictionary<string, int> EmployeeAges { get; set; }
        }

        [TestMethod]
        public void TestGetScalarPropertyDataFromModel()
        {
            using (Context ctx = new Context(typeof(IDatabase)))
            {
                using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
                {
                    ws.Data.Name = "Test Name";
                    ws.Data.Age = 32;
                    ws.Commit();

                    IOGDataStructure structure = ctx.GetDataFromModel();
                    
                    Assert.AreEqual("Root", structure.Name);
                    Assert.AreEqual("Name", (structure.SubStructures as List<IOGDataStructure>)[0].Name);
                    Assert.AreEqual("Age", (structure.SubStructures as List<IOGDataStructure>)[1].Name);
                    Assert.AreEqual("String", (structure.SubStructures as List<IOGDataStructure>)[0].Type);
                    Assert.AreEqual("Int32", (structure.SubStructures as List<IOGDataStructure>)[1].Type);
                    Assert.AreEqual("Test Name", (structure.SubStructures as List<IOGDataStructure>)[0].Value);
                    Assert.AreEqual("32", (structure.SubStructures as List<IOGDataStructure>)[1].Value);
                }
            }
        }

        [TestMethod]
        public void TestGetCollectionPropertyDataFromModel()
        {
            using (Context ctx = new Context(typeof(IDatabase)))
            {
                using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
                {
                    ws.Data.EmployeeNames = ws.New<ICollection<string>>();
                    ws.Data.EmployeeNames.Add("Employee 1");
                    ws.Data.EmployeeNames.Add("Employee 2");
                    ws.Data.EmployeeAges = ws.New<IDictionary<string, int>>();
                    ws.Data.EmployeeAges.Add("Employee 1", 32);
                    ws.Data.EmployeeAges.Add("Employee 2", 45);
                    ws.Commit();

                    IOGDataStructure structure = ctx.GetDataFromModel();
                }
            }
        }

        [TestMethod]
        public void TestGetDictionaryPropertyDataFromModel()
        {
            using (Context ctx = new Context(typeof(IDatabase)))
            {
                using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
                {
                    ws.Data.EmployeeAges = ws.New<IDictionary<string, int>>();
                    ws.Data.EmployeeAges.Add("Employee 1", 32);
                    ws.Data.EmployeeAges.Add("Employee 2", 45);
                    ws.Commit();

                    IOGDataStructure structure = ctx.GetDataFromModel();
                }
            }
        }
    }
}
