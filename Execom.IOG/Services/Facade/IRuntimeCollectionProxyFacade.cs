// -----------------------------------------------------------------------
// <copyright file="IRuntimeCollectionProxyFacade.cs" company="Execom">
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

namespace Execom.IOG.Services.Facade
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// This interface defines collection operations for collection proxy objects
    /// </summary>
    /// <author>Nenad Sabo</author>
    public interface IRuntimeCollectionProxyFacade
    {
        /// <summary>
        /// Adds new element to collection
        /// </summary>
        /// <param name="instanceId">Collection instance ID</param>
        /// <param name="itemTypeId">Type ID of element</param>
        /// <param name="item">Item to add. Can be a scalar value or proxy.</param>
        /// <param name="isScalar">Determines if item is of scalar type</param>
        void CollectionAdd(Guid instanceId, Guid itemTypeId, object item, bool isScalar);

        /// <summary>
        /// Adds new element to collection
        /// </summary>
        /// <param name="instanceId">Collection instance ID</param>
        /// <param name="itemTypeId">Type ID of element</param>
        /// <param name="item">Item to add. Can be a scalar value or proxy.</param>
        /// <param name="isScalar">Determines if item is of scalar type</param>
        void CollectionAddOrdered(Guid instanceId, Guid itemTypeId, object item, bool isScalar);

        /// <summary>
        /// Adds new element to set
        /// </summary>
        /// <param name="instanceId">Collection set instance ID</param>
        /// <param name="itemTypeId">Type ID of element</param>
        /// <param name="item">Item to add. Can be a scalar value or proxy.</param>
        /// <param name="isScalar">Determines if item is of scalar type</param>
        void SetAdd(Guid instanceId, Guid itemTypeId, object item, bool isScalar);

        /// <summary>
        /// Clears the entire collection
        /// </summary>
        /// <param name="instanceId">Collection instance ID</param>
        void CollectionClear(Guid instanceId);

        /// <summary>
        /// Checks if item is contained in the collection. For proxy types the instance ID is checked.
        /// For scalar types item is found by using Comparer.Default.
        /// </summary>
        /// <param name="instanceId">Collection instance ID</param>
        /// <param name="item">Item to check. Can be a scalar value or a proxy.</param>
        /// <param name="isScalar">Determines if item is of scalar type</param>
        /// <returns>True if item exists in the collection.</returns>
        bool CollectionContains(Guid instanceId, object item, bool isScalar, bool isReadOnly);

        /// <summary>
        /// Checks if item is contained in the ordered collection. For proxy types the instance ID is checked.
        /// For scalar types item is found by using Comparer.Default.
        /// </summary>
        /// <param name="instanceId">Collection instance ID</param>
        /// <param name="item">Item to check. Can be a scalar value or a proxy.</param>
        /// <param name="isScalar">Determines if item is of scalar type</param>
        /// <returns>True if item exists in the collection.</returns>
        bool CollectionContainsOrdered(Guid instanceId, object item, bool isScalar, bool isReadOnly);

        /// <summary>
        /// Checks if item is contained in the set. For proxy types the instance ID is checked.
        /// For scalar types item is found by using Comparer.Default.
        /// </summary>
        /// <param name="instanceId">Collection instance ID</param>
        /// <param name="item">Item to check. Can be a scalar value or a proxy.</param>
        /// <param name="isScalar">Determines if item is of scalar type</param>
        /// <returns>True if item exists in the collection.</returns>
        bool SetContains(Guid instanceId, object item, bool isScalar, bool isReadOnly);

        /// <summary>
        /// Copies the elements of the Collection to an Array, starting at a particular Array index
        /// </summary>
        /// <typeparam name="TElementType">Type of collection elements</typeparam>
        /// <param name="instanceId">Collection instance ID</param>
        /// <param name="isScalar">Determines if item is of scalar type</param>
        /// <param name="isReadOnly">Determines if collection is read only</param>
        /// <param name="array">Destination array</param>
        /// <param name="arrayIndex">Start index</param>
        void CollectionCopyTo<TElementType>(Guid elementTypeId, Guid instanceId, bool isScalar, bool isReadOnly, Array array, int arrayIndex);

        /// <summary>
        /// Returns number of elements in the collection
        /// </summary>
        /// <param name="instanceId">Collection instance ID</param>
        /// <param name="isReadOnly">Determines if collection is read only</param>
        /// <returns>Number of elements</returns>
        int CollectionCount(Guid instanceId, bool isReadOnly);

        /// <summary>
        /// Removes the first occurrence of a specific object from the collection
        /// </summary>
        /// <param name="instanceId">Collection instance ID</param>
        /// <param name="item">Item to check. Can be a scalar value or a proxy.</param>
        /// <param name="isScalar">Determines if item is of scalar type</param>
        /// <returns>True if item was successfully removed from the ICollection; otherwise, false. 
        /// This method also returns false if item is not found in the original Collection</returns>
        bool CollectionRemove(Guid instanceId, object item, bool isScalar);

        /// <summary>
        /// Removes the first occurrence of a specific object from the ordered collection
        /// </summary>
        /// <param name="instanceId">Collection instance ID</param>
        /// <param name="item">Item to check. Can be a scalar value or a proxy.</param>
        /// <param name="isScalar">Determines if item is of scalar type</param>
        /// <returns>True if item was successfully removed from the ICollection; otherwise, false. 
        /// This method also returns false if item is not found in the original Collection</returns>
        bool CollectionRemoveOrdered(Guid instanceId, object item, bool isScalar);

        /// <summary>
        /// Removes the first occurrence of a specific object from the set
        /// </summary>
        /// <param name="instanceId">Collection instance ID</param>
        /// <param name="item">Item to check. Can be a scalar value or a proxy.</param>
        /// <param name="isScalar">Determines if item is of scalar type</param>
        /// <returns>True if item was successfully removed from the ICollection; otherwise, false. 
        /// This method also returns false if item is not found in the original Collection</returns>
        bool SetRemove(Guid instanceId, object item, bool isScalar);

        /// <summary>
        /// Called when proxy is collected by the GC
        /// </summary>
        /// <param name="proxy">Proxy instance</param>
        void ProxyCollected(object proxy);

        /// <summary>
        /// Determines type ID by given type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Guid identifier</returns>
        Guid GetTypeId(Type type);

        /// <summary>
        /// Determines if type is scalar
        /// </summary>
        /// <param name="typeId">Type identifier</param>
        /// <returns>True if type is a scalar type</returns>
        bool IsScalarType(Guid typeId);

        /// <summary>
        /// Returns collection enumerator
        /// </summary>
        /// <typeparam name="TElementType">Type of collection element</typeparam>
        /// <param name="elementType">Type ID of element</param>
        /// <param name="instanceId">Collection instance ID</param>
        /// <param name="isScalar">Determines if element type is scalar</param>
        /// <param name="isReadOnly">Determines if collection is read only</param>
        /// <returns>Enumerator object</returns>
        IEnumerator<TElementType> CollectionGetEnumerator<TElementType>(Guid elementType, Guid instanceId, bool isScalar, bool isReadOnly);

        /// <summary>
        /// Finds collection element by key if it exists
        /// </summary>
        /// <typeparam name="TElementType">Type of collection element</typeparam>
        /// <param name="elementType">Type ID of element</param>
        /// <param name="instanceId">Collection instance ID</param>
        /// <param name="isScalar">Determines if element type is scalar</param>
        /// <param name="isReadOnly">Determines if collection is read only</param>
        /// <param name="key">Key to look for</param>
        /// <param name="value">Found element</param>
        /// <returns>True if element was found, otherwise false</returns>
        bool CollectionTryFindPrimaryKey<TElementType>(Guid elementType, Guid instanceId, bool isScalar, bool isReadOnly, object key, out TElementType value);

        /// <summary>
        /// Finds collection element by key
        /// </summary>
        /// <typeparam name="TElementType">Type of collection element</typeparam>
        /// <param name="elementType">Type ID of element</param>
        /// <param name="instanceId">Collection instance ID</param>
        /// <param name="isScalar">Determines if element type is scalar</param>
        /// <param name="isReadOnly">Determines if collection is read only</param>
        /// <param name="key">Key to look for</param>
        /// <returns>Collection element, if not found error is reported</returns>
        TElementType CollectionFindByPrimaryKey<TElementType>(Guid elementType, Guid instanceId, bool isScalar, bool isReadOnly, object key);

        /// <summary>
        /// Determines if collection contains the primary key
        /// </summary>
        /// <param name="instanceId">Collection instance ID</param>
        /// <param name="key">Key value</param>
        /// <returns>True if collection contains the key</returns>
        bool CollectionContainsPrimaryKey(Guid instanceId, object key, bool isReadOnly);
    }
}
