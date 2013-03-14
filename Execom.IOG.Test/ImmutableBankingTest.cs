using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Attributes;
using System.Collections.ObjectModel;
using System.Threading;
using System.Diagnostics;

namespace Execom.IOG.Test
{
    [TestClass]
    public class ImmutableBankingTest
    {
        public ImmutableBankingTest()
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
            int Index { get; set; }
            ICollection<IAccount> Accounts { get; set; }
        }

        [Concurrent]
        public interface IAccount : IBaseEntity
        {
            int Index { get; set; }
            ICollection<ITransaction> Transactions { get; set; }
        }

        [Concurrent]
        public interface ITransaction : IBaseEntity
        {
            IAccount From { get; set; }
            IAccount To { get; set; }
            int Amount { get; set; }
        }

        public interface IQuarterReport : IBaseEntity
        {
            int SequenceNumber { get; set; }
            [Immutable]
            ICollection<IUser> Users { get; set; }
        }

        [Concurrent]
        public interface IBankData
        {
            ICollection<IUser> Users { get; set; }
            ICollection<IQuarterReport> Reports { get; set; }
        }

        [TestMethod]
        public void TestIterrativeTransactionsRandom()
        {
            int nrThreads = 10;
            int nrRepeats = 25;
            int nrUsers = 5;
            int nrAccounts = 5;

            // Setup
            Context facade = new Context(typeof(IBankData), new Type[] { typeof(IBaseEntity), typeof(IBankData) });

            using (var ws = facade.OpenWorkspace<IBankData>(IsolationLevel.Exclusive))
            {
                ws.Data.Users = ws.New<ICollection<IUser>>();

                for (int usr = 0; usr < nrUsers; usr++)
                {
                    IUser user = ws.New<IUser>();
                    user.ID = Guid.NewGuid();
                    user.Name = "User" + usr;
                    user.Index = usr;
                    user.Accounts = ws.New<ICollection<IAccount>>();
                    ws.Data.Users.Add(user);

                    for (int acc = 0; acc < nrAccounts; acc++)
                    {
                        IAccount account = ws.New<IAccount>();
                        account.ID = Guid.NewGuid();
                        account.Index = acc;
                        account.Transactions = ws.New<ICollection<ITransaction>>();
                        user.Accounts.Add(account);
                    }
                }

                ws.Data.Reports = ws.New<ICollection<IQuarterReport>>();

                ws.Commit();
            }

            //Make modifications
            for (int i = 0; i < nrRepeats; i++)
            {
                Random r = new Random();

                Guid snapshotId = facade.LastSnapshotId();

                for (int iter = 0; iter < nrThreads; iter++)
                {
                    using (var ws = facade.OpenWorkspace<IBankData>(snapshotId, IsolationLevel.Snapshot))
                    {

                        int actual = 0;
                        foreach (var u in ws.Data.Users)
                        {
                            actual++;
                        }

                        Assert.AreEqual(nrUsers, actual);

                        int firstUserIndex = r.Next(nrUsers);
                        int secondUserIndex = r.Next(nrUsers);

                        while (firstUserIndex == secondUserIndex)
                        {
                            firstUserIndex = r.Next(nrUsers);
                            secondUserIndex = r.Next(nrUsers);
                        }


                        IUser firstUser = ws.Data.Users.First(o => o.Index.Equals(firstUserIndex));
                        IUser secondUser = ws.Data.Users.First(o => o.Index.Equals(secondUserIndex));

                        Debug.WriteLine(firstUserIndex + "->" + secondUserIndex);

                        int firstAccountIndex = r.Next(nrAccounts);
                        int secondAccountIndex = r.Next(nrAccounts);

                        IAccount firstAccount = firstUser.Accounts.First(o => o.Index.Equals(firstAccountIndex));
                        IAccount secondAccount = secondUser.Accounts.First(o => o.Index.Equals(secondAccountIndex));

                        ITransaction transaction = ws.New<ITransaction>();
                        transaction.ID = Guid.NewGuid();
                        transaction.Amount = 1;
                        transaction.From = firstAccount;
                        transaction.To = secondAccount;

                        firstAccount.Transactions.Add(transaction);
                        secondAccount.Transactions.Add(transaction);

                        ws.Commit();
                    }
                }

                using (var ws = facade.OpenWorkspace<IBankData>(IsolationLevel.Exclusive))
                {
                    var report = ws.New<IQuarterReport>();
                    report.SequenceNumber = i;
                    report.Users = ws.Data.Users;

                    ws.Data.Reports.Add(report);

                    ws.Commit();
                }
            }

            facade.Cleanup();

            using (var ws = facade.OpenWorkspace<IBankData>(IsolationLevel.ReadOnly))
            {
                for (int iter = 0; iter < nrRepeats; iter++)
                {
                    var report = ws.Data.Reports.Single(r => r.SequenceNumber == iter);

                    int transactionCount = report.Users.Sum(u => u.Accounts.Sum(a => a.Transactions.Count));
                    int reportSum = report.Users.Sum(u => u.Accounts.Sum(a => a.Transactions.Sum(t => t.From.Equals(a) ? -t.Amount : t.Amount)));

                    Assert.AreEqual((iter + 1) * nrThreads * 2, transactionCount);
                    Assert.AreEqual(0, reportSum);
                }


                int totalSum = 0;

                for (int usr = 0; usr < nrUsers; usr++)
                {
                    IUser user = ws.Data.Users.First(o => o.Index.Equals(usr));
                    Assert.AreEqual(user.Name, "User" + usr);
                    Assert.AreEqual(user.Index, usr);

                    int userSum = 0;

                    for (int acc = 0; acc < nrAccounts; acc++)
                    {
                        IAccount account = user.Accounts.First(o => o.Index.Equals(acc));
                        Assert.AreEqual(account.Index, acc);

                        int accountSum = 0;

                        foreach (ITransaction trans in account.Transactions)
                        {
                            if (!trans.From.ID.Equals(trans.To.ID))
                            {
                                if (trans.From.ID.Equals(account.ID))
                                {
                                    accountSum -= trans.Amount;
                                }
                                else
                                {
                                    if (trans.To.ID.Equals(account.ID))
                                    {
                                        accountSum += trans.Amount;
                                    }
                                    else
                                    {
                                        Assert.Fail("Transaction error");
                                    }
                                }
                            }
                            else
                            {
                                Assert.Fail("Error");
                            }

                        }

                        userSum += accountSum;
                    }

                    Debug.WriteLine("User" + usr + " = " + userSum);

                    totalSum += userSum;
                }

                Assert.AreEqual(0, totalSum);
            }

        }
    }
}
