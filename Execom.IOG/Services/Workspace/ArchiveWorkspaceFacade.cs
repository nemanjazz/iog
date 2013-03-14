// -----------------------------------------------------------------------
// <copyright file="ArchiveWorkspaceFacade.cs" company="Execom">
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
    using Execom.IOG.Storage;
    using Execom.IOG.Services.Data;

    internal class ArchiveWorkspaceFacade : IWorkspaceFacade
    {
        private INodeProvider<Guid, object, EdgeData> archiveProvider;

        public ArchiveWorkspaceFacade(INodeProvider<Guid, object, EdgeData> archiveProvider)
        {
            this.archiveProvider = archiveProvider;
        }

        public CommitResult<Guid> Commit(Guid workspaceId, IsolatedChangeSet<Guid, object, EdgeData> changeSet)
        {
            throw new NotImplementedException();
        }

        public void OpenWorkspace(Guid workspaceId, Guid snapshotId, IsolationLevel isolationLevel, TimeSpan timeout)
        {
            
        }

        public void CloseWorkspace(Guid workspaceId)
        {
            
        }

        public Dictionary<Guid, Guid> ChangesBetween(Guid oldSnapshotId, Guid newSnapshotId)
        {
            throw new NotImplementedException();
        }

        public Subscription CreateSubscription(Guid workspaceId, Guid instanceId, bool notifyChangesFromSameWorkspace, EventHandler<ObjectChangedEventArgs> del)
        {
            throw new NotImplementedException();
        }

        public Subscription CreateSubscription(Guid workspaceId, Guid instanceId, string propertyName, bool notifyChangesFromSameWorkspace, EventHandler<ObjectChangedEventArgs> del)
        {
            throw new NotImplementedException();
        }

        public void RemoveSubscription(Subscription subscription)
        {
            throw new NotImplementedException();
        }

        public Guid GetRootObjectId(Guid snapshotId)
        {
            var node = archiveProvider.GetNode(Constants.SnapshotsNodeId, NodeAccess.Read);
            return node.FindEdge(new EdgeData(EdgeType.Contains, null)).ToNodeId;
        }

        public Guid LastSnapshotId()
        {
            throw new NotImplementedException();
        }

        public void UpdateWorkspace(Guid workspaceId, Guid snapshotId)
        {
            throw new NotImplementedException();
        }


        public void ChangeWorkspaceIsolationLevel(Guid workspaceId, IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();
        }
    }
}
