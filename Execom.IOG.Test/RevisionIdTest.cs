using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Attributes;

namespace Execom.IOG.Test
{
    [TestClass]
    public class RevisionIdTest
    {
        public interface IDatabase
        {            
            [RevisionId]
            Guid Id { get; }

            int Value { get; set; }
        }

        public interface IDatabaseAncestor : IDatabase
        {
            int Value2 { get; set; }
        }


        [TestMethod]
        public void TestRevisionId()
        {
            Context ctx = new Context(typeof(IDatabase));

            Guid lastRevision = Guid.Empty;

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                database.Value = 1;
                lastRevision = database.Id;

                // Test for attribute in generated type also
                Assert.AreEqual(1, database.GetType().GetProperty("Id").GetCustomAttributes(typeof(RevisionIdAttribute), false).Length);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                database.Value = 2;                
                ws.Commit();
                Assert.AreNotEqual(lastRevision, database.Id);
            }
        }

        [TestMethod]
        public void TestRevisionIdAncestor()
        {
            Context ctx = new Context(typeof(IDatabaseAncestor));

            Guid lastRevision = Guid.Empty;

            using (var ws = ctx.OpenWorkspace<IDatabaseAncestor>(IsolationLevel.Exclusive))
            {
                IDatabaseAncestor database = ws.Data;
                database.Value = 1;
                lastRevision = database.Id;

                // Test for attribute in generated type also
                Assert.AreEqual(1, database.GetType().GetProperty("Id").GetCustomAttributes(typeof(RevisionIdAttribute), false).Length);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabaseAncestor>(IsolationLevel.Exclusive))
            {
                IDatabaseAncestor database = ws.Data;
                database.Value = 2;
                ws.Commit();
                Assert.AreNotEqual(lastRevision, database.Id);
            }
        }
    }
}
