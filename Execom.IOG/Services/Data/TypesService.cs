// -----------------------------------------------------------------------
// <copyright file="TypesService.cs" company="Microsoft">
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
    using System.Reflection;
    using System.Collections;
    using System.Collections.ObjectModel;
    using Execom.IOG.Services.Runtime;
    using System.Diagnostics;
    using Execom.IOG.Attributes;
    using Execom.IOG.Types;
    using Execom.IOG.TypeVisual;

    /// <summary>
    /// Methods for type manipulation on the node provider
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal class TypesService
    {     
        /// <summary>
        /// List of supported scalar types
        /// </summary>
        private Type[] supportedScalarTypes = new Type[] { typeof(Boolean), typeof(String), typeof(Int32), typeof(Int64), typeof(Double), typeof(DateTime), typeof(Guid), typeof(TimeSpan), typeof(Byte), typeof(Char) };

        /// <summary>
        /// Contains hash table which contains (typeId, type) pairs for all scalar types
        /// </summary>
        private Hashtable scalarTypesTable = new Hashtable();

        /// <summary>
        /// Node provider
        /// </summary>
        private INodeProvider<Guid, object, EdgeData> provider;

        /// <summary>
        /// Mapping between type and type identifier
        /// </summary>
        private Dictionary<Type, Guid> typeToIdMapping;

        /// <summary>
        /// Mapping between type identifier and type
        /// </summary>
        private Dictionary<Guid, Type> typeIdToTypeMapping;

        /// <summary>
        /// Collection of collection types
        /// </summary>
        private Dictionary<Guid, Type> collectionTypesTable = new Dictionary<Guid, Type>();

        /// <summary>
        /// Collection of dictionary types
        /// </summary>
        private Dictionary<Guid, Type> dictionaryTypesTable = new Dictionary<Guid, Type>();

        /// <summary>
        /// Initializes a new instance of the TypesService class
        /// </summary>
        /// <param name="provider">Node provider which contains nodes</param>
        public TypesService(INodeProvider<Guid, object, EdgeData> provider)
        {
            this.provider = provider;
        }

        /// <summary>
        /// Defines new type, it shall add used sub-types, such as member types
        /// </summary>
        /// <param name="t">Type to add</param>
        /// <returns>Type Id</returns>
        public Guid AddType(Type t)
        {
            ValidateType(t);

            // Try finding the type first
            Guid typeId = GetTypeId(t);

            // Type not found?
            if (typeId == Guid.Empty)
            {
                // Assign new ID
                typeId = Guid.NewGuid();
                // Create new node
                var node = new Node<Guid, object, EdgeData>(NodeType.Type, GetTypeName(t));
                // Types "group" node ---> The type
                var typesNode = provider.GetNode(Constants.TypesNodeId, NodeAccess.ReadWrite);
                typesNode.AddEdge(new Edge<Guid, EdgeData>(typeId, new EdgeData(EdgeType.Contains, typeId)));
                provider.SetNode(Constants.TypesNodeId, typesNode);
                // Add node to collection
                provider.SetNode(typeId, node);

                if (t.IsInterface)
                {
                    // Extract properties
                    var properties = new Collection<PropertyInfo>();
                    Utils.ExtractProperties(t, properties);

                    // Property member types are also added
                    foreach (PropertyInfo p in properties)
                    {
                        bool isPermanent = p.GetCustomAttributes(typeof(ImmutableAttribute), false).Length == 1;

                        bool isPrimaryKey = p.GetCustomAttributes(typeof(PrimaryKeyAttribute), false).Length == 1;

                        bool saveParentNodes = p.GetCustomAttributes(typeof(StoreParentNodesAttribute), false).Length == 1;

                        // Add the type
                        AddType(p.PropertyType);
                        // Add the member to type 
                        Guid memberId = AddTypeMember(p.Name, p.PropertyType, isPrimaryKey);

                        //Add edge to the member
                        node.AddEdge(new Edge<Guid, EdgeData>(memberId, new EdgeData(EdgeType.Property, CalculateEdgeFlags(isPermanent, saveParentNodes), memberId)));
                    }

                    Type collectionType=null;
                    Type dictionaryType= null;

                    // Find all base interfaces and add them too, but not for collections/dictionary
                    if (!Utils.IsCollectionType(t, ref collectionType) &&
                        !Utils.IsDictionaryType(t, ref dictionaryType))
                    {
                        foreach (var baseType in t.GetInterfaces())
                        {
                            var id = AddType(baseType);
                            node.AddEdge(new Edge<Guid, EdgeData>(id, new EdgeData(EdgeType.OfType, id)));
                        }

                        provider.SetNode(typeId, node);
                    }
                    else
                    {
                        foreach (var baseType in t.GetGenericArguments())
                        {
                            AddType(baseType);
                        }
                    }                                       
                }                                               

                // For types with constant list of values, add the values
                var values = GetConstantValues(t);

                if (values.Count > 0)
                {
                    foreach (var value in values)
                    {
                        Guid valueId = Guid.NewGuid();
                        var valueNode = new Node<Guid, object, EdgeData>(NodeType.Scalar, value.ToString());
                        provider.SetNode(valueId, valueNode);
                        node.AddEdge(new Edge<Guid, EdgeData>(valueId, new EdgeData(EdgeType.Contains, value.ToString())));
                    }
                }

                // Add node to collection
                provider.SetNode(typeId, node);
            }

            return typeId;
        }

        /// <summary>
        /// Returns list of constant values for given type.
        /// Constant values are used for representing enumerations(non-flagged) and boolean values with global nodes.
        /// This makes for smaller data footprint for these types of data.
        /// </summary>
        /// <param name="t">Type</param>
        /// <returns>List of possible values</returns>
        private Collection<object> GetConstantValues(Type t)
        {
            Collection<object> list = new Collection<object>();

            // Enumerations with no flag attribute are treated as constant list
            if (t.IsEnum && t.GetCustomAttributes(typeof(FlagsAttribute), false).Length == 0)
            {
                foreach (var item in Enum.GetValues(t))
                {
                    list.Add(item);
                }
            }

            // Boolean type is treated as constant list
            if (t.Equals(typeof(bool)))
            {
                list.Add(false);
                list.Add(true);
            }

            return list;
        }

        /// <summary>
        /// Adds supported basic scalar types
        /// </summary>
        public void EnsureBasicScalarTypes()
        {
            foreach (var t in supportedScalarTypes)
            {
                AddType(t);                
            }
        }

        /// <summary>
        /// Determines if given type id is a scalar
        /// </summary>
        /// <param name="typeId">Type id</param>
        /// <returns>True if scalar</returns>
        public bool IsScalarType(Guid typeId)
        {
            return scalarTypesTable.ContainsKey(typeId);
        }

        /// <summary>
        /// Determines if given type id is a collection
        /// </summary>
        /// <param name="typeId">Type id</param>
        /// <returns>True if collection</returns>
        public bool IsCollectionType(Guid typeId)
        {
            return collectionTypesTable.ContainsKey(typeId);
        }

        /// <summary>
        /// Determines if given type id is a dictionary
        /// </summary>
        /// <param name="typeId">Type id</param>
        /// <returns>True if dictionary</returns>
        public bool IsDictionaryType(Guid typeId)
        {
            return dictionaryTypesTable.ContainsKey(typeId);
        }

        /// <summary>
        /// Determines type id for given type
        /// </summary>
        /// <param name="t">Type to search for</param>
        /// <returns>Identifier of type, or empty Guid if type was not defined</returns>
        public Guid GetTypeId(Type t)
        {
            var name = GetTypeName(t);
            foreach (var edge in provider.GetNode(Constants.TypesNodeId, NodeAccess.Read).Edges.Values)
            {
                if (edge.Data.Semantic == EdgeType.Contains)
                {
                    Guid candidateNodeId = edge.ToNodeId;
                    var node = provider.GetNode(candidateNodeId, NodeAccess.Read);

                    if (node.Data.Equals(name))
                    {
                        return candidateNodeId;
                    }
                }
            }

            return Guid.Empty;
        }

        /// <summary>
        /// Determines type id for given type
        /// </summary>
        /// <param name="t">Type to search for</param>
        /// <returns>Identifier of type</returns>
        public Guid GetTypeIdCached(Type t)
        {
            Guid result = Guid.Empty;
            if (typeToIdMapping.TryGetValue(t, out result))
            {
                return result;
            }
            else
            {
                return Guid.Empty;
            }
        }



        /// <summary>
        /// Returns type name of given type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Type name</returns>
        private static string GetTypeName(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return type.AssemblyQualifiedName;
        }

        /// <summary>
        /// Adds new member to type
        /// </summary>
        /// <param name="name">Member name</param>
        /// <param name="type">Member type</param>
        /// <returns>Identifier of the member node</returns>
        private Guid AddTypeMember(string name, Type type, bool isPrimaryKey)
        {
            Guid memberId = Guid.NewGuid();
            var node = new Node<Guid, object, EdgeData>(NodeType.TypeMember, name);

            if (isPrimaryKey)
            {
                node.Values.Add(Constants.TypeMemberPrimaryKeyId, null);
            }

            // Ensure the member type
            Guid memberTypeId = GetTypeId(type);
            if (memberTypeId == Guid.Empty)
            {
                memberTypeId = AddType(type);
            }

            //Link member --> type            
            node.AddEdge(new Edge<Guid, EdgeData>(memberTypeId, new EdgeData(EdgeType.OfType, null)));

            provider.SetNode(memberId, node);

            return memberId;
        }

        /// <summary>
        /// Returns member ID for a given name
        /// </summary>
        /// <param name="typeId">Type to search in</param>
        /// <param name="propertyName">Property name</param>
        /// <returns>Member ID, or empty Guid if not found</returns>
        public Guid GetTypeMemberId(Guid typeId, string propertyName)
        {
            foreach (var edge in provider.GetNode(typeId, NodeAccess.Read).Edges.Values)
            {
                if (edge.Data.Semantic == EdgeType.Property)
                {
                    var candidateNode = provider.GetNode(edge.ToNodeId, NodeAccess.Read);

                    if (propertyName.Equals(candidateNode.Data))
                    {
                        return edge.ToNodeId;
                    }
                }
            }

            return Guid.Empty;
        }

        /// <summary>
        /// Returns type identifier for given member
        /// </summary>
        /// <param name="memberId">Member node identifier</param>
        /// <returns>Type ID</returns>
        public Guid GetMemberTypeId(Guid memberId)
        {
            return provider.GetNode(memberId, NodeAccess.Read).FindEdge(new EdgeData(EdgeType.OfType, null)).ToNodeId;
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
        /// Returns type ID of given instance node
        /// </summary>
        /// <param name="instance">Instance node</param>
        /// <returns>Type ID</returns>
        public Guid GetInstanceTypeId(Node<Guid, object, EdgeData> instance)
        {
            return instance.FindEdge(new EdgeData(EdgeType.OfType, null)).ToNodeId;
        }

        /// <summary>
        /// Initializes new type system by importing types or upgrading existing type definitions
        /// </summary>
        /// <param name="types">Types to import</param>
        /// <returns>Dictionary mapping between interface type and type Id </returns>
        public Dictionary<Type, Guid> InitializeTypeSystem(Type[] types)
        {
            typeToIdMapping = new Dictionary<Type, Guid>();
            typeIdToTypeMapping = new Dictionary<Guid, Type>();

            if (provider.Contains(Constants.TypesNodeId))
            {
                // TODO (nsabo) Type and data upgrade procedure
            }
            else
            {
                provider.SetNode(Constants.TypesNodeId, new Node<Guid, object, EdgeData>(NodeType.TypesRoot, null));

                EnsureBasicScalarTypes();

                foreach (var type in types)
                {                    
                    AddType(type);                    
                }           
     
                //TODO (nsabo) Cleanup on type validation error, or use isolated provider?
            }

            foreach(var typeId in GetTypes())
            {
                var type = GetTypeFromId(typeId);
                typeToIdMapping.Add(type, typeId);
                typeIdToTypeMapping.Add(typeId, type);
                
                Type collectionType = null;

                if (Utils.IsCollectionType(type, ref collectionType))
                {
                    collectionTypesTable.Add(typeId, type);
                }

                if (Utils.IsDictionaryType(type, ref collectionType))
                {
                    dictionaryTypesTable.Add(typeId, type);
                }
            }

            CacheScalarTypes();

            return typeToIdMapping;
        }        

        /// <summary>
        /// Returns list of all typ identifiers in the provider
        /// </summary>
        /// <returns>List of identifiers</returns>
        private Collection<Guid> GetTypes()
        {
            Collection<Guid> list = new Collection<Guid>();
            foreach (var edge in provider.GetNode(Constants.TypesNodeId, NodeAccess.Read).Edges.Values)
            {
                if (edge.Data.Semantic.Equals(EdgeType.Contains))
                {
                    list.Add(edge.ToNodeId);
                }

            }

            return list;
        }

        /// <summary>
        /// Run through types and cache scalar types in a table
        /// </summary>
        private void CacheScalarTypes()
        {
            foreach (var edge in provider.GetNode(Constants.TypesNodeId, NodeAccess.Read).Edges.Values)
            {
                if (edge.Data.Semantic.Equals(EdgeType.Contains))
                {
                    Guid typeId = edge.ToNodeId;                    
                    Type type = GetTypeFromId(typeId);

                    if (IsSupportedScalarType(type))
                    {
                        scalarTypesTable.Add(typeId, type);
                    }

                    if (type.IsEnum)
                    {
                        scalarTypesTable.Add(typeId, type);
                    }
                }
            }
        }

        /// <summary>
        /// Determines if a type is one of supported scalar types
        /// </summary>
        /// <param name="type">Type to verify</param>
        /// <returns>True if type is from supported scalars</returns>
        private bool IsSupportedScalarType(Type type)
        {
            bool foundScalar = false;
            foreach (var candidateType in supportedScalarTypes)
            {
                if (candidateType.Equals(type))
                {
                    foundScalar = true;
                    break;
                }
            }
            return foundScalar;
        }

        /// <summary>
        /// Determines if a type is one of supported scalar types
        /// </summary>
        /// <param name="typeName">Name of the Type to verify</param>
        /// <returns>True if type is from supported scalars</returns>
        public bool IsSupportedScalarTypeName(string typeName)
        {
            bool foundScalar = false;
            foreach (var candidateType in supportedScalarTypes)
            {
                if (candidateType.Name.Equals(typeName))
                {
                    foundScalar = true;
                    break;
                }
            }

            return foundScalar;
        }

        /// <summary>
        /// Performs validation of entity interface type
        /// </summary>
        /// <param name="type">Interface entity type</param>
        private void ValidateType(Type type)
        {
            if (!type.IsInterface && !type.IsEnum && !IsSupportedScalarType(type))
            {
                throw new ArgumentException("Only scalar, interface and enumeration types may be defined:" + type.AssemblyQualifiedName);
            }

            if (type.IsGenericType)
            {
                Type baseGenericType = type.GetGenericTypeDefinition();

                if (baseGenericType != typeof(ICollection<>) &&
                    baseGenericType != typeof(IIndexedCollection<>) &&
                    baseGenericType != typeof(IOrderedCollection<>) &&
                    baseGenericType != typeof(IScalarSet<>) &&
                    baseGenericType != typeof(IDictionary<,>))
                {
                    throw new ArgumentException("Only generic types ICollection<>, IIndexedCollection<>, ISetCollection<> and IDictionary<,> are supported. Invalid generic type found: " + type);
                }

                if (baseGenericType == typeof(IIndexedCollection<>))
                {
                    if (!CheckPrimaryKeyField(type.GetGenericArguments()[0]))
                    {
                        throw new ArgumentException("IIndexedCollection element of type: " + type.GetGenericArguments()[0] + " must have defined primary key field. Invalid occurrence found in type: " + type);
                    }
                }

                if (baseGenericType == typeof(IScalarSet<>))
                {
                    if (!IsSupportedScalarType(type.GetGenericArguments()[0]))
                    {
                        throw new ArgumentException("IScalarSet element must be of supported scalar type. Invalid type found: " + type.GetGenericArguments()[0] + " Invalid occurrence found in type: " + type);
                    }
                }

                if (baseGenericType == typeof(IDictionary<,>))
                {
                    if (!IsSupportedScalarType(type.GetGenericArguments()[0]))
                    {
                        throw new ArgumentException("IDictionary key must be of supported scalar type. Invalid type found: " + type.GetGenericArguments()[0] + " Invalid occurrence found in type: " + type);
                    }
                }
            }
            else
            {
                if (!IsSupportedScalarType(type))
                {
                    foreach (var ancestor in type.GetInterfaces())
                    {
                        if (ancestor == typeof(ICollection<>) ||
                        ancestor == typeof(IIndexedCollection<>) ||
                        ancestor == typeof(IOrderedCollection<>) ||
                        ancestor == typeof(IScalarSet<>) ||
                        ancestor == typeof(IDictionary<,>))
                        {
                            throw new ArgumentException("Inheritance from collection types is not allowed. Invalid occurrence found in type: " + type);
                        }

                        ValidateType(ancestor);
                    }

                    // Check that there are no methods
                    if (type.GetMethods(BindingFlags.DeclaredOnly).Length > 0)
                    {
                        throw new ArgumentException("Methods in entity interfaces are not allowed. Invalid occurrence found in type: " + type);
                    }


                    Collection<PropertyInfo> properties = new Collection<PropertyInfo>();
                    Utils.ExtractProperties(type, properties);

                    // Validate that RevisionIdAttribute may appear on read-only Guid properties
                    foreach (var prop in properties)
                    {
                        if (prop.GetCustomAttributes(typeof(RevisionIdAttribute), false).Length > 0)
                        {
                            if (!prop.PropertyType.Equals(typeof(Guid)) ||
                                !prop.CanRead ||
                                prop.CanWrite)
                            {
                                throw new ArgumentException("RevisionIdAttribute is allowed only on read-only Guid type property. Invalid occurrence found in type: " + type);
                            }
                        }
                    }
                }
            }
            

            
        }

        private bool CheckPrimaryKeyField(Type type)
        {
            var count = 0;
            Collection<PropertyInfo> properties = new Collection<PropertyInfo>();
            Utils.ExtractProperties(type, properties);
            foreach (var prop in properties)
            {
                if (prop.GetCustomAttributes(typeof(PrimaryKeyAttribute), false).Length > 0)
                {
                    count++;
                }
            }

            return count == 1;
        }

        /// <summary>
        /// Returns list of member ID for given type
        /// </summary>
        /// <param name="typeId">Type ID</param>
        /// <returns>Collection of member IDs</returns>
        public IList<Edge<Guid, EdgeData>> GetTypeEdges(Guid typeId)
        {
            return provider.GetNode(typeId, NodeAccess.Read).Edges.Values;
        }

        /// <summary>
        /// Returns default value for property of given type
        /// </summary>
        /// <param name="typeId">Type ID</param>
        /// <returns>Default value</returns>
        public object GetDefaultPropertyValue(Guid typeId)
        {
            if (!IsScalarType(typeId))
            {
                throw new ArgumentException("Scalar type expected");
            }

            Type t = typeIdToTypeMapping[typeId];
            object val = null;

            if (t.Equals(typeof(Boolean)))
            {
                val = false;
            }
            else
                if (t.Equals(typeof(DateTime)))
                {
                    val = DateTime.MinValue;
                }
                else
                    if (t.Equals(typeof(TimeSpan)))
                    {
                        val = TimeSpan.Zero;
                    }
                    else
                        if (t.Equals(typeof(Int32)))
                        {
                            val = (Int32)0;
                        }
                        else
                            if (t.Equals(typeof(Int64)))
                            {
                                val = (Int64)0;
                            }
                            else
                                if (t.Equals(typeof(Double)))
                                {
                                    val = 0.0;
                                }
                                else
                                    if (t.Equals(typeof(String)))
                                    {
                                        val = String.Empty;
                                    }
                                    else
                                        if (t.Equals(typeof(Guid)))
                                        {
                                            val = Guid.Empty;
                                        }
                                        else
                                            if (t.Equals(typeof(Byte)))
                                            {
                                                val = (Byte)0;
                                            }
                                            else
                                                if (t.IsEnum)
                                                {
                                                    val = Enum.GetValues(t).GetValue(0);
                                                }
                                                else
                                                {
                                                    throw new ArgumentException("Default undefined for type " + t.ToString());
                                                }

            return val;
        }

        /// <summary>
        /// Returns runtime type from given type ID
        /// </summary>
        /// <param name="typeId">Type ID</param>
        /// <returns>Runtime type</returns>
        public Type GetTypeFromId(Guid typeId)
        {
            var node = provider.GetNode(typeId, NodeAccess.Read);
            return Type.GetType((string)node.Data);
        }

        /// <summary>
        /// Returns runtime type from given type ID
        /// </summary>
        /// <param name="typeId">Type ID</param>
        /// <returns>Runtime type</returns>
        public Type GetTypeFromIdCached(Guid typeId)
        {
            return typeIdToTypeMapping[typeId];
        }        

        /// <summary>
        /// Determines if type has no derived types in the model
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>True if element type has no descendants in the entity model</returns>
        public bool IsSealedType(Type type)
        {
            var id = GetTypeId(type);
            return IsSealedType(id);
        }

        /// <summary>
        /// Determines if type has no derived types in the model
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>True if element type has no descendants in the entity model</returns>
        public bool IsSealedType(Guid typeId)
        {
            // Scalars are sealed
            if (IsScalarType(typeId))
            {
                return true;
            }

            // Try finding an ancestor
            foreach (var candidate in GetTypes())
            {
                if (candidate != typeId)
                {
                    foreach (var edge in GetTypeEdges(candidate))
                    {
                        if (edge.Data.Semantic == EdgeType.OfType)
                        {
                            if (edge.ToNodeId == typeId)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Returns ID of primary key member
        /// </summary>
        /// <param name="typeId">Type identifier</param>
        /// <returns>Member ID, or empty Guid if primary key member doesn't exist</returns>
        public Guid GetTypePrimaryKeyMemberId(Guid typeId)
        {
            foreach (var edge in GetTypeEdges(typeId))
            {
                if (edge.Data.Semantic == EdgeType.Property)
                {
                    var memberNode = provider.GetNode(edge.ToNodeId, NodeAccess.Read);
                    if (memberNode.Values.ContainsKey(Constants.TypeMemberPrimaryKeyId))
                    {
                        return edge.ToNodeId;
                    }
                }
            }

            return Guid.Empty;
        }

        internal string GetMemberName(Guid typeId, Guid memberId)
        {
            return provider.GetNode(memberId, NodeAccess.Read).Data.ToString();
        }

        /// <summary>
        /// Returns list of registered type identifiers
        /// </summary>
        /// <returns>Enumerable of Guid</returns>
        internal IEnumerable<Guid> GetRegisteredTypes()
        {
            return this.typeIdToTypeMapping.Keys;
        }

        /// <summary>
        /// Calculate edge flags from type attribute values.
        /// </summary>
        /// <param name="isPermanent"></param>
        /// <param name="saveParentNodes"></param>
        /// <returns></returns>
        public EdgeFlags CalculateEdgeFlags(bool isPermanent, bool saveParentNodes)
        {
            EdgeFlags edgeFlags = EdgeFlags.None;
            if (isPermanent)
            {
                edgeFlags |= EdgeFlags.Permanent;
            }
            if (saveParentNodes)
            {
                edgeFlags |= EdgeFlags.StoreParentNodes;
            }
            return edgeFlags;
        }
    }
}
