using System;
using System.Collections.Generic;
using System.Text;
using Execom.IOG.Graph;

namespace Execom.IOG.Services.Data
{
    /// <summary>
    /// Represents data structure for IOG model
    /// </summary>
    public class IOGDataStructure
    {
        /// <summary>
        /// Gets the name of data structure
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the type name of data structure
        /// </summary>
        public string TypeName { get; private set; }

        /// <summary>
        /// Gets the value of data structure
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Gets the current data structure instance in data structure hierarchy
        /// </summary>
        public IOGDataStructure CurrentDataStructure { get; private set; }

        /// <summary>
        /// Gets the collection of substructures of this data structure
        /// </summary>
        public ICollection<IOGDataStructure> SubStructures { get; private set; }

        /// <summary>
        /// Creates a new intance of IOG data structure
        /// </summary>
        public IOGDataStructure()
        {
            CurrentDataStructure = this;
        }

        /// <summary>
        /// Adds new substuructure to the given structure
        /// </summary>
        /// <param name="structure">instance of structure where new substructure is added</param>
        public void AddNewSubStructure(IOGDataStructure structure)
        {
            if (structure == null)
            {
                if (this.SubStructures == null)
                    this.SubStructures = new List<IOGDataStructure>();

                IOGDataStructure returnDataStructure = new IOGDataStructure();
                SubStructures.Add(returnDataStructure);

                CurrentDataStructure = returnDataStructure;
            }
            else
            {
                IOGDataStructure dataStructure = findDataStructure(structure);

                if (dataStructure != null)
                {
                    if (dataStructure.SubStructures == null)
                        dataStructure.SubStructures = new List<IOGDataStructure>();

                    IOGDataStructure returnDataStructure = new IOGDataStructure();
                    dataStructure.SubStructures.Add(returnDataStructure);

                    CurrentDataStructure = returnDataStructure;
                }
            }
        }

        /// <summary>
        /// Adds name to the current data structure
        /// </summary>
        /// <param name="name">name to add</param>
        public void AddName(string name)
        {
            CurrentDataStructure.Name = name;
        }

        /// <summary>
        /// Adds type name to the current data structure
        /// </summary>
        /// <param name="type">type name to add</param>
        public void AddTypeName(string type)
        {
            CurrentDataStructure.TypeName = type;
        }

        public void AddCollectionOrDictionaryType(Type type)
        {
            string collectionTypeName = type.Name.Substring(0, type.Name.IndexOf('`'));
            Type[] collectionGenericTypes = type.GetGenericArguments();
            if (collectionGenericTypes != null && collectionGenericTypes.Length > 0)
            {
                string collectionGenericTypeNames = "";
                foreach (var genericType in collectionGenericTypes)
                {
                    collectionGenericTypeNames += genericType.Name + ", ";
                }
                collectionGenericTypeNames = collectionGenericTypeNames.Trim().Substring(0, collectionGenericTypeNames.Length - 2);
                CurrentDataStructure.TypeName = String.Format("{0}<{1}>", collectionTypeName, collectionGenericTypeNames);
            }
        }

        /// <summary>
        /// Adds value to the current data structure
        /// </summary>
        /// <param name="value">value to add</param>
        public void AddValue(object value)
        {
            CurrentDataStructure.Value = (value == null || String.IsNullOrEmpty(value.ToString())) ? "NULL" : value.ToString();
        }

        public override string ToString()
        {
            return String.Format("({0}) {1}{2}", TypeName, Name, Value == null ? "" : " : " + Value);
        }

        private IOGDataStructure findDataStructure(IOGDataStructure structure)
        {
            foreach (IOGDataStructure dataStructure in SubStructures)
            {
                if (dataStructure.Equals(structure))
                    return dataStructure;

                findDataStructure(dataStructure);
            }

            return null;
        }
    }

    /// <summary>
    /// Service for creating data structure for IOG model
    /// </summary>
    internal class IOGDataValuesService
    {
        private INodeProvider<Guid, object, EdgeData> nodes;
        private TypesService typesService;
        private IOGDataStructure dataStructure;
        private IOGDataStructure currentDataStructure;
        private IOGDataStructure previousDataStructure;
        private int collectionOrDictionaryIndex;
        private string collectionOrDictionaryTypeName;
        private string dictionaryKey;

        /// <summary>
        /// Creates a new intance of IOG data values service
        /// </summary>
        /// <param name="nodes">Node provider instance</param>
        /// <param name="typesService">Types provider instance</param>
        public IOGDataValuesService(INodeProvider<Guid, object, EdgeData> nodes, TypesService typesService)
        {
            this.nodes = nodes;
            this.typesService = typesService;
            dataStructure = new IOGDataStructure();
        }

        /// <summary>
        /// Generates IOG data structure starting from a given node
        /// </summary>
        /// <param name="startingNode">Root node which represent starting point for search</param>
        /// <returns>IOG data structure object</returns>
        public IOGDataStructure GetDataForSelectedNode(Node<Guid, object, EdgeData> startingNode)
        {
            if (startingNode != null)
            {
                dataStructure.AddName("Root");
                setDataStructureType(startingNode, false);
                getDataRecursive(startingNode);
            }

            return dataStructure;
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

            // TODO: Resolve problem with retreiving data type when node is null

            if (node != null)
            {
                if (node.Values.Count > 0)
                    addScalarsToDataStructure(node);

                if (node.NodeType == NodeType.Collection && isCollectionOrDictionaryOfScalarTypes(node))
                {
                    previousDataStructure = currentDataStructure;
                    currentDataStructure = dataStructure.CurrentDataStructure;
                    collectionOrDictionaryIndex = 0;
                    addScalarCollectionToDataStructure(node);
                    collectionOrDictionaryIndex = 0;
                    collectionOrDictionaryTypeName = null;
                    currentDataStructure = previousDataStructure;
                }

                if (node.NodeType == NodeType.Dictionary && isCollectionOrDictionaryOfScalarTypes(node))
                {
                    previousDataStructure = currentDataStructure;
                    currentDataStructure = dataStructure.CurrentDataStructure;
                    collectionOrDictionaryIndex = 0;
                    addScalarDictionaryToDataStructure(node);
                    collectionOrDictionaryIndex = 0;
                    collectionOrDictionaryTypeName = null;
                    dictionaryKey = null;
                    currentDataStructure = previousDataStructure;
                }
                
                foreach (var edge in node.Edges.Values)
                {
                    if (edge.Data.Semantic == EdgeType.Property)
                    {
                        dataStructure.AddNewSubStructure(currentDataStructure);
                        dataStructure.AddName(nodes.GetNode(new Guid(edge.Data.Data.ToString()), NodeAccess.Read).Data.ToString());
                        getDataRecursive(nodes.GetNode(edge.ToNodeId, NodeAccess.Read));
                    }
                }
            }
        }

        private void addScalarsToDataStructure(Node<Guid, object, EdgeData> node)
        {
            foreach (var value in node.Values)
            {
                dataStructure.AddNewSubStructure(currentDataStructure);
                dataStructure.AddName(typesService.GetMemberName(typesService.GetMemberTypeId(value.Key), value.Key));
                var typeId = typesService.GetInstanceTypeId(value.Key);
                var type = typesService.GetTypeFromId(typeId);
                dataStructure.AddTypeName(type.Name);
                dataStructure.AddValue(value.Value);
            }
        }

        private void addScalarCollectionToDataStructure(Node<Guid, object, EdgeData> node)
        {
            setDataStructureType(node, true);

            if (node.NodeType == NodeType.Scalar)
            {
                dataStructure.AddNewSubStructure(currentDataStructure);
                dataStructure.AddName("Item " + collectionOrDictionaryIndex);
                collectionOrDictionaryIndex++;
                dataStructure.AddTypeName(collectionOrDictionaryTypeName);
                dataStructure.AddValue(node.Data);
            }

            foreach (var edge in node.Edges.Values)
            {
                if (edge.Data.Semantic == EdgeType.ListItem)
                {
                    addScalarCollectionToDataStructure(nodes.GetNode(edge.ToNodeId, NodeAccess.Read));
                }
            }
        }

        private void addScalarDictionaryToDataStructure(Node<Guid, object, EdgeData> node)
        {
            setDataStructureType(node, true);

            if (node.NodeType == NodeType.Scalar)
            {
                dataStructure.AddNewSubStructure(currentDataStructure);
                dataStructure.AddName("Item " + collectionOrDictionaryIndex);
                collectionOrDictionaryIndex++;
                dataStructure.AddTypeName(collectionOrDictionaryTypeName);
                dataStructure.AddValue(dictionaryKey + "->" + node.Data);
            }

            foreach (var edge in node.Edges.Values)
            {
                if (edge.Data.Semantic == EdgeType.ListItem)
                {
                    dictionaryKey = edge.Data.Data.ToString();
                    addScalarDictionaryToDataStructure(nodes.GetNode(edge.ToNodeId, NodeAccess.Read));
                }
            }
        }

        private void setDataStructureType(Node<Guid, object, EdgeData> node, bool isCollectionOrDictionary)
        {
            foreach (var edge in node.Edges.Values)
            {
                if (edge.Data.Semantic == EdgeType.OfType)
                {
                    if(!isCollectionOrDictionary)
                        dataStructure.AddTypeName(typesService.GetTypeFromId(edge.ToNodeId).Name);
                    else
                        dataStructure.AddCollectionOrDictionaryType(typesService.GetTypeFromId(edge.ToNodeId));

                    return;
                }
            }
        }

        private bool isCollectionOrDictionaryOfScalarTypes(Node<Guid, object, EdgeData> node)
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
                        {
                            collectionOrDictionaryTypeName = null;
                            return false;
                        }

                        collectionOrDictionaryTypeName += genericArgument.Name + ", ";
                    }

                    collectionOrDictionaryTypeName = collectionOrDictionaryTypeName.Trim().Substring(0, collectionOrDictionaryTypeName.Length - 2);
                    
                    return true;
                }
            }

            return false;
        }
    }
}
