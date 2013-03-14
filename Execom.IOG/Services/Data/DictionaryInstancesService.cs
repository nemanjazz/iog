// -----------------------------------------------------------------------
// <copyright file="DictionaryInstancesService.cs" company="Execom">
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

namespace Execom.IOG.Services.Data
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Execom.IOG.Graph;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Service for collection instance manipulations
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal class DictionaryInstancesService
    {
        /// <summary>
        /// Node provider
        /// </summary>
        private INodeProvider<Guid, object, EdgeData> provider;
        private TypesService typesService;
        public const int BPlusTreeOrder = 100;

        public DictionaryInstancesService(INodeProvider<Guid, object, EdgeData> provider, TypesService typesService)
        {
            this.provider = provider;
            this.typesService = typesService;
        }

        /// <summary>
        /// Initializes new dictionary instance
        /// </summary>
        /// <param name="typeId">Type ID of dictionary</param>
        /// <returns>Instance ID</returns>
        public Guid NewInstance(Guid typeId)
        {
            //Determine new id
            Guid id = Guid.NewGuid();
            // Create node as root of B+ tree
            var node = BPlusTreeOperations.CreateRootNode(NodeType.Dictionary, id);
            // Add node in provider
            provider.SetNode(id, node);
            // Add edge from node to the type of node
            BPlusTreeOperations.InsertEdge(provider, id, new Edge<Guid, EdgeData>(typeId, new EdgeData(EdgeType.OfType, null)), BPlusTreeOrder);
            // Add node in provider
            provider.SetNode(id, node);
            // Return the instance id
            return id;
        }

        internal void AddScalar(Guid instanceId, Guid itemTypeId, object key, object value)
        {
            Guid id = Guid.NewGuid();

            // Create new value node
            var node = new Node<Guid, object, EdgeData>(NodeType.Scalar, value);
            provider.SetNode(id, node);

            BPlusTreeOperations.InsertEdge(provider, instanceId, new Edge<Guid, EdgeData>(id, new EdgeData(EdgeType.ListItem, key)), BPlusTreeOrder);
        }

        internal void AddReference(Guid instanceId, object key, Guid referenceId)
        {
            BPlusTreeOperations.InsertEdge(provider, instanceId, new Edge<Guid, EdgeData>(referenceId, new EdgeData(EdgeType.ListItem, key)), BPlusTreeOrder);
        }

        internal bool ContainsKey(Guid instanceId, object key)
        {
            Edge<Guid, EdgeData> edge = null;
            return BPlusTreeOperations.TryFindEdge(provider, instanceId, new EdgeData(EdgeType.ListItem, key), out edge);
        }

        internal IEnumerator<Edge<Guid, EdgeData>> GetEnumerator(Guid instanceId)
        {
            return BPlusTreeOperations.GetEnumerator(provider, instanceId, EdgeType.ListItem);
        }

        internal bool Remove(Guid instanceId, object key)
        {
            return BPlusTreeOperations.RemoveEdge(provider, instanceId, new EdgeData(EdgeType.ListItem, key), BPlusTreeOrder);
        }

        internal bool TryGetScalar(Guid instanceId, object key, out object value)
        {
            Edge<Guid, EdgeData> edge = null;
            if (BPlusTreeOperations.TryFindEdge(provider, instanceId, new EdgeData(EdgeType.ListItem, key), out edge))
            {
                value = provider.GetNode(edge.ToNodeId, NodeAccess.Read).Data;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        internal bool TryGetReference(Guid instanceId, object key, out Guid referenceId)
        {
            Edge<Guid, EdgeData> edge = null;
            if (BPlusTreeOperations.TryFindEdge(provider, instanceId, new EdgeData(EdgeType.ListItem, key), out edge))
            {
                referenceId = edge.ToNodeId;
                return true;
            }
            else
            {
                referenceId = Guid.Empty;
                return false;
            }
        }

        internal void SetScalar(Guid instanceId, Guid itemTypeId, object key, object value)
        {
            Guid id = Guid.NewGuid();

            // Create new value node
            var node = new Node<Guid, object, EdgeData>(NodeType.Scalar, value);
            provider.SetNode(id, node);

            if (!BPlusTreeOperations.TrySetEdgeToNode(provider, instanceId, new EdgeData(EdgeType.ListItem, key), id))
            {
                throw new KeyNotFoundException("Item not found with the specified key");
            }
        }      

        internal void SetReference(Guid instanceId, object key, Guid referenceId)
        {            
            if (!BPlusTreeOperations.TrySetEdgeToNode(provider, instanceId, new EdgeData(EdgeType.ListItem, key), referenceId))
            {
                throw new KeyNotFoundException("Item not found with the specified key");
            }
        }

        internal void Clear(Guid instanceId)
        {
            var removalKeys = new Collection<EdgeData>();
            using (var enumerator = GetEnumerator(instanceId))
            {
                while (enumerator.MoveNext())
                {
                    removalKeys.Add(enumerator.Current.Data);
                }
            }

            foreach (var key in removalKeys)
            {
                BPlusTreeOperations.RemoveEdge(provider, instanceId, key, BPlusTreeOrder);
            }
        }

        internal int Count(Guid instanceId)
        {
            return BPlusTreeOperations.Count(provider, instanceId, EdgeType.ListItem);
        }
    }
}
