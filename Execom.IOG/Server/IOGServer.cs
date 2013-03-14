using System;
using System.Collections.Generic;
using System.Text;
using Execom.IOG.Distributed;
using Execom.IOG.Storage;

namespace Execom.IOG.Server
{
    ///<summary>
    ///IOGServerContext is class that is implementing intereface IServerContext which is needed
    /// for making ASP.NET IOG database server.
    ///
    ///</summary>
    ///<author>Ivan Vasiljevic</author>
    public class IOGServerContext<RootType> :  IServerContext
    {
        LocalContextSingleton localContext;
        /// <summary>
        /// Creates new instance of ServerContext type
        /// </summary>
        public IOGServerContext()
        {
            localContext = LocalContextSingleton.GetInstance<RootType>();
        }

        /// <summary>
        /// Creates new instance of ServerContext type with specified KeyValueStorage
        /// </summary>
        public IOGServerContext(IKeyValueStorage<Guid, object> newStorage)
        {
            localContext = LocalContextSingleton.GetInstance<RootType>(newStorage);
        }

        public bool SnapshotIsolationEnabled
        {
            get
            {
                return localContext.LocalContext.SnapshotIsolationEnabled;
            }
        }

        public TimeSpan DefaultWorkspaceTimeout
        {
            get { return localContext.LocalContext.DefaultWorkspaceTimeout; }
        }

        public void EnterExclusiveLock()
        {
            localContext.LocalContext.workspaceExclusiveLockProvider.EnterLockExclusive();
        }


        public Type[] EntityTypes
        {
            get { return localContext.LocalContext.entityTypes; }
        }

        public void SetNode(Guid identifier, Execom.IOG.Graph.Node<Guid, object, Execom.IOG.Graph.EdgeData> node)
        {
            localContext.LocalContext.provider.SetNode(identifier, node);
        }


        public Execom.IOG.Graph.Node<Guid, object, Execom.IOG.Graph.EdgeData> GetNode(Guid nodeId, 
            Execom.IOG.Graph.NodeAccess access)
        {
            return localContext.LocalContext.provider.GetNode(nodeId, access);
        }

        public bool Contains(Guid identifier)
        {
            return localContext.LocalContext.provider.Contains(identifier);
        }

        public void Remove(Guid identifier)
        {
            // TODO (nsabo) Allow this ?
            localContext.LocalContext.provider.Remove(identifier);
        }

        public System.Collections.IEnumerable EnumerateNodes()
        {
            return localContext.LocalContext.provider.EnumerateNodes();
        }

        public void Clear()
        {
            // TODO (nsabo) Allow this ?
            localContext.LocalContext.provider.Clear();
        }

        public Execom.IOG.Graph.CommitResult<Guid> Commit(Guid workspaceId, Execom.IOG.Graph.IsolatedChangeSet<Guid, 
            object, Execom.IOG.Graph.EdgeData> changeSet)
        {
            return localContext.LocalContext.workspaceFacade.Commit(workspaceId, changeSet);
        }

        public void OpenWorkspace(Guid workspaceId, Guid snapshotId, IsolationLevel isolationLevel, TimeSpan timeout)
        {
            localContext.LocalContext.workspaceFacade.OpenWorkspace(workspaceId, snapshotId, 
                isolationLevel, timeout);
        }

        public void CloseWorkspace(Guid workspaceId)
        {
            localContext.LocalContext.workspaceFacade.CloseWorkspace(workspaceId);
        }

        public void UpdateWorkspace(Guid workspaceId, Guid snapshotId)
        {
            localContext.LocalContext.workspaceFacade.UpdateWorkspace(workspaceId, snapshotId);
        }

        public Dictionary<Guid, Guid> ChangesBetween(Guid oldSnapshotId, Guid newSnapshotId)
        {
            return localContext.LocalContext.workspaceFacade.ChangesBetween(oldSnapshotId, 
                newSnapshotId);
        }

        public void RemoveSubscription(Execom.IOG.Events.Subscription subscription)
        {
            localContext.LocalContext.workspaceFacade.RemoveSubscription(subscription);
        }

        public Guid GetRootObjectId(Guid snapshotId)
        {
            return localContext.LocalContext.snapshotsService.GetRootObjectId(snapshotId);
        }

        public Guid LastSnapshotId()
        {
            return localContext.LocalContext.LastSnapshotId();
        }

        public Execom.IOG.Events.Subscription CreateSubscription(Guid workspaceId, Guid instanceId, bool notifyChangesFromSameWorkspace, EventHandler<Execom.IOG.Events.ObjectChangedEventArgs> del)
        {
            return localContext.LocalContext.workspaceFacade.CreateSubscription(workspaceId, instanceId, notifyChangesFromSameWorkspace, del);
        }

        public Execom.IOG.Events.Subscription CreateSubscription(Guid workspaceId, Guid instanceId, string propertyName, bool notifyChangesFromSameWorkspace, EventHandler<Execom.IOG.Events.ObjectChangedEventArgs> del)
        {
            return localContext.LocalContext.workspaceFacade.CreateSubscription(workspaceId, instanceId, propertyName, notifyChangesFromSameWorkspace, del);
        }

        public Type GetRootType()
        {
            return typeof(RootType);
        }


        public void ExitExclusiveLock()
        {
            localContext.LocalContext.workspaceExclusiveLockProvider.EnterLockExclusive();
        }


        public void ChangeWorkspaceIsolationLevel(Guid workspaceId, IsolationLevel isolationLevel)
        {
            //TODO don't know how to use this at the momment! ivanvasiljevic
            throw new NotImplementedException();
        }

    }
}
