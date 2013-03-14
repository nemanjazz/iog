// -----------------------------------------------------------------------
// <copyright file="ServerContext.cs" company="Execom">
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

    /// <summary>
    /// Server context implementation.
    /// </summary>
    /// <author>Nenad Sabo</author>
    public class ServerContext : MarshalByRefObject, IServerContext
    {
        /// <summary>
        /// Underlying local context which is going to be distributed
        /// </summary>
        private Context localContext;

        /// <summary>
        /// Creates new instance of ServerContext type
        /// </summary>
        /// <param name="localContext">Underlying local context which is going to be distributed</param>
        public ServerContext(Context localContext)
        {
            this.localContext = localContext;
        }

        public bool SnapshotIsolationEnabled
        {
            get
            {
                return localContext.SnapshotIsolationEnabled;
            }
        }

        public TimeSpan DefaultWorkspaceTimeout
        {
            get { return localContext.DefaultWorkspaceTimeout; }
        }


        public void EnterExclusiveLock()
        {
            localContext.workspaceExclusiveLockProvider.EnterLockExclusive();
        }

        public void ExitExclusiveLock()
        {
            localContext.workspaceExclusiveLockProvider.ExitLockExclusive();
        }

        public Type[] EntityTypes
        {
            get { return localContext.entityTypes; }
        }

        public void SetNode(Guid identifier, Graph.Node<Guid, object, Graph.EdgeData> node)
        {
            localContext.provider.SetNode(identifier, node);
        }

        public Graph.Node<Guid, object, Graph.EdgeData> GetNode(Guid nodeId, Graph.NodeAccess access)
        {
            return localContext.provider.GetNode(nodeId, access);
        }

        public bool Contains(Guid identifier)
        {
            return localContext.provider.Contains(identifier);
        }

        public void Remove(Guid identifier)
        {
            // TODO (nsabo) Allow this ?
            localContext.provider.Remove(identifier);
        }

        public System.Collections.IEnumerable EnumerateNodes()
        {
            return localContext.provider.EnumerateNodes();
        }

        public void Clear()
        {
            // TODO (nsabo) Allow this ?
            localContext.provider.Clear();
        }

        public Graph.CommitResult<Guid> Commit(Guid workspaceId, Graph.IsolatedChangeSet<Guid, object, Graph.EdgeData> changeSet)
        {
            return localContext.workspaceFacade.Commit(workspaceId, changeSet);
        }

        public void OpenWorkspace(Guid workspaceId, Guid snapshotId, IsolationLevel isolationLevel, TimeSpan timeout)
        {
            localContext.workspaceFacade.OpenWorkspace(workspaceId, snapshotId, isolationLevel, timeout);
        }

        public void CloseWorkspace(Guid workspaceId)
        {
            localContext.workspaceFacade.CloseWorkspace(workspaceId);
        }

        public void UpdateWorkspace(Guid workspaceId, Guid snapshotId)
        {
            localContext.workspaceFacade.UpdateWorkspace(workspaceId, snapshotId);
        }

        public Dictionary<Guid, Guid> ChangesBetween(Guid oldSnapshotId, Guid newSnapshotId)
        {
            return localContext.workspaceFacade.ChangesBetween(oldSnapshotId, newSnapshotId);
        }

        public void RemoveSubscription(Events.Subscription subscription)
        {
            throw new NotImplementedException();
            //localContext.workspaceFacade.RemoveSubscription(subscription);
        }


        public Guid GetRootObjectId(Guid snapshotId)
        {
            return localContext.snapshotsService.GetRootObjectId(snapshotId);
        }

        public Guid LastSnapshotId()
        {
            return localContext.LastSnapshotId();
        }


        public Events.Subscription CreateSubscription(Guid workspaceId, Guid instanceId, bool notifyChangesFromSameWorkspace, EventHandler<Events.ObjectChangedEventArgs> del)
        {
            throw new NotImplementedException();
            // For this to work the delegate must point to marshall-ed object
            // It is probably best to have local method passed in delegate and queue of events which is 
            // being processed by the client periodically
            //return localContext.workspaceFacade.CreateSubscription(workspaceId, instanceId, del);
        }

        public Events.Subscription CreateSubscription(Guid workspaceId, Guid instanceId, string propertyName, bool notifyChangesFromSameWorkspace, EventHandler<Events.ObjectChangedEventArgs> del)
        {
            throw new NotImplementedException();
        }

        public void ChangeWorkspaceIsolationLevel(Guid workspaceId, IsolationLevel isolationLevel)
        {
            // TODO (brus, 27062012) Check if this is ok
            throw new NotImplementedException();
        }
    }
}
