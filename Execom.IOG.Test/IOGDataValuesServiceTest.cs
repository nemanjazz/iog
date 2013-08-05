using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Attributes;
using Execom.IOG.Services.Data;

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

                    Assert.AreEqual("IDatabase", structure.DataStructureName);
                    Assert.IsNull(structure.SubDataStructure);
                    Assert.IsTrue(structure.ScalarValues.Count == 2);
                }
            }
        }
    }
}
