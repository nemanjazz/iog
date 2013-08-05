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
        public void TestGetPropertyDataFromModel()
        {
            using (Context ctx = new Context(typeof(IDatabase)))
            {
                using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
                {
                    ws.Data.Name = "Test Name";
                    ws.Data.Age = 32;
                    ws.Commit();

                    IOGDataStructure structure = ctx.GetDataFromModel();

                    Assert.AreEqual("Root", structure.DataStructureName);
                    Assert.AreEqual(typeof(IDatabase), structure.DataStructureType);
                    Assert.IsTrue(structure.ScalarValues.Count == 2);
                }
            }
        }

        [TestMethod]
        public void TestGetCollectionDataFromModel()
        {
            using (Context ctx = new Context(typeof(IDatabase)))
            {
                using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
                {
                    ws.Data.EmployeeNames = ws.New<ICollection<string>>();
                    ws.Data.EmployeeNames.Add("Employee 1");
                    ws.Data.EmployeeNames.Add("Employee 2");
                    ws.Commit();

                    IOGDataStructure structure = ctx.GetDataFromModel();

                    Assert.AreEqual("Root", structure.DataStructureName);
                    Assert.AreEqual(typeof(ICollection<string>), structure.ScalarValuesCollection.DataStructureType);
                    Assert.IsTrue(structure.ScalarValuesCollection.DataStructureMemberName == "EmployeeNames" &&
                        structure.ScalarValuesCollection.Values.Count == 2);
                }
            }
        }

        [TestMethod]
        public void TestGetDictionaryDataFromModel()
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

                    Assert.AreEqual("Root", structure.DataStructureName);
                    Assert.AreEqual(typeof(IDictionary<string, int>), structure.ScalarValuesDictionary.DataStructureType);
                    Assert.IsTrue(structure.ScalarValuesDictionary.DataStructureMemberName == "EmployeeAges" &&
                        structure.ScalarValuesDictionary.Values.Count == 2);
                }
            }
        }
    }
}
