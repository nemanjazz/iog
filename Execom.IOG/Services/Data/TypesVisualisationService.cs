using System;
using System.Collections.Generic;
using System.Text;
using Execom.IOG.TypeVisual;
using Execom.IOG.Graph;
using Execom.IOG.Storage;
using Execom.IOG.Providers;

namespace Execom.IOG.Services.Data
{
    public class TypesVisualisationService
    {
        #region Private fields

        /// <summary>
        /// Node provider for this context, contains all nodes in the data storage
        /// </summary>
        private INodeProvider<Guid, object, EdgeData> provider;
        /// <summary>
        /// Service for type manipulations
        /// </summary>
        private TypesService typesService;
        /// <summary>
        /// Object serialization service
        /// </summary>
        private ObjectSerializationService objectSerializationService = new ObjectSerializationService();

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="storage">The storage from which the types needed for visualisation are loaded</param>
        public TypesVisualisationService(INodeProvider<Guid, object, EdgeData> provider)
        {
            this.provider = provider;
            typesService = new TypesService(provider);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="storage">The storage from which the types needed for visualisation are loaded</param>
        public TypesVisualisationService(IKeyValueStorage<Guid, object> storage)
        {
            if (storage is ISerializingStorage)
            {
                (storage as ISerializingStorage).Serializer = objectSerializationService;
            }

            var storageProvider = new DirectNodeProviderSafe<Guid, object, EdgeData>(storage, storage is IForceUpdateStorage);
            provider = new CachedReadNodeProvider<Guid, object, EdgeData>(storageProvider, new DirectNodeProviderSafe<Guid, object, EdgeData>(new LimitedMemoryStorageSafe<Guid, object>(Properties.Settings.Default.DataCacheMinimumCount, Properties.Settings.Default.DataCacheMaximumCount), false));
            typesService = new TypesService(provider);
            typesService.InitializeTypeSystem(null);
            objectSerializationService.TypesService = typesService;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Returns the Id of the type that represents the root of the database (the representation of the Data Model)
        /// </summary>
        /// <returns></returns>
        public Guid GetRootTypeId()
        {
            var snapshotRootNode = provider.GetNode(Constants.SnapshotsNodeId, NodeAccess.Read);
            var snapshotNode = provider.GetNode(snapshotRootNode.Edges.Values[0].ToNodeId, NodeAccess.Read);
            var objectNode = provider.GetNode(snapshotNode.Edges.Values[0].ToNodeId, NodeAccess.Read);
            foreach (var edge in objectNode.Edges.Values)
            {
                if (edge.Data.Semantic.Equals(EdgeType.OfType))
                {
                    return edge.ToNodeId;
                }
            }

            return Guid.Empty;
        }

        /// <summary>
        /// Returns the name of the type that represents the root of the database (the representation of the Data Model)
        /// </summary>
        /// <returns></returns>
        public string getRootTypeName()
        {
            var typeNode = provider.GetNode(GetRootTypeId(), NodeAccess.Read);
            return TypeVisualUtilities.GetTypeNameFromAssemblyName((string)typeNode.Data);
        }

        /// <summary>
        /// Gets the information of all types in the context (with the rootTypeId as the root type, doesn't actually have to be the root entity), and puts them in a dictionary, where the key is the name of the type.
        /// </summary>
        /// <returns></returns>
        public IDictionary<String, TypeVisualUnit> GetTypeVisualUnits(Guid rootTypeId)
        {
            IDictionary<String, TypeVisualUnit> retVal = new Dictionary<String, TypeVisualUnit>();
            List<string> ignoreTypeNames = new List<string>();
            if (!rootTypeId.Equals(Guid.Empty))
            {
                GetTypeVisualUnitsRecursive(rootTypeId, retVal, ignoreTypeNames);
            }
            return retVal;
        }

        /// <summary>
        /// Creates and returns a content for a GraphViz (.gv) file (using DOT language), for the visualisation of types that are in the storage.
        /// For this purpose, a Context is created only for this purpose, and cannot be used for any other.
        /// </summary>
        /// <param name="storage">Storage from where the types are extracted</param>
        /// <returns>content for the GraphViz file</returns>
        public String GetGraphVizContentFromStorage(IKeyValueStorage<Guid, object> storage)
        {
            String graphVizContent;
            IDictionary<String, TypeVisualUnit> typeUnits = GetTypeVisualUnits(GetRootTypeId());
            GVTemplate template = new GVTemplate(typeUnits);
            graphVizContent = template.TransformText();
            return graphVizContent;
        }

        /// <summary>
        /// Creates and returns a content for a GraphViz (.gv) file (using DOT language), for the visualisation of types that are in the storage.
        /// <para>For this purpose, a Context is created only for this purpose, and cannot be used for any other.</para>
        /// </summary>
        /// <param name="storage">Storage from where the types are extracted</param>
        /// <returns>content for the GraphViz file</returns>
        public String GetGraphVizContentFromStorage(string typeName, IKeyValueStorage<Guid, object> storage)
        {
            String graphVizContent;
            Guid typeId = GetIdFromTypeName(typeName);
            if (typeId.Equals(Guid.Empty))
                throw new Exception("Type with the following name : " + typeName + " doesn't exist in this Context");
            IDictionary<String, TypeVisualUnit> typeUnits = GetTypeVisualUnits(typeId);
            GVTemplate template = new GVTemplate(typeUnits);
            graphVizContent = template.TransformText();
            return graphVizContent;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Recursively fills the units dictionary with a TypeVisualisationUnit of the Guid - typeNodeId.
        /// </summary>
        /// <param name="typeNodeId"></param>
        /// <param name="units"></param>
        /// <param name="ignoreTypeNames"></param>
        private void GetTypeVisualUnitsRecursive(Guid typeNodeId, IDictionary<String, TypeVisualUnit> units, ICollection<string> ignoreTypeNames)
        {
            TypeVisualUnit unit = null;
            string typeName = null;
            ICollection<TypeVisualProperty> scalarProperties = new List<TypeVisualProperty>();
            ICollection<TypeVisualProperty> nonScalarProperties = new List<TypeVisualProperty>();

            if (!typeNodeId.Equals(Guid.Empty))
            {
                var typeNode = provider.GetNode(typeNodeId, NodeAccess.Read);
                typeName = TypeVisualUtilities.GetTypeNameFromAssemblyName((string)typeNode.Data);
                if (!ignoreTypeNames.Contains(typeName))
                {
                    ignoreTypeNames.Add(typeName);
                    foreach (var edge in typeNode.Edges.Values)
                    {
                        if (edge.Data.Semantic.Equals(EdgeType.Property))
                        {
                            //setting name and type
                            bool isScalar;
                            var nodeProperty = provider.GetNode(edge.ToNodeId, NodeAccess.Read);
                            var nodePropertyType = provider.GetNode(nodeProperty.Edges.Values[0].ToNodeId, NodeAccess.Read);
                            string propertyTypeName = TypeVisualUtilities.GetTypeNameFromAssemblyName((string)nodePropertyType.Data);

                            string collectionKey, collectionValue;
                            PropertyCollectionType collectionType = TypeVisualUtilities.CheckIfCollectionOrDictionary(propertyTypeName,
                                out collectionKey, out collectionValue);

                            //checking if property is a collection and if property is scalar.
                            if (collectionType != PropertyCollectionType.NotACollection)
                            {
                                if (!typesService.IsSupportedScalarTypeName(collectionValue))
                                {
                                    Guid genericArgumentTypeId = GetIdFromTypeName(collectionValue);
                                    if (!genericArgumentTypeId.Equals(Guid.Empty))
                                    {
                                        //ignoreTypeNames.Add(typeName);
                                        GetTypeVisualUnitsRecursive(genericArgumentTypeId, units, ignoreTypeNames);
                                    }
                                    isScalar = false;
                                }
                                else
                                    isScalar = true;
                                propertyTypeName = collectionValue;
                            }
                            else if (!typesService.IsSupportedScalarTypeName(propertyTypeName))
                            {
                                //ignoreTypeNames.Add(typeName);
                                GetTypeVisualUnitsRecursive(nodeProperty.Edges.Values[0].ToNodeId, units, ignoreTypeNames);
                                isScalar = false;
                            }
                            else
                                isScalar = true;

                            //setting additional property attributes
                            bool isImmutable, isPrimaryKey;
                            PropertyAttribute propAttribute;
                            isImmutable = edge.Data.Flags.Equals(EdgeFlags.Permanent);
                            isPrimaryKey = nodeProperty.Values.ContainsKey(Constants.TypeMemberPrimaryKeyId);
                            if (isImmutable && isPrimaryKey)
                                propAttribute = PropertyAttribute.PrimaryKeyAndImmutableProperty;
                            else if (isImmutable)
                                propAttribute = PropertyAttribute.ImmutableProperty;
                            else if (isPrimaryKey)
                                propAttribute = PropertyAttribute.PrimaryKeyProperty;
                            else
                                propAttribute = PropertyAttribute.None;

                            //adding the property to either scalar or non-scalar lists.
                            TypeVisualProperty property = new TypeVisualProperty((string)nodeProperty.Data, propertyTypeName,
                                propAttribute, collectionType, collectionKey);

                            if (isScalar)
                                scalarProperties.Add(property);
                            else
                                nonScalarProperties.Add(property);

                        }
                        else if (edge.Data.Semantic.Equals(EdgeType.Contains))
                        {
                            var nodeProperty = provider.GetNode(edge.ToNodeId, NodeAccess.Read);
                            TypeVisualProperty property = new TypeVisualProperty((string)nodeProperty.Data, TypeVisualProperty.EnumType,
                                PropertyAttribute.None, PropertyCollectionType.NotACollection, "");
                            scalarProperties.Add(property);
                        }
                    }
                    unit = new TypeVisualUnit(typeName, scalarProperties, nonScalarProperties);
                    units.Add(unit.Name, unit);
                }
            }
        }

        /// <summary>
        /// Returns the Id of the node type with the specified name. It looks in the types pointed by the TypesRoot node.
        /// </summary>
        /// <param name="typeName">Type name to compare possible results to.</param>
        /// <returns>The Id of the node pointing to the type. Empty if node is not found</returns>
        private Guid GetIdFromTypeName(string typeName)
        {
            foreach (var edge in provider.GetNode(Constants.TypesNodeId, NodeAccess.Read).Edges.Values)
            {
                if (edge.Data.Semantic.Equals(EdgeType.Contains))
                {
                    var node = provider.GetNode(edge.ToNodeId, NodeAccess.Read);
                    string nodeData = TypeVisualUtilities.GetTypeNameFromAssemblyName((string)node.Data);
                    if (typeName.Equals(nodeData))
                        return edge.ToNodeId;
                }
            }

            return Guid.Empty;
        }

        #endregion
    }
}
