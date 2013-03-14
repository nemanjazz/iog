// -----------------------------------------------------------------------
// <copyright file="IServerContext.cs" company="Execom">
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

namespace Execom.IOG.Distributed
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Execom.IOG.Services.Workspace;
    using Execom.IOG.Graph;

    /// <summary>
    /// Server context interface definition
    /// </summary>
    /// <typeparam name="TDataType">Type of root entity interface for data navigation. This is the "all containing" entity, which has access to all other entities.</typeparam>
    /// <author>Nenad Sabo</author>
    public interface IServerContext : INodeProvider<Guid, object, EdgeData>, IWorkspaceFacade
    {
        /// <summary>
        /// Defines if snapshot isolation is enabled
        /// </summary>
        bool SnapshotIsolationEnabled { get; }

        /// <summary>
        /// Defines the default workspace expiration timeout
        /// </summary>
        TimeSpan DefaultWorkspaceTimeout { get; }

        /// <summary>
        /// Enters exclusive isolation lock
        /// </summary>
        void EnterExclusiveLock();

        /// <summary>
        /// Exits exclusive isolation lock
        /// </summary>
        void ExitExclusiveLock();

          
        /// <summary>
        /// Returns list of entity types
        /// </summary>
        Type[] EntityTypes { get; }
    }
}
