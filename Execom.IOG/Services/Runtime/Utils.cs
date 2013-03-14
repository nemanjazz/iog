// -----------------------------------------------------------------------
// <copyright file="Utils.cs" company="Execom">
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
    using System.Collections.ObjectModel;
    using System.Reflection;
    using Execom.IOG.Services.Data;
    using Execom.IOG.Graph;
    using System.Diagnostics;
    using System.Collections;

    /// <summary>
    /// Static utility methods for runtime operations
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal static class Utils
    {
        /// <summary>
        /// Returns proxy item instance id
        /// </summary>
        /// <param name="item">Proxy instance</param>
        /// <returns>ID of proxy instance</returns>
        public static Guid GetItemId(object item)
        {
            Type t = item.GetType();
            Guid id = (Guid)t.InvokeMember(Constants.InstanceIdFieldName, System.Reflection.BindingFlags.GetField, null, item, null);
            return id;
        }

        /// <summary>
        /// Checks if given object is a proxy
        /// </summary>
        /// <param name="item">Proxy instance</param>
        /// <returns>True if object has a ID and therefore is a proxy object</returns>
        public static bool HasItemId(object item)
        {
            Type t = item.GetType();
            return t.GetMember(Constants.InstanceIdFieldName).Length > 0;
        }

        /// <summary>
        /// Sets proxy item instance id
        /// </summary>
        /// <param name="item">Proxy instance</param>
        /// <param name="instanceId">New instance ID</param>
        public static void SetItemId(object item, Guid instanceId)
        {
            Type t = item.GetType();
            t.InvokeMember(Constants.InstanceIdFieldName, BindingFlags.SetField, null, item, new object[] { instanceId });
        }

        /// <summary>
        /// Collect list of properties in type including the inherited type
        /// </summary>
        /// <param name="type">Interface type</param>
        /// <param name="props">List of properties found</param>
        public static void ExtractProperties(Type type, Collection<PropertyInfo> props)
        {
            foreach (PropertyInfo p in type.GetProperties())
            {
                bool exists = false;

                foreach (PropertyInfo pr in props)
                {
                    if (pr.Name.Equals(p.Name))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    props.Add(p);
                }
                else
                {
                    // TODO (nsabo) What if property was already defined? Log a warning?
                }

            }

            foreach (Type baseType in type.GetInterfaces())
            {
                ExtractProperties(baseType, props);
            }
        }

        /// <summary>
        /// Determines if given type is collection type
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <param name="collectionType">Type of the collection</param>
        /// <returns>True if type is collection</returns>
        public static bool IsCollectionType(Type type, ref Type collectionType)
        {
            bool isCollection = false;
            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(ICollection<>)))
            {
                isCollection = true;
                collectionType = type;
            }
            else
            {
                foreach (Type t in type.GetInterfaces())
                {
                    if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(ICollection<>)))
                    {
                        isCollection = true;
                        collectionType = t;
                    }
                }
            }

            return isCollection;
        }

        /// <summary>
        /// Determines if type is dictionary type
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <param name="dictionaryType">Type of the dictionary</param>
        /// <returns>True if type is a dictionary</returns>
        public static bool IsDictionaryType(Type type, ref Type dictionaryType)
        {
            bool isDictionary = false;
            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(IDictionary<,>)))
            {
                isDictionary = true;
                dictionaryType = type;
            }
            else
            {
                foreach (Type t in type.GetInterfaces())
                {
                    if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(IDictionary<,>)))
                    {
                        isDictionary = true;
                        dictionaryType = t;
                    }
                }
            }

            return isDictionary;
        }

        /// <summary>
        /// Defines if edge is pointing to permanent data
        /// </summary>
        /// <param name="edge">Edge to check</param>
        /// <returns>True if edge is pointing to permanent data</returns>
        internal static bool IsPermanentEdge(Graph.Edge<Guid, Graph.EdgeData> edge)
        {
            return  edge.Data.Semantic == EdgeType.OfType || // Types never change during runtime
                    edge.ToNodeId.Equals(Constants.NullReferenceNodeId) || // Null never changes
                    ((edge.Data.Flags & EdgeFlags.Permanent) == EdgeFlags.Permanent); // Edge marked as permanent
        }

        /// <summary>
        /// Returns member ID of primary key member
        /// </summary>
        /// <param name="value">Proxy object</param>
        /// <returns>ID or empty Guid</returns>
        internal static Guid GetItemPrimaryKeyId(object value)
        {
            Type t = value.GetType();
            return (Guid)t.InvokeMember(Constants.PrimaryKeyIdFieldName, System.Reflection.BindingFlags.GetField, null, value, null);
        }

        internal static void LogNodesRecursive(Guid nodeId, INodeProvider<Guid, object, EdgeData> nodes, INodeProvider<Guid, object, EdgeData> changes, int tabLevel, Hashtable visited, TypesService typesService)
        {
            if (visited.ContainsKey(nodeId))
            {
                Debug.WriteLine(LogTabs(tabLevel) + nodeId);
                return;
            }

            visited.Add(nodeId, null);

            if (changes.Contains(nodeId))
            {
                var node = changes.GetNode(nodeId, NodeAccess.Read);
                switch (node.NodeType)
                {
                    case NodeType.Object:
                        LogObject(nodeId, node, nodes, changes, tabLevel, visited, typesService);
                        break;
                    case NodeType.Collection:
                        LogCollection(nodeId, node, nodes, changes, tabLevel, visited, typesService);
                        break;
                    case NodeType.Dictionary:
                        LogCollection(nodeId, node, nodes, changes, tabLevel, visited, typesService);
                        break;
                    default:
                        Debug.WriteLine(LogTabs(tabLevel) + node.Previous + "->" + nodeId + "[" + node.NodeType + "]");
                        foreach (var edge in node.Edges)
                        {
                            Debug.WriteLine(LogTabs(tabLevel) + edge.Key + "=");
                            LogNodesRecursive(edge.Value.ToNodeId, nodes, changes, tabLevel + 1, visited, typesService);
                        }
                        break;
                }
            }
            else
            {
                Debug.WriteLine(LogTabs(tabLevel) + nodeId);
            }
        }

        private static string LogTabs(int nrTabs)
        {
            string res = "";

            for (int i = 0; i < nrTabs; i++)
            {
                res += " ";
            }

            return res;
        }

        private static void LogObject(Guid nodeId, Node<Guid, object, EdgeData> node, INodeProvider<Guid, object, EdgeData> nodes, INodeProvider<Guid, object, EdgeData> changes, int tabLevel, Hashtable visited, TypesService typesService)
        {
            var typeId = typesService.GetInstanceTypeId(node);
            var typeName = typesService.GetTypeFromId(typeId).Name;

            Debug.WriteLine(LogTabs(tabLevel) + nodeId + "(" + typeName + ")");
            Debug.WriteLine(LogTabs(tabLevel) + "Previous=" + node.Previous);

            foreach (var value in node.Values)
            {
                Debug.WriteLine(LogTabs(tabLevel) + typesService.GetMemberName(typeId, value.Key) + "=" + value.Value);
            }

            foreach (var edge in node.Edges)
            {
                if (edge.Value.Data.Semantic == EdgeType.Property)
                {
                    Debug.WriteLine(LogTabs(tabLevel) + typesService.GetMemberName(typeId, (Guid)edge.Value.Data.Data) + "=");
                    LogNodesRecursive(edge.Value.ToNodeId, nodes, changes, tabLevel + 1, visited, typesService);
                }
            }

        }

        private static void LogCollection(Guid nodeId, Node<Guid, object, EdgeData> node, INodeProvider<Guid, object, EdgeData> nodes, INodeProvider<Guid, object, EdgeData> changes, int tabLevel, Hashtable visited, TypesService typesService)
        {
            Edge<Guid, EdgeData> typeEdge = null;
            BPlusTreeOperations.TryFindEdge(nodes, nodeId, new EdgeData(EdgeType.OfType, null), out typeEdge);

            var typeId = typeEdge.ToNodeId;
            var typeName = typesService.GetTypeFromId(typeId).Name;

            Debug.WriteLine(LogTabs(tabLevel) + nodeId + "(" + typeName + ")");
            Debug.WriteLine(LogTabs(tabLevel) + "Previous=" + node.Previous);


            using (var enumeration = BPlusTreeOperations.GetEnumerator(nodes, nodeId, EdgeType.ListItem))
            {
                while (enumeration.MoveNext())
                {
                    Debug.WriteLine(LogTabs(tabLevel) + enumeration.Current.Data + "=");
                    LogNodesRecursive(enumeration.Current.ToNodeId, nodes, changes, tabLevel + 1, visited, typesService);
                }
            }
        }  
    }
}
