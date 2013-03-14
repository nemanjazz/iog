// -----------------------------------------------------------------------
// <copyright file="CollectionInstancesService.cs" company="Execom">
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
    internal class CollectionInstancesService
    {
        /// <summary>
        /// Node provider
        /// </summary>
        private INodeProvider<Guid, object, EdgeData> provider;
        private TypesService typesService;
        public const int BPlusTreeOrder = 100;

        public CollectionInstancesService(INodeProvider<Guid, object, EdgeData> provider, TypesService typesService)
        {
            this.provider = provider;
            this.typesService = typesService;
        }

        /// <summary>
        /// Initializes new collection instance
        /// </summary>
        /// <param name="typeId">Type ID of collection</param>
        /// <returns>Instance ID</returns>
        public Guid NewInstance(Guid typeId)
        {
            //Determine new id
            Guid id = Guid.NewGuid();
            // Create node as root of B+ tree
            var node = BPlusTreeOperations.CreateRootNode(NodeType.Collection, id);
            // Add node in provider
            provider.SetNode(id, node);
            // Add edge from node to the type of node
            BPlusTreeOperations.InsertEdge(provider, id, new Edge<Guid, EdgeData>(typeId, new EdgeData(EdgeType.OfType, null)), BPlusTreeOrder);
            // Add node in provider
            provider.SetNode(id, node);
            // Return the instance id
            return id;
        }

        internal void AddScalar(Guid instanceId, Guid itemTypeId, object value)
        {
            Guid id = Guid.NewGuid();

            // Create new value node
            var node = new Node<Guid, object, EdgeData>(NodeType.Scalar, value);
            provider.SetNode(id, node);


            var data = Guid.NewGuid();

            BPlusTreeOperations.InsertEdge(provider, instanceId, new Edge<Guid, EdgeData>(id, new EdgeData(EdgeType.ListItem, data)), BPlusTreeOrder);
        }

        internal void AddScalar(Guid instanceId, Guid itemTypeId, object value, object key)
        {
            Guid id = Guid.NewGuid();

            // Create new value node
            var node = new Node<Guid, object, EdgeData>(NodeType.Scalar, value);
            provider.SetNode(id, node);

            BPlusTreeOperations.InsertEdge(provider, instanceId, new Edge<Guid, EdgeData>(id, new EdgeData(EdgeType.ListItem, key)), BPlusTreeOrder);
        }

        internal void AddReference(Guid instanceId, Guid referenceId)
        {
            var data = Guid.NewGuid();

            BPlusTreeOperations.InsertEdge(provider, instanceId, new Edge<Guid, EdgeData>(referenceId, new EdgeData(EdgeType.ListItem, data)), BPlusTreeOrder);
        }

        internal void AddReference(Guid instanceId, Guid referenceId, object key)
        {            
            BPlusTreeOperations.InsertEdge(provider, instanceId, new Edge<Guid, EdgeData>(referenceId, new EdgeData(EdgeType.ListItem, key)), BPlusTreeOrder);
        }

        internal void Clear(Guid instanceId)
        {
            var removalKeys= new Collection<EdgeData>();
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

        internal bool ContainsScalar(Guid instanceId, object value)
        {
            using (var enumerator = GetEnumerator(instanceId))
            {
                while (enumerator.MoveNext())
                {
                    var node = provider.GetNode(enumerator.Current.ToNodeId, NodeAccess.Read);
                    if (node.Data.Equals(value))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        internal bool ContainsScalar(Guid instanceId, object value, object key)
        {
            Edge<Guid, EdgeData> edge = null;
            return BPlusTreeOperations.TryFindEdge(provider, instanceId, new EdgeData(EdgeType.ListItem, key), out edge);
        }

        internal bool ContainsReference(Guid instanceId, Guid referenceId)
        {
            using (var enumerator = GetEnumerator(instanceId))
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.ToNodeId.Equals(referenceId))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        internal bool ContainsReference(Guid instanceId, Guid referenceId, object key)
        {
            Edge<Guid, EdgeData> edge = null;
            return BPlusTreeOperations.TryFindEdge(provider, instanceId, new EdgeData(EdgeType.ListItem, key), out edge);
        }

        internal int Count(Guid instanceId)
        {
            return BPlusTreeOperations.Count(provider, instanceId, EdgeType.ListItem);
        }

        internal long MaxOrderedIdentifier(Guid instanceId)
        {
            if (Count(instanceId) == 0)
            {
                return (long)0;
            }
            else
            {
                return (long)BPlusTreeOperations.RightEdge(provider, provider.GetNode(instanceId, NodeAccess.Read)).Data.Data;
            }
        }

        internal bool RemoveScalar(Guid instanceId, object value)
        {
            using (var enumerator = GetEnumerator(instanceId))
            {
                while (enumerator.MoveNext())
                {
                    var node = provider.GetNode(enumerator.Current.ToNodeId, NodeAccess.Read);
                    if (node.Data.Equals(value))
                    {
                        return BPlusTreeOperations.RemoveEdge(provider, instanceId, enumerator.Current.Data, BPlusTreeOrder);
                    }
                }

                return false;
            }
        }

        internal bool RemoveScalar(Guid instanceId, object value, object key)
        {
            return BPlusTreeOperations.RemoveEdge(provider, instanceId, new EdgeData(EdgeType.ListItem, key), BPlusTreeOrder);
        }

        internal bool RemoveReference(Guid instanceId, Guid referenceId)
        {
            using (var enumerator = GetEnumerator(instanceId))
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.ToNodeId.Equals(referenceId))
                    {
                        return BPlusTreeOperations.RemoveEdge(provider, instanceId, enumerator.Current.Data, BPlusTreeOrder);
                    }
                }

                return false;
            }
        }

        internal bool RemoveReference(Guid instanceId, Guid referenceId, object key)
        {
            return BPlusTreeOperations.RemoveEdge(provider, instanceId, new EdgeData(EdgeType.ListItem, key), BPlusTreeOrder);
        }

        internal bool IsCollectionInstance(Guid referenceId)
        {
            var data = provider.GetNode(referenceId, NodeAccess.Read).Data;
            return data != null && data is Guid && (data.Equals(BPlusTreeOperations.InternalNodeData) || data.Equals(BPlusTreeOperations.LeafNodeData));
        }

        internal Guid GetInstanceTypeId(Guid referenceId)
        {
            Edge<Guid, EdgeData> edge = null;
            if (BPlusTreeOperations.TryFindEdge(provider, referenceId, new EdgeData(EdgeType.OfType, null), out edge))
            {
                return edge.ToNodeId;
            }
            else
            {
                return Guid.Empty;
            }
        }

        internal IEnumerator<Edge<Guid, EdgeData>> GetEnumerator(Guid instanceId)
        {
            return BPlusTreeOperations.GetEnumerator(provider, instanceId, EdgeType.ListItem);
        }

        internal bool TryFindReferenceByKey(Guid instanceId, object key, out Guid referenceId)
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
    }
}
