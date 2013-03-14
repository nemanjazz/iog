using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace Execom.IOG.Test
{
    [TestClass]
    public class SnapshotExclusiveTest
    {
        public interface IDatabase
        {
            string Data { get; set; }
        }

        public SnapshotExclusiveTest()
        {
            Properties.Settings.Default["SnapshotIsolationEnabled"] = true;            
        }

        [TestMethod]
        public void TestSnapshotExclusiveCanOpen()
        {
            Context ctx = new Context(typeof(IDatabase));

            var workspaceEx = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive);
            var workspaceSnap = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);

            ctx.Dispose();
        }

        [TestMethod]
        public void TestSnapshotExclusiveCanOpen2()
        {
            Context ctx = new Context(typeof(IDatabase));

            var workspaceSnap = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot);
            var workspaceEx = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive);            

            ctx.Dispose();
        }

        [TestMethod]
        public void TestSnapshotExclusiveThreads()
        {
            Context ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                ws.Data.Data = "Initial";
                ws.Commit();
            }

            var workspaceEx = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive);

            Thread t = new Thread(() =>
            {
                using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Snapshot))
                {
                    ws.Data.Data = "Snapshot";
                    ws.Commit();
                }
            });

            t.Start();

            Thread.Sleep(500);

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                Assert.AreEqual("Initial", ws.Data.Data);
            }

            workspaceEx.Data.Data = "Exclusive";
            workspaceEx.Commit();

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                Assert.AreEqual("Exclusive", ws.Data.Data);                
            }

            workspaceEx.Dispose();

            Thread.Sleep(500);

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                Assert.AreEqual("Snapshot", ws.Data.Data);
            }

            ctx.Dispose();
        }


    }
}
