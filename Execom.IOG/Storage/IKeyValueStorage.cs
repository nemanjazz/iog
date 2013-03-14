/*
   Copyright 2011 EXECOM d.o.o

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Execom.IOG.Storage
{
    /// <summary>
    /// Generic key,value storage
    /// </summary>
    /// <typeparam name="TKey">Type for key</typeparam>
    /// <typeparam name="TValue">Type for data</typeparam>
    public interface IKeyValueStorage<TKey, TValue>
    {
        /// <summary>
        /// Adds or updates value in the storage
        /// </summary>
        /// <param name="key">Key which identifies the data</param>
        /// <param name="value">Data to store</param>
        /// <returns>True if operation was success</returns>
        bool AddOrUpdate(TKey key, TValue value);

        /// <summary>
        /// Reads data from storage
        /// </summary>
        /// <param name="key">Key which identifies the data</param>
        /// <returns>Storage data</returns>
        TValue Value(TKey key);

        /// <summary>
        /// Removes data from storage
        /// </summary>
        /// <param name="key">Key which identifies the data</param>
        /// <returns>True if data was present in the storage</returns>
        bool Remove(TKey key);

        /// <summary>
        /// Determines if data is present in the storage
        /// </summary>
        /// <param name="key">Key which identifies the data</param>
        /// <returns>True if data is present</returns>
        bool Contains(TKey key);

        /// <summary>
        /// Returns the keys
        /// </summary>
        /// <returns>List of keys in the storage</returns>
        IEnumerable ListKeys();

        /// <summary>
        /// Clears all data in the storage
        /// </summary>
        void Clear();
    }
}
