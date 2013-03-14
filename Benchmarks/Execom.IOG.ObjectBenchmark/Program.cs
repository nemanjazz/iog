using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Execom.IOG.Storage;
using Execom.IOG.Attributes;
using Execom.IOG.Types;

namespace Execom.IOG.ObjectBenchmark
{
    public interface IPerson
    {
        [PrimaryKey]
        int ID { get; set; }
        string Name { get; set; }
        string Address { get; set; }
        int Age { get; set; }
    }

    public interface IDataModel
    {
        IIndexedCollection<IPerson> PersonCollection { get; set; }
    }

    class Program
    {
        private static int batchSize = 1;
        private static int reportingFrequency = 1000;
        private static int maxElements = 5000;
        private static long startMemory = 0;
        private static int ID = 0;

        static void Main(string[] args)
        {
            Console.WriteLine("Items,Inserts per second,Reads per second,Updates per second, Memory(MB)");

            var fs = new FileStream("data.dat", FileMode.Create);
            Context ctx = new Context(typeof(IDataModel), null, new IndexedFileStorage(fs, 512, true));
            //Context<IDataModel> ctx = new Context<IDataModel>(new Type[] { typeof(IDataModel) });

            using (var ws = ctx.OpenWorkspace<IDataModel>(IsolationLevel.Exclusive))
            {
                if (ws.Data.PersonCollection == null)
                {
                    ws.Data.PersonCollection = ws.New<IIndexedCollection<IPerson>>();
                    ws.Commit();
                }
            }

            

            Stopwatch sw = new Stopwatch();
            sw.Start();

            double sumInsertMs = 0;
            double sumReadMs = 0;
            double sumUpdateMs = 0;

            for (int iter = 0; iter < maxElements / batchSize; iter++)
            {

                sumInsertMs += TestInsert(sw, ctx);
                sumReadMs += TestRead(sw, ctx);
                sumUpdateMs += TestUpdate(sw, ctx);

                long memoryStatus = GC.GetTotalMemory(false) - startMemory;

                if ((iter) % reportingFrequency == 0)
                {
                    Console.WriteLine((Program.ID).ToString()
                        + "," + (1000 / (sumInsertMs / reportingFrequency))
                        + "," + (1000 / (sumReadMs / reportingFrequency))
                        + "," + (1000 / (sumUpdateMs / reportingFrequency))
                        + "," + memoryStatus / 1024 / 1024
                        );

                    sumInsertMs = 0;
                    sumReadMs = 0;
                    sumUpdateMs = 0;

                    ctx.Cleanup();
                }
            }

            ctx.Dispose();
        }

        private static double TestInsert(Stopwatch sw, Context ctx)
        {
            var startTicks = sw.ElapsedTicks;


            using (var workspace = ctx.OpenWorkspace<IDataModel>(IsolationLevel.Exclusive))
            {
                for (int i = 0; i < batchSize; i++)
                {
                    var person = workspace.New<IPerson>();                    
                    person.ID = Program.ID;
                    Program.ID++;
                    person.Name = "Nenad Sabo";
                    person.Address = "Some street in Novi Sad";
                    person.Age = 32;

                    workspace.Data.PersonCollection.Add(person);                    
                }

                workspace.Commit();
            }

            return (double)1000 * ((double)sw.ElapsedTicks - startTicks) / Stopwatch.Frequency;
        }

        private static double TestRead(Stopwatch sw, Context ctx)
        {
            var startTicks = sw.ElapsedTicks;
            Random rnd = new Random();

            using (var workspace = ctx.OpenWorkspace<IDataModel>(IsolationLevel.ReadOnly))
            {
                long sum = 0;
                for (int i = 0; i < batchSize; i++)
                {
                    int key = rnd.Next(Program.ID);
                    var item = workspace.Data.PersonCollection.FindByPrimaryKey(key);
                    sum += item.Age;
                }
            }

            return (double)1000 * ((double)sw.ElapsedTicks - startTicks) / Stopwatch.Frequency;
        }

        private static double TestUpdate(Stopwatch sw, Context ctx)
        {
           var startTicks = sw.ElapsedTicks;

           using (var workspace = ctx.OpenWorkspace<IDataModel>(IsolationLevel.Exclusive))
            {
                int i = 0;
                foreach (var item in workspace.Data.PersonCollection)
                {
                    item.Age++;

                    i++;
                    if (i == batchSize)
                    {
                        break;
                    }
                }                

                workspace.Commit();
            }

            return (double)1000 * ((double)sw.ElapsedTicks - startTicks) / Stopwatch.Frequency;
        }
        
    }
}
