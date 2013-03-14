// -----------------------------------------------------------------------
// <copyright file="LimitedMemoryStorage.cs" company="Execom">
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
using System.Diagnostics;
    using System.Collections;

    /// <summary>
    /// Memory storage with limited number of recent items
    /// </summary>
    /// <author>Nenad Sabo</author>
    public class LimitedMemoryStorageSafe<TKey, TValue> : IKeyValueStorage<TKey, TValue>, IEvictingStorage<TKey>
    {
        private class DictionaryEntry : IComparable
        {
            public TValue Value;
            public long AccessCount;

            public DictionaryEntry(TValue value, long lastAccess)
            {
                this.Value = value;
                this.AccessCount = lastAccess;
            }

            public int CompareTo(object obj)
            {
                return AccessCount.CompareTo((obj as DictionaryEntry).AccessCount);
            }
        }

        private ReaderWriterLock rwLock;
        private Hashtable dictionary;
        private int minElements;
        private int maxElements;

        public LimitedMemoryStorageSafe(int minElements, int maxElements)
        {
            this.minElements = minElements;
            this.maxElements = maxElements;

            dictionary = new Hashtable();
            rwLock = new ReaderWriterLock();
        }

        public bool Remove(TKey key)
        {
            rwLock.AcquireWriterLock(-1);
            try
            {
                dictionary.Remove(key);
                return true;
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
                if (dictionary.ContainsKey(key))
                {
                    DictionaryEntry entry = (DictionaryEntry)dictionary[key];
                    entry.AccessCount++;
                    return entry.Value;
                }
                else
                {
                    return default(TValue);
                }
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
                    DictionaryEntry entry = (DictionaryEntry)dictionary[key];
                    entry.AccessCount++;
                    entry.Value = value;
                }
                else
                {
                    dictionary.Add(key, new DictionaryEntry(value, 0));

                    if (dictionary.Count > maxElements)
                    {
                        RemoveOldElements();
                    }
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

        private void RemoveOldElements()
        {
            TKey[] accessIds = new TKey[dictionary.Count];
            DictionaryEntry[] entries = new DictionaryEntry[dictionary.Count];

            dictionary.Values.CopyTo(entries, 0);
            dictionary.Keys.CopyTo(accessIds, 0);

            Array.Sort(entries, accessIds);

            int nrToRemove = dictionary.Count - minElements;

            if (OnBeforeKeysEvicted != null)
            {
                TKey[] keys = new TKey[nrToRemove];
                Array.Copy(accessIds, keys, nrToRemove);
                OnBeforeKeysEvicted(this, new KeysEvictedEventArgs<TKey>(keys));
            }

            foreach (var key in accessIds)
            {
                dictionary.Remove(key);

                nrToRemove--;
                if (nrToRemove == 0)
                {
                    break;
                }
            }

            foreach (var item in dictionary.Values)
            {
                ((DictionaryEntry)item).AccessCount = 0;
            }
        }

        public event EventHandler<KeysEvictedEventArgs<TKey>> OnBeforeKeysEvicted;
    }
}
