// -----------------------------------------------------------------------
// <copyright file="TrackingWorkspaceStateProvider.cs" company="Execom">
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

namespace Execom.IOG.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Provider which tracks workspace states with regards of their usage of snapshots
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal class TrackingWorkspaceStateProvider
    {
        /// <summary>
        /// Defines workspace state
        /// </summary>
        private class WorkspaceStateElement
        {
            /// <summary>
            /// Open snapshot ID
            /// </summary>
            public Guid SnapshotId;

            /// <summary>
            /// Workspace isolation
            /// </summary>
            public IsolationLevel IsolationLevel;

            /// <summary>
            /// Date of last access
            /// </summary>
            public DateTime LastAccessDateTime;

            /// <summary>
            /// Timeout interval when the worspace is considered expired
            /// </summary>
            public TimeSpan Timeout;

            /// <summary>
            /// Creates new instance of WorkspaceStateElement type
            /// </summary>
            /// <param name="snapshotId">Snapshot ID</param>
            /// <param name="isolationLevel">Workspace isolation</param>
            /// <param name="openedDateTime">Date when workspace was opened</param>
            /// <param name="timeout">Expiration timeout</param>
            public WorkspaceStateElement(Guid snapshotId, IsolationLevel isolationLevel, DateTime openedDateTime, TimeSpan timeout)
            {
                this.SnapshotId = snapshotId;
                this.IsolationLevel = isolationLevel;
                this.LastAccessDateTime = openedDateTime;
                this.Timeout = timeout;
            }

            /// <summary>
            /// Determines if workspace is expired
            /// </summary>
            /// <returns>True if timeout has elapsed from last access</returns>
            public bool IsExpired()
            {
                return LastAccessDateTime.Add(Timeout) < DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Stores workspace states
        /// </summary>
        private Dictionary<Guid, WorkspaceStateElement> workspaceStates = new Dictionary<Guid, WorkspaceStateElement>();

        /// <summary>
        /// Provider of workspace exclusive lock
        /// </summary>
        private WorkspaceExclusiveLockProvider workspaceExclusiveLockProvider;

        public TrackingWorkspaceStateProvider(WorkspaceExclusiveLockProvider workspaceExclusiveLockProvider)
        {
            this.workspaceExclusiveLockProvider = workspaceExclusiveLockProvider;
        }

        /// <summary>
        /// Registers openning of a workspace
        /// </summary>
        /// <param name="workspaceId">Workspace ID</param>
        /// <param name="snapshotId">Opened snapshot ID</param>
        /// <param name="isolationLevel">Isolation level</param>
        /// <param name="timeout">Workspace timeout</param>
        public void AddWorkspace(Guid workspaceId, Guid snapshotId, IsolationLevel isolationLevel, TimeSpan timeout)
        {
            lock (workspaceStates)
            {
                workspaceStates.Add(workspaceId, new WorkspaceStateElement(snapshotId, isolationLevel, DateTime.UtcNow, timeout));
            }
        }

        /// <summary>
        /// Determines if workspace is expired
        /// </summary>
        /// <param name="workspaceId">Workspace ID</param>
        /// <returns>True if workspace is expired</returns>
        public bool IsWorkspaceExpired(Guid workspaceId)
        {
            lock (workspaceStates)
            {
                return !workspaceStates.ContainsKey(workspaceId) || workspaceStates[workspaceId].IsExpired();
            }
        }

        public IsolationLevel WorkspaceIsolationLevel(Guid workspaceId)
        {
            lock (workspaceStates)
            {
                return workspaceStates[workspaceId].IsolationLevel;
            }
        }

        public Guid GetWorkspaceSnapshotId(Guid workspaceId)
        {
            lock (workspaceStates)
            {
                return workspaceStates[workspaceId].SnapshotId;
            }
        }

        /// <summary>
        /// Removes the workspace from tracking
        /// </summary>
        /// <param name="workspaceId">Workspace ID</param>
        public void RemoveWorkspace(Guid workspaceId)
        {
            IsolationLevel level = IsolationLevel.ReadOnly;
            lock (workspaceStates)
            {
                level = workspaceStates[workspaceId].IsolationLevel;
                workspaceStates.Remove(workspaceId);
            }

            if (level != IsolationLevel.ReadOnly)
            {
                if (level == IsolationLevel.Exclusive)
                {
                    workspaceExclusiveLockProvider.ExitLockExclusive();
                }
            }
        }        

        /// <summary>
        /// Removes expired workspaces from the register
        /// </summary>
        public void Cleanup()
        {
            Collection<Guid> expiredWorkspaces = new Collection<Guid>();

            lock (workspaceStates)
            {
                foreach (var item in workspaceStates)
                {
                    if (item.Value.IsExpired())
                    {
                        expiredWorkspaces.Add(item.Key);
                    }
                }

                foreach (var key in expiredWorkspaces)
                {
                    RemoveWorkspace(key);
                }
            }
        }

        /// <summary>
        /// Determines snapshots which are in use by workspaces
        /// </summary>
        /// <returns>Collection of snapshot IDs</returns>
        public Collection<Guid> UsedSnapshotIds()
        {
            Collection<Guid> res = new Collection<Guid>();

            lock (workspaceStates)
            {
                foreach (var item in workspaceStates)
                {
                    if (!item.Value.IsExpired())
                    {
                        res.Add(item.Value.SnapshotId);
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Updates workspace to new snapshot ID and sets new access timestamp
        /// </summary>
        /// <param name="workspaceId">Workspace ID</param>
        /// <param name="snapshotId">Snapshot ID</param>
        public void UpdateWorspace(Guid workspaceId, Guid snapshotId)
        {
            lock (workspaceStates)
            {
                workspaceStates[workspaceId].LastAccessDateTime = DateTime.UtcNow;
                workspaceStates[workspaceId].SnapshotId = snapshotId;
            }
        }

        /// <summary>
        /// Changes workspace isolation level.
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="isolationLevel"></param>
        internal void ChangeWorkspaceIsolationLevel(Guid workspaceId, IsolationLevel isolationLevel)
        {
            workspaceStates[workspaceId].IsolationLevel = isolationLevel;
        }
    }
}
