// -----------------------------------------------------------------------
// <copyright file="IIndexedCollection.cs" company="Execom">
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

namespace Execom.IOG.Types
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Collection which allows access by primary key.
    /// </summary>
    /// <author>Nenad Sabo</author>
    public interface IIndexedCollection<T> : ICollection<T>
    {
        /// <summary>
        /// Tries to acces collection element by the primary key
        /// </summary>
        /// <param name="key">Primary key</param>
        /// <param name="value">Found object, or null if not found</param>
        /// <returns>True if object is found</returns>
        bool TryFindPrimaryKey(object key, out T value);

        /// <summary>
        /// Gets item by primary key
        /// </summary>
        /// <param name="key">Primary key</param>
        /// <returns>Object with given key, if not found error is reported</returns>
        T FindByPrimaryKey(object key);

        /// <summary>
        /// Returns if collection contains an element with given key
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>True if key was found</returns>
        bool ContainsPrimaryKey(object key);
    }
}
