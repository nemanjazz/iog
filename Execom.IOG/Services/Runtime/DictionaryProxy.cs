// -----------------------------------------------------------------------
// <copyright file="DictionaryProxy.cs" company="Execom">
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

namespace Execom.IOG.Services.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Execom.IOG.Services.Facade;

    /// <summary>
    /// Defines a proxy which implements dictionary functionality.
    /// Value type may have descendants.
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal class DictionaryProxy<TKey, TValue> : IDictionary<TKey, TValue>
    {
        public Guid __instanceId__;
        public bool __readOnly__;
        private IRuntimeDictionaryProxyFacade __facade__;
        private bool isScalar;

        public DictionaryProxy(IRuntimeProxyFacade facade, Guid instanceId, Boolean readOnly)
        {
            this.__facade__ = facade;
            this.__readOnly__ = readOnly;
            this.__instanceId__ = instanceId;

            Guid elementTypeId = StaticProxyFacade.Instance.GetTypeId(typeof(TValue));
            this.isScalar = StaticProxyFacade.Instance.IsScalarType(elementTypeId);
        }


        public void Add(TKey key, TValue value)
        {
            if (__readOnly__)
            {
                throw new InvalidOperationException("Operation not allowed for read only dictionary");
            }

            __facade__.DictionaryAdd<TKey, TValue>(__instanceId__, Guid.Empty, isScalar, key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return __facade__.DictionaryContainsKey<TKey>(__instanceId__, key, __readOnly__);
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return __facade__.DictionaryKeys<TKey>(__instanceId__, __readOnly__);
            }
        }

        public bool Remove(TKey key)
        {
            if (__readOnly__)
            {
                throw new InvalidOperationException("Operation not allowed for read only dictionary");
            }

            return __facade__.DictionaryRemove<TKey>(__instanceId__, key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return __facade__.DictionaryTryGetValue<TKey, TValue>(__instanceId__, Guid.Empty, isScalar, __readOnly__, key, out value);
        }

        public ICollection<TValue> Values
        {
            get
            {
                return __facade__.DictionaryValues<TKey, TValue>(__instanceId__, Guid.Empty, isScalar, __readOnly__);
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                return __facade__.DictionaryGetValue<TKey, TValue>(__instanceId__, Guid.Empty, isScalar, __readOnly__, key);
            }
            set
            {
                if (__readOnly__)
                {
                    throw new InvalidOperationException("Operation not allowed for read only dictionary");
                }
                __facade__.DictionarySetValue<TKey, TValue>(__instanceId__, Guid.Empty, isScalar, __readOnly__, key, value);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            __facade__.DictionaryAdd<TKey, TValue>(__instanceId__, Guid.Empty, isScalar, item);
        }

        public void Clear()
        {
            if (__readOnly__)
            {
                throw new InvalidOperationException("Operation not allowed for read only dictionary");
            }
            __facade__.DictionaryClear(__instanceId__);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return __facade__.DictionaryContains<TKey, TValue>(__instanceId__, item, __readOnly__);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            __facade__.DictionaryCopyTo<TKey, TValue>(__instanceId__, Guid.Empty, isScalar, __readOnly__, array, arrayIndex);
        }

        public int Count
        {
            get { return __facade__.DictionaryCount(__instanceId__, __readOnly__); }
        }

        public bool IsReadOnly
        {
            get { return __readOnly__; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (__readOnly__)
            {
                throw new InvalidOperationException("Operation not allowed for read only dictionary");
            }
            return __facade__.DictionaryRemove<TKey, TValue>(__instanceId__, item);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return __facade__.DictionaryGetEnumerator<TKey, TValue>(__instanceId__, Guid.Empty, isScalar, __readOnly__);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
