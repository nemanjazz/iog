// -----------------------------------------------------------------------
// <copyright file="PlainInstancesService.cs" company="Execom">
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

    /// <summary>
    /// Service with operations for management of plain instances (simple objects with properties)
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal class ObjectInstancesService
    {
        /// <summary>
        /// Node provider
        /// </summary>
        private INodeProvider<Guid, object, EdgeData> provider;

        // <summary>
        /// Service which allows access to type information
        /// </summary>
        private TypesService typesService;

        /// <summary>
        /// Initializes a new instance of the PlainInstancesService class
        /// </summary>
        /// <param name="provider">Node provider</param>
        /// <param name="typesService">Types service</param>
        public ObjectInstancesService(INodeProvider<Guid, object, EdgeData> provider, TypesService typesService)
        {
            this.provider = provider;
            this.typesService = typesService;
        }        

        /// <summary>
        /// Add instance of type
        /// </summary>
        /// <param name="typeId">Type id we wish to instanciate</param>
        /// <returns>Instance ID</returns>
        public Guid NewInstance(Guid typeId)
        {
            //Determine new id
            Guid id = Guid.NewGuid();
            // Create node
            var node = new Node<Guid, object, EdgeData>(NodeType.Object, null);     
            // Node is not commited
            node.Commited = false;
            // Add edge from node to the type of node
            node.AddEdge(new Edge<Guid, EdgeData>(typeId, new EdgeData(EdgeType.OfType, null)));
            // Initialize instance to default values
            InitializeInstance(node, typeId);
            // Add node in provider
            provider.SetNode(id, node);
            // Return the instance id
            return id;
        }

        /// <summary>
        /// Returns scalar value for instance member
        /// </summary>
        /// <param name="instanceId">Instance ID</param>
        /// <param name="memberId">Member ID</param>
        /// <returns>Scalar value</returns>
        public object GetScalarInstanceMember(Guid instanceId, Guid memberId)
        {
            // Go into the node and look for member
            return provider.GetNode(instanceId, NodeAccess.Read).Values[memberId];
        }

        public object GetScalarInstanceValue(Guid valueInstanceId)
        {
            return provider.GetNode(valueInstanceId, NodeAccess.Read).Data;
        }

        /// <summary>
        /// Returns ID of instance for member
        /// </summary>
        /// <param name="instanceId">Instance ID</param>
        /// <param name="memberId">Member ID</param>
        /// <param name="isPermanent">Determines if edge is permanent</param>
        /// <returns>Id of referenced instance</returns>
        public Guid GetReferenceInstanceMember(Guid instanceId, Guid memberId, out bool isPermanent)
        {
            var node = provider.GetNode(instanceId, NodeAccess.Read);
            var edge = node.FindEdge(new EdgeData(EdgeType.Property, memberId));
            isPermanent = (edge.Data.Flags & EdgeFlags.Permanent) == EdgeFlags.Permanent && node.Commited;
            return edge.ToNodeId;
        }

        /// <summary>
        /// Sets scalar value for instance member
        /// </summary>
        /// <param name="instanceId">Instance ID</param>
        /// <param name="memberId">Member ID</param>
        /// <param name="value">Scalar value</param>
        public void SetScalarInstanceMember(Guid instanceId, Guid memberId, object value)
        {
            var node = provider.GetNode(instanceId, NodeAccess.ReadWrite);

            if (node.Values.ContainsKey(memberId))
            {
                node.Values[memberId] = value;
            }
            else
            {
                node.Values.Add(memberId, value);
            }

            provider.SetNode(instanceId, node);
        }

        /// <summary>
        /// Sets new value to the reference property
        /// </summary>
        /// <param name="instanceId">Instance ID</param>
        /// <param name="memberId">Member ID</param>
        /// <param name="referenceId">Id of referenced instance</param>
        public void SetReferenceInstanceMember(Guid instanceId, Guid memberId, Guid referenceId)
        {
            // Set new reference
            var node = provider.GetNode(instanceId, NodeAccess.ReadWrite);
            node.SetEdgeToNode(new EdgeData(EdgeType.Property, memberId), referenceId);
            provider.SetNode(instanceId, node);
        }

        /// <summary>
        /// Initializes instance properties to default values
        /// </summary>
        /// <param name="instanceId">Instance ID to initialize</param>
        /// <param name="typeId">Type ID</param>
        private void InitializeInstance(Node<Guid, object, EdgeData> instance, Guid typeId)
        {
            foreach (var edge in typesService.GetTypeEdges(typeId))
            {
                if (edge.Data.Semantic == EdgeType.Property)
                {
                    Guid memberId = edge.ToNodeId;
                    Guid memberTypeId = typesService.GetMemberTypeId(memberId);
                    if (typesService.IsScalarType(memberTypeId))
                    {
                        var value = typesService.GetDefaultPropertyValue(memberTypeId);
                        instance.Values.Add(memberId, value);
                    }
                    else
                    {
                        // Initialize as permanent if it was declared so
                        bool isPermanentEdge = (edge.Data.Flags & EdgeFlags.Permanent) == EdgeFlags.Permanent;
                        // Initialize reference types with null value
                        instance.AddEdge(new Edge<Guid, EdgeData>(Constants.NullReferenceNodeId, new EdgeData(EdgeType.Property, isPermanentEdge ? EdgeFlags.Permanent : EdgeFlags.None, memberId)));
                    }
                }
            }
        }

        /// <summary>
        /// Returns type ID of given instance ID
        /// </summary>
        /// <param name="instanceId">Instance ID</param>
        /// <returns>Type ID</returns>
        public Guid GetInstanceTypeId(Guid instanceId)
        {
            return provider.GetNode(instanceId, NodeAccess.Read).FindEdge(new EdgeData(EdgeType.OfType, null)).ToNodeId;
        }

        /// <summary>
        /// Sets reference of instance as immutable
        /// </summary>
        /// <param name="instanceId">Instance ID</param>
        /// <param name="memberId">Member ID to set as immutable</param>
        public void SetImmutable(Guid instanceId, Guid memberId)
        {
            // Get node
            var node = provider.GetNode(instanceId, NodeAccess.ReadWrite);
            // Find edge by member
            var edge = node.FindEdge(new EdgeData(EdgeType.Property, memberId));
            // Remove existing edge
            node.Edges.Remove(edge.Data);
            // Create new edge with permanent flag
            Edge<Guid, EdgeData> newEdge = new Edge<Guid, EdgeData>(edge.ToNodeId, new EdgeData(edge.Data.Semantic, EdgeFlags.Permanent, edge.Data.Data));
            // Add to node
            node.AddEdge(newEdge);
            // Save node
            provider.SetNode(instanceId, node);
        }
    }
}
