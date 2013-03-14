// -----------------------------------------------------------------------
// <copyright file="ClientContext.cs" company="Execom">
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
    using Execom.IOG.Storage;
    using Execom.IOG.Graph;
    using Execom.IOG.Providers;
    using Execom.IOG.Services.Data;
    using Execom.IOG.Services.Runtime;
    using Execom.IOG.Services.Facade;

    /// <summary>
    /// Context implementation for client applications with local data cache.
    /// </summary>
    /// <typeparam name="TDataType">Type of root entity interface for data navigation. This is the "all containing" entity, which has access to all other entities.</typeparam>
    /// <author>Nenad Sabo</author>
    public class ClientContext : IContext
    {
        /// <summary>
        /// Service for type manipulations
        /// </summary>
        private TypesService typesService;

        /// <summary>
        /// Service for proxy creation
        /// </summary>
        private ProxyCreatorService proxyCreatorService;

        /// <summary>
        /// Generation service
        /// </summary>
        private GenerationService generationService;

        /// <summary>
        /// Server context
        /// </summary>
        private IServerContext serverContext;

        /// <summary>
        /// Defines default workspace timeout interval
        /// </summary>
        private TimeSpan defaultWorkspaceTimeout;

        /// <summary>
        /// Defines if snapshot isolation is enabled
        /// </summary>
        private bool snapshotIsolationEnabled;

        /// <summary>
        /// Local data provider
        /// </summary>
        private INodeProvider<Guid, object, EdgeData> provider;

        /// <summary>
        /// Map of created read-only proxies in the workspace which is may be shared among workspaces
        /// </summary>
        private IProxyMap immutableProxyMap = new LimitedProxyMap(Properties.Settings.Default.ObjectCacheMinimumCount, Properties.Settings.Default.ObjectCacheMaximumCount);

        /// <summary>
        /// Creates new instance of ClientContext type
        /// </summary>
        /// <param name="serverContext">Server context interface</param>
        /// <param name="localStorage">Storage for local data</param>
        public ClientContext(IServerContext serverContext, IKeyValueStorage<Guid, object> localStorage)
        {
            this.serverContext = serverContext;
            this.provider = new CachedReadNodeProvider<Guid, object, EdgeData>(serverContext, new DirectNodeProviderUnsafe<Guid, object, EdgeData>(localStorage, localStorage is IForceUpdateStorage));
            this.snapshotIsolationEnabled = serverContext.SnapshotIsolationEnabled;
            this.defaultWorkspaceTimeout = serverContext.DefaultWorkspaceTimeout;

            InitializeServices();
        }

        public void Backup(IKeyValueStorage<Guid, object> storage)
        {
            throw new NotImplementedException();
        }

        public void Backup(Guid snapshotId, IKeyValueStorage<Guid, object> storage)
        {
            throw new NotImplementedException();
        }

        public void Cleanup()
        {
            throw new NotImplementedException();
        }

        public void ExpireWorkspaces()
        {
            throw new NotImplementedException();
        }

        public Guid LastSnapshotId()
        {
            return serverContext.LastSnapshotId();
        }

        /// <summary>
        /// Opens a new workspace with assumed latest available snapshot
        /// </summary>
        /// <typeparam name="TDataType">Type of root entity interface for data navigation. This is the "all containing" entity, which has access to all other entities.</typeparam>
        /// <param name="isolationLevel">Workspace isolation</param>
        /// <returns>New workspace instance</returns>
        public Workspace<TDataType> OpenWorkspace<TDataType>(IsolationLevel isolationLevel)
        {
            return OpenWorkspace<TDataType>(isolationLevel, defaultWorkspaceTimeout);
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
            return OpenWorkspace<TDataType>(snapshotId, isolationLevel, defaultWorkspaceTimeout);
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
            if (!snapshotIsolationEnabled && isolationLevel == IsolationLevel.Snapshot)
            {
                throw new ArgumentException("Snapshot isolation level disabled by configuration");
            }

            if (isolationLevel == IsolationLevel.Exclusive)
            {
                serverContext.EnterExclusiveLock();
            }

            Guid snapshotId = serverContext.LastSnapshotId();
            return new Workspace<TDataType>(snapshotId, timeout, provider, serverContext, proxyCreatorService, typesService, isolationLevel, immutableProxyMap);
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
            if (!snapshotIsolationEnabled && isolationLevel == IsolationLevel.Snapshot)
            {
                throw new ArgumentException("Snapshot isolation level disabled by configuration");
            }

            if (isolationLevel == IsolationLevel.Exclusive)
            {
                serverContext.EnterExclusiveLock();

                Guid lastSnapshotId = serverContext.LastSnapshotId();
                if (snapshotId != lastSnapshotId)
                {
                    throw new ArgumentException("Snapshot other than the last snapshot cannot be opened for exclusive write.");
                }
            }

            return new Workspace<TDataType>(snapshotId, timeout, provider, serverContext, proxyCreatorService, typesService, isolationLevel, immutableProxyMap);
        }

        private void InitializeServices()
        {
            typesService = new TypesService(provider);

            var interfaceToTypeIdMapping = typesService.InitializeTypeSystem(serverContext.EntityTypes);

            var completeTypesList = interfaceToTypeIdMapping.Keys;

            generationService = new GenerationService(typesService);
            // TODO (nsabo) Optional loading of proxy types from the given assembly (we dont want always to generate on small devices, Silverlight...)
            // Note: Collection/Dictionary types are not saved in the assembly
            var interfaceToGeneratedMapping = generationService.GenerateProxyTypes(completeTypesList, Properties.Settings.Default.SaveGeneratedAssemblyToDisk, Properties.Settings.Default.GeneratedAssemblyFileName);

            proxyCreatorService = new ProxyCreatorService(completeTypesList, interfaceToTypeIdMapping, interfaceToGeneratedMapping);

            StaticProxyFacade.Initialize(typesService);
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
            serverContext.EnterExclusiveLock();
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
            serverContext.ExitExclusiveLock();
            workspace.ChangeIsolationLevel(IsolationLevel.Snapshot);
        }
    }
}
