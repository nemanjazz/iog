// -----------------------------------------------------------------------
// <copyright file="IRuntimeDictionaryProxyFacade.cs" company="Execom">
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
    /// Interface which defines methods visible by dictionary proxies.
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal interface IRuntimeDictionaryProxyFacade
    {
        void DictionaryAdd<TKey, TValue>(Guid instanceId, Guid elementTypeId, bool isScalar, TKey key, TValue value);

        bool DictionaryContainsKey<TKey>(Guid instanceId, TKey key, bool readOnly);

        ICollection<TKey> DictionaryKeys<TKey>(Guid instanceId, bool readOnly);

        bool DictionaryRemove<TKey>(Guid instanceId, TKey key);

        bool DictionaryTryGetValue<TKey, TValue>(Guid instanceId, Guid elementTypeId, bool isSclar, bool readOnly, TKey key, out TValue value);

        ICollection<TValue> DictionaryValues<TKey, TValue>(Guid instanceId, Guid elementTypeId, bool isSclar, bool readOnly);

        TValue DictionaryGetValue<TKey, TValue>(Guid instanceId, Guid elementTypeId, bool isSclar, bool readOnly, TKey key);

        void DictionarySetValue<T1, T2>(Guid instanceId, Guid elementType, bool isSclar, bool readOnly, object key, object value);

        void DictionaryAdd<TKey, TValue>(Guid instanceId, Guid elementType, bool isScalar, KeyValuePair<TKey, TValue> item);

        void DictionaryClear(Guid instanceId);

        bool DictionaryContains<TKey, TValue>(Guid instanceId, KeyValuePair<TKey, TValue> item, bool readOnly);

        void DictionaryCopyTo<TKey, TValue>(Guid instanceId, Guid elementTypeId, bool isSclar, bool readOnly, KeyValuePair<TKey, TValue>[] array, int arrayIndex);

        int DictionaryCount(Guid instanceId, bool readOnly);

        bool DictionaryRemove<TKey, TValue>(Guid instanceId, KeyValuePair<TKey, TValue> item);

        IEnumerator<KeyValuePair<TKey, TValue>> DictionaryGetEnumerator<TKey, TValue>(Guid instanceId, Guid elementTypeId, bool isSclar, bool readOnly);
    }
}
