using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Types;
using System.Collections.ObjectModel;
using System.Threading;

namespace Execom.IOG.Test
{
    [TestClass]
    public class CollectionMultiRemoveTest
    {
        public interface IUser
        {
            String Name { get; set; }
        }

        public interface IDataModel
        {
            IOrderedCollection<IUser> Users { get; set; }
        }

        public CollectionMultiRemoveTest()
        {
            Properties.Settings.Default["SnapshotIsolationEnabled"] = true;  
        }

        [TestMethod]
        public void TestRemove()
        {
            int nrThreads = 5;            
            int nrUsers = 100;
            int nrRepeats = nrUsers / nrThreads;

            // Setup
            Context facade = new Context(typeof(IDataModel));

            using (var ws = facade.OpenWorkspace<IDataModel>(IsolationLevel.Exclusive))
            {
                ws.Data.Users = ws.New<IOrderedCollection<IUser>>();

                for (int usr = 0; usr < nrUsers; usr++)
                {
                    IUser user = ws.New<IUser>();
                    user.Name = "User" + usr;
                    ws.Data.Users.Add(user);
                }

                ws.Commit();
            }

            var openWs = facade.OpenWorkspace<IDataModel>(IsolationLevel.Snapshot);

            //Make modifications
            for (int i = 0; i < nrRepeats; i++)
            {
                Random r = new Random();
                Collection<Thread> threads = new Collection<Thread>();

                for (int iter = 0; iter < nrThreads; iter++)
                {
                    Thread t = new Thread((p) =>
                    {
                        int usr = (int)p;
                        try
                        {
                            using (var ws = facade.OpenWorkspace<IDataModel>(IsolationLevel.Snapshot))
                            {                                
                                ws.Data.Users.Remove(ws.Data.Users.Single(u => u.Name.Equals("User" + usr)));

                                ws.Commit();
                            }
                        }
                        catch (Exception e)
                        {
                            Assert.Fail(e.ToString());
                        }

                    });

                    t.Start(i * nrThreads + iter);
                    threads.Add(t);
                }

                // Wait for modifications end

                foreach (var thread in threads)
                {
                    thread.Join();
                }
            }

            openWs.Update();

            openWs.Dispose();

            using (var ws = facade.OpenWorkspace<IDataModel>(IsolationLevel.ReadOnly))
            {
                Assert.AreEqual(0, ws.Data.Users.Count);
            }
        }
    }
}
