// -----------------------------------------------------------------------
// <copyright file="Context.cs" company="Execom">
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
    using Execom.IOG.Graph;
    using Execom.IOG.Services.Data;
    using Execom.IOG.Services.Runtime;
    using Execom.IOG.Providers;
    using Execom.IOG.Storage;
    using System.Reflection;
    using Execom.IOG.Services.Facade;
    using System.IO;
    using Execom.IOG.Tracking;
    using Execom.IOG.Services.Merging;
    using Execom.IOG.Services.Workspace;
    using Execom.IOG.Events;
    using Execom.IOG.Services.Events;
    using System.Collections.ObjectModel;
    using Execom.IOG.Upgrade;
    using System.Collections;
    using Execom.IOG.TypeVisual;
    using Execom.IOG.TypeVisual;
    
    /// <summary>
    /// Context is an instance which provides access to data in the storage.
    /// For more details see http://iog.codeplex.com/wikipage?title=Context
    /// </summary>
    /// <typeparam name="TDataType">Type of root entity interface for data navigation. This is the "all containing" entity, which has access to all other entities.</typeparam>
    /// <author>Nenad Sabo</author>
    public class Context : IContext, IDisposable
    {
        /// <summary>
        /// Node provider for this context, contains all nodes in the data storage
        /// </summary>
        internal INodeProvider<Guid, object, EdgeData> provider;

        /// <summary>
        /// Provider for node parents
        /// </summary>
        private IParentMapProvider<Guid, object, EdgeData> mutableParentProvider;

        /// <summary>
        /// Provider for changeset history
        /// </summary>
        private IChangeSetProvider<Guid, object, EdgeData> changeSetProvider;

        /// <summary>
        /// Provider for collected nodes
        /// </summary>
        private ICollectedNodesProvider<Guid, object, EdgeData> collectedNodesProvider;

        /// <summary>
        /// Service for type manipulations
        /// </summary>
        private TypesService typesService;

        /// <summary>
        /// Service which handles object instances on the root provider
        /// </summary>
        private ObjectInstancesService objectInstancesService;

        /// <summary>
        /// Entity types to be registered
        /// </summary>
        internal Type[] entityTypes;

        /// <summary>
        /// Type of root entity
        /// </summary>
        internal Type rootEntityType;

        /// <summary>
        /// Service for proxy creation
        /// </summary>
        private ProxyCreatorService proxyCreatorService;

        /// <summary>
        /// Snapshots service
        /// </summary>
        internal SnapshotsService snapshotsService;

        /// <summary>
        /// Generation service
        /// </summary>
        private GenerationService generationService;

        /// <summary>
        /// Service which performs the commit
        /// </summary>
        private CommitDataService commitDataService;

        /// <summary>
        /// Facade for workspace operations which interact with context
        /// </summary>
        internal IWorkspaceFacade workspaceFacade;

        /// <summary>
        /// Service which manages subscriptions
        /// </summary>
        private ISubscriptionManagerService subscriptionManagerService;

        /// <summary>
        /// Object serialization service
        /// </summary>
        private ObjectSerializationService objectSerializationService = new ObjectSerializationService();

        /// <summary>
        /// Map of created read-only proxies in the workspace which is may be shared among workspaces
        /// </summary>
        private IProxyMap immutableProxyMap = new LimitedProxyMap(Properties.Settings.Default.ObjectCacheMinimumCount, Properties.Settings.Default.ObjectCacheMaximumCount);

        /// <summary>
        /// Tracking of open workspaces and their timeouts
        /// </summary>
        private TrackingWorkspaceStateProvider trackingWorkspaceStateProvider;

        /// <summary>
        /// Determines if snapshot isolation is enabled
        /// </summary>
        internal bool SnapshotIsolationEnabled = Properties.Settings.Default.SnapshotIsolationEnabled;

        /// <summary>
        /// Defines default workspace timeout interval
        /// </summary>
        internal TimeSpan DefaultWorkspaceTimeout = TimeSpan.FromSeconds(Properties.Settings.Default.DefaultWorkspaceTimeoutSeconds);

        /// <summary>
        /// Provider of exclusive workspace lock
        /// </summary>
        internal WorkspaceExclusiveLockProvider workspaceExclusiveLockProvider;

        /// <summary>
        /// Backup service
        /// </summary>
        private BackupService backupService;

        /// <summary>
        /// View Data Service
        /// </summary>
        private IOGDataValuesService viewDataService;

        private string parentMappingFileName = Properties.Settings.Default.ParentMappingFileName;

        /// <summary>
        /// Synchronization object for cleanup operation
        /// </summary>
        private object cleanupSync = new object();

        /// <summary>
        /// Internal disposable dependencies, will be disposed on cleanup
        /// </summary>
        private Collection<IDisposable> disposables = new Collection<IDisposable>();

        /// <summary>
        /// Creates new instance of Context type. All types from the root data type assembly are considered entity types.
        /// </summary>
        /// <param name="rootEntityType">Type of root entity</param>
        public Context(Type rootEntityType)
        {
            InitializeDefaultProvider();
            Assembly entityAssembly = Assembly.GetAssembly(rootEntityType);
            InitializeServices(rootEntityType, new Type[] { rootEntityType }, null);
        }

        /// <summary>
        /// Creates new instance of Context type.
        /// </summary>
        /// <param name="rootEntityType">Type of root entity</param>
        /// <param name="entityTypes">Array of entity interface types to register. All reachable sub-types are registered automatically.</param>
        public Context(Type rootEntityType, Type[] entityTypes)
        {
            InitializeDefaultProvider();
            InitializeServices(rootEntityType, entityTypes, null);
        }

        /// <summary>
        /// Creates new instance of Context type.
        /// </summary>
        /// <param name="rootEntityType">Type of root entity</param>
        /// <param name="entityTypes">Array of entity interface types to register. All reachable sub-types are registered automatically.</param>
        /// <param name="storage">Storage to be used</param>
        public Context(Type rootEntityType, Type[] entityTypes, IKeyValueStorage<Guid, object> storage)
        {
            InitializeProvider(storage);
            if (entityTypes == null)
            {
                entityTypes = new Type[] { rootEntityType };
            }
            InitializeServices(rootEntityType, entityTypes, null);
        }

        /// <summary>
        /// Creates new instance of Context type with specified parent mapping file location.
        /// </summary>
        /// <param name="rootEntityType">Type of root entity</param>
        /// <param name="entityTypes">Array of entity interface types to register. All reachable sub-types are registered automatically.</param>
        /// <param name="storage">Storage to be used</param>
        public Context(Type rootEntityType, Type[] entityTypes, IKeyValueStorage<Guid, object> storage, string parentMappingFileName)
        {
            this.parentMappingFileName = parentMappingFileName;
            InitializeProvider(storage);
            if (entityTypes == null)
            {
                entityTypes = new Type[] { rootEntityType };
            }
            InitializeServices(rootEntityType, entityTypes, null);
        }

        /// <summary>
        /// Creates new instance of Context type and upgrades the data from existing storage.
        /// </summary>
        /// <param name="rootEntityType">Type of root entity</param>
        /// <param name="entityTypes">Array of entity interface types to register. All reachable sub-types are registered automatically.</param>
        /// <param name="storage">Storage to be used</param>
        /// <param name="upgradeConfiguration">Upgrade configuration</param>
        public Context(Type rootEntityType, Type[] entityTypes, IKeyValueStorage<Guid, object> storage, UpgradeConfiguration upgradeConfiguration)
        {
            InitializeProvider(storage);
            if (entityTypes == null)
            {
                entityTypes = new Type[] { rootEntityType };
            }
            InitializeServices(rootEntityType, entityTypes, upgradeConfiguration);
        }



        /// <summary>
        /// Opens a new workspace with assumed latest available snapshot
        /// </summary>
        /// <typeparam name="TDataType">Type of root entity interface for data navigation. This is the "all containing" entity, which has access to all other entities.</typeparam>
        /// <param name="isolationLevel">Workspace isolation</param>
        /// <returns>New workspace instance</returns>
        public Workspace<TDataType> OpenWorkspace<TDataType>(IsolationLevel isolationLevel)
        {
            return OpenWorkspace<TDataType>(isolationLevel, DefaultWorkspaceTimeout);
        }

        /// <summary>
        /// Opens a new workspace with given snapshot
        /// </summary>
        /// <typeparam name="TDataType">Type of root entity interface for data navigation. This is the "all containing" entity, which has access to all other entities.</typeparam>
        /// <param name="snapshotId">Snapshot ID</param>
        /// <param name="isolationLevel">Workspace isolation</param>
        /// <returns>New workspace instance</returns>
        public Workspace<TDataType> OpenWorkspace<TDataType>(Guid snapshotId, IsolationLevel isolationLevel)
        {
            return OpenWorkspace<TDataType>(snapshotId, isolationLevel, DefaultWorkspaceTimeout);
        }

        /// <summary>
        /// Opens a new workspace with assumed latest available snapshot
        /// </summary>
        /// <typeparam name="TDataType">Type of root entity interface for data navigation. This is the "all containing" entity, which has access to all other entities.</typeparam>
        /// <param name="isolationLevel">Workspace isolation</param>
        /// <param name="timeout">Workspace timeout</param>
        /// <returns>New workspace instance</returns>
        public Workspace<TDataType> OpenWorkspace<TDataType>(IsolationLevel isolationLevel, TimeSpan timeout)
        {
            if (!SnapshotIsolationEnabled && isolationLevel == IsolationLevel.Snapshot)
            {
                throw new ArgumentException("Snapshot isolation level disabled by configuration");
            }

            if (isolationLevel == IsolationLevel.Exclusive)
            {
                workspaceExclusiveLockProvider.EnterLockExclusive();
            }

            Guid snapshotId = snapshotsService.GetLatestSnapshotId();
            return new Workspace<TDataType>(snapshotId, timeout, provider, workspaceFacade, proxyCreatorService, typesService, isolationLevel, immutableProxyMap);
        }

        /// <summary>
        /// Opens a new workspace with given snapshot
        /// </summary>
        /// <typeparam name="TDataType">Type of root entity interface for data navigation. This is the "all containing" entity, which has access to all other entities.</typeparam>
        /// <param name="snapshotId">Snapshot ID</param>
        /// <param name="isolationLevel">Workspace isolation</param>
        /// <param name="timeout">Workspace timeout</param>
        /// <returns>New workspace instance</returns>
        public Workspace<TDataType> OpenWorkspace<TDataType>(Guid snapshotId, IsolationLevel isolationLevel, TimeSpan timeout)
        {
            if (!SnapshotIsolationEnabled && isolationLevel == IsolationLevel.Snapshot)
            {
                throw new ArgumentException("Snapshot isolation level disabled by configuration");
            }

            if (isolationLevel == IsolationLevel.Exclusive)
            {
                workspaceExclusiveLockProvider.EnterLockExclusive();

                Guid lastSnapshotId = snapshotsService.GetLatestSnapshotId();
                if (snapshotId != lastSnapshotId)
                {
                    throw new ArgumentException("Snapshot other than the last snapshot cannot be opened for exclusive write.");
                }
            }

            return new Workspace<TDataType>(snapshotId, timeout, provider, workspaceFacade, proxyCreatorService, typesService, isolationLevel, immutableProxyMap);
        }

        /// <summary>
        /// Updates given worksapce isolation level to exclusive.
        /// Workspace is updated to last snapshot because exclusive workspace can change data only for last snapshot.
        /// If given workspace isoaltion level is read only workspace isolation level cannot be updated and exception will be thrown.
        /// </summary>
        /// <typeparam name="TDataType"></typeparam>
        /// <param name="workspace"></param>
        public void UpdateWorkspaceToExclusive<TDataType>(Workspace<TDataType> workspace)
        {
            workspaceExclusiveLockProvider.EnterLockExclusive();
            workspace.Update();
            workspace.ChangeIsolationLevel(IsolationLevel.Exclusive);
        }

        /// <summary>
        /// Updates given worksapce isolation level to snapshot.
        /// If given workspace isoaltion level is read only workspace isolation level cannot be updated and exception will be thrown.
        /// </summary>
        /// <typeparam name="TDataType"></typeparam>
        /// <param name="workspace"></param>
        public void UpdateWorkspaceToSnapshot<TDataType>(Workspace<TDataType> workspace)
        {
            workspaceExclusiveLockProvider.ExitLockExclusive();
            workspace.ChangeIsolationLevel(IsolationLevel.Snapshot);
        }

        /// <summary>
        /// Returns last commited snapshot ID
        /// </summary>
        /// <returns>Snapshot ID</returns>
        public Guid LastSnapshotId()
        {
            return snapshotsService.GetLatestSnapshotId();
        }        

        /// <summary>
        /// Check for expired workspaces and unlock any locks they might hold
        /// </summary>
        public void ExpireWorkspaces()
        {
            // Clean expired workspaces
            trackingWorkspaceStateProvider.Cleanup();
        }

        /// <summary>
        /// Performs cleanup of historic data which is not used
        /// </summary>
        public void Cleanup()
        {
            lock (cleanupSync)
            {
                // Clean expired workspaces
                trackingWorkspaceStateProvider.Cleanup();

                // Determine which snapshots are used
                var usedSnapshots = trackingWorkspaceStateProvider.UsedSnapshotIds();

                // Ensure that last is always used
                var lastSnapshot = snapshotsService.GetLatestSnapshotId();
                if (!usedSnapshots.Contains(lastSnapshot))
                {
                    usedSnapshots.Add(lastSnapshot);
                }                

                // Determine which snapshots are not used, remove them from shapshots list
                var unusedSnapshots = snapshotsService.RemoveUnusedSnapshots(usedSnapshots);
                
                // Process removed snapshots
                foreach (var snapshotId in unusedSnapshots)
                {
                    // Garbage collect nodes which are not used
                    var collectedNodesEnumerator = collectedNodesProvider.GetEdges(snapshotId);

                    if (collectedNodesEnumerator != null)
                    {
                        using (collectedNodesEnumerator)
                        {
                            while (collectedNodesEnumerator.MoveNext())
                            {
                                provider.Remove(collectedNodesEnumerator.Current.ToNodeId);
                            }
                        }
                    }

                    // Remove change set history information
                    changeSetProvider.RemoveChangeSet(snapshotId);

                    // Remove information about collected nodes in the snapshot
                    collectedNodesProvider.Cleanup(snapshotId);

                    // Remove the snapshot node
                    provider.Remove(snapshotId);
                }
            }
        }

        public string ObjectsReport()
        {
            Hashtable usedNodes = new Hashtable();

            // Collect used nodes
            AddUsedNodesRecursive(LastSnapshotId(), usedNodes);

            StringBuilder sb = new StringBuilder();

            foreach (Guid nodeId in usedNodes.Keys)
            {
                // We found a node which is not used and should be collected
                var node = provider.GetNode(nodeId, NodeAccess.Read);

                string line = nodeId.ToString() + "\t" + node.NodeType.ToString() + "\t";

                var typeData = new EdgeData(EdgeType.OfType, null);

                if (node.Edges.ContainsKey(typeData))
                {
                    var typeEdge = node.FindEdge(typeData);

                    if (typeEdge != null)
                    {
                        line += typesService.GetTypeFromIdCached(typeEdge.ToNodeId).Name + "\t";
                    }
                }

                sb.AppendLine(line);
            }

            return sb.ToString();
        }

        public string CollectionReport()
        {
            Hashtable usedNodes = new Hashtable();

            // Collect used nodes
            foreach (var snapshotId in snapshotsService.ListSnapshots())
            {
                AddUsedNodesRecursive(snapshotId, usedNodes);
            }

            StringBuilder sb = new StringBuilder();

            foreach (Guid nodeId in provider.EnumerateNodes())
            {
                if (!usedNodes.ContainsKey(nodeId))
                {
                    // We found a node which is not used and should be collected
                    var node = provider.GetNode(nodeId, NodeAccess.Read);

                    string line = nodeId.ToString()+"\t"+ node.NodeType.ToString()+"\t";

                    var typeData = new EdgeData(EdgeType.OfType, null);

                    if (node.Edges.ContainsKey(typeData))
                    {
                        var typeEdge = node.FindEdge(typeData);

                        if (typeEdge != null)
                        {
                            line += typesService.GetTypeFromIdCached(typeEdge.ToNodeId).Name + "\t";
                        }
                    }

                    sb.AppendLine(line);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Detect unused nodes from old snapshots and delete them leaving only the most recent snapshot available
        /// </summary>
        private void OptimizeData()
        {
            Hashtable usedNodes = new Hashtable();            

            // Initialize only the last snapshot
            snapshotsService.ResetSnapshots();

            // Collect used nodes
            AddUsedNodesRecursive(Constants.SnapshotsNodeId, usedNodes);

            // Types are also used
            AddUsedNodesRecursive(Constants.TypesNodeId, usedNodes);

            Hashtable unusedNodes = new Hashtable();

            foreach (Guid nodeId in provider.EnumerateNodes())
            {
                if (!usedNodes.ContainsKey(nodeId))
                {
                    unusedNodes.Add(nodeId, null);
                }
            }

            foreach (Guid nodeId in unusedNodes.Keys)
            {
                provider.Remove(nodeId);
            }
        }

        private void AddUsedNodesRecursive(Guid nodeId, Hashtable usedNodes)
        {
            if (!usedNodes.ContainsKey(nodeId))
            {
                usedNodes.Add(nodeId, null);

                var node = provider.GetNode(nodeId, NodeAccess.Read);

                foreach (var edge in node.Edges.Values)
                {
                    if (edge.ToNodeId != Constants.NullReferenceNodeId)
                    {
                        AddUsedNodesRecursive(edge.ToNodeId, usedNodes);
                    }
                }
            }
        }


        /// <summary>
        /// Backup/replicate the data from last commited snapshot to another storage
        /// </summary>
        /// <param name="storage">Target storage for the backup</param>
        public void Backup(IKeyValueStorage<Guid, object> storage)
        {
            Backup(LastSnapshotId(), storage);
        }

        /// <summary>
        /// Backup/replicate the data from given snapshot to another storage
        /// </summary>
        /// <param name="snapshotId">Snapshot Id to backup</param>
        /// <param name="storage">Target storage for the backup</param>
        public void Backup(Guid snapshotId, IKeyValueStorage<Guid, object> storage)
        {
            if (storage is ISerializingStorage)
            {
                // Inject the serializer
                ((ISerializingStorage)storage).Serializer = objectSerializationService;
            }

            backupService.Backup(snapshotId, provider, new DirectNodeProviderUnsafe<Guid, object, EdgeData>(storage, false));

            if (storage is ISerializingStorage)
            {
                // Reset the serializer
                ((ISerializingStorage)storage).Serializer = null;
            }
        }      
  
        /// <summary>
        /// Create archive which contains an object tree in a separate storage
        /// </summary>
        /// <param name="revisionId">Object revision Id to archive. All dependent and related child data will be archived also.</param>
        /// <param name="storage">Target storage which will contain the archive. It must be empty prior to archiving.</param>
        public void CreateArchive(Guid revisionId, IKeyValueStorage<Guid, object> storage)
        {
            if (storage is ISerializingStorage)
            {
                // Inject the serializer
                ((ISerializingStorage)storage).Serializer = objectSerializationService;
            }

            backupService.Archive(revisionId, provider, new DirectNodeProviderUnsafe<Guid, object, EdgeData>(storage, false));

            if (storage is ISerializingStorage)
            {
                // Reset the serializer
                ((ISerializingStorage)storage).Serializer = null;
            }
        }

        /// <summary>
        /// Open an workspace over an archive. Archive must have the same schema as the main context.
        /// </summary>
        /// <typeparam name="TDataType">Data type of root archive object.</typeparam>
        /// <param name="storage">Storage which contains the archive.</param>
        /// <returns>A read-only workspace which allows data reading from an archive.</returns>
        public Workspace<TDataType> OpenArchive<TDataType>(IKeyValueStorage<Guid, object> storage)
        {
            INodeProvider<Guid, object, EdgeData> archiveProvider = new DirectNodeProviderUnsafe<Guid, object, EdgeData>(storage, false);

            if (storage is ISerializingStorage)
            {
                // Inject the serializer
                ((ISerializingStorage)storage).Serializer = objectSerializationService;
            }

            return new Workspace<TDataType>(Guid.Empty, TimeSpan.Zero, archiveProvider, new ArchiveWorkspaceFacade(archiveProvider), proxyCreatorService, typesService, IsolationLevel.ReadOnly, new LimitedProxyMap(Properties.Settings.Default.ObjectCacheMinimumCount, Properties.Settings.Default.ObjectCacheMaximumCount));
        }

        /// <summary>
        /// Performs clean shutdown
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "workspaceExclusiveLockProvider", Justification="Object is disposed through disposables collection")]
        protected virtual void Dispose(bool disposing)
        {
            foreach (var item in disposables)
            {
                item.Dispose();
            }
        }

        private void InitializeDefaultProvider()
        {
            // Nodes are stored in-memory only
            provider = new DirectNodeProviderSafe<Guid, object, EdgeData>(new MemoryStorageSafe<Guid, object>(), false);
        }

        private void InitializeProvider(IKeyValueStorage<Guid, object> storage)
        {
            if (storage is ISerializingStorage)
            {
                (storage as ISerializingStorage).Serializer = objectSerializationService;
            }

            var storageProvider = new DirectNodeProviderSafe<Guid, object, EdgeData>(storage, storage is IForceUpdateStorage);
            provider = new CachedReadNodeProvider<Guid, object, EdgeData>(storageProvider, new DirectNodeProviderSafe<Guid, object, EdgeData>(new LimitedMemoryStorageSafe<Guid, object>(Properties.Settings.Default.DataCacheMinimumCount, Properties.Settings.Default.DataCacheMaximumCount), false));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification="Objects disposed through disposables collection")]
        private void InitializeServices(Type rootEntityType, Type[] entityTypes, UpgradeConfiguration upgradeConfiguration)
        {
            this.rootEntityType = rootEntityType;
            this.entityTypes = entityTypes;

            typesService = new TypesService(provider);

            objectSerializationService.TypesService = typesService;
            
            var interfaceToTypeIdMapping = typesService.InitializeTypeSystem(entityTypes);

            var completeTypesList = interfaceToTypeIdMapping.Keys;

            generationService = new GenerationService(typesService);
            // TODO (nsabo) Optional loading of proxy types from the given assembly (we dont want always to generate on small devices, Silverlight...)
            // Note: Collection/Dictionary types are not saved in the assembly
            var interfaceToGeneratedMapping = generationService.GenerateProxyTypes(completeTypesList, Properties.Settings.Default.SaveGeneratedAssemblyToDisk, Properties.Settings.Default.GeneratedAssemblyFileName);

            proxyCreatorService = new ProxyCreatorService(completeTypesList, interfaceToTypeIdMapping, interfaceToGeneratedMapping);                       

            snapshotsService = new SnapshotsService(provider);

            #region Parent map provider setup
            if (Properties.Settings.Default.ParentMappingFileStorageUsed)
            {
                // Usage of file for caching parent information
                var indexedFile = new IndexedFileStorage(new FileStream(this.parentMappingFileName, FileMode.Create), Properties.Settings.Default.ParentMappingFileBlockSize, false);
                indexedFile.Serializer = this.objectSerializationService;
                disposables.Add(indexedFile);

                var parentProviderStorage = new CachedWriteNodeProviderUnsafe<Guid, object, EdgeData>(
                                                new DirectNodeProviderUnsafe<Guid, object, EdgeData>(indexedFile, true),
                                                new LimitedDirectNodeProviderUnsafe<Guid, object, EdgeData>(
                                                    new LimitedMemoryStorageUnsafe<Guid, object>(Properties.Settings.Default.ParentMappingMemoryMinimumCount, Properties.Settings.Default.ParentMappingMemoryMaximumCount), false)
                                                    );
                disposables.Add(parentProviderStorage);

                mutableParentProvider = new ParentMapProvider(parentProviderStorage, provider, null, true);
            }
            else
            {
                // Default parent information is stored in memory and has only the last snapshot available
                mutableParentProvider = new ParentMapProvider(new DirectNodeProviderUnsafe<Guid, object, EdgeData>(new MemoryStorageUnsafe<Guid, object>(), false), provider, null, true);
            } 
            #endregion

            #region Merge rule provider setup
            IMergeRuleProvider mergeRuleProvider = null;

            if (SnapshotIsolationEnabled)
            {

                if (Properties.Settings.Default.ConcurrencyAutoOverrideResolution)
                {
                    mergeRuleProvider = new AutoOverrideMergeRuleProvider();
                }
                else
                {
                    if (Properties.Settings.Default.ConcurrencyAttributesEnabled)
                    {
                        mergeRuleProvider = new AttributeBasedMergeRuleProvider(typesService);
                    }
                    else
                    {
                        throw new ArgumentException("No selected provider for merge rules in snapshot isolation conflicts. Check configuration of merge rule providers.");
                    }
                }
            } 
            #endregion

            #region Setup change set provider
            // TODO (nsabo) Provide option for change set safety when context goes offline, OfflineWorkspaces should enable commits when context is back online

            if (Properties.Settings.Default.ChangeSetHistoryFileStorageUsed)
            {
                var indexedFile = new IndexedFileStorage(new FileStream(Properties.Settings.Default.ChangeSetHistoryFileStorageFileName, FileMode.Create), 256, false);
                indexedFile.Serializer = this.objectSerializationService;
                disposables.Add(indexedFile);

                var changeSetProviderStorage = new CachedWriteNodeProviderUnsafe<Guid, object, EdgeData>(
                                                new DirectNodeProviderUnsafe<Guid, object, EdgeData>(indexedFile, true),
                                                new LimitedDirectNodeProviderUnsafe<Guid, object, EdgeData>(
                                                    new LimitedMemoryStorageUnsafe<Guid, object>(Properties.Settings.Default.ChangeSetHistoryWriteCacheMinimumCount, Properties.Settings.Default.ChangeSetHistoryWriteCacheMaximumCount), false)
                                                    );
                disposables.Add(changeSetProviderStorage);

                changeSetProvider = new TrackingChangeSetProvider(changeSetProviderStorage);
            }
            else
            {
                changeSetProvider = new TrackingChangeSetProvider(new DirectNodeProviderUnsafe<Guid, object, EdgeData>(new MemoryStorageUnsafe<Guid, object>(), false));
            }
            
            #endregion

            var immutableParentProvider = new ParentMapProvider(new DirectNodeProviderUnsafe<Guid, object, EdgeData>(new MemoryStorageUnsafe<Guid, object>(), false), provider, null, false);
            collectedNodesProvider = new CollectedNodesProvider(new DirectNodeProviderUnsafe<Guid, object, EdgeData>(new MemoryStorageUnsafe<Guid, object>(), false), provider);

            commitDataService = new CommitDataService(provider, typesService, snapshotsService, mutableParentProvider, immutableParentProvider, changeSetProvider, new NodeMergeExecutor(mergeRuleProvider, typesService), collectedNodesProvider);

            workspaceExclusiveLockProvider = new WorkspaceExclusiveLockProvider();
            disposables.Add(workspaceExclusiveLockProvider);

            trackingWorkspaceStateProvider = new TrackingWorkspaceStateProvider(workspaceExclusiveLockProvider);

            objectInstancesService = new ObjectInstancesService(provider, typesService);

            subscriptionManagerService = new SubscriptionManagerService(typesService, objectInstancesService);

            workspaceFacade = new WorkspaceFacade(commitDataService, trackingWorkspaceStateProvider, subscriptionManagerService, snapshotsService, workspaceExclusiveLockProvider);

            backupService = new BackupService();

            viewDataService = new IOGDataValuesService(provider, typesService);

            bool firstRun = snapshotsService.InitializeSnapshots();

            if (firstRun)
            {
                InitializeDefaultSnapshot();
            }
            else
            {
                OptimizeData();
            }
            
            StaticProxyFacade.Initialize(typesService);
        }        

        private void InitializeDefaultSnapshot()
        {
            Guid snapshotId = Guid.NewGuid();
            var snapshotNode = new Node<Guid, object, EdgeData>(NodeType.Snapshot, null);
                       
            Guid rootObjectId = objectInstancesService.NewInstance(typesService.GetTypeId(rootEntityType));

            
            snapshotNode.AddEdge(new Edge<Guid, EdgeData>(rootObjectId, new EdgeData(EdgeType.RootObject, null)));
            provider.SetNode(snapshotId, snapshotNode);

            snapshotsService.AddSnapshot(snapshotId);
        }

        /// <summary>
        /// Returns the Id of the type that represents the root of the database (the representation of the Data Model)
        /// </summary>
        /// <returns></returns>
        public Guid GetRootTypeId()
        {
            var snapshotRootNode = provider.GetNode(Constants.SnapshotsNodeId, NodeAccess.Read);
            var snapshotNode = provider.GetNode(snapshotRootNode.Edges.Values[0].ToNodeId, NodeAccess.Read);
            var objectNode = provider.GetNode(snapshotNode.Edges.Values[0].ToNodeId, NodeAccess.Read);
            foreach (var edge in objectNode.Edges.Values)
            {
                if (edge.Data.Semantic.Equals(EdgeType.OfType))
                {
                    return edge.ToNodeId;
                }
            }

            return Guid.Empty;
        }

        /// <summary>
        /// Returns the name of the type that represents the root of the database (the representation of the Data Model)
        /// </summary>
        /// <returns></returns>
        public string getRootTypeName()
        {
            var typeNode = provider.GetNode(GetRootTypeId(), NodeAccess.Read);
            return TypeVisualUtilities.GetTypeNameFromAssemblyName((string)typeNode.Data);
        }
        
        /// <summary>
        /// Gets the information of all types in the context (with the rootTypeId as the root type, doesn't actually have to be the root entity), and puts them in a dictionary, where the key is the name of the type.
        /// </summary>
        /// <returns></returns>
        public IDictionary<String,TypeVisualUnit> GetTypeVisualUnits(Guid rootTypeId)
        {
            IDictionary<String,TypeVisualUnit> retVal = new Dictionary<String,TypeVisualUnit>();
            List<string> ignoreTypeNames = new List<string>();
            if (!rootTypeId.Equals(Guid.Empty))
            {
                GetTypeVisualUnitsRecursive(rootTypeId, retVal, ignoreTypeNames);
            }
            return retVal;
        }

        /// <summary>
        /// Recursively fills the units dictionary with a TypeVisualisationUnit of the Guid - typeNodeId.
        /// </summary>
        /// <param name="typeNodeId"></param>
        /// <param name="units"></param>
        /// <param name="ignoreTypeNames"></param>
        private void GetTypeVisualUnitsRecursive(Guid typeNodeId, IDictionary<String,TypeVisualUnit> units, ICollection<string> ignoreTypeNames)
        {
            TypeVisualUnit unit = null;
            string typeName = null;
            ICollection<TypeVisualProperty> scalarProperties = new List<TypeVisualProperty>();
            ICollection<TypeVisualProperty> nonScalarProperties = new List<TypeVisualProperty>();

            if (!typeNodeId.Equals(Guid.Empty))
            {
                var typeNode = provider.GetNode(typeNodeId, NodeAccess.Read);
                typeName = TypeVisualUtilities.GetTypeNameFromAssemblyName((string)typeNode.Data);
                if(!ignoreTypeNames.Contains(typeName))
                {
                    ignoreTypeNames.Add(typeName);
                    foreach (var edge in typeNode.Edges.Values)
                    {
                        if (edge.Data.Semantic.Equals(EdgeType.Property))
                        {
                            //setting name and type
                            bool isScalar;
                            var nodeProperty = provider.GetNode(edge.ToNodeId, NodeAccess.Read);
                            var nodePropertyType = provider.GetNode(nodeProperty.Edges.Values[0].ToNodeId, NodeAccess.Read);
                            string propertyTypeName = TypeVisualUtilities.GetTypeNameFromAssemblyName((string)nodePropertyType.Data);

                            string collectionKey,collectionValue;
                            PropertyCollectionType collectionType = TypeVisualUtilities.CheckIfCollectionOrDictionary(propertyTypeName, 
                                out collectionKey, out collectionValue);

                            //checking if property is a collection and if property is scalar.
                            if (collectionType != PropertyCollectionType.NotACollection)
                            {
                                if (!typesService.IsSupportedScalarTypeName(collectionValue))
                                {
                                    Guid genericArgumentTypeId = typesService.GetIdFromTypeName(collectionValue);
                                    if (!genericArgumentTypeId.Equals(Guid.Empty))
                                    {
                                        //ignoreTypeNames.Add(typeName);
                                        GetTypeVisualUnitsRecursive(genericArgumentTypeId, units, ignoreTypeNames);
                                    }
                                    isScalar = false;
                                }
                                else
                                    isScalar = true;
                                propertyTypeName = collectionValue;
                            }
                            else if (!typesService.IsSupportedScalarTypeName(propertyTypeName))
                            {
                                //ignoreTypeNames.Add(typeName);
                                GetTypeVisualUnitsRecursive(nodeProperty.Edges.Values[0].ToNodeId, units, ignoreTypeNames);
                                isScalar = false;
                            }
                            else
                                isScalar = true;

                            //setting additional property attributes
                            bool isImmutable, isPrimaryKey;
                            PropertyAttribute propAttribute;
                            isImmutable = edge.Data.Flags.Equals(EdgeFlags.Permanent);
                            isPrimaryKey = nodeProperty.Values.ContainsKey(Constants.TypeMemberPrimaryKeyId);
                            if (isImmutable && isPrimaryKey)
                                propAttribute = PropertyAttribute.PrimaryKeyAndImmutableProperty;
                            else if (isImmutable)
                                propAttribute = PropertyAttribute.ImmutableProperty;
                            else if (isPrimaryKey)
                                propAttribute = PropertyAttribute.PrimaryKeyProperty;
                            else
                                propAttribute = PropertyAttribute.None;

                            //adding the property to either scalar or non-scalar lists.
                            TypeVisualProperty property = new TypeVisualProperty((string)nodeProperty.Data, propertyTypeName, 
                                propAttribute, collectionType, collectionKey);

                            if (isScalar)
                                scalarProperties.Add(property);
                            else
                                nonScalarProperties.Add(property);
             
                        }
                        else if (edge.Data.Semantic.Equals(EdgeType.Contains))
                        {
                            var nodeProperty = provider.GetNode(edge.ToNodeId, NodeAccess.Read);
                            TypeVisualProperty property = new TypeVisualProperty((string)nodeProperty.Data, TypeVisualProperty.EnumType,
                                PropertyAttribute.None, PropertyCollectionType.NotACollection, "");
                            scalarProperties.Add(property);
                        }
                    }
                    unit = new TypeVisualUnit(typeName, scalarProperties, nonScalarProperties);
                    units.Add(unit.Name, unit);
                }
            }
        }

        /// <summary>
        /// Method for creating a Context specifically for Type Visualisation. 
        /// <para>For that reason, only the provider and types services are initialized, 
        /// because the types that are needed for visualisation might not be available in the assembly.</para>
        /// </summary>
        /// <param name="storage">The storage from which the types needed for visualisation are loaded</param>
        /// <returns>Context created for type visualisation.</returns>
        private static Context CreateContextForTypeVisualisation(IKeyValueStorage<Guid, object> storage)
        {
            return new Context(storage);
        }

        /// <summary>
        /// Private constructor of Context for usage in Type Visualisation. For that reason, only the provider and types services are initialized, 
        /// because the types that are needed for visualisation might not be available in the assembly. 
        /// </summary>
        /// <param name="storage">The storage from which the types needed for visualisation are loaded</param>
        private Context(IKeyValueStorage<Guid, object> storage)
        {
            InitializeProvider(storage);
            typesService = new TypesService(provider);

        }

        /// <summary>
        /// Creates and returns a content for a GraphViz (.gv) file (using DOT language), for the visualisation of types that are in the storage.
        /// For this purpose, a Context is created only for this purpose, and cannot be used for any other.
        /// </summary>
        /// <param name="storage">Storage from where the types are extracted</param>
        /// <returns>content for the GraphViz file</returns>
        public static String GetGraphVizContentFromStorage(IKeyValueStorage<Guid, object> storage)
        {
            String graphVizContent;
            Context ctx = CreateContextForTypeVisualisation(storage);
            IDictionary<String,TypeVisualUnit> typeUnits = ctx.GetTypeVisualUnits(ctx.GetRootTypeId());
            GVTemplate template = new GVTemplate(typeUnits);
            graphVizContent = template.TransformText();
            return graphVizContent;
        }

        /// <summary>
        /// Creates and returns a content for a GraphViz (.gv) file (using DOT language), for the visualisation of types that are represented in this Context.
        /// </summary>
        /// <returns>content for the GraphViz file</returns>
        public String getGraphVizContent()
        {
            String graphVizContent;
            IDictionary<String, TypeVisualUnit> typeUnits = GetTypeVisualUnits(GetRootTypeId());
            GVTemplate template = new GVTemplate(typeUnits);
            graphVizContent = template.TransformText();
            return graphVizContent;
        }

        /// <summary>
        /// Creates and returns a content for a GraphViz (.gv) file (using DOT language), for the visualisation of types that are in the storage.
        /// <para>For this purpose, a Context is created only for this purpose, and cannot be used for any other.</para>
        /// </summary>
        /// <param name="storage">Storage from where the types are extracted</param>
        /// <returns>content for the GraphViz file</returns>
        public static String GetGraphVizContentFromStorage(string typeName,IKeyValueStorage<Guid, object> storage)
        {
            String graphVizContent;
            Context ctx = CreateContextForTypeVisualisation(storage);
            Guid typeId = ctx.typesService.GetIdFromTypeName(typeName);
            if(typeId.Equals(Guid.Empty))
                throw new Exception("Type with the following name : " + typeName + " doesn't exist in this Context");
            IDictionary<String, TypeVisualUnit> typeUnits = ctx.GetTypeVisualUnits(typeId);
            GVTemplate template = new GVTemplate(typeUnits);
            graphVizContent = template.TransformText();
            return graphVizContent;
        }

        /// <summary>
        /// Creates and returns a content for a GraphViz (.gv) file (using DOT language), for the visualisation of types that are represented in this Context.
        /// </summary>
        /// <returns>content for the GraphViz file</returns>
        public String getGraphVizContent(string typeName)
        {
            String graphVizContent;
            Guid typeId = typesService.GetIdFromTypeName(typeName);
            if (typeId.Equals(Guid.Empty))
                throw new Exception("Type with the following name : " + typeName + " doesn't exist in this Context");
            IDictionary<String, TypeVisualUnit> typeUnits = GetTypeVisualUnits(typeId);
            GVTemplate template = new GVTemplate(typeUnits);
            graphVizContent = template.TransformText();
            return graphVizContent;
        }

        /// <summary>
        /// Returns a collection of data for the model
        /// </summary>
        /// <returns>An IEnumerable of objects representing the model data</returns>
        public IOGDataStructure GetDataFromModel()
        {
            var snapshotRootNode = provider.GetNode(Constants.SnapshotsNodeId, NodeAccess.Read);
            var snapshotNode = provider.GetNode(snapshotRootNode.Edges.Values[0].ToNodeId, NodeAccess.Read);
            var objectNode = provider.GetNode(snapshotNode.Edges.Values[0].ToNodeId, NodeAccess.Read);
            return viewDataService.GetDataForSelectedNode(objectNode);
        }

        /// <summary>
        /// Returns a dictionary of TypeVisualUnits (where the type name is the key) stored in the context of the passed storage. 
        /// </summary>
        /// <param name="storage">The storage from which the context will be created.</param>
        /// <returns>A dictionary of TypeVisualUnits (where the type name is the key) containing information about the types in the storage.</returns>
        public static IDictionary<String, TypeVisualUnit> GetTypeVisualisationUnitsFromStorage(IKeyValueStorage<Guid, object> storage)
        {
            Context ctx = CreateContextForTypeVisualisation(storage);
            return ctx.GetTypeVisualUnits(ctx.GetRootTypeId());
        }

        /// <summary>
        /// Returns the name of the root type from the information in the storage.
        /// </summary>
        /// <param name="storage"></param>
        /// <returns></returns>
        public static string GetRootTypeNameFromStorage(IKeyValueStorage<Guid, object> storage)
        {
            Context ctx = CreateContextForTypeVisualisation(storage);
            return ctx.getRootTypeName();
        }
    }
}
