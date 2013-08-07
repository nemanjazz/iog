// -----------------------------------------------------------------------
// <copyright file="ObjectSerializationService.cs" company="Execom">
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
    using Execom.IOG.Storage;
    using Execom.IOG.Graph;
    using System.IO;

    /// <summary>
    /// Service which serializes / deserializes nodes for persistence
    /// </summary>
    internal class ObjectSerializationService : IObjectSerializer
    {

        public TypesService TypesService { get; set; }

        /// <summary>
        /// Serialize object
        /// </summary>
        /// <param name="value">Object</param>
        /// <param name="bw">Binary writer to use</param>
        public void Serialize(object value, BinaryWriter bw)
        {
            WriteObjectToStream(value, bw);
        }

        /// <summary>
        /// Deserialize object
        /// </summary>
        /// <param name="br">Reader to use</param>
        /// <returns>Object</returns>
        public object Deserialize(BinaryReader br)
        {
            return ReadObjectFromStream(br);
        }

        /// <summary>
        /// List of markers for different object types
        /// </summary>
        private enum ObjectType : byte
        {
            Null = 0,
            Node = 1,
            Edge = 2,
            Int16 = 3,
            Guid = 4,
            Boolean = 5,
            String = 6,
            Int32 = 7,
            Int64 = 8,
            Double = 9,
            DateTime = 10,
            TimeSpan = 11,
            Byte = 12,
            Enum = 13,
            ByteArray = 14

        }

        /// <summary>
        /// Reads object from stream
        /// </summary>
        /// <param name="br">Reader to use</param>
        /// <returns>Object</returns>
        private object ReadObjectFromStream(BinaryReader br)
        {
            ObjectType typ = (ObjectType)br.ReadByte();

            switch (typ)
            {
                case ObjectType.Null:
                    return null;
                case ObjectType.Node:
                    {
                        NodeType nodeType = (NodeType)br.ReadByte();
                        bool commited = br.ReadBoolean();
                        int edgeCount = br.ReadInt32();
                        Dictionary<EdgeData, Edge<Guid, EdgeData>> edges = new Dictionary<EdgeData, Edge<Guid, EdgeData>>(edgeCount);
                        
                        for (int i = 0; i < edgeCount; i++)
                        {
                            var edge = (Edge<Guid, EdgeData>)ReadObjectFromStream(br);
                            edges.Add(edge.Data, edge);
                        }

                        int valueCount = br.ReadInt32();
                        Dictionary<Guid, object> values = new Dictionary<Guid, object>(valueCount);

                        for (int i = 0; i < valueCount; i++)
                        {
                            values.Add((Guid)ReadObjectFromStream(br), ReadObjectFromStream(br));
                        }

                        int parentNodesCount = br.ReadInt32();
                        List<Guid> parentNodes = new List<Guid>(parentNodesCount);

                        for (int i = 0; i < parentNodesCount; i++)
                        {
                            parentNodes.Add((Guid)ReadObjectFromStream(br));
                        }

                        Node<Guid, object, EdgeData> node = new Node<Guid, object, EdgeData>(nodeType, ReadObjectFromStream(br), edges, values, parentNodes);
                        node.Previous = (Guid)ReadObjectFromStream(br);
                        node.Commited = commited;
                        return node;
                    }
                case ObjectType.Edge:
                    {
                        Edge<Guid, EdgeData> edge = new Edge<Guid, EdgeData>(
                            new Guid(br.ReadBytes(16)),                            
                            ReadEdgeData(br));
                        return edge;
                    }                
                case ObjectType.Guid:
                    {
                        return new Guid(br.ReadBytes(16));
                    }
                case ObjectType.Boolean:
                    {
                        return br.ReadBoolean();
                    }
                case ObjectType.String:
                    {
                        return br.ReadString();
                    }
                case ObjectType.Int16:
                    {
                        return br.ReadInt16();
                    }
                case ObjectType.Int32:
                    {
                        return br.ReadInt32();
                    }
                case ObjectType.Int64:
                    {
                        return br.ReadInt64();
                    }
                case ObjectType.Double:
                    {
                        return br.ReadDouble();
                    }
                case ObjectType.DateTime:
                    {
                        return new DateTime(br.ReadInt64());
                    }
                case ObjectType.TimeSpan:
                    {
                        return new TimeSpan(br.ReadInt64());
                    }
                case ObjectType.Byte:
                    {
                        return br.ReadByte();
                    }
                case ObjectType.Enum:
                    {
                        var typeId = new Guid(br.ReadBytes(16));
                        var enumType = TypesService.GetTypeFromIdCached(typeId);

                        Type underlyingType = Enum.GetUnderlyingType(enumType);

                        object value = null;

                        if (underlyingType == typeof(Int64))
                        {
                            value = br.ReadInt64();
                        }
                        else
                            if (underlyingType == typeof(Int32))
                            {
                                value = br.ReadInt32();
                            }
                            else
                                if (underlyingType == typeof(Int16))
                                {
                                    value = br.ReadInt16();
                                }
                                else
                                    if (underlyingType == typeof(byte))
                                    {
                                        value = br.ReadByte();
                                    }
                                    else
                                    {
                                        throw new ArgumentException("Unexpected type " + underlyingType);
                                    }

                        return Enum.ToObject(enumType, value);
                    }
                case ObjectType.ByteArray:
                    {
                        int length = br.ReadInt32();
                        return br.ReadBytes(length);
                    }
                default:
                    throw new ArgumentException("Undefined object type :" + typ);
            }
        }

        /// <summary>
        /// Reads edge data object
        /// </summary>
        /// <param name="br">Reader to use</param>
        /// <returns>Edge data</returns>
        private EdgeData ReadEdgeData(BinaryReader br)
        {
            return new EdgeData((EdgeType)br.ReadByte(), (EdgeFlags)br.ReadByte(), ReadObjectFromStream(br));
        }

        /// <summary>
        /// Writes object to stream
        /// </summary>
        /// <param name="value">Object to write</param>
        /// <param name="bw">Writer to use</param>
        private void WriteObjectToStream(object value, BinaryWriter bw)
        {
            if (value == null)
            {
                bw.Write((byte)ObjectType.Null);
            }
            else
                if (value is Node<Guid, object, EdgeData>)
                {
                    Node<Guid, object, EdgeData> node = (Node<Guid, object, EdgeData>)value;

                    bw.Write((byte)ObjectType.Node);
                    bw.Write((byte)node.NodeType);
                    bw.Write(node.Commited);
                    bw.Write(node.Edges.Count);

                    foreach (var edge in node.Edges)
                    {
                        WriteObjectToStream(edge.Value, bw);
                    }

                    bw.Write(node.Values.Count);

                    foreach (var scalarValue in node.Values)
                    {
                        WriteObjectToStream(scalarValue.Key, bw);
                        WriteObjectToStream(scalarValue.Value, bw);
                    }

                    bw.Write(node.ParentNodes.Count);

                    foreach (var parentNode in node.ParentNodes)
                    {
                        WriteObjectToStream(parentNode, bw);
                    }

                    WriteObjectToStream(node.Data, bw);
                    WriteObjectToStream(node.Previous, bw);
                }
                else

                    if (value is Edge<Guid, EdgeData>)
                    {
                        bw.Write((byte)ObjectType.Edge);

                        Edge<Guid, EdgeData> edge = (Edge<Guid, EdgeData>)value;

                        bw.Write(edge.ToNodeId.ToByteArray());
                        WriteEdgeData(bw, edge.Data);
                    }
                    else
                        if (value is Guid)
                        {
                            bw.Write((byte)ObjectType.Guid);
                            bw.Write(((Guid)value).ToByteArray()); //16 bytes
                        }
                        else
                            if (value is Boolean)
                            {
                                bw.Write((byte)ObjectType.Boolean);
                                bw.Write((Boolean)value);
                            }
                            else
                                if (value is String)
                                {
                                    bw.Write((byte)ObjectType.String);
                                    bw.Write((String)value);
                                }
                                else
                                    if (value is Int16)
                                    {
                                        bw.Write((byte)ObjectType.Int16);
                                        bw.Write((Int16)value);
                                    }
                                    else
                                        if (value is Int32)
                                        {
                                            bw.Write((byte)ObjectType.Int32);
                                            bw.Write((Int32)value);
                                        }
                                        else
                                            if (value is Int64)
                                            {
                                                bw.Write((byte)ObjectType.Int64);
                                                bw.Write((Int64)value);
                                            }
                                            else
                                                if (value is Double)
                                                {
                                                    bw.Write((byte)ObjectType.Double);
                                                    bw.Write((Double)value);
                                                }
                                                else
                                                    if (value is DateTime)
                                                    {
                                                        bw.Write((byte)ObjectType.DateTime);
                                                        bw.Write(((DateTime)value).Ticks);
                                                    }
                                                    else
                                                        if (value is TimeSpan)
                                                        {
                                                            bw.Write((byte)ObjectType.TimeSpan);
                                                            bw.Write(((TimeSpan)value).Ticks);
                                                        }
                                                        else
                                                            if (value is Byte)
                                                            {
                                                                bw.Write((byte)ObjectType.Byte);
                                                                bw.Write((Byte)value);
                                                            }
                                                            else
                                                                if (value is Enum)
                                                                {
                                                                    bw.Write((byte)ObjectType.Enum);
                                                                    Type enumType = ((Enum)value).GetType();
                                                                    Guid typeId = TypesService.GetTypeIdCached(enumType);
                                                                    bw.Write(typeId.ToByteArray());

                                                                    Type underlyingType = Enum.GetUnderlyingType(enumType);

                                                                    if (underlyingType == typeof(Int64))
                                                                    {
                                                                        bw.Write((Int64)value);
                                                                    }
                                                                    else
                                                                        if (underlyingType == typeof(Int32))
                                                                        {
                                                                            bw.Write((Int32)value);
                                                                        }
                                                                        else
                                                                            if (underlyingType == typeof(Int16))
                                                                            {
                                                                                bw.Write((Int16)value);
                                                                            }
                                                                            else
                                                                                if (underlyingType == typeof(byte))
                                                                                {
                                                                                    bw.Write((byte)value);
                                                                                }
                                                                                else
                                                                                {
                                                                                    throw new ArgumentException("Unexpected type " + underlyingType);
                                                                                }

                                                                }
                                                                else
                                                                    if (value is byte[])
                                                                    {
                                                                        bw.Write((byte)ObjectType.ByteArray);
                                                                        bw.Write(((byte[])value).Length);
                                                                        bw.Write((byte[])value);
                                                                    }
                                                                    else
                                                                        throw new ArgumentException("Unsupported object " + value);
        }

        /// <summary>
        /// Writes edge data object to stream
        /// </summary>
        /// <param name="bw">Writer to use</param>
        /// <param name="data">Edge data</param>
        private void WriteEdgeData(BinaryWriter bw, EdgeData data)
        {
            bw.Write((byte)data.Semantic);
            bw.Write((byte)data.Flags);
            if (data.Data == null)
            {
                bw.Write((byte)ObjectType.Null);
            }
            else
            {
                WriteObjectToStream(data.Data, bw);
            }
        }
    }
}
