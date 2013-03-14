using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Attributes;

namespace Execom.IOG.Test.UseCases
{
    [TestClass]
    public class ScalarCollectionDataModel
    {
        private Context ctx = null;
        private DateTime dateTimeNow = DateTime.Now;

        public interface IIntHolder
        {
            int Number { get; set; }
        }

        public interface IDoubleHolder
        {
            double ParentDouble { get; set; }
            ICollection<Double> ChildDoubles { get; set; }
        }

        public interface IDatabase
        {
            [Immutable]
            ICollection<Int32> Integers { get; set; }
            ICollection<Double> Doubles { get; set; }
            ICollection<DateTime> DateTimes { get; set; }
            IIntHolder IntHolder { get; set; }
            ICollection<IDoubleHolder> DoubleHolders { get; set; }
        }

        [TestInitialize]
        public void SetUp()
        {
            ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                ICollection<Int32> tempIntegers = ws.New<ICollection<Int32>>();
                for (int i = 0; i < 20; i++)
                {
                    tempIntegers.Add(i);
                }
                database.Integers = tempIntegers;


                database.DoubleHolders = ws.New<ICollection<IDoubleHolder>>();
                IDoubleHolder dobuleHolder = ws.New<IDoubleHolder>();
                dobuleHolder.ParentDouble = 666;
                database.Doubles = ws.New<ICollection<Double>>();
                for (double i = 0; i < 10; i += 0.2)
                {
                    database.Doubles.Add(i);
                }

                dobuleHolder.ChildDoubles = database.Doubles;
                database.DoubleHolders.Add(dobuleHolder);

                database.DateTimes = ws.New<ICollection<DateTime>>();
                for (int i = 0; i < 10; i++)
                {
                    database.DateTimes.Add(dateTimeNow);
                }

                int num = database.Integers.Single(n => n == 10);
                IIntHolder intHolder = ws.New<IIntHolder>();
                intHolder.Number = num;
                database.IntHolder = intHolder;

                ws.Commit();
            }
        }

        [TestMethod]
        public void TestReadData()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                int integer;
                for (int i = 0; i < 20; i++)
                {
                    integer = database.Integers.Single(intNum => intNum == i);
                    Assert.IsNotNull(integer);
                }

                IDoubleHolder doubleHolder = database.DoubleHolders.Single(dh => dh.ParentDouble == 666);
                Assert.IsNotNull(doubleHolder);

                double doubleNum;
                double doubleHolderNum;
                for (double i = 0; i < 10; i += 0.2)
                {
                    doubleNum = database.Doubles.Single(dNum => dNum == i);
                    Assert.IsNotNull(doubleNum);
                    doubleHolderNum = doubleHolder.ChildDoubles.Single(dhn => dhn == i);
                    Assert.IsNotNull(doubleHolderNum);
                    Assert.AreEqual(doubleNum, doubleHolderNum);
                }

                foreach (var dateTime in database.DateTimes)
                {
                    Assert.AreEqual(dateTimeNow.Date, dateTime.Date);
                }

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                int integer;
                for (int i = 0; i < 20; i++)
                {
                    integer = database.Integers.Single(intNum => intNum == i);
                    Assert.IsNotNull(integer);
                }

                IDoubleHolder doubleHolder = database.DoubleHolders.Single(dh => dh.ParentDouble == 666);
                Assert.IsNotNull(doubleHolder);

                double doubleNum;
                double doubleHolderNum;
                for (double i = 0; i < 10; i += 0.2)
                {
                    doubleNum = database.Doubles.Single(dNum => dNum == i);
                    Assert.IsNotNull(doubleNum);
                    doubleHolderNum = doubleHolder.ChildDoubles.Single(dhn => dhn == i);
                    Assert.IsNotNull(doubleHolderNum);
                    Assert.AreEqual(doubleNum, doubleHolderNum);
                }

                foreach (var dateTime in database.DateTimes)
                {
                    Assert.AreEqual(dateTimeNow.Date, dateTime.Date);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.InvalidOperationException))]
        public void TestModificationData()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                int integer;
                for (int i = 0; i < 20; i++)
                {
                    integer = database.Integers.Single(intNum => intNum == i);
                    Assert.IsNotNull(integer);
                }

                int num1 = 100;
                int num2 = 500;

                Assert.IsNull(database.Integers.Single(intNum => intNum == num1));
                Assert.IsNull(database.Integers.Single(intNum => intNum == num2));

                database.Integers.Add(num1);
                database.Integers.Add(num2);


                IDoubleHolder doubleHolder = database.DoubleHolders.Single(dh => dh.ParentDouble == 666);
                Assert.IsNotNull(doubleHolder);

                double doubleNum;
                double doubleHolderNum;
                for (double i = 0; i < 10; i += 0.2)
                {
                    doubleNum = database.Doubles.Single(dNum => dNum == i);
                    Assert.IsNotNull(doubleNum);
                    doubleHolderNum = doubleHolder.ChildDoubles.Single(dhn => dhn == i);
                    Assert.IsNotNull(doubleHolderNum);
                    Assert.AreEqual(doubleNum, doubleHolderNum);
                }

                double doubleNum1 = 100.05;
                double doubleNum2 = 500.23;

                Assert.IsNull(database.Doubles.Single(dNum => dNum == doubleNum1));
                Assert.IsNull(database.Doubles.Single(dNum => dNum == doubleNum2));

                Assert.IsNull(doubleHolder.ChildDoubles.Single(dhn => dhn == doubleNum1));
                Assert.IsNull(doubleHolder.ChildDoubles.Single(dhn => dhn == doubleNum2));

                database.Doubles.Add(doubleNum1);
                database.Doubles.Add(doubleNum2);

                doubleHolder.ChildDoubles.Add(doubleNum1);
                doubleHolder.ChildDoubles.Add(doubleNum2);

                foreach (var dateTime in database.DateTimes)
                {
                    Assert.AreEqual(dateTimeNow.Date, dateTime.Date);
                }

                DateTime dateTime1 = DateTime.Now;
                dateTime1.AddDays(100);
                DateTime dateTime2 = DateTime.Now;
                dateTime2.AddDays(200);

                Assert.IsNull(database.DateTimes.Single(dt => dt.Date.Equals(dateTime1.Date)));
                Assert.IsNull(database.DateTimes.Single(dt => dt.Date.Equals(dateTime2.Date)));

                database.DateTimes.Add(dateTime1);
                database.DateTimes.Add(dateTime2);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                Assert.IsNotNull(database.Integers.Single(intNum => intNum == 100));
                Assert.IsNotNull(database.Integers.Single(intNum => intNum == 200));

                Assert.IsNotNull(database.Doubles.Single(dNum => dNum == 100.05));
                Assert.IsNotNull(database.Doubles.Single(dNum => dNum == 500.23));

                IDoubleHolder doubleHolder = database.DoubleHolders.Single(dh => dh.ParentDouble == 666);
                Assert.IsNotNull(doubleHolder);

                Assert.IsNotNull(doubleHolder.ChildDoubles.Single(dhn => dhn == 100.05));
                Assert.IsNotNull(doubleHolder.ChildDoubles.Single(dhn => dhn == 500.23));

                DateTime dateTime1 = DateTime.Now;
                dateTime1.AddDays(100);
                DateTime dateTime2 = DateTime.Now;
                dateTime2.AddDays(200);

                Assert.IsNotNull(database.DateTimes.Single(dt => dt.Date.Equals(dateTime1.Date)));
                Assert.IsNotNull(database.DateTimes.Single(dt => dt.Date.Equals(dateTime2.Date)));
            }
        }


        [TestMethod]
        [ExpectedException(typeof(System.InvalidOperationException))]
        public void TestModifAndRollback()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                int integer;
                for (int i = 0; i < 20; i++)
                {
                    integer = database.Integers.Single(intNum => intNum == i);
                    Assert.IsNotNull(integer);
                }

                int num1 = 100;
                int num2 = 500;

                database.Integers.Add(num1);
                database.Integers.Add(num2);

                IDoubleHolder doubleHolder = database.DoubleHolders.Single(dh => dh.ParentDouble == 666);
                Assert.IsNotNull(doubleHolder);

                double doubleNum;
                double doubleHolderNum;
                for (double i = 0; i < 10; i += 0.2)
                {
                    doubleNum = database.Doubles.Single(dNum => dNum == i);
                    Assert.IsNotNull(doubleNum);
                    doubleHolderNum = doubleHolder.ChildDoubles.Single(dhn => dhn == i);
                    Assert.IsNotNull(doubleHolderNum);
                    Assert.AreEqual(doubleNum, doubleHolderNum);
                }

                double doubleNum1 = 100.05;
                double doubleNum2 = 500.23;

                database.Doubles.Add(doubleNum1);
                database.Doubles.Add(doubleNum2);

                doubleHolder.ChildDoubles.Add(doubleNum1);
                doubleHolder.ChildDoubles.Add(doubleNum2);

                Assert.IsNotNull(database.Integers.Single(intNum => intNum == 100));
                Assert.IsNotNull(database.Integers.Single(intNum => intNum == 200));

                Assert.IsNotNull(database.Doubles.Single(dNum => dNum == 100.05));
                Assert.IsNotNull(database.Doubles.Single(dNum => dNum == 500.23));

                Assert.IsNotNull(doubleHolder.ChildDoubles.Single(dhn => dhn == 100.05));
                Assert.IsNotNull(doubleHolder.ChildDoubles.Single(dhn => dhn == 500.23));

                ws.Rollback();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                int integer;
                for (int i = 0; i < 20; i++)
                {
                    integer = database.Integers.Single(intNum => intNum == i);
                    Assert.IsNotNull(integer);
                }

                IDoubleHolder doubleHolder = database.DoubleHolders.Single(dh => dh.ParentDouble == 666);
                Assert.IsNotNull(doubleHolder);

                double doubleNum;
                double doubleHolderNum;
                for (double i = 0; i < 10; i += 0.2)
                {
                    doubleNum = database.Doubles.Single(dNum => dNum == i);
                    Assert.IsNotNull(doubleNum);
                    doubleHolderNum = doubleHolder.ChildDoubles.Single(dhn => dhn == i);
                    Assert.IsNotNull(doubleHolderNum);
                    Assert.AreEqual(doubleNum, doubleHolderNum);
                }

                Assert.IsNull(database.Integers.Single(intNum => intNum == 100));
                Assert.IsNull(database.Integers.Single(intNum => intNum == 200));

                Assert.IsNull(database.Doubles.Single(dNum => dNum == 100.05));
                Assert.IsNull(database.Doubles.Single(dNum => dNum == 500.23));

                Assert.IsNull(doubleHolder.ChildDoubles.Single(dhn => dhn == 100.05));
                Assert.IsNull(doubleHolder.ChildDoubles.Single(dhn => dhn == 500.23));
            }
        }

        [TestMethod]
        public void TestCollectionContains()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                for (int i = 0; i < 20; i++)
                {
                    Assert.IsTrue(database.Integers.Contains(i));
                }

                Assert.IsFalse(database.Integers.Contains(100));
                Assert.IsFalse(database.Integers.Contains(500));

                ICollection<Int32> tempIntegers = ws.New<ICollection<Int32>>();
                for (int i = 0; i < 20; i++)
                {
                    tempIntegers.Add(i);
                }

                tempIntegers.Add(100);
                tempIntegers.Add(500);

                database.Integers = tempIntegers;

                Assert.IsTrue(database.Integers.Contains(100));
                Assert.IsTrue(database.Integers.Contains(500));

                IDoubleHolder doubleHolder = database.DoubleHolders.Single(dh => dh.ParentDouble == 666);
                Assert.IsNotNull(doubleHolder);

                for (double i = 0; i < 10; i += 0.2)
                {
                    Assert.IsTrue(database.Doubles.Contains(i));
                    Assert.IsTrue(doubleHolder.ChildDoubles.Contains(i));
                }

                Assert.IsFalse(database.Doubles.Contains(100.05));
                Assert.IsFalse(database.Doubles.Contains(500.23));

                Assert.IsFalse(doubleHolder.ChildDoubles.Contains(100.05));
                Assert.IsFalse(doubleHolder.ChildDoubles.Contains(500.23));

                ICollection<Double> tempDoubles = ws.New<ICollection<Double>>();
                doubleHolder.ChildDoubles = ws.New<ICollection<Double>>();
                for (double i = 0; i < 10; i += 0.2)
                {
                    tempDoubles.Add(i);
                    doubleHolder.ChildDoubles.Add(i);
                }

                tempDoubles.Add(100.05);
                tempDoubles.Add(500.23);

                doubleHolder.ChildDoubles.Add(100.05);
                doubleHolder.ChildDoubles.Add(500.23);

                database.Doubles = tempDoubles;

                Assert.IsTrue(database.Doubles.Contains(100.05));
                Assert.IsTrue(database.Doubles.Contains(500.23));

                Assert.IsTrue(doubleHolder.ChildDoubles.Contains(100.05));
                Assert.IsTrue(doubleHolder.ChildDoubles.Contains(500.23));

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                Assert.IsTrue(database.Integers.Contains(100));
                Assert.IsTrue(database.Integers.Contains(500));

                Assert.IsTrue(database.Doubles.Contains(100.05));
                Assert.IsTrue(database.Doubles.Contains(500.23));

                IDoubleHolder doubleHolder = database.DoubleHolders.Single(dh => dh.ParentDouble == 666);
                Assert.IsNotNull(doubleHolder);

                Assert.IsTrue(doubleHolder.ChildDoubles.Contains(100.05));
                Assert.IsTrue(doubleHolder.ChildDoubles.Contains(500.23));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.InvalidOperationException))]
        public void TestCollectionClear()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                int integer;
                for (int i = 0; i < 20; i++)
                {
                    integer = database.Integers.Single(intNum => intNum == i);
                    Assert.IsNotNull(integer);
                }

                ICollection<Int32> tmpInteger = ws.New<ICollection<Int32>>();

                database.Integers = tmpInteger;

                Assert.AreEqual(0, database.Integers.Count);

                Assert.IsNotNull(database.IntHolder.Number);
                Assert.AreEqual(10, database.IntHolder.Number);

                IDoubleHolder doubleHolder = database.DoubleHolders.Single(dh => dh.ParentDouble == 666);
                Assert.IsNotNull(doubleHolder);

                Assert.AreEqual(51, doubleHolder.ChildDoubles.Count);

                doubleHolder.ChildDoubles.Clear();

                Assert.AreEqual(0, doubleHolder.ChildDoubles.Count);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                Assert.AreNotEqual(20, database.Integers.Count);
                Assert.AreEqual(0, database.Integers.Count);

                Assert.IsNull(database.Integers.Single(intNum => intNum == 10));

                Assert.AreEqual(10, database.IntHolder.Number);

                IDoubleHolder doubleHolder = database.DoubleHolders.Single(dh => dh.ParentDouble == 666);
                Assert.IsNotNull(doubleHolder);

                Assert.AreNotEqual(51, doubleHolder.ChildDoubles.Count);
                Assert.AreEqual(0, doubleHolder.ChildDoubles.Count);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.InvalidOperationException))]
        public void TestCollectionRemove()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                int integer;
                for (int i = 0; i < 20; i++)
                {
                    integer = database.Integers.Single(intNum => intNum == i);
                    Assert.AreEqual(i, integer);
                }

                Assert.IsTrue(database.Integers.Remove(4));

                Assert.AreEqual(19, database.Integers.Count);

                IDoubleHolder doubleHolder = database.DoubleHolders.Single(dh => dh.ParentDouble == 666);
                Assert.IsNotNull(doubleHolder);

                for (double i = 0; i < 10; i += 0.2)
                {
                    Assert.IsTrue(doubleHolder.ChildDoubles.Contains(i));
                }

                Assert.IsTrue(doubleHolder.ChildDoubles.Remove(0.6));
                Assert.AreEqual(50, doubleHolder.ChildDoubles.Count);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                Assert.AreEqual(19, database.Integers.Count);

                Assert.IsTrue(database.Integers.Contains(4));

                Assert.AreEqual(4, database.IntHolder.Number);

                IDoubleHolder doubleHolder = database.DoubleHolders.Single(dh => dh.ParentDouble == 666);
                Assert.IsNotNull(doubleHolder);

                Assert.IsFalse(doubleHolder.ChildDoubles.Contains(0.2));
                Assert.AreEqual(50, doubleHolder.ChildDoubles.Count);
            }
        }
    }
}
