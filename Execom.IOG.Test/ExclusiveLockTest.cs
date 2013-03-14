using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Tracking;
using System.Threading;
using System.Collections.ObjectModel;

namespace Execom.IOG.Test
{
    [TestClass]
    public class ExclusiveLockTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            int nrThreads = 33;
            int nrRepeats = 300;
            int counter = 0;
                        
            for (int j = 0; j < nrRepeats; j++)
            {
                WorkspaceExclusiveLockProvider provider = new WorkspaceExclusiveLockProvider();
                Collection<Thread> threads = new Collection<Thread>();

                for (int i = 0; i < nrThreads; i++)
                {
                    Thread t = new Thread(() =>
                    {
                        provider.EnterLockExclusive();
                        Interlocked.Increment(ref counter);

                        Assert.AreEqual(1, counter);

                        Interlocked.Decrement(ref counter);

                        provider.ExitLockExclusive();
                    });

                    threads.Add(t);
                }


                foreach (var t in threads)
                {
                    t.Start();
                }

                foreach (var t in threads)
                {
                    t.Join();
                }
            }
        }
    }
}
