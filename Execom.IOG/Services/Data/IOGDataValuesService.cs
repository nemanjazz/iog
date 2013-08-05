using System;
using System.Collections.Generic;
using System.Text;
using Execom.IOG.Graph;
using System.Collections;

namespace Execom.IOG.Services.Data
{
    /// <summary>
    /// Represents data structure for IOG model
    /// </summary>
    public class IOGDataStructure
    {
        /// <summary>
        /// Gets or sets name of the data structure
        /// </summary>
        public string DataStructureName { get; set; }

        /// <summary>
        /// Gets or sets type of the data structure
        /// </summary>
        public Type DataStructureType { get; set; }

        /// <summary>
        /// Gets or sets sub-data structure
        /// </summary>
        public IOGDataStructure SubDataStructure { get; set; }

        /// <summary>
        /// Gets or sets dictionary (property name, value) of scalar values
        /// </summary>
        public IDictionary<string, object> ScalarValues { get; set; }

        /// <summary>
        /// Gets or sets object which represents scalar values collection
        /// </summary>
        public ScalarValuesCollectionData ScalarValuesCollection { get; set; }

        /// <summary>
        /// Gets or sets object which represents scalar values dictionary
        /// </summary>
        public ScalarValuesDictionaryData ScalarValuesDictionary { get; set; }

        /// <summary>
        /// Represents scalar values collection
        /// </summary>
        public class ScalarValuesCollectionData
        {
            public string DataStructureMemberName { get; set; }
            public ICollection<object> Values { get; set; }

            public ScalarValuesCollectionData()
            {
                Values = new List<object>();
            }
        }

        /// <summary>
        /// Represents scalar values dictionary
        /// </summary>
        public class ScalarValuesDictionaryData
        {
            public string DataStructureMemberName { get; set; }
            public IDictionary<object, object> Values { get; set; }

            public ScalarValuesDictionaryData()
            {
                Values = new Dictionary<object, object>();
            }
        }
    }
    
    /// <summary>
    /// Service for creating data structure for IOG model
    /// </summary>
    internal class IOGDataValuesService
    {
        private INodeProvider<Guid, object, EdgeData> nodes;
        private TypesService typesService;

        private IOGDataStructure structure = new IOGDataStructure();

        private string previousPropertyName;
        private object previousDictionaryKey;

        /// <summary>
        /// Creates a new intance of ViewDataService
        /// </summary>
        /// <param name="nodes">Node provider instance</param>
        /// <param name="typesService">Types provider instance</param>
        public IOGDataValuesService(INodeProvider<Guid, object, EdgeData> nodes, TypesService typesService)
        {
            this.nodes = nodes;
            this.typesService = typesService;
        }

        /// <summary>
        /// Generates IOG data structure starting from a given node
        /// </summary>
        /// <param name="startingNode">Root node representing starting point for search</param>
        /// <returns>Object representing the data structure of a given model</returns>
        public IOGDataStructure GetDataForSelectedNode(Node<Guid, object, EdgeData> startingNode)
        {
            if (startingNode != null)
            {
                getDataRecursive(startingNode);
            }

            return structure;
        }

        private void getDataRecursive(Node<Guid, object, EdgeData> node)
        {
            /* Use this Node Types:
             * Scalar, Object, Collection, Dictionary, TypeMember
             */

            /* Use this Edge Types:
             * OfType - B node defines the type of node A
             * Property - A node contains B node as a property
             * ListItem - A node contains a list of B nodes
             */

            // TODO: Resolve null node

            if (node == null)
                return;

            if (node.Values.Count > 0)
            {
                structure.ScalarValues = new Dictionary<string, object>();
                addScalarValuesToDataStructure(node);
            }

            if (node.NodeType == NodeType.Collection && isCollectionOfScalarTypes(node))
            {
                structure.ScalarValuesCollection = new IOGDataStructure.ScalarValuesCollectionData();
                structure.ScalarValuesCollection.DataStructureMemberName = previousPropertyName;
                addScalarValueCollectionToDataStructure(node);
            }

            if (node.NodeType == NodeType.Dictionary && isCollectionOfScalarTypes(node))
            {
                structure.ScalarValuesDictionary = new IOGDataStructure.ScalarValuesDictionaryData();
                structure.ScalarValuesDictionary.DataStructureMemberName = previousPropertyName;
                addScalarValueDictionaryToDataStructure(node);
            }

            foreach (var edge in node.Edges.Values)
            {
                if (edge.Data.Semantic == EdgeType.Property)
                {
                    var test = nodes.GetNode(new Guid(edge.Data.Data.ToString()), NodeAccess.Read);
                    previousPropertyName = nodes.GetNode(new Guid(edge.Data.Data.ToString()), NodeAccess.Read).Data.ToString();
                    getDataRecursive(nodes.GetNode(edge.ToNodeId, NodeAccess.Read));
                }
            }
        }

        private void addScalarValuesToDataStructure(Node<Guid, object, EdgeData> node)
        {
            setDataStructureName(node);
            
            foreach (var value in node.Values)
            {
                string memberName = typesService.GetMemberName(typesService.GetMemberTypeId(value.Key), value.Key);
                structure.ScalarValues.Add(memberName, value.Value);
            }
        }

        private void addScalarValueCollectionToDataStructure(Node<Guid, object, EdgeData> node)
        {
            if (node.NodeType == NodeType.Scalar)
            {
                structure.ScalarValuesCollection.Values.Add(node.Data);
            }

            foreach (var edge in node.Edges.Values)
            {   
                if (edge.Data.Semantic == EdgeType.ListItem)
                    addScalarValueCollectionToDataStructure(nodes.GetNode(edge.ToNodeId, NodeAccess.Read));
            }
        }

        private void addScalarValueDictionaryToDataStructure(Node<Guid, object, EdgeData> node)
        {
            if (node.NodeType == NodeType.Scalar)
            {
                structure.ScalarValuesDictionary.Values.Add(previousDictionaryKey, node.Data);
            }

            foreach (var edge in node.Edges.Values)
            {
                if (edge.Data.Semantic == EdgeType.ListItem)
                {
                    previousDictionaryKey = edge.Data.Data;
                    addScalarValueDictionaryToDataStructure(nodes.GetNode(edge.ToNodeId, NodeAccess.Read));
                }
            }
        }

        private void setDataStructureName(Node<Guid, object, EdgeData> node)
        {
            foreach (var edge in node.Edges.Values)
            {
                if (edge.Data.Semantic == EdgeType.OfType)
                    structure.DataStructureName = typesService.GetTypeFromId(edge.ToNodeId).Name;
            }
        }

        private bool isCollectionOfScalarTypes(Node<Guid, object, EdgeData> node)
        {
            foreach (var edge in node.Edges.Values)
            {
                if (edge.Data.Semantic == EdgeType.OfType)
                {
                    var type = typesService.GetTypeFromId(edge.ToNodeId);
                    var genericArguments = type.GetGenericArguments();

                    foreach (var genericArgument in genericArguments)
                    {
                        if (!typesService.IsSupportedScalarTypeName(genericArgument.Name))
                            return false;
                    }

                    return true;
                }
            }
            
            return false;
        }
    }
}
