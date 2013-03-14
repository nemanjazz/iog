// -----------------------------------------------------------------------
// <copyright file="RuntimeProxyFacade.cs" company="Execom">
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
    using Execom.IOG.Services.Data;
    using Execom.IOG.Services.Runtime;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Implementation of facade visible to runtime proxy objects
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal class RuntimeProxyFacade : IRuntimeProxyFacade
    {
        private ObjectInstancesService objectInstancesService;
        private ObjectInstancesService immutableInstancesService;
        private CollectionInstancesService collectionInstancesService;
        private DictionaryInstancesService dictionaryInstancesService;
        private CollectionInstancesService immutableCollectionInstancesService;
        private DictionaryInstancesService immutableDictionaryInstancesService;
        private IProxyMap mutableProxyMap;
        private IProxyMap immutableProxyMap;
        private ProxyCreatorService proxyCreatorService;
        private TypesService typesService;

        public RuntimeProxyFacade(TypesService typesService, ObjectInstancesService objectInstancesService, ObjectInstancesService immutableInstancesService, CollectionInstancesService collectionInstancesService, CollectionInstancesService immutableCollectionInstancesService, DictionaryInstancesService dictionaryInstancesService, DictionaryInstancesService immutableDictionaryInstancesService, IProxyMap mutableProxyMap, IProxyMap immutableProxyMap, ProxyCreatorService proxyCreatorService)
        {
            this.objectInstancesService = objectInstancesService;
            this.immutableInstancesService = immutableInstancesService;
            this.collectionInstancesService = collectionInstancesService;
            this.dictionaryInstancesService = dictionaryInstancesService;
            this.immutableCollectionInstancesService = immutableCollectionInstancesService;
            this.immutableDictionaryInstancesService = immutableDictionaryInstancesService;
            this.mutableProxyMap = mutableProxyMap;
            this.immutableProxyMap = immutableProxyMap;
            this.proxyCreatorService = proxyCreatorService;
            this.typesService = typesService;
        }


        /// <summary>
        /// Returns value of a property: 
        /// - If property is scalar, returns the scalar value
        /// - If property is not scalar, returns the proxy object
        /// </summary>
        /// <param name="instanceId">Instance ID</param>
        /// <param name="memberId">Member ID</param>
        /// <param name="isScalar">Determines if member is scalar</param>
        /// <param name="isReadOnly">Determines if instance is read only</param>
        /// <returns>Scalar value, or a proxy</returns>
        public object GetInstanceMemberValue(Guid instanceId, Guid memberId, bool isScalar, bool isReadOnly)
        {
            if (instanceId.Equals(Guid.Empty))
            {
                throw new ArgumentException("Instance was accessed outside of workspace scope or it was rolled back.");
            }

            if (isScalar)
            {
                if (isReadOnly)
                {
                    return immutableInstancesService.GetScalarInstanceMember(instanceId, memberId);
                }
                else
                {
                    return objectInstancesService.GetScalarInstanceMember(instanceId, memberId);
                }
            }
            else
            {
                bool isPermanent = false;
                Guid referenceId = Guid.Empty;

                if (isReadOnly)
                {
                    // Get ID of referenced instance
                    referenceId = immutableInstancesService.GetReferenceInstanceMember(instanceId, memberId, out isPermanent);
                }
                else
                {
                    // Get ID of referenced instance
                    referenceId = objectInstancesService.GetReferenceInstanceMember(instanceId, memberId, out isPermanent);
                }

                return GetProxyInstance(isReadOnly || isPermanent, referenceId, Guid.Empty);
            }
        }

        private object GetProxyInstance(bool isReadOnly, Guid referenceId, Guid typeId)
        {
            if (referenceId.Equals(Constants.NullReferenceNodeId))
            {
                return null;
            }
            else
            {
                object proxy = null;

                IProxyMap map = mutableProxyMap;
                var collectionService = collectionInstancesService;
                var objectService = objectInstancesService;

                if (isReadOnly)
                {
                    map = immutableProxyMap;
                    collectionService = immutableCollectionInstancesService;
                    objectService = immutableInstancesService;
                }

                if (!map.TryGetProxy(referenceId, out proxy))
                {
                    if (typeId.Equals(Guid.Empty))
                    {
                        if (collectionService.IsCollectionInstance(referenceId)) // TODO (nsabo) Merge this information into proxy static data
                        {
                            typeId = collectionService.GetInstanceTypeId(referenceId);
                        }
                        else
                        {
                            // Get type ID of referenced instance
                            typeId = objectService.GetInstanceTypeId(referenceId);
                        }
                    }
                    // Create proxy object                    
                    proxy = proxyCreatorService.NewObject(this, typeId, referenceId, isReadOnly);
                    // Add to proxy map
                    map.AddProxy(referenceId, proxy);
                }

                return proxy;
            }
        }

        /// <summary>
        /// Sets value for a property
        /// </summary>
        /// <param name="instanceId">Instance ID</param>
        /// <param name="memberId">Member ID</param>
        /// <param name="value">New value to set</param>
        /// <param name="isScalar">Determines if member is scalar</param>
        /// <param name="isReadOnly">Determines if instance is read only</param>
        public void SetInstanceMemberValue(Guid instanceId, Guid memberId, object value, bool isScalar, bool isReadOnly)
        {
            if (instanceId.Equals(Guid.Empty))
            {
                throw new ArgumentException("Instance was accessed outside of workspace scope or it was rolled back.");
            }

            if (isReadOnly)
            {
                throw new InvalidOperationException("Setting property on read only instance not allowed");
            }

            if (isScalar)
            {
                objectInstancesService.SetScalarInstanceMember(instanceId, memberId, value);
            }
            else
            {
                Guid referenceId = Constants.NullReferenceNodeId;

                if (value != null)
                {
                    if (!Utils.HasItemId(value))
                    {
                        throw new InvalidOperationException("Object set is not a valid IOG proxy");
                    }

                    referenceId = Utils.GetItemId(value);
                }

                objectInstancesService.SetReferenceInstanceMember(instanceId, memberId, referenceId);
            }
        }

        /// <summary>
        /// Method is called when .NET GC collects a proxy
        /// </summary>
        /// <param name="instance">Proxy instance</param>
        public void ProxyCollected(object instance)
        {
            // Implement deinitialization of a proxy as needed
        }

        public void CollectionAdd(Guid instanceId, Guid valueTypeId, object value, bool isScalar)
        {
            PerformAdd(instanceId, valueTypeId, value, isScalar, false, false);
        }

        public void CollectionAddOrdered(Guid instanceId, Guid valueTypeId, object value, bool isScalar)
        {
            PerformAdd(instanceId, valueTypeId, value, isScalar, false, true);
        }

        private void PerformAdd(Guid instanceId, Guid valueTypeId, object value, bool isScalar, bool isSet, bool isOrdered)
        {
            if (instanceId.Equals(Guid.Empty))
            {
                throw new ArgumentException("Instance was accessed outside of workspace scope or it was rolled back.");
            }

            if (isScalar)
            {
                if (!isSet)
                {
                    if (isOrdered)
                    {
                        long maxId = collectionInstancesService.MaxOrderedIdentifier(instanceId);
                        collectionInstancesService.AddScalar(instanceId, valueTypeId, value, maxId + 1);
                    }
                    else
                    {
                        collectionInstancesService.AddScalar(instanceId, valueTypeId, value);
                    }
                }
                else
                {
                    collectionInstancesService.AddScalar(instanceId, valueTypeId, value, value);
                }
            }
            else
            {


                Guid referenceId = Constants.NullReferenceNodeId;
                object primaryKey = null;

                if (value != null)
                {
                    if (!Utils.HasItemId(value))
                    {
                        throw new InvalidOperationException("Object set is not a valid IOG proxy");
                    }

                    referenceId = Utils.GetItemId(value);

                    Guid primaryKeyId = Utils.GetItemPrimaryKeyId(value);

                    if (primaryKeyId != Guid.Empty)
                    {
                        primaryKey = objectInstancesService.GetScalarInstanceMember(referenceId, primaryKeyId);
                    }
                }

                if (isOrdered)
                {
                    long maxId = collectionInstancesService.MaxOrderedIdentifier(instanceId);
                    collectionInstancesService.AddReference(instanceId, referenceId, maxId + 1);
                }
                else
                {

                    if (primaryKey == null)
                    {
                        collectionInstancesService.AddReference(instanceId, referenceId);
                    }
                    else
                    {
                        collectionInstancesService.AddReference(instanceId, referenceId, primaryKey);
                    }
                }

            }
        }

        public void CollectionClear(Guid instanceId)
        {
            if (instanceId.Equals(Guid.Empty))
            {
                throw new ArgumentException("Instance was accessed outside of workspace scope or it was rolled back.");
            }

            collectionInstancesService.Clear(instanceId);
        }

        public bool CollectionContains(Guid instanceId, object value, bool isScalar, bool isReadOnly)
        {
            return PerformContains(instanceId, value, isScalar, false, isReadOnly, false);
        }

        public bool CollectionContainsOrdered(Guid instanceId, object value, bool isScalar, bool isReadOnly)
        {
            return PerformContains(instanceId, value, isScalar, false, isReadOnly, true);
        }

        private bool PerformContains(Guid instanceId, object value, bool isScalar, bool isSet, bool isReadOnly, bool isOrdered)
        {
            if (instanceId.Equals(Guid.Empty))
            {
                throw new ArgumentException("Instance was accessed outside of workspace scope or it was rolled back.");
            }

            var service = collectionInstancesService;

            if (isReadOnly)
            {
                service = immutableCollectionInstancesService;
            }

            if (isScalar)
            {
                if (!isSet)
                {
                    return service.ContainsScalar(instanceId, value);
                }
                else
                {
                    return service.ContainsScalar(instanceId, value, value);
                }
            }
            else
            {
                Guid referenceId = Constants.NullReferenceNodeId;
                object primaryKey = null;

                if (value != null)
                {
                    if (!Utils.HasItemId(value))
                    {
                        throw new InvalidOperationException("Object set is not a valid IOG proxy");
                    }

                    referenceId = Utils.GetItemId(value);

                    Guid primaryKeyId = Utils.GetItemPrimaryKeyId(value);

                    if (primaryKeyId != Guid.Empty)
                    {
                        primaryKey = objectInstancesService.GetScalarInstanceMember(referenceId, primaryKeyId);
                    }
                }

                // Ordered collection ignores primary key
                if (primaryKey == null || isOrdered)  
                {
                    return service.ContainsReference(instanceId, referenceId);
                }
                else
                {
                    return service.ContainsReference(instanceId, referenceId, primaryKey);
                }
            }
        }

        public void CollectionCopyTo<TElementType>(Guid elementTypeId, Guid instanceId, bool isScalar, bool isReadOnly, Array array, int arrayIndex)
        {
            if (instanceId.Equals(Guid.Empty))
            {
                throw new ArgumentException("Instance was accessed outside of workspace scope or it was rolled back.");
            }

            using (var enumerator = CollectionGetEnumerator<TElementType>(elementTypeId, instanceId, isScalar, isReadOnly))
            {
                int index = arrayIndex;
                while (enumerator.MoveNext())
                {
                    array.SetValue(enumerator.Current, index);
                    index++;
                }
            }
        }

        public int CollectionCount(Guid instanceId, bool isReadOnly)
        {
            if (instanceId.Equals(Guid.Empty))
            {
                throw new ArgumentException("Instance was accessed outside of workspace scope or it was rolled back.");
            }

            return isReadOnly ? immutableCollectionInstancesService.Count(instanceId) : collectionInstancesService.Count(instanceId);
        }

        public bool CollectionRemove(Guid instanceId, object value, bool isScalar)
        {
            return PerformRemove(instanceId, value, isScalar, false, false);
        }

        public bool CollectionRemoveOrdered(Guid instanceId, object value, bool isScalar)
        {
            return PerformRemove(instanceId, value, isScalar, false, true);
        }

        private bool PerformRemove(Guid instanceId, object value, bool isScalar, bool isSet, bool isOrdered)
        {
            if (instanceId.Equals(Guid.Empty))
            {
                throw new ArgumentException("Invalid instance revision ID");
            }

            if (isScalar)
            {
                if (!isSet)
                {
                    return collectionInstancesService.RemoveScalar(instanceId, value);
                }
                else
                {
                    return collectionInstancesService.RemoveScalar(instanceId, value, value);
                }
            }
            else
            {
                Guid referenceId = Constants.NullReferenceNodeId;
                object primaryKey = null;

                if (value != null)
                {
                    if (!Utils.HasItemId(value))
                    {
                        throw new InvalidOperationException("Object set is not a valid IOG proxy");
                    }

                    referenceId = Utils.GetItemId(value);

                    Guid primaryKeyId = Utils.GetItemPrimaryKeyId(value);

                    if (primaryKeyId != Guid.Empty)
                    {
                        primaryKey = objectInstancesService.GetScalarInstanceMember(referenceId, primaryKeyId);
                    }
                }

                // Ordered collection ignores primary key
                if (primaryKey == null || isOrdered)
                {
                    return collectionInstancesService.RemoveReference(instanceId, referenceId);
                }
                else
                {
                    return collectionInstancesService.RemoveReference(instanceId, referenceId, primaryKey);
                }
            }
        }

        public bool IsScalarType(Guid typeId)
        {
            return typesService.IsScalarType(typeId);
        }


        public Guid GetTypeId(Type type)
        {
            return typesService.GetTypeId(type);
        }

        public class CollectionEnumerator<TElementType> : IEnumerator<TElementType>
        {
            private IEnumerator<Graph.Edge<Guid, Graph.EdgeData>> edgeEnumerator;
            private bool isScalar;
            private bool isReadOnly;
            private ObjectInstancesService objectInstancesService;
            private RuntimeProxyFacade proxyFacade;
            private Guid elementType;

            public CollectionEnumerator(Guid elementType, IEnumerator<Graph.Edge<Guid, Graph.EdgeData>> edgeEnumerator, bool isScalar, bool isReadOnly, ObjectInstancesService objectInstancesService, RuntimeProxyFacade proxyFacade)
            {
                this.edgeEnumerator = edgeEnumerator;
                this.isScalar = isScalar;
                this.isReadOnly = isReadOnly;
                this.objectInstancesService = objectInstancesService;
                this.proxyFacade = proxyFacade;
                this.elementType = elementType;
            }            

            public TElementType Current
            {
                get 
                {
                    if (isScalar)
                    {
                        return (TElementType)objectInstancesService.GetScalarInstanceValue(edgeEnumerator.Current.ToNodeId);
                    }
                    else
                    {
                        return (TElementType)proxyFacade.GetProxyInstance(isReadOnly, edgeEnumerator.Current.ToNodeId, elementType);
                    }
                }
            }

            public void Dispose()
            {
                edgeEnumerator.Dispose();
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                return edgeEnumerator.MoveNext();
            }

            public void Reset()
            {
                edgeEnumerator.Reset();
            }
        }

        public IEnumerator<TElementType> CollectionGetEnumerator<TElementType>(Guid elementType, Guid instanceId, bool isScalar, bool isReadOnly)
        {
            return new CollectionEnumerator<TElementType>(elementType, isReadOnly ? immutableCollectionInstancesService.GetEnumerator(instanceId) : collectionInstancesService.GetEnumerator(instanceId), isScalar, isReadOnly, isReadOnly ? immutableInstancesService : objectInstancesService, this);
        }



        public bool CollectionTryFindPrimaryKey<TElementType>(Guid elementType, Guid instanceId, bool isScalar, bool isReadOnly, object key, out TElementType value)
        {
            Guid referenceId = Guid.Empty;
            if (collectionInstancesService.TryFindReferenceByKey(instanceId, key, out referenceId))
            {
                value = (TElementType)GetProxyInstance(isReadOnly, referenceId, elementType);
                return true;
            }
            else
            {
                value = default(TElementType);
                return false;
            }
        }

        public TElementType CollectionFindByPrimaryKey<TElementType>(Guid elementType, Guid instanceId, bool isScalar, bool isReadOnly, object key)
        {
            Guid referenceId = Guid.Empty;
            if (collectionInstancesService.TryFindReferenceByKey(instanceId, key, out referenceId))
            {
                return (TElementType)GetProxyInstance(isReadOnly, referenceId, elementType);
            }
            else
            {
                throw new ArgumentException("Key not found");
            }
        }

        public bool CollectionContainsPrimaryKey(Guid instanceId, object key, bool isReadOnly)
        {
            Guid referenceId = Guid.Empty;
            if (isReadOnly)
            {
                return immutableCollectionInstancesService.TryFindReferenceByKey(instanceId, key, out referenceId);
            }
            else
            {
                return collectionInstancesService.TryFindReferenceByKey(instanceId, key, out referenceId);
            }
        }

        public void DictionaryAdd<TKey, TValue>(Guid instanceId, Guid elementType, bool isScalar, TKey key, TValue value)
        {
            if (instanceId.Equals(Guid.Empty))
            {
                throw new ArgumentException("Instance was accessed outside of workspace scope or it was rolled back.");
            }

            if (isScalar)
            {
                dictionaryInstancesService.AddScalar(instanceId, elementType, key, value);
            }
            else
            {
                Guid referenceId = Constants.NullReferenceNodeId;

                if (value != null)
                {
                    if (!Utils.HasItemId(value))
                    {
                        throw new InvalidOperationException("Object set is not a valid IOG proxy");
                    }

                    referenceId = Utils.GetItemId(value);
                }

                dictionaryInstancesService.AddReference(instanceId, key, referenceId);
            }
        }

        public bool DictionaryContainsKey<TKey>(Guid instanceId, TKey key, bool readOnly)
        {
            if (readOnly)
            {
                return immutableDictionaryInstancesService.ContainsKey(instanceId, key);
            }
            else
            {
                return dictionaryInstancesService.ContainsKey(instanceId, key);
            }
        }

        public ICollection<TKey> DictionaryKeys<TKey>(Guid instanceId, bool readOnly)
        {
            var service = dictionaryInstancesService;

            if (readOnly)
            {
                service = immutableDictionaryInstancesService;
            }

            Collection<TKey> keys = new Collection<TKey>();
            using (var enumerator = service.GetEnumerator(instanceId))
            {
                while (enumerator.MoveNext())
                {
                    keys.Add((TKey)enumerator.Current.Data.Data);
                }
            }

            return keys;
        }

        public bool DictionaryRemove<TKey>(Guid instanceId, TKey key)
        {
            return dictionaryInstancesService.Remove(instanceId, key);
        }

        public bool DictionaryTryGetValue<TKey, TValue>(Guid instanceId, Guid elementTypeId, bool isSclar, bool readOnly, TKey key, out TValue value)
        {
            if (isSclar)
            {
                object scalarValue = null;
                try
                {
                    return dictionaryInstancesService.TryGetScalar(instanceId, key, out scalarValue);
                }
                finally
                {
                    value = (TValue)scalarValue;
                }
            }
            else
            {
                Guid referenceId = Guid.Empty;
                value = default(TValue);

                if (dictionaryInstancesService.TryGetReference(instanceId, key, out referenceId))
                {
                    value = (TValue)GetProxyInstance(readOnly, referenceId, elementTypeId);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public ICollection<TValue> DictionaryValues<TKey, TValue>(Guid instanceId, Guid elementTypeId, bool isSclar, bool readOnly)
        {
            Collection<TValue> values = new Collection<TValue>();

            using (var enumerator = dictionaryInstancesService.GetEnumerator(instanceId))
            {
                while (enumerator.MoveNext())
                {
                    values.Add(DictionaryGetValue<TKey, TValue>(instanceId, elementTypeId, isSclar, readOnly, (TKey)enumerator.Current.Data.Data));
                }
            }

            return values;
        }

        public TValue DictionaryGetValue<TKey, TValue>(Guid instanceId, Guid elementTypeId, bool isSclar, bool readOnly, TKey key)
        {
            if (isSclar)
            {
                object scalarValue = null;
                if (dictionaryInstancesService.TryGetScalar(instanceId, key, out scalarValue))
                {
                    return (TValue)scalarValue;
                }
                else
                {
                    throw new KeyNotFoundException("Element not found with given key");
                }
            }
            else
            {
                Guid referenceId = Guid.Empty;
                if (dictionaryInstancesService.TryGetReference(instanceId, key, out referenceId))
                {
                    return (TValue)GetProxyInstance(readOnly, referenceId, elementTypeId);
                }
                else
                {
                    throw new KeyNotFoundException("Element not found with given key");
                }
            }
        }

        public void DictionarySetValue<TKey, TValue>(Guid instanceId, Guid elementTypeId, bool isSclar, bool readOnly, object key, object value)
        {
            if (isSclar)
            {
                dictionaryInstancesService.SetScalar(instanceId, elementTypeId, key, value);
            }
            else
            {
                if (!Utils.HasItemId(value))
                {
                    throw new InvalidOperationException("Object set is not a valid IOG proxy");
                }

                dictionaryInstancesService.SetReference(instanceId, key, Utils.GetItemId(value));
            }
        }

        public void DictionaryAdd<TKey, TValue>(Guid instanceId, Guid elementTypeId, bool isScalar, KeyValuePair<TKey, TValue> item)
        {
            DictionaryAdd<TKey, TValue>(instanceId, elementTypeId, isScalar, item.Key, item.Value);
        }

        public void DictionaryClear(Guid instanceId)
        {
            dictionaryInstancesService.Clear(instanceId);
        }

        public bool DictionaryContains<TKey, TValue>(Guid instanceId, KeyValuePair<TKey, TValue> item, bool readOnly)
        {
            if (readOnly)
            {
                return immutableDictionaryInstancesService.ContainsKey(instanceId, item.Key);
            }
            else
            {
                return dictionaryInstancesService.ContainsKey(instanceId, item.Key);
            }
        }

        public void DictionaryCopyTo<TKey, TValue>(Guid instanceId, Guid elementTypeId, bool isSclar, bool readOnly, KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            int index = arrayIndex;
            using (var enumerator = dictionaryInstancesService.GetEnumerator(instanceId))
            {
                while (enumerator.MoveNext())
                {
                    TKey key = (TKey)enumerator.Current.Data.Data;
                    array[arrayIndex] = new KeyValuePair<TKey,TValue>(key, DictionaryGetValue<TKey, TValue>(instanceId, elementTypeId, isSclar, readOnly, key));
                    arrayIndex++;
                }
            }
        }

        public int DictionaryCount(Guid instanceId, bool readOnly)
        {
            if (readOnly)
            {
                return immutableDictionaryInstancesService.Count(instanceId);
            }
            else 
            { 
                return dictionaryInstancesService.Count(instanceId); 
            }
        }

        public bool DictionaryRemove<TKey, TValue>(Guid instanceId, KeyValuePair<TKey, TValue> item)
        {
            return dictionaryInstancesService.Remove(instanceId, item.Key);
        }

        public class DictionaryEnumerator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>
        {

            private IEnumerator<Graph.Edge<Guid, Graph.EdgeData>> edgeEnumerator;
            private bool isScalar;
            private bool isReadOnly;
            private ObjectInstancesService objectInstancesService;
            private RuntimeProxyFacade proxyFacade;
            private Guid elementType;

            public DictionaryEnumerator(Guid elementType, IEnumerator<Graph.Edge<Guid, Graph.EdgeData>> edgeEnumerator, bool isScalar, bool isReadOnly, ObjectInstancesService objectInstancesService, RuntimeProxyFacade proxyFacade)
            {
                this.edgeEnumerator = edgeEnumerator;
                this.isScalar = isScalar;
                this.isReadOnly = isReadOnly;
                this.objectInstancesService = objectInstancesService;
                this.proxyFacade = proxyFacade;
                this.elementType = elementType;
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    return new KeyValuePair<TKey, TValue>(
                    (TKey)edgeEnumerator.Current.Data.Data,
                    isScalar ?
                    (TValue)objectInstancesService.GetScalarInstanceValue(edgeEnumerator.Current.ToNodeId)
                    : (TValue)proxyFacade.GetProxyInstance(isReadOnly, edgeEnumerator.Current.ToNodeId, elementType)
                    );

                }
            }

            public void Dispose()
            {
                edgeEnumerator.Dispose();
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                return edgeEnumerator.MoveNext();
            }

            public void Reset()
            {
                edgeEnumerator.Reset();
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> DictionaryGetEnumerator<TKey, TValue>(Guid instanceId, Guid elementTypeId, bool isSclar, bool readOnly)
        {
            return new DictionaryEnumerator<TKey, TValue>(elementTypeId, readOnly ? immutableDictionaryInstancesService.GetEnumerator(instanceId) : dictionaryInstancesService.GetEnumerator(instanceId), isSclar, readOnly, readOnly ? immutableInstancesService : objectInstancesService, this);
        }


        public void SetAdd(Guid instanceId, Guid itemTypeId, object item, bool isScalar)
        {
            PerformAdd(instanceId, itemTypeId, item, isScalar, true, false);
        }

        public bool SetContains(Guid instanceId, object item, bool isScalar, bool isReadOnly)
        {
            return PerformContains(instanceId, item, isScalar, true, isReadOnly, false);
        }

        public bool SetRemove(Guid instanceId, object item, bool isScalar)
        {
            return PerformRemove(instanceId, item, isScalar, true, false);
        }
    }
}
