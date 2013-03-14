// -----------------------------------------------------------------------
// <copyright file="MemoryStorage.cs" company="Execom">
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
    using System.Threading;
    using System.Collections;

    [Serializable]
    public class MemoryStorageSafe<TKey, TValue> : IKeyValueStorage<TKey, TValue>
    {
        private Dictionary<TKey, TValue> dictionary;

        [NonSerialized]
        private ReaderWriterLock rwLock;

        public MemoryStorageSafe()
        {
            dictionary = new Dictionary<TKey, TValue>();
            rwLock = new ReaderWriterLock();
        }        

        public bool Remove(TKey key)
        {
            rwLock.AcquireWriterLock(-1);
            try
            {
                return dictionary.Remove(key);
            }
            finally
            {
                rwLock.ReleaseLock();
            }
        }

        public bool Contains(TKey key)
        {
            rwLock.AcquireReaderLock(-1);
            try
            {
                return dictionary.ContainsKey(key);
            }
            finally
            {
                rwLock.ReleaseLock();
            }
        }

        public TValue Value(TKey key)
        {
            rwLock.AcquireReaderLock(-1);
            try
            {
                TValue value;
                dictionary.TryGetValue(key, out value);
                return value;
            }
            finally
            {
                rwLock.ReleaseLock();
            }
        }

        public bool AddOrUpdate(TKey key, TValue value)
        {
            rwLock.AcquireWriterLock(-1);
            try
            {
                if (dictionary.ContainsKey(key))
                {
                    dictionary[key] = value;
                }
                else
                {
                    dictionary.Add(key, value);
                }
                return true;
            }
            finally
            {
                rwLock.ReleaseLock();
            }
        }

        public IEnumerable ListKeys()
        {
            rwLock.AcquireReaderLock(-1);
            try
            {
                return dictionary.Keys;
            }
            finally
            {
                rwLock.ReleaseLock();
            }
        }


        public void Clear()
        {
            rwLock.AcquireWriterLock(-1);
            try
            {
                dictionary.Clear();
            }
            finally
            {
                rwLock.ReleaseLock();
            }
        }
    }
}
