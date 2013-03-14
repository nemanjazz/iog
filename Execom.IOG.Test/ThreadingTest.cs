using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Collections.ObjectModel;

namespace Execom.IOG.Test
{
    [TestClass]
    public class ThreadingTest
    {
        public interface IDataModel
        {
            int Counter { get; set; }
        }

        public ThreadingTest()
        {
        }
        
        [TestMethod]
        public void TestExclusiveCounter()
        {
            int nrThreads = 30;

            var ctx = new Context(typeof(IDataModel));

            Collection<Thread> threads = new Collection<Thread>();

            // Start up threads in exclusive mode
            for (int i = 0; i < nrThreads; i++)
            {
                Thread t = new Thread(() => {
                    using (var ws = ctx.OpenWorkspace<IDataModel>(IsolationLevel.Exclusive))
                    {
                        ws.Data.Counter = ws.Data.Counter + 1;
                        ws.Commit();
                    }
                });

                t.Start();

                threads.Add(t);
            }

            // Wait for completion
            foreach (var thread in threads)
            {
                thread.Join();
            }

            using (var ws = ctx.OpenWorkspace<IDataModel>(IsolationLevel.ReadOnly))
            {
                Assert.AreEqual(nrThreads, ws.Data.Counter);
            }
        }        
    }
}
