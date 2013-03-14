// -----------------------------------------------------------------------
// <copyright file="RecursiveResolutionParameters.cs" company="Execom">
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

namespace Execom.IOG.Services.Merging
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Collections;
    using Execom.IOG.Graph;
    using Execom.IOG.Providers;

    /// <summary>
    /// Describes parameters which are passed in the recursive resolution
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal class RecursiveResolutionParameters
    {
        public Hashtable SubTree;
        public IsolatedNodeProvider DestinationProvider;
        public IsolatedNodeProvider SourceProvider;
        public IsolatedChangeSet<Guid, object, EdgeData> ChangeSet;
        public Dictionary<Guid, Guid> IntermediateChanges;
        public Hashtable VisitedNodes;

        public RecursiveResolutionParameters(Hashtable subTree, IsolatedNodeProvider destinationProvider, IsolatedNodeProvider sourceProvider, IsolatedChangeSet<Guid, object, EdgeData> changeSet, Dictionary<Guid, Guid> intermediateChanges, Hashtable visitedNodes)
        {
            this.SubTree = subTree;
            this.DestinationProvider = destinationProvider;
            this.SourceProvider = sourceProvider;
            this.ChangeSet = changeSet;
            this.IntermediateChanges = intermediateChanges;
            this.VisitedNodes = visitedNodes;
        }
    }

    internal delegate void MergeRecursiveDelegate(Guid nodeId, RecursiveResolutionParameters parameters);

    internal delegate void InsertRecursiveDelegate(Guid nodeId, RecursiveResolutionParameters parameters);
}
