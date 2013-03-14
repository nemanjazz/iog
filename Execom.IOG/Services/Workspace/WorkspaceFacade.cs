// -----------------------------------------------------------------------
// <copyright file="WorkspaceFacade.cs" company="Execom">
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
    using Execom.IOG.Services.Data;
    using Execom.IOG.Tracking;
    using Execom.IOG.Services.Events;
    using Execom.IOG.Events;

    /// <summary>
    /// Implementation of workspace facade
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal class WorkspaceFacade : IWorkspaceFacade
    {
        /// <summary>
        /// Commit data service
        /// </summary>
        private CommitDataService commitDataService;

        /// <summary>
        /// Workspace state tracking
        /// </summary>
        private TrackingWorkspaceStateProvider workspaceStateProvider;

        /// <summary>
        /// Service which manages subscriptions
        /// </summary>
        private ISubscriptionManagerService subscriptionManagerService;

        /// <summary>
        /// Service which manages snapshots
        /// </summary>
        private SnapshotsService snapshotsService;

        /// <summary>
        /// Exclusive lock provider
        /// </summary>
        private WorkspaceExclusiveLockProvider workspaceExclusiveLockProvider;

        /// <summary>
        /// Syncronization object for commit operation
        /// </summary>
        private object commitSync = new object();

        /// <summary>
        /// Creates new instance of WorkspaceFacade type
        /// </summary>
        /// <param name="commitDataService">Commit data service</param>
        /// <param name="workspaceStateProvider">Workspace state tracker</param>
        public WorkspaceFacade(CommitDataService commitDataService, TrackingWorkspaceStateProvider workspaceStateProvider, ISubscriptionManagerService subscriptionManagerService, SnapshotsService snapshotsService, WorkspaceExclusiveLockProvider workspaceExclusiveLockProvider)
        {
            this.commitDataService = commitDataService;
            this.workspaceStateProvider = workspaceStateProvider;
            this.subscriptionManagerService = subscriptionManagerService;
            this.snapshotsService = snapshotsService;
            this.workspaceExclusiveLockProvider = workspaceExclusiveLockProvider;
        }

        /// <summary>
        /// Performs a commit
        /// </summary>
        /// <param name="workspaceId">Workspace ID</param>
        /// <param name="changeSet">Changes to commit</param>
        /// <returns>Changes which were commited</returns>
        public CommitResult<Guid> Commit(Guid workspaceId, IsolatedChangeSet<Guid, object, EdgeData> changeSet)
        {
            bool isSnapshotIsolation = workspaceStateProvider.WorkspaceIsolationLevel(workspaceId) == IsolationLevel.Snapshot;

            if (isSnapshotIsolation)
            {
                workspaceExclusiveLockProvider.EnterLockExclusive();
            }
            try
            {
                lock (commitSync)
                {
                    if (!workspaceStateProvider.IsWorkspaceExpired(workspaceId))
                    {
                        var result = commitDataService.AcceptCommit(changeSet);
                        workspaceStateProvider.UpdateWorspace(workspaceId, result.ResultSnapshotId);
                        subscriptionManagerService.InvokeEvents(workspaceId, result);
                        return result;
                    }
                    else
                    {
                        throw new TimeoutException("Workspace timeout has elapsed");
                    }
                }
            }
            finally
            {
                if (isSnapshotIsolation)
                {
                    workspaceExclusiveLockProvider.ExitLockExclusive();
                }
            }
        }

        /// <summary>
        /// Registers workspace openning
        /// </summary>
        /// <param name="workspaceId">Workspace ID</param>
        /// <param name="snapshotId">Snapshot ID</param>
        /// <param name="isolationLevel">Isolation level</param>
        /// <param name="timeout">Workspace timeout</param>
        public void OpenWorkspace(Guid workspaceId, Guid snapshotId, IsolationLevel isolationLevel, TimeSpan timeout)
        {
            workspaceStateProvider.AddWorkspace(workspaceId, snapshotId, isolationLevel, timeout);
        }

        /// <summary>
        /// Registers that workspace has forwarded to new snapshot
        /// </summary>
        /// <param name="workspaceId">Workspace Id</param>
        /// <param name="snapshotId">New snapshot Id</param>
        public void UpdateWorkspace(Guid workspaceId, Guid snapshotId)
        {
            workspaceStateProvider.UpdateWorspace(workspaceId, snapshotId);
        }

        /// <summary>
        /// Removes workspace from register
        /// </summary>
        /// <param name="workspaceId">Workspace ID</param>
        public void CloseWorkspace(Guid workspaceId)
        {
            workspaceStateProvider.RemoveWorkspace(workspaceId);
        }

        /// <summary>
        /// Returns mapping between two snapshots
        /// </summary>
        /// <param name="oldSnapshotId">Old snapshot Id</param>
        /// <param name="newSnapshotId">New snapshot Id</param>
        /// <returns>Mapping dictionary (old Id -> new Id)</returns>
        public Dictionary<Guid, Guid> ChangesBetween(Guid oldSnapshotId, Guid newSnapshotId)
        {
            return commitDataService.ChangesBetween(oldSnapshotId, newSnapshotId);
        }

        /// <summary>
        /// Creates subscription to object changes
        /// </summary>
        /// <param name="workspaceId">Workspace ID</param>
        /// <param name="instanceId">Instance Id which changes are going to be notified</param>
        /// <param name="notifyChangesFromSameWorkspace">Defines if changes from same workspace should be notified</param>
        /// <param name="del">Delegate to be called</param>
        /// <returns>Subscription object</returns>
        public Subscription CreateSubscription(Guid workspaceId, Guid instanceId, bool notifyChangesFromSameWorkspace, EventHandler<ObjectChangedEventArgs> del)
        {
            lock (commitSync)
            {
                Guid snapshotId = workspaceStateProvider.GetWorkspaceSnapshotId(workspaceId);

                var result = subscriptionManagerService.Create(workspaceId, instanceId, notifyChangesFromSameWorkspace, del);

                var lastSnapshotId = snapshotsService.GetLatestSnapshotId();

                if (!lastSnapshotId.Equals(snapshotId))
                {
                    var changes = commitDataService.ChangesBetween(snapshotId, lastSnapshotId);
                    subscriptionManagerService.InvokeEvents(Guid.Empty, new CommitResult<Guid>(lastSnapshotId, changes));
                }

                return result;
            }
        }

        /// <summary>
        /// Creates subscription to object changes of specific property
        /// </summary>
        /// <param name="workspaceId">Workspace ID</param>
        /// <param name="instanceId">Instance Id which changes are going to be notified</param>
        /// <param name="propertyName">Property name which changes are going to be notified</param>
        /// <param name="notifyChangesFromSameWorkspace">Defines if changes from same workspace should be notified</param>
        /// <param name="del">Delegate to be called</param>
        /// <returns>Subscription object</returns>
        public Subscription CreateSubscription(Guid workspaceId, Guid instanceId, string propertyName, bool notifyChangesFromSameWorkspace, EventHandler<ObjectChangedEventArgs> del)
        {
            lock (commitSync)
            {
                Guid snapshotId = workspaceStateProvider.GetWorkspaceSnapshotId(workspaceId);

                var result = subscriptionManagerService.Create(workspaceId, instanceId, propertyName, notifyChangesFromSameWorkspace, del);

                var lastSnapshotId = snapshotsService.GetLatestSnapshotId();

                if (!lastSnapshotId.Equals(snapshotId))
                {
                    var changes = commitDataService.ChangesBetween(snapshotId, lastSnapshotId);
                    subscriptionManagerService.InvokeEvents(Guid.Empty, new CommitResult<Guid>(lastSnapshotId, changes));
                }

                return result;
            }
        }

        /// <summary>
        /// Removes subscription
        /// </summary>
        /// <param name="subscription">Subscription to remove</param>
        public void RemoveSubscription(Subscription subscription)
        {
            subscriptionManagerService.Remove(subscription);
        }

        /// <summary>
        /// Returns root object Id for given snapshot
        /// </summary>
        /// <param name="snapshotId">Snapshot Id</param>
        /// <returns>Root object Id</returns>
        public Guid GetRootObjectId(Guid snapshotId)
        {
            return snapshotsService.GetRootObjectId(snapshotId);
        }

        /// <summary>
        /// Returns last commited snapshot Id
        /// </summary>
        /// <returns>Snapshot Id</returns>
        public Guid LastSnapshotId()
        {
            return snapshotsService.GetLatestSnapshotId();
        }

        /// <summary>
        /// Updates isolation level for workspace in WorkspaceStateProvider.
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="isolationLevel"></param>
        public void ChangeWorkspaceIsolationLevel(Guid workspaceId, IsolationLevel isolationLevel)
        {
            workspaceStateProvider.ChangeWorkspaceIsolationLevel(workspaceId, isolationLevel);
        }
    }
}
