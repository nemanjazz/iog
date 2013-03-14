using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Storage;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace Execom.IOG.Test
{    
    [TestClass]
    public class ContextTest
    {
        public interface IPerson
        {
            string Name { get; set; }
            int Age { get; set; }
        }

        public interface ISkilledPerson : IPerson
        {
            int Skill { get; set; }
        }

        public enum Enumeration
        {
            None,
            First,
            Second
        }

        [Flags]
        public enum FlagEnum
        {
            None,
            HasFirst,
            HasSecond
        }

        public interface IDatabase
        {
            int Value { get; set; }
            bool BoolProp { get; set; }
            Enumeration EnumProp { get; set; }
            FlagEnum FlagProp { get; set; }
            IPerson Person { get; set; }
        }

        [TestMethod]
        public void BasicEntityTest()
        {            
            Context ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                var person = ws.New<IPerson>();

                database.Value = 1;
                database.BoolProp = true;
                database.Person = person;
                database.EnumProp = Enumeration.Second;
                database.FlagProp = FlagEnum.HasFirst | FlagEnum.HasSecond;
                person.Age = 25;
                person.Name = "John Connor";

                Assert.AreEqual(1, database.Value);
                Assert.AreEqual(true, database.BoolProp);
                Assert.AreEqual(Enumeration.Second, database.EnumProp);
                Assert.AreEqual(FlagEnum.HasFirst | FlagEnum.HasSecond, database.FlagProp);
                Assert.AreEqual(25, database.Person.Age);
                Assert.AreEqual("John Connor", database.Person.Name);

                ws.Commit();                
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;
                Assert.AreEqual(1, database.Value);
                Assert.AreEqual(true, database.BoolProp);
                Assert.AreEqual(Enumeration.Second, database.EnumProp);
                Assert.AreEqual(FlagEnum.HasFirst | FlagEnum.HasSecond, database.FlagProp);
                Assert.AreEqual(25, database.Person.Age);
                Assert.AreEqual("John Connor", database.Person.Name);
            }
        }

        [TestMethod]
        public void BasicStoredEntityTest()
        {
            var fs = new FileStream("data.dat", FileMode.Create);
            Context ctx = new Context(typeof(IDatabase), null, new IndexedFileStorage(fs, 256, true));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                var person = ws.New<IPerson>();
                person.Age = 25;
                person.Name = "John Connor";
                
                database.Value = 1;
                database.BoolProp = true;
                database.Person = person;
                database.EnumProp = Enumeration.Second;
                database.FlagProp = FlagEnum.HasFirst | FlagEnum.HasSecond;                
                
                Assert.AreEqual(1, database.Value);
                Assert.AreEqual(true, database.BoolProp);
                Assert.AreEqual(Enumeration.Second, database.EnumProp);
                Assert.AreEqual(FlagEnum.HasFirst | FlagEnum.HasSecond, database.FlagProp);
                Assert.AreEqual(25, database.Person.Age);
                Assert.AreEqual("John Connor", database.Person.Name);
            }
            
            fs.Close();
        }

        [TestMethod]
        public void BasicIndexedFileStorageTest()
        {
            // Initialize new IOG context
            var fs = new FileStream("data.dat", FileMode.Create);
            Context ctx = new Context(typeof(IDatabase), null, new IndexedFileStorage(fs, 256, true));
            fs.Close();

            // Reopen the IOG context
            fs = new FileStream("data.dat", FileMode.Open);
            Context ctx2 = new Context(typeof(IDatabase), null, new IndexedFileStorage(fs, 256, true));
            fs.Close();
        }

        [TestMethod]
        public void BasicEntityTestNull()
        {
            Context ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                var person = ws.New<IPerson>();
                
                Assert.AreEqual(null, database.Person);

                database.Person = person;
                Assert.AreEqual(person, database.Person);
                Assert.AreEqual(person.GetHashCode(), database.Person.GetHashCode());

                database.Person = null;
                Assert.AreEqual(null, database.Person);                
            }
        }

        [TestMethod]
        public void BasicInheritanceTest()
        {
            Context ctx = new Context(typeof(IDatabase), new Type[] { typeof(IDatabase), typeof(ISkilledPerson), typeof(IPerson)});

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                ISkilledPerson person = ws.New<ISkilledPerson>();

                database.Value = 1;
                database.Person = person;
                person.Age = 25;
                person.Name = "John Connor";
                person.Skill = 2;

                Assert.AreEqual(1, database.Value);
                Assert.AreEqual(25, database.Person.Age);
                Assert.AreEqual("John Connor", database.Person.Name);
                Assert.AreEqual(2, (database.Person as ISkilledPerson).Skill);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public void BasicTimeoutTest()
        {
            Context ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive, TimeSpan.FromSeconds(1)))
            {
                Thread.Sleep(1100);

                IDatabase database = ws.Data;
                var person = ws.New<IPerson>();
                
                person.Age = 25;
                person.Name = "John Connor";

                database.Person = person;                
                
                Assert.AreEqual(25, database.Person.Age);
                Assert.AreEqual("John Connor", database.Person.Name);

                ws.Commit();
            }            
        }
    }
}
