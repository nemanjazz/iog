// -----------------------------------------------------------------------
// <copyright file="IWorkspaceFacade.cs" company="Execom">
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

namespace Execom.IOG.Services.Workspace
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Execom.IOG.Graph;
    using Execom.IOG.Events;

    /// <summary>
    /// Interface describes object which is able to accept data commits
    /// </summary>
    /// <author>Nenad Sabo</author>
    public interface IWorkspaceFacade
    {
        /// <summary>
        /// Performs actions based on changeset
        /// </summary>
        /// <param name="workspaceId">Workspace ID</param>
        /// <param name="changeSet">Change set which describes changes made to data</param>
        CommitResult<Guid> Commit(Guid workspaceId, IsolatedChangeSet<Guid, object, EdgeData> changeSet);

        /// <summary>
        /// Registers new workspace
        /// </summary>
        /// <param name="workspaceId">Workspace ID</param>
        /// <param name="snapshotId">Snapshot ID</param>
        /// <param name="isolationLevel">Isolation level</param>
        /// <param name="timeout">Time after which workspace will be considered closed</param>
        void OpenWorkspace(Guid workspaceId, Guid snapshotId, IsolationLevel isolationLevel, TimeSpan timeout);

        /// <summary>
        /// Closes the workspace
        /// </summary>
        /// <param name="workspaceId">Workspace ID</param>
        void CloseWorkspace(Guid workspaceId);

        /// <summary>
        /// Returns mapping between two snapshots
        /// </summary>
        /// <param name="oldSnapshotId">Old snapshot Id</param>
        /// <param name="newSnapshotId">New snapshot Id</param>
        /// <returns>Mapping dictionary (old Id -> new Id)</returns>
        Dictionary<Guid, Guid> ChangesBetween(Guid oldSnapshotId, Guid newSnapshotId);

        /// <summary>
        /// Creates subscription to object changes
        /// </summary>
        /// <param name="workspaceId">Workspace ID</param>
        /// <param name="instanceId">Instance Id which changes are going to be notified</param>
        /// <param name="notifyChangesFromSameWorkspace">Defines if changes from same workspace should be notified</param>
        /// <param name="del">Delegate to be called</param>
        /// <returns>Subscription object</returns>
        Subscription CreateSubscription(Guid workspaceId, Guid instanceId, bool notifyChangesFromSameWorkspace, EventHandler<ObjectChangedEventArgs> del);

        /// <summary>
        /// Creates subscription to object changes of specific property
        /// </summary>
        /// <param name="workspaceId">Workspace ID</param>
        /// <param name="instanceId">Instance Id which changes are going to be notified</param>
        /// <param name="propertyName">Property name which changes are going to be notified</param>
        /// <param name="notifyChangesFromSameWorkspace">Defines if changes from same workspace should be notified</param>
        /// <param name="del">Delegate to be called</param>
        /// <returns>Subscription object</returns>
        Subscription CreateSubscription(Guid workspaceId, Guid instanceId, string propertyName, bool notifyChangesFromSameWorkspace, EventHandler<ObjectChangedEventArgs> del);

        /// <summary>
        /// Removes subscription
        /// </summary>
        /// <param name="subscription">Subscription to remove</param>
        void RemoveSubscription(Subscription subscription);

        /// <summary>
        /// Returns root object Id for given snapshot
        /// </summary>
        /// <param name="snapshotId">Snapshot Id</param>
        /// <returns>Root object Id</returns>
        Guid GetRootObjectId(Guid snapshotId);

        /// <summary>
        /// Returns last commited snapshot Id
        /// </summary>
        /// <returns>Snapshot Id</returns>
        Guid LastSnapshotId();

        /// <summary>
        /// Registers that workspace has forwarded to new snapshot
        /// </summary>
        /// <param name="workspaceId">Workspace Id</param>
        /// <param name="snapshotId">New snapshot Id</param>
        void UpdateWorkspace(Guid workspaceId, Guid snapshotId);

        /// <summary>
        /// Updates isolation level for workspace in WorkspaceStateProvider.
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="isolationLevel"></param>
        void ChangeWorkspaceIsolationLevel(Guid workspaceId, IsolationLevel isolationLevel);
    }
}
