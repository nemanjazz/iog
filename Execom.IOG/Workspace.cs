// -----------------------------------------------------------------------
// <copyright file="Workspace.cs" company="Execom">
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
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using Execom.IOG.Graph;
    using Execom.IOG.Services.Runtime;
    using Execom.IOG.Services.Data;
    using Execom.IOG.Providers;
    using Execom.IOG.Storage;
    using Execom.IOG.Services.Facade;
    using System.Collections.ObjectModel;
    using Execom.IOG.Services.Workspace;
    using Execom.IOG.Exceptions;
    using Execom.IOG.Events;

    /// <summary>
    /// Workspace is an entity which implements data isolation. 
    /// It contains and tracks all changes made to the data, isolated from other workspaces. 
    /// Also, changes from other workspaces and commits are not visible from a workspace after it was opened.
    /// For more details see http://iog.codeplex.com/wikipage?title=Workspace
    /// </summary>
    /// <typeparam name="TDataType">Type of root entity interface for data navigation. This is the "all containing" entity, which has access to all other entities.</typeparam>
    /// <author>Nenad Sabo</author>
    public class Workspace<TDataType> : IWorkspace<TDataType>, IDisposable
    {
        /// <summary>
        /// Identifier of the workspace
        /// </summary>
        private Guid workspaceId;

        /// <summary>
        /// Thread which has access to the workspace.
        /// </summary>
        private Thread thread;

        /// <summary>
        /// Node provider for the workspace
        /// </summary>
        private INodeProvider<Guid, object, EdgeData> nodeProvider;

        /// <summary>
        /// Node provider for the isolated workspace changes
        /// </summary>
        private IsolatedNodeProvider isolatedProvider;

        /// <summary>
        /// Service for type manipulations
        /// </summary>
        private TypesService typesService;

        /// <summary>
        /// Service for instance manipulations
        /// </summary>
        private ObjectInstancesService objectInstancesService;

        /// <summary>
        /// Service for immutable instance manipulations
        /// </summary>
        private ObjectInstancesService immutableInstancesService;

        /// <summary>
        /// Service for collection manipulations
        /// </summary>
        private CollectionInstancesService collectionInstancesService;

        /// <summary>
        /// Service for dictionary manipulations
        /// </summary>
        private DictionaryInstancesService dictionaryInstancesService;

        /// <summary>
        /// Service for proxy creation
        /// </summary>
        private ProxyCreatorService proxyCreatorService;

        /// <summary>
        /// Snapshot ID for this workspace
        /// </summary>
        private Guid snapshotId;

        /// <summary>
        /// Root proxy object
        /// </summary>
        private TDataType rootProxy;

        /// <summary>
        /// Workspace isolation level
        /// </summary>
        private IsolationLevel isolationLevel;        

        /// <summary>
        /// Object which is able to accept commits
        /// </summary>
        private IWorkspaceFacade workspaceFacade;

        /// <summary>
        /// Map of created mutable proxies in the workspace
        /// </summary>
        private IProxyMap mutableProxyMap = new WeakProxyMap();

        /// <summary>
        /// Map of created read-only proxies in the workspace which is may be shared among workspaces
        /// </summary>
        private IProxyMap immutableProxyMap;

        private RuntimeProxyFacade runtimeProxyFacade;

        /// <summary>
        /// Creates new instance of Workspace type
        /// </summary>
        internal Workspace(Guid snapshotId, TimeSpan timeout, INodeProvider<Guid, object, EdgeData> nodeProvider, IWorkspaceFacade commitTarget, ProxyCreatorService proxyCreatorService, TypesService typesService, IsolationLevel isolationLevel, IProxyMap immutableProxyMap)
        {
            this.workspaceId = Guid.NewGuid();
            this.thread = Thread.CurrentThread;

            if (!typeof(TDataType).IsInterface)
            {
                throw new ArgumentException("Interface type expected: " + typeof(TDataType).AssemblyQualifiedName);
            }

            this.snapshotId = snapshotId;
            this.nodeProvider = nodeProvider;
            this.proxyCreatorService = proxyCreatorService;
            this.typesService = typesService;
            this.isolationLevel = isolationLevel;
            this.workspaceFacade = commitTarget;
            this.immutableProxyMap = immutableProxyMap;

            workspaceFacade.OpenWorkspace(workspaceId, snapshotId, isolationLevel, timeout);

            if (isolationLevel == IsolationLevel.ReadOnly)
            {
                // Rely directly on parent provider if read only
                this.objectInstancesService = new ObjectInstancesService(nodeProvider, typesService);
                this.immutableInstancesService = new ObjectInstancesService(nodeProvider, typesService);
                this.collectionInstancesService = new CollectionInstancesService(nodeProvider, typesService);
                this.dictionaryInstancesService = new DictionaryInstancesService(nodeProvider, typesService);
            }
            else
            {
                // Construct isolated provider for local changes
                var isolatedStorage = new DirectNodeProviderUnsafe<Guid, object, EdgeData>(new MemoryStorageUnsafe<Guid, object>(), false);
                isolatedProvider = new IsolatedNodeProvider(nodeProvider, isolatedStorage, thread);
                this.objectInstancesService = new ObjectInstancesService(isolatedProvider, typesService);
                this.immutableInstancesService = new ObjectInstancesService(nodeProvider, typesService);
                this.collectionInstancesService = new CollectionInstancesService(isolatedProvider, typesService);
                this.dictionaryInstancesService = new DictionaryInstancesService(isolatedProvider, typesService);
            }

            this.runtimeProxyFacade = new RuntimeProxyFacade(typesService, objectInstancesService, immutableInstancesService, collectionInstancesService, new CollectionInstancesService(nodeProvider, typesService), dictionaryInstancesService, new DictionaryInstancesService(nodeProvider, typesService), mutableProxyMap, immutableProxyMap, proxyCreatorService);
        
            // Initialize root data proxy
            var rootObjectId = commitTarget.GetRootObjectId(snapshotId);
            rootProxy = proxyCreatorService.NewObject<TDataType>(runtimeProxyFacade, rootObjectId, isolationLevel == IsolationLevel.ReadOnly);

            if (isolationLevel == IsolationLevel.ReadOnly)
            {
                immutableProxyMap.AddProxy(rootObjectId, rootProxy);
            }
            else
            {
                mutableProxyMap.AddProxy(rootObjectId, rootProxy);
            }
        }

        /// <summary>
        /// Provides access to data
        /// </summary>
        public TDataType Data
        {
            get
            {
                CheckThread();
                return rootProxy;
            }
        }

        /// <summary>
        /// Gets the unique ID of the data version opened by the workspace.
        /// </summary>
        public Guid SnapshotId
        {
            get 
            {
                CheckThread();
                return snapshotId; 
            }
        }

        /// <summary>
        /// Creates new instance of type T
        /// </summary>
        /// <typeparam name="T">Interface type of instance to create</typeparam>
        /// <returns>Created object instance</returns>
        public T New<T>()
        {
            CheckThread();

            Guid typeId = typesService.GetTypeIdCached(typeof(T));

            if (typeId.Equals(Guid.Empty))
            {
                throw new ArgumentException("Type not registered:" + typeof(T).AssemblyQualifiedName);
            }

            Guid instanceId = Guid.Empty;
            if (typesService.IsDictionaryType(typeId))
            {
                instanceId = dictionaryInstancesService.NewInstance(typeId);
            }
            else
                if (typesService.IsCollectionType(typeId))
                {
                    instanceId = collectionInstancesService.NewInstance(typeId);
                }
                else
                {
                    instanceId = objectInstancesService.NewInstance(typeId);
                }

            T proxy = proxyCreatorService.NewObject<T>(runtimeProxyFacade, instanceId, false);
            mutableProxyMap.AddProxy(instanceId, proxy);
            return proxy;
        }

        /// <summary>
        /// Creates object from given revision ID. If object version was 
        /// altered in the workspace object returned would also appear altered.      
        /// Returned object can be changed, but must be assigned in a tree to be reachable by the Commit.
        /// </summary>
        /// <typeparam name="T">Type of object instance expected</typeparam>
        /// <param name="revisionId">Revision ID to spawn</param>
        /// <returns>Entity object instance</returns>
        public T Spawn<T>(Guid revisionId)
        {
            object proxy = null;
            if (!mutableProxyMap.TryGetProxy(revisionId, out proxy))
            {
                proxy = proxyCreatorService.NewObject<T>(runtimeProxyFacade, revisionId, false);
                mutableProxyMap.AddProxy(revisionId, proxy);
            }

            return (T)proxy;
        }

        /// <summary>
        /// Creates object from given revision ID returning the last commited state.
        /// This method is not usable on new uncommited objects, because they dont have commited state.
        /// Returned object is read only.
        /// </summary>
        /// <typeparam name="T">Type of object instance expected</typeparam>
        /// <param name="revisionId">Revision ID to create</param>
        /// <returns>Entity object instance</returns>
        public T SpawnImmutable<T>(Guid revisionId)
        {
            if (isolatedProvider.GetNodeState(revisionId) == NodeState.Created)
            {
                throw new ArgumentException("Operation not allowed for uncommited instance");
            }

            object proxy = null;
            if (!immutableProxyMap.TryGetProxy(revisionId, out proxy))
            {
                proxy = proxyCreatorService.NewObject<T>(runtimeProxyFacade, revisionId, true);
                immutableProxyMap.AddProxy(revisionId, proxy);
            }

            return (T)proxy;
        }

        /// <summary>
        /// Creates immutable view of given object, returning the last commited state.
        /// This method is not usable on new uncommited objects, because they dont have commited state.
        /// Returned object is read only.
        /// </summary>
        /// <typeparam name="T">Type of object instance expected</typeparam>
        /// <param name="instance">Mutable or immutable object</param>
        /// <returns>Immutable view of entity object instance</returns>
        public T ImmutableView<T>(object instance)
        {
            var id = Utils.GetItemId(instance);

            if (isolatedProvider.GetNodeState(id) == NodeState.Created)
            {
                throw new ArgumentException("Operation not allowed for uncommited instance");
            }

            object proxy = null;
            if (!immutableProxyMap.TryGetProxy(id, out proxy))
            {
                proxy = proxyCreatorService.NewObject<T>(runtimeProxyFacade, id, true);
                immutableProxyMap.AddProxy(id, proxy);
            }

            return (T)proxy;
        }
        
        /// <summary>
        /// Returns instance revision ID of given object.
        /// Object can be reconstructed back with given ID via Spawn method.
        /// </summary>
        /// <param name="instance">Instance which revision is determined</param>
        /// <returns>Unique ID for the object.</returns>
        public Guid InstanceRevisionId(object instance)
        {
            return Utils.GetItemId(instance);
        }

        /// <summary>
        /// Sets reference of object as immutable
        /// </summary>
        /// <param name="instance">Object which holds the reference</param>
        /// <param name="propertyName">Property name of the reference</param>
        public void SetImmutable(object instance, string propertyName)
        {
            var instanceId = Utils.GetItemId(instance);
            objectInstancesService.SetImmutable(instanceId, typesService.GetTypeMemberId(typesService.GetInstanceTypeId(instanceId), propertyName));
        }

        /// <summary>
        /// This operation makes data changes in the workspace available for other workspaces.
        /// Commit creates new data version, with new snapshot ID. After commit, new data version will be 
        /// accessible as last version to new workspaces created with Context.OpenWorkspace()
        /// </summary>
        /// <returns>New snapshot ID</returns>
        public Guid Commit()
        {
            CheckThread();

            if (isolationLevel == IsolationLevel.ReadOnly)
            {
                throw new InvalidOperationException("Invalid commit operation in Read only isolation");
            }

            var isolatedChanges = isolatedProvider.GetChanges(snapshotId);

            var changeSet = workspaceFacade.Commit(workspaceId, isolatedChanges);

            mutableProxyMap.UpgradeProxies(changeSet.Mapping);

            isolatedProvider.Clear();

            snapshotId = changeSet.ResultSnapshotId;

            return snapshotId;
        }

        /// <summary>
        /// This operation cancels all data changes made in the workspace.
        /// Workspace stays in original snapshot, and does not reflect the updates in the mean time.
        /// </summary>
        public void Rollback()
        {
            CheckThread();

            if (isolationLevel == IsolationLevel.ReadOnly)
            {
                throw new InvalidOperationException("Invalid rollback operation in Read only isolation");
            }

            Collection<Guid> newInstances = new Collection<Guid>();

            foreach(Guid item in isolatedProvider.EnumerateChanges())
            {
                if (isolatedProvider.GetNodeState(item)== NodeState.Created)
                {
                    newInstances.Add(item);
                }
            }

            mutableProxyMap.InvalidateProxies(newInstances);

            isolatedProvider.Clear();
        }        

        /// <summary>
        /// This operation moves workspace to latest snapshot ID.
        /// All uncommited changes which are made in the workspace are going to be preserved.
        /// In case that conflicting changes were made, ConcurrencyModificationException will be thrown.
        /// </summary>
        public void Update()
        {
            CheckThread();

            var newSnapshotId = workspaceFacade.LastSnapshotId();

            Update(newSnapshotId);
        }

        /// <summary>
        /// This operation moves workspace to given snapshot ID.
        /// All uncommited changes which are made in the workspace are going to be preserved.
        /// In case that conflicting changes were made, ConcurrentModificationException will be thrown and workspace will remain not updated.
        /// </summary>
        /// <param name="newSnapshotId">Snapshot Id to be loaded, must be in sequence after the current snapshot ID</param>
        public void Update(Guid newSnapshotId)
        {
            CheckThread();

            var mapping = workspaceFacade.ChangesBetween(snapshotId, newSnapshotId);

            if (isolatedProvider != null)
            {
                foreach (Guid changedNodeId in isolatedProvider.EnumerateChanges())
                {
                    if (mapping.ContainsKey(changedNodeId))
                    {

                        Guid typeId = Guid.Empty;

                        try
                        {
                            typeId = typesService.GetInstanceTypeId(changedNodeId);
                        }
                        catch
                        {
                            throw new ConcurrentModificationException();
                        }

                        throw new ConcurrentModificationException(typesService.GetTypeFromId(typeId).Name);
                    }
                }
            }

            mutableProxyMap.UpgradeProxies(mapping);            

            snapshotId = newSnapshotId;

            workspaceFacade.UpdateWorkspace(workspaceId, snapshotId);
        }

        /// <summary>
        /// This operation moves workspace to latest snapshot ID.
        /// All uncommited changes which are made in the workspace are going to be preserved.
        /// In case that conflicting changes were made, return value will be false and workspace will remain not updated.
        /// </summary>
        /// <returns>True if update was successful</returns>
        public bool TryUpdate()
        {
            CheckThread();

            var newSnapshotId = workspaceFacade.LastSnapshotId();

            return TryUpdate(newSnapshotId);
        }

        /// <summary>
        /// This operation moves workspace to given snapshot ID.
        /// All uncommited changes which are made in the workspace are going to be preserved.
        /// In case that conflicting changes were made, return value will be false and workspace will remain not updated.
        /// </summary>
        /// <param name="newSnapshotId">Snapshot Id to be loaded, must be in sequence after the current snapshot ID</param>
        /// <returns>True if update was successful</returns>
        public bool TryUpdate(Guid newSnapshotId)
        {
            CheckThread();

            var mapping = workspaceFacade.ChangesBetween(snapshotId, newSnapshotId);

            if (isolatedProvider != null)
            {
                foreach (Guid changedNodeId in isolatedProvider.EnumerateChanges())
                {
                    if (mapping.ContainsKey(changedNodeId))
                    {
                        return false;
                    }
                }
            }

            mutableProxyMap.UpgradeProxies(mapping);

            snapshotId = newSnapshotId;

            workspaceFacade.UpdateWorkspace(workspaceId, snapshotId);

            return true;
        }

        /// <summary>
        /// Creates subscription to object instance changes.
        /// If objects which are referenced are changed, it is considered that parent object is also changed.
        /// </summary>
        /// <param name="instance">Object which changes are going to be notified</param>
        /// <param name="del">Delegate to be called</param>
        /// <returns>Subscription object</returns>
        public Subscription CreateSubscription(object instance, EventHandler<ObjectChangedEventArgs> del)
        {
            /*
            if (isolationLevel != IsolationLevel.Exclusive)
            {
                throw new InvalidOperationException("Exclusive isolation level is required for creating subscriptions");
            }
             * */

            return workspaceFacade.CreateSubscription(workspaceId, Utils.GetItemId(instance), false, del);
        }


        /// <summary>
        /// Creates subscription to object instance changes.
        /// If objects which are referenced are changed, it is considered that parent object is also changed.
        /// </summary>
        /// <param name="instance">Object which changes are going to be notified</param>
        /// <param name="notifyChangesFromSameWorkspace">Defines if changes from same workspace should be notified</param>
        /// <param name="del">Delegate to be called</param>
        /// <returns>Subscription object</returns>
        public Subscription CreateSubscription(object instance, bool notifyChangesFromSameWorkspace, EventHandler<ObjectChangedEventArgs> del)
        {
            return workspaceFacade.CreateSubscription(workspaceId, Utils.GetItemId(instance), notifyChangesFromSameWorkspace, del);
        }

        /// <summary>
        /// Creates subscription to object instance changes.
        /// If objects which are referenced are changed, it is considered that parent object is also changed.
        /// </summary>
        /// <param name="instance">Object which changes are going to be notified</param>
        /// <param name="propertyName">Property name which changes are going to be notified</param>
        /// <param name="del">Delegate to be called</param>
        /// <returns>Subscription object</returns>
        public Subscription CreateSubscription(object instance, string propertyName, EventHandler<ObjectChangedEventArgs> del)
        {
            return workspaceFacade.CreateSubscription(workspaceId, Utils.GetItemId(instance), propertyName, false, del);
        }

        /// <summary>
        /// Creates subscription to object instance changes.
        /// If objects which are referenced are changed, it is considered that parent object is also changed.
        /// </summary>
        /// <param name="instance">Object which changes are going to be notified</param>
        /// <param name="propertyName">Property name which changes are going to be notified</param>
        /// <param name="notifyChangesFromSameWorkspace">Defines if changes from same workspace should be notified</param>
        /// <param name="del">Delegate to be called</param>
        /// <returns>Subscription object</returns>
        public Subscription CreateSubscription(object instance, string propertyName, bool notifyChangesFromSameWorkspace, EventHandler<ObjectChangedEventArgs> del)
        {
            return workspaceFacade.CreateSubscription(workspaceId, Utils.GetItemId(instance), propertyName, notifyChangesFromSameWorkspace, del);
        }

        /// <summary>
        /// Removes subscription
        /// </summary>
        /// <param name="subscription">Subscription to remove</param>
        public void RemoveSubscription(Subscription subscription)
        {
            workspaceFacade.RemoveSubscription(subscription);
        }

        /// <summary>
        /// Creates restricted scope on the data in the workspace. It does not provide separate isolation of sub workspace, but makes access to this workspace directly.
        /// </summary>
        /// <typeparam name="T">Type of sub-object</typeparam>
        /// <param name="del">Delegate which selects the root object of sub workspace</param>
        /// <returns>Workspace of restricted scope</returns>
        public IWorkspace<T> SubWorkspace<T>(NavigateChildDelegate<T> del)
        {
            return new SubWorkspaceImpl<T, TDataType>(this, del);
        }

        /// <summary>
        /// Changes workspace isolation level to given value.
        /// Isolation level for workspace is also changed in WorkspaceStateProvider.
        /// </summary>
        /// <param name="isolationLevel"></param>
        public void ChangeIsolationLevel(IsolationLevel newIsolationLevel)
        {
            if (isolationLevel == IsolationLevel.ReadOnly)
            {
                throw new ArgumentException("Cannot change isolation level for read only workspace.");
            }
            this.isolationLevel = newIsolationLevel;
            workspaceFacade.ChangeWorkspaceIsolationLevel(workspaceId, newIsolationLevel);
        }

        /// <summary>
        /// Disposes the workspace. 
        /// All uncommited changes in the workspace are cancelled.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            CheckThread();

            if (disposing)
            {
                workspaceFacade.CloseWorkspace(workspaceId);

                mutableProxyMap.InvalidateProxies();

                // Make root proxy unusable
                Utils.SetItemId(rootProxy, Guid.Empty);
                rootProxy = default(TDataType);
            }
        }     
   
        private void CheckThread()
        {
            if (this.thread != Thread.CurrentThread)
            {
                throw new InvalidOperationException("Call was made from the thread which is not allowed by the workspace");
            }
        }


        public ICollection<object> ParentNodes<T>(object instance)
        {
            var id = Utils.GetItemId(instance);
            ICollection<object> result = new Collection<object>();
            var node = nodeProvider.GetNode(id, NodeAccess.Read);
            foreach (var item in collection)
            {
                
            }
            object proxy = null;
            if (!immutableProxyMap.TryGetProxy(id, out proxy))
            {
                proxy = proxyCreatorService.NewObject<T>(runtimeProxyFacade, id, true);
                immutableProxyMap.AddProxy(id, proxy);
            }

            return (T)proxy;
        }
    }
}
