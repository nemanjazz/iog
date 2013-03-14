// -----------------------------------------------------------------------
// <copyright file="IContext.cs" company="Execom">
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

namespace Execom.IOG
{
    using System;
    using Execom.IOG.Storage;

    /// <summary>
    /// Context which defines available functionality to IOG users
    /// </summary>    
    /// <author>Nenad Sabo</author>
    public interface IContext
    {        
        /// <summary>
        /// Backup/replicate the data from last commited snapshot to another storage
        /// </summary>
        /// <param name="storage">Target storage for the backup</param>
        void Backup(IKeyValueStorage<Guid, object> storage);

        /// <summary>
        /// Backup/replicate the data from given snapshot to another storage
        /// </summary>
        /// <param name="snapshotId">Snapshot Id to backup</param>
        /// <param name="storage">Target storage for the backup</param>
        void Backup(Guid snapshotId, IKeyValueStorage<Guid, object> storage);

        /// <summary>
        /// Performs cleanup of historic data which is not used
        /// </summary>
        void Cleanup();

        /// <summary>
        /// Check for expired workspaces and unlock any locks they might hold
        /// </summary>
        void ExpireWorkspaces();

        /// <summary>
        /// Returns last commited snapshot ID
        /// </summary>
        /// <returns>Snapshot ID</returns>
        Guid LastSnapshotId();

        /// <summary>
        /// Opens a new workspace with assumed latest available snapshot
        /// </summary>
        /// <typeparam name="TDataType">Type of root entity interface for data navigation. This is the "all containing" entity, which has access to all other entities.</typeparam>
        /// <param name="isolationLevel">Workspace isolation</param>
        /// <returns>New workspace instance</returns>
        Workspace<TDataType> OpenWorkspace<TDataType>(IsolationLevel isolationLevel);

        /// <summary>
        /// Opens a new workspace with assumed latest available snapshot
        /// </summary>
        /// <typeparam name="TDataType">Type of root entity interface for data navigation. This is the "all containing" entity, which has access to all other entities.</typeparam>
        /// <param name="isolationLevel">Workspace isolation</param>
        /// <param name="timeout">Workspace timeout</param>
        /// <returns>New workspace instance</returns>
        Workspace<TDataType> OpenWorkspace<TDataType>(IsolationLevel isolationLevel, TimeSpan timeout);

        /// <summary>
        /// Opens a new workspace with given snapshot
        /// </summary>
        /// <typeparam name="TDataType">Type of root entity interface for data navigation. This is the "all containing" entity, which has access to all other entities.</typeparam>
        /// <param name="snapshotId">Snapshot ID</param>
        /// <param name="isolationLevel">Workspace isolation</param>
        /// <returns>New workspace instance</returns>
        Workspace<TDataType> OpenWorkspace<TDataType>(Guid snapshotId, IsolationLevel isolationLevel);

        /// <summary>
        /// Opens a new workspace with given snapshot
        /// </summary>
        /// <typeparam name="TDataType">Type of root entity interface for data navigation. This is the "all containing" entity, which has access to all other entities.</typeparam>
        /// <param name="snapshotId">Snapshot ID</param>
        /// <param name="isolationLevel">Workspace isolation</param>
        /// <param name="timeout">Workspace timeout</param>
        /// <returns>New workspace instance</returns>
        Workspace<TDataType> OpenWorkspace<TDataType>(Guid snapshotId, IsolationLevel isolationLevel, TimeSpan timeout);

        /// <summary>
        /// Updates given worksapce isolation level to exclusive.
        /// Workspace is updated to last snapshot because exclusive workspace can change data only for last snapshot.
        /// If given workspace isoaltion level is read only workspace isolation level cannot be updated and exception will be thrown.
        /// </summary>
        /// <typeparam name="TDataType"></typeparam>
        /// <param name="workspace"></param>
        void UpdateWorkspaceToExclusive<TDataType>(Workspace<TDataType> workspace);

        /// <summary>
        /// Updates given worksapce isolation level to snapshot.
        /// If given workspace isoaltion level is read only workspace isolation level cannot be updated and exception will be thrown.
        /// </summary>
        /// <typeparam name="TDataType"></typeparam>
        /// <param name="workspace"></param>
        void UpdateWorkspaceToSnapshot<TDataType>(Workspace<TDataType> workspace);
    }
}
