using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Attributes;
using Execom.IOG.Storage;

namespace Execom.IOG.Test
{
    [TestClass]
    public class GarbageCollectionTest
    {
        public interface ISubObject
        {
            string Name { get; set; }
        }

        public interface IPerson
        {            
            string Name { get; set; }
            int Age { get; set; }
            ISubObject Sub { get; set; }
        }

        public interface IParameter
        {
            string Name { get; set; }
            [Immutable]
            IPerson Person { get; set; }
            int Parameter { get; set; }
        }

        public interface IDatabase
        {
            ICollection<IParameter> ParameterCollection { get; set; }
            ICollection<IPerson> PersonCollection { get; set; }            
        }

        [TestMethod]
        public void TestGCPermanent()
        {

            Context ctx = new Context(typeof(IDatabase));
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                ws.Data.PersonCollection = ws.New<ICollection<IPerson>>();
                ws.Data.ParameterCollection = ws.New<ICollection<IParameter>>();
                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {

                IPerson p = ws.New<IPerson>();
                p.Name = "Name";
                p.Age = 33;
                p.Sub = ws.New<ISubObject>();
                ws.Data.PersonCollection.Add(p);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {

                IParameter p = ws.New<IParameter>();
                p.Name = "Parameter";
                p.Person = ws.Data.PersonCollection.First();
                ws.Data.ParameterCollection.Add(p);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                ws.Data.PersonCollection.First().Sub.Name = "Changed";
                ws.Commit();
            }

            ctx.Cleanup();

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                ws.Data.ParameterCollection.First().Parameter = 100;
                ws.Commit();
            }

            ctx.Cleanup();

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                Assert.AreEqual(ws.Data.ParameterCollection.First().Person.Sub.Name, "");
                Assert.AreEqual(ws.Data.PersonCollection.First().Sub.Name, "Changed");
            }

            ctx.Backup(new MemoryStorageUnsafe<Guid, object>());
        }
    }
}
