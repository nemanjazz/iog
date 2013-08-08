// -----------------------------------------------------------------------
// <copyright file="EdgeData.cs" company="Execom">
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
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Execom.IOG.Graph
{
    /// <summary>
    /// Defines edge types - semantics
    /// </summary>
    /// <author>Nenad Sabo</author>
    [Serializable]
    public enum EdgeType
    {
        Contains,
        OfType,
        Property,
        ListItem,
        RootObject,
        Special
    }

    /// <summary>
    /// Defines edge flags - markers
    /// </summary>
    [Flags]
    [Serializable]
    public enum EdgeFlags : byte
    {
        /// <summary>
        /// Empty
        /// </summary>
        None = 0,

        /// <summary>
        /// Edge is pointing to permanent version and should not be updated
        /// </summary>
        Permanent = 1,

        /// <summary>
        /// Parent nodes should be stored for this edge
        /// </summary>
        StoreParentNodes = 2
    }

    /// <summary>
    /// Defines contents of single edge
    /// </summary>
    /// <author>Nenad Sabo</author>
    [Serializable]
    public class EdgeData : IComparable<EdgeData>
    {
        /// <summary>
        /// Represents max value of edge data
        /// </summary>
        public static EdgeData MaxValue = new EdgeData(EdgeType.Special, new Guid("{53F11357-62B7-430F-B446-9EC8F9702406}"));

        /// <summary>
        /// Represents min value of edge data
        /// </summary>
        public static EdgeData MinValue = new EdgeData(EdgeType.Special, new Guid("{76367091-B69D-4BDF-A643-779032AF3503}"));

        /// <summary>
        /// Edge semantic which describes type of edge
        /// </summary>
        public readonly EdgeType Semantic;

        /// <summary>
        /// Edge flags
        /// </summary>
        public readonly EdgeFlags Flags;

        /// <summary>
        /// Optional edge data object
        /// </summary>
        public readonly object Data;

        /// <summary>
        /// Creates new instance of EdgeData type
        /// </summary>
        /// <param name="semantic">Edge semantic</param>
        /// <param name="data">Optional edge data</param>
        public EdgeData(EdgeType semantic, object data)
        {
            Semantic = semantic;
            Data = data;
            Flags = EdgeFlags.None;
        }

        /// <summary>
        /// Creates new instance of EdgeData type
        /// </summary>
        /// <param name="semantic">Edge semantic</param>
        /// <param name="flags">Edge flags</param>
        /// <param name="data">Optional edge data</param>
        public EdgeData(EdgeType semantic, EdgeFlags flags, object data)
        {
            Semantic = semantic;
            Data = data;
            Flags = flags;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            EdgeData other = obj as EdgeData;

            if (other == null)
            {
                return false;
            }

            return other.Semantic.Equals(Semantic) &&
                (Data == null ? other.Data == null : Data.Equals(other.Data));
        }

        /// <summary>
        /// Calculates hash code for the edge data object. It is derived from semantic and data hash when available.
        /// </summary>
        /// <returns>Hash</returns>
        public override int GetHashCode()
        {
            if (Data == null)
            {
                return Semantic.GetHashCode();
            }
            else
            {
                return Semantic.GetHashCode() ^ Data.GetHashCode();
            }
        }

        /// <summary>
        /// Comparison of two edge data objects
        /// </summary>
        /// <param name="other">Other object</param>
        /// <returns>Comparison result</returns>
        public int CompareTo(EdgeData other)
        {
            if (this == other)
            {
                return 0;
            }

            if (this.Semantic == EdgeType.Special)
            {
                if (this.Equals(MinValue))
                {
                    return -1;
                }

                if (other.Equals(MinValue))
                {
                    return 1;
                }

                if (this.Equals(MaxValue))
                {
                    return 1;
                }

                if (other.Equals(MaxValue))
                {
                    return -1;
                }
            }

            if (this.Semantic.Equals(other.Semantic))
            {
                return Comparer.Default.Compare(this.Data, other.Data);
            }
            else
            {
                return this.Semantic.CompareTo(other.Semantic);
            }
        }

        public override string ToString()
        {
            if (this.Equals(MaxValue))
            {
                return "MaxValue";
            }
            else
                if (this.Equals(MinValue))
                {
                    return "MinValue";
                }
                else
                {
                    return Semantic.ToString() + "(" + (Data != null ? Data.ToString() : "NULL") + ")";
                }
        }
    }
}
