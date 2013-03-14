// -----------------------------------------------------------------------
// <copyright file="IndexedFileStorage.cs" company="Microsoft">
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

namespace Execom.IOG.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Execom.IOG.IndexedFile;
    using System.Threading;
    using System.IO;
    using System.Collections;

    /// <summary>
    /// Represents a key-value storage over an indexed file
    /// </summary>
    /// <author>Nenad Sabo</author>
    public class IndexedFileStorage : IKeyValueStorage<Guid, object>, IForceUpdateStorage, ISerializingStorage, IDisposable
    {
        private IndexedFile file;
        private IObjectSerializer serializer;
        private object sync = new object();
        private MemoryStream memory;
        private BinaryReader reader;
        private BinaryWriter writer;

        public IndexedFileStorage(Stream stream, int clusterSize, bool safeWrites, string header)
        {
            this.file = new IndexedFile(stream, clusterSize, safeWrites, header);
            this.memory = new MemoryStream();
            this.reader = new BinaryReader(memory);
            this.writer = new BinaryWriter(memory);
        }    

        public IndexedFileStorage(Stream stream, int clusterSize, bool safeWrites)
        {
            this.file = new IndexedFile(stream, clusterSize, safeWrites);
            this.memory = new MemoryStream();
            this.reader = new BinaryReader(memory);
            this.writer = new BinaryWriter(memory);
        }        

        public bool Remove(Guid key)
        {
            lock (sync)
            {
                return file.Delete(key);
            }
        }

        public bool AddOrUpdate(Guid key, object value)
        {
            lock (sync)
            {
                memory.Position = 0;
                serializer.Serialize(value, writer);
                file.Write(key, memory.GetBuffer(), 0, (int)memory.Position);
                return true;
            }
        }

        public bool Contains(Guid key)
        {
            lock (sync)
            {
                return file.Contains(key);
            }
        }

        public object Value(Guid key)
        {
            lock (sync)
            {
                if (!Contains(key))
                {
                    return null;
                }

                memory.Position = 0;
                byte[] buffer = file.Read(key);
                memory.Write(buffer, 0, buffer.Length); // TODO (nsabo) See how to overcome this step

                memory.Position = 0;
                return serializer.Deserialize(reader);
            }
        }
        
        public IObjectSerializer Serializer
        {
            get
            {
                return serializer;
            }
            set
            {
                serializer = value;
            }
        }

        public string FragmentationReport()
        {
            lock (sync)
            {
                return file.FragmentationReport();
            }
        }

        /// <summary>
        /// Performs clean file shutdown
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (sync)
            {
                if (file != null)
                {
                    file.Dispose();
                    file = null;
                }

                if (reader != null)
                {
                    reader.Close();
                    reader = null;
                }

                if (writer != null)
                {
                    writer.Close();
                    writer = null;
                }

                if (memory != null)
                {
                    memory.Dispose();
                    memory = null;
                }
            }
        }


        public IEnumerable ListKeys()
        {
            return file.ListKeys();
        }


        public void Clear()
        {
            lock (sync)
            {
                file.Clear();
            }
        }
    }
}
