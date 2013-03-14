using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Attributes;
using System.Collections.ObjectModel;
using System.Threading;

namespace Execom.IOG.Test
{
    [TestClass]
    public class DictionaryMultiInsertTest
    {
        public DictionaryMultiInsertTest()
        {
            Properties.Settings.Default["SnapshotIsolationEnabled"] = true;  
        }

        public interface IBaseEntity
        {
            Guid ID { get; set; }
        }

        [Concurrent]
        public interface IUser : IBaseEntity
        {
            String Name { get; set; }
            IDictionary<Guid, ITransaction> Transactions { get; set; }
        }

        [Concurrent]
        public interface ITransaction : IBaseEntity
        {
            int Ammount { get; set; }
        }

        [Concurrent]
        public interface IBankData
        {
            IDictionary<int, IUser> Users { get; set; }
        }

        
        [TestMethod]
        public void TestIterrativeTransactionsRandom()
        {
            int nrThreads = 5;
            int nrRepeats = 500;
            int nrUsers = 3;

            // Setup
            Context facade = new Context(typeof(IBankData));

            using (var ws = facade.OpenWorkspace<IBankData>(IsolationLevel.Exclusive))
            {
                ws.Data.Users = ws.New<IDictionary<int, IUser>>();

                for (int usr = 0; usr < nrUsers; usr++)
                {
                    IUser user = ws.New<IUser>();
                    user.ID = Guid.NewGuid();
                    user.Name = "User" + usr;
                    user.Transactions = ws.New<IDictionary<Guid, ITransaction>>();
                    ws.Data.Users.Add(usr, user);                    
                }

                ws.Commit();
            }

            //Make modifications
            for (int i = 0; i < nrRepeats; i++)
            {
                Random r = new Random();
                Collection<Thread> threads = new Collection<Thread>();

                for (int iter = 0; iter < nrThreads; iter++)
                {
                    Thread t = new Thread(() =>
                    {
                        try
                        {
                            using (var ws = facade.OpenWorkspace<IBankData>(IsolationLevel.Snapshot))
                            {
                                int firstUserIndex = r.Next(nrUsers);
                                int secondUserIndex = r.Next(nrUsers);

                                while (firstUserIndex == secondUserIndex)
                                {
                                    firstUserIndex = r.Next(nrUsers);
                                    secondUserIndex = r.Next(nrUsers);
                                }


                                IUser firstUser = ws.Data.Users[firstUserIndex];
                                IUser secondUser = ws.Data.Users[secondUserIndex];

                                int amount = r.Next(100);

                                ITransaction t1 = ws.New<ITransaction>();
                                t1.Ammount = amount;
                                firstUser.Transactions.Add(Guid.NewGuid(), t1);

                                ITransaction t2 = ws.New<ITransaction>();
                                t2.Ammount = -amount;
                                firstUser.Transactions.Add(Guid.NewGuid(), t2);

                                ws.Commit();
                            }
                        }
                        catch (Exception e)
                        {
                            Assert.Fail(e.ToString());
                        }

                    });

                    t.Start();
                    threads.Add(t);
                }

                // Wait for modifications end

                foreach (var thread in threads)
                {
                    thread.Join();
                }
            }

            using (var ws = facade.OpenWorkspace<IBankData>(IsolationLevel.ReadOnly))
            {

                int totalSum = 0;

                for (int usr = 0; usr < nrUsers; usr++)
                {
                    IUser user = ws.Data.Users[usr];
                    Assert.AreEqual(user.Name, "User" + usr);

                    int userSum = 0;

                    foreach (ITransaction trans in user.Transactions.Values)
                    {
                        userSum += trans.Ammount;
                    }

                    totalSum += userSum;
                }

                Assert.AreEqual(0, totalSum);
            }
        }
    }
}
