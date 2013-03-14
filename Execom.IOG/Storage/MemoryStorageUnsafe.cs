﻿// -----------------------------------------------------------------------
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
    public class MemoryStorageUnsafe<TKey, TValue> : IKeyValueStorage<TKey, TValue>
    {
        private Dictionary<TKey, TValue> dictionary;

        public MemoryStorageUnsafe()
        {
            dictionary = new Dictionary<TKey, TValue>();
        }

        public bool Remove(TKey key)
        {
            return dictionary.Remove(key);
        }

        public bool Contains(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        public TValue Value(TKey key)
        {
            TValue value;
            dictionary.TryGetValue(key, out value);
            return value;
        }

        public bool AddOrUpdate(TKey key, TValue value)
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

        public IEnumerable ListKeys()
        {
            return dictionary.Keys;
        }


        public void Clear()
        {
            dictionary.Clear();
        }
    }
}
