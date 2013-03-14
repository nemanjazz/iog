using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Attributes;
using Execom.IOG.Types;

namespace Execom.IOG.Test.UseCases
{
    [TestClass]
    public class ScalarSetCollectionDataModel
    {
        private Context ctx = null;
        private DateTime dateTimeNow = DateTime.Now;        

        public interface IDatabase
        {
            IScalarSet<int> Integers { get; set; }
            IScalarSet<double> Doubles { get; set; }
            IScalarSet<DateTime> DateTimes { get; set; }
        }

        [TestInitialize]
        public void SetUp()
        {
            ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                database.Integers = ws.New<IScalarSet<int>>();
                for (int i = 0; i < 20; i ++)
                {
                    database.Integers.Add(i);
                }

                database.Doubles = ws.New<IScalarSet<double>>();
                for (double i = 0; i < 10; i += 0.2)
                {
                    database.Doubles.Add(i);
                }

                database.DateTimes = ws.New<IScalarSet<DateTime>>();
                database.DateTimes.Add(dateTimeNow);

                int num = database.Integers.Single(n => n == 10);

                ws.Commit();
            }
        }

        [TestMethod]
        public void TestScalarData()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                int integer;
                for (int i = 0; i < 20; i++)
                {
                    Assert.IsTrue(database.Integers.Contains(i));
                    integer = database.Integers.Single(intNum => intNum == i);
                    Assert.IsNotNull(integer);
                    database.Integers.Remove(i);
                }

                Assert.AreEqual(0, database.Integers.Count);

                double doubleNum;
                for (double i = 0; i < 10; i += 0.2)
                {
                    Assert.IsTrue(database.Doubles.Contains(i));
                    doubleNum = database.Doubles.Single(dNum => dNum == i);
                    Assert.IsNotNull(doubleNum);
                    database.Doubles.Remove(i);
                }

                Assert.AreEqual(0, database.Doubles.Count);

                Assert.AreEqual(dateTimeNow.Date, ws.Data.DateTimes.First().Date);

                ws.Commit();
            }            
        }        
    }
}
