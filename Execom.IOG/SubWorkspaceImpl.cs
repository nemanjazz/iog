// -----------------------------------------------------------------------
// <copyright file="SubWorkspaceImpl.cs" company="Execom">
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
    using Execom.IOG.Events;

    /// <summary>
    /// Defines workspace which provides restricted view within a parent workspace
    /// </summary>
    /// <typeparam name="TDataType">Root data type</typeparam>
    /// <author>Nenad Sabo</author>
    public class SubWorkspaceImpl<TDataType, TParentDataType> : IWorkspace<TDataType>
    {
        /// <summary>
        /// Parent workspace
        /// </summary>
        private IWorkspace<TParentDataType> parentWorkspace;

        /// <summary>
        /// Delegate which returns root object from the parent
        /// </summary>
        private NavigateChildDelegate<TDataType> del;

        /// <summary>
        /// Creates new instance of SubWorkspace type
        /// </summary>
        /// <param name="parentWorkspace">Parent workspace</param>
        /// <param name="del">Delegate which returns root object from the parent</param>
        public SubWorkspaceImpl(IWorkspace<TParentDataType> parentWorkspace, NavigateChildDelegate<TDataType> del)
        {
            this.parentWorkspace = parentWorkspace;
            this.del = del;
        }

        /// <summary>
        /// This operation makes data changes in the workspace available for other workspaces.
        /// Commit creates new data version, with new snapshot ID. After commit, new data version will be 
        /// accessible as last version to new workspaces created with Context.OpenWorkspace()
        /// </summary>
        /// <returns>New snapshot ID</returns>
        public Guid Commit()
        {
            return parentWorkspace.Commit();
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
            return parentWorkspace.CreateSubscription(instance, del);
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
            return parentWorkspace.CreateSubscription(instance, notifyChangesFromSameWorkspace, del);
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
            return parentWorkspace.CreateSubscription(instance, propertyName, del);
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
            return parentWorkspace.CreateSubscription(instance, propertyName, notifyChangesFromSameWorkspace, del);
        }

        /// <summary>
        /// Provides access to data
        /// </summary>
        public TDataType Data
        {
            get
            {
                // Call the delegate to resolve the root
                return del();
            }
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
            return parentWorkspace.ImmutableView<T>(instance);
        }

        /// <summary>
        /// Returns instance revision ID of given object.
        /// Object can be reconstructed back with given ID via Spawn method.
        /// </summary>
        /// <param name="instance">Instance which revision is determined</param>
        /// <returns>Unique ID for the object.</returns>
        public Guid InstanceRevisionId(object instance)
        {
            return parentWorkspace.InstanceRevisionId(instance);
        }

        /// <summary>
        /// Creates new instance of type T
        /// </summary>
        /// <typeparam name="T">Interface type of instance to create</typeparam>
        /// <returns>Created object instance</returns>
        public T New<T>()
        {
            return parentWorkspace.New<T>();
        }

        /// <summary>
        /// Removes subscription
        /// </summary>
        /// <param name="subscription">Subscription to remove</param>
        public void RemoveSubscription(Subscription subscription)
        {
            parentWorkspace.RemoveSubscription(subscription);
        }

        /// <summary>
        /// This operation cancels all data changes made in the workspace.
        /// Workspace stays in original snapshot, and does not reflect the updates in the mean time.
        /// </summary>
        public void Rollback()
        {
            parentWorkspace.Rollback();
        }

        /// <summary>
        /// Sets reference of object as immutable
        /// </summary>
        /// <param name="instance">Object which holds the reference</param>
        /// <param name="propertyName">Property name of the reference</param>
        public void SetImmutable(object instance, string propertyName)
        {
            parentWorkspace.SetImmutable(instance, propertyName);
        }

        /// <summary>
        /// Gets the unique ID of the data version opened by the workspace.
        /// </summary>
        public Guid SnapshotId
        {
            get
            {
                return parentWorkspace.SnapshotId;
            }
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
            return parentWorkspace.Spawn<T>(revisionId);
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
            return parentWorkspace.SpawnImmutable<T>(revisionId);
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
        /// This operation moves workspace to latest snapshot ID.
        /// All uncommited changes which are made in the workspace are going to be preserved.
        /// In case that conflicting changes were made, ConcurrencyModificationException will be thrown.
        /// </summary>
        public void Update()
        {
            parentWorkspace.Update();
        }

        /// <summary>
        /// This operation moves workspace to given snapshot ID.
        /// All uncommited changes which are made in the workspace are going to be preserved.
        /// In case that conflicting changes were made, ConcurrentModificationException will be thrown and workspace will remain not updated.
        /// </summary>
        /// <param name="newSnapshotId">Snapshot Id to be loaded, must be in sequence after the current snapshot ID</param>
        public void Update(Guid newSnapshotId)
        {
            parentWorkspace.Update(newSnapshotId);
        }

        /// <summary>
        /// This operation moves workspace to latest snapshot ID.
        /// All uncommited changes which are made in the workspace are going to be preserved.
        /// In case that conflicting changes were made, return value will be false and workspace will remain not updated.
        /// </summary>
        /// <returns>True if update was successful</returns>
        public bool TryUpdate()
        {
            return parentWorkspace.TryUpdate();
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
            return parentWorkspace.TryUpdate(newSnapshotId);
        }

        /// <summary>
        /// Changes workspace isolation level to given value.
        /// Isolation level for workspace is also changed in WorkspaceStateProvider.
        /// </summary>
        /// <param name="isolationLevel"></param>
        public void ChangeIsolationLevel(IsolationLevel isolationLevel)
        {
            parentWorkspace.ChangeIsolationLevel(isolationLevel);
        }


        public System.Collections.Generic.ICollection<T> ParentNodes<T>(object instance)
        {
            throw new NotImplementedException();
        }
    }
}
