// -----------------------------------------------------------------------
// <copyright file="Edge.cs" company="Execom">
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

namespace Execom.IOG.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Defines an edge in a graph
    /// </summary>
    /// <typeparam name="TIdentifier">Type of identifier</typeparam>
    /// <typeparam name="TEdgeData">Type of edge data</typeparam>
    /// <author>Nenad Sabo</author>
    [Serializable]
    public class Edge<TIdentifier, TEdgeData> where TEdgeData : IComparable<TEdgeData>
    {
        /// <summary>
        /// Edge points to following node with unique ID
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Due to performance reasons this remains a field")]
        public TIdentifier ToNodeId;

        /// <summary>
        /// Edge carries following data
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Due to performance reasons this remains a field")]
        public TEdgeData Data;

        /// <summary>
        /// Initializes a new instance of the Edge class
        /// </summary>
        /// <param name="toNodeId">Target node identifier</param>
        /// <param name="data">Edge data</param>
        public Edge(TIdentifier toNodeId, TEdgeData data)
        {
            ToNodeId = toNodeId;
            Data = data;
        }
    }
}
