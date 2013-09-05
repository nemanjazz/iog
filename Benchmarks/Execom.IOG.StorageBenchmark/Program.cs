// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Execom">
// Copyright 2011 EXECOM d.o.o
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
// -----------------------------------------------------------------------

namespace Execom.IOG.StorageBenchmark
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Execom.IOG.Storage;
    using System.IO;
    using System.Diagnostics;
    using System.Collections.ObjectModel;
    using Execom.IOG.Services.Data;
    using Execom.IOG.MongoStorage;

    /// <summary>
    /// Console application to test the various storages performance
    /// </summary>
    class Program
    {
        private static int reportingFrequency = 1000;
        private static int maxElements = 5000 * reportingFrequency; // In reporting frequency
        private static long startMemory = 0;

        private static byte[] testBuffer = new byte[122];

        static void Main(string[] args)
        {
            startMemory = GC.GetTotalMemory(false);           
            //var storage = new IndexedFileStorage(new FileStream("data.dat", FileMode.OpenOrCreate), 256, true);
            var storage = new MongoStorage("mongodb://ws014:27017/test", "test", "test");
            storage.Serializer = new ObjectSerializationService();
            TestStorage(storage, maxElements);
            //storage.Dispose();
            //TestStorage(new MemoryStorage<Guid, object>(), maxElements);
        }

        private static void TestStorage(IKeyValueStorage<Guid, object> storage, int maxElements)
        {
            Console.WriteLine("Testing " + storage.GetType().Name);
            Console.WriteLine("Items,Avg insert (ms),Inserts per second,Avg read (ms),Reads per second,Avg update (ms),Updates per second, Memory(MB)");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int iter = 0; iter < maxElements / reportingFrequency; iter++)
            {
                Collection<Guid> ids = new Collection<Guid>();
                for (int inner = 0; inner < reportingFrequency; inner++)
                {
                    Guid id = Guid.NewGuid();
                    ids.Add(id);
                }

                var avgInsertMs = TestInsert(storage, sw, ids);
                var avgReadMs = TestRead(storage, sw, ids);
                var avgUpdateMs = TestUpdate(storage, sw, ids);

                long memoryStatus = GC.GetTotalMemory(false) - startMemory;

                Console.WriteLine(((iter + 1) * reportingFrequency).ToString()
                    + "," + avgInsertMs.ToString() + "," + 1000 / avgInsertMs
                    + "," + avgReadMs.ToString() + "," + 1000 / avgReadMs
                    + "," + avgUpdateMs.ToString() + "," + 1000 / avgUpdateMs
                    + "," + memoryStatus / 1024 / 1024
                    );
            }
        }

        private static double TestInsert(IKeyValueStorage<Guid, object> storage, Stopwatch sw, Collection<Guid> ids)
        {
            var startTicks = sw.ElapsedTicks;

            for (int inner = 0; inner < reportingFrequency; inner++)
            {
                storage.AddOrUpdate(ids[inner], testBuffer);
            }

            return (double)1000 * (sw.ElapsedTicks - startTicks) / Stopwatch.Frequency / reportingFrequency;
        }

        private static double TestRead(IKeyValueStorage<Guid, object> storage, Stopwatch sw, Collection<Guid> ids)
        {
            var startTicks = sw.ElapsedTicks;

            for (int inner = 0; inner < reportingFrequency; inner++)
            {                
                storage.Value(ids[inner]);                               
            }

            return (double)1000 * (sw.ElapsedTicks - startTicks) / Stopwatch.Frequency / reportingFrequency;
        }

        private static double TestUpdate(IKeyValueStorage<Guid, object> storage, Stopwatch sw, Collection<Guid> ids)
        {
            var startTicks = sw.ElapsedTicks;

            for (int inner = 0; inner < reportingFrequency; inner++)
            {
                storage.AddOrUpdate(ids[inner], testBuffer);                
            }

            return (double)1000 * (sw.ElapsedTicks - startTicks) / Stopwatch.Frequency / reportingFrequency;
        }
    }
}
