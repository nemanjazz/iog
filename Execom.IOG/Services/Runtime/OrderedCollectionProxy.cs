// -----------------------------------------------------------------------
// <copyright file="OrderedCollectionProxy.cs" company="Execom">
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
    using Execom.IOG.Graph;
    using Execom.IOG.Services.Facade;
    using Execom.IOG.Types;

    /// <summary>
    /// Generic proxy definition for ordered collection of elements which may have descendants
    /// </summary>
    /// <typeparam name="TElementType">Type of collection element</typeparam>
    /// <author>Nenad Sabo</author>
    internal class OrderedCollectionProxy<TElementType> : IOrderedCollection<TElementType>
    {
        public Guid __instanceId__;
        public bool __readOnly__;
        private IRuntimeCollectionProxyFacade __facade__;
        private bool isScalar;

        public OrderedCollectionProxy(IRuntimeProxyFacade facade, Guid instanceId, Boolean readOnly)
        {
            Guid elementTypeId = StaticProxyFacade.Instance.GetTypeId(typeof(TElementType));
            this.isScalar = StaticProxyFacade.Instance.IsScalarType(elementTypeId);
            this.__instanceId__ = instanceId;
            this.__readOnly__ = readOnly;
            this.__facade__ = facade;
        }

        ~OrderedCollectionProxy()
        {
            __facade__.ProxyCollected(this);
        }

        public void Add(TElementType item)
        {
            if (__readOnly__)
            {
                throw new InvalidOperationException("Operation not allowed for read only collection");
            }

            __facade__.CollectionAddOrdered(__instanceId__, Guid.Empty, item, isScalar);
        }

        public void Clear()
        {
            if (__readOnly__)
            {
                throw new InvalidOperationException("Operation not allowed for read only collection");
            }

            __facade__.CollectionClear(__instanceId__);
        }

        public bool Contains(TElementType item)
        {
            return __facade__.CollectionContainsOrdered(__instanceId__, item, isScalar, __readOnly__);
        }

        public void CopyTo(TElementType[] array, int arrayIndex)
        {
            __facade__.CollectionCopyTo<TElementType>(Guid.Empty, __instanceId__, isScalar, __readOnly__, array, arrayIndex);
        }

        public int Count
        {
            get { return __facade__.CollectionCount(__instanceId__, __readOnly__); }
        }

        public bool IsReadOnly
        {
            get { return __readOnly__; }
        }

        public bool Remove(TElementType item)
        {
            if (__readOnly__)
            {
                throw new InvalidOperationException("Operation not allowed for read only collection");
            }

            return __facade__.CollectionRemoveOrdered(__instanceId__, item, isScalar);
        }

        public IEnumerator<TElementType> GetEnumerator()
        {
            return __facade__.CollectionGetEnumerator<TElementType>(Guid.Empty, __instanceId__, isScalar, __readOnly__);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }        
    }
}
