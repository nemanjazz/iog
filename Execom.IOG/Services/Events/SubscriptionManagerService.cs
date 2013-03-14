// -----------------------------------------------------------------------
// <copyright file="SubscriptionManagerService.cs" company="Execom">
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

namespace Execom.IOG.Services.Events
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Execom.IOG.Events;
    using Execom.IOG.Graph;
    using System.Collections.ObjectModel;
    using Execom.IOG.Services.Data;
    using System.Collections;

    /// <summary>
    /// Implementation of subscription manager.
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal class SubscriptionManagerService : ISubscriptionManagerService
    {
        /// <summary>
        /// Service which manages types
        /// </summary>
        private TypesService typesService;

        /// <summary>
        /// Service which handles object instances
        /// </summary>
        private ObjectInstancesService objectInstancesService;

        /// <summary>
        /// Defines a subscription element
        /// </summary>
        private class SubscriptionElement
        {
            /// <summary>
            /// Unique ID of subscription
            /// </summary>
            public Guid SubscriptionId;

            /// <summary>
            /// Workspace identifier which made the subscription
            /// </summary>
            public Guid WorkspaceId;

            /// <summary>
            /// Object instance ID which is monitored for changes
            /// </summary>
            public Guid InstanceId;

            /// <summary>
            /// Member ID which is monitored for changes
            /// </summary>
            public Guid MemberId;

            /// <summary>
            /// Delegate which is invoked on change
            /// </summary>
            public EventHandler<ObjectChangedEventArgs> Del;

            /// <summary>
            /// Defines if changes from same workspace should be notified
            /// </summary>
            public bool NotifyChangesFromSameWorkspace;

            /// <summary>
            /// Creates new instance of SubscriptionElement type
            /// </summary>
            /// <param name="subscriptionId">Unique ID of subscription</param>
            /// <param name="workspaceId">Workspace identifier which made the subscription</param>
            /// <param name="instanceId">Object instance ID which is monitored for changes</param>
            /// <param name="memberId">Member ID which is monitored for changes</param>
            /// <param name="notifyChangesFromSameWorkspace">Defines if changes from same workspace should be notified</param>
            /// <param name="del">Delegate which is invoked on change</param>
            public SubscriptionElement(Guid subscriptionId, Guid workspaceId, Guid instanceId, Guid memberId, bool notifyChangesFromSameWorkspace, EventHandler<ObjectChangedEventArgs> del)
            {
                this.SubscriptionId = subscriptionId;
                this.WorkspaceId = workspaceId;
                this.InstanceId = instanceId;
                this.MemberId = memberId;
                this.NotifyChangesFromSameWorkspace = notifyChangesFromSameWorkspace;
                this.Del = del;
            }

        }

        /// <summary>
        /// Stores subscription elements by subscription IDs
        /// </summary>
        private Dictionary<Guid, SubscriptionElement> subscriptions = new Dictionary<Guid, SubscriptionElement>();
        
        /// <summary>
        /// Creates new instance of SubscriptionManagerService type
        /// </summary>
        /// <param name="typesService">Service which manages types</param>
        /// <param name="objectInstancesService">Service which handles object instances</param>
        public SubscriptionManagerService(TypesService typesService, ObjectInstancesService objectInstancesService)
        {
            this.typesService = typesService;
            this.objectInstancesService = objectInstancesService;
        }

        /// <summary>
        /// Creates subscription to object changes
        /// </summary>
        /// <param name="workspaceId">Workspace identifier which is making a subscription</param>
        /// <param name="instanceId">Instance Id which changes are going to be notified</param>
        /// <param name="notifyChangesFromSameWorkspace">Defines if changes from same workspace should be notified</param>
        /// <param name="del">Delegate to be called</param>
        /// <returns>Subscription object</returns>
        public Subscription Create(Guid workspaceId, Guid instanceId, bool notifyChangesFromSameWorkspace, EventHandler<ObjectChangedEventArgs> del)
        {
            lock (subscriptions)
            {
                var id = Guid.NewGuid();
                var res = new SubscriptionElement(id, workspaceId, instanceId, Guid.Empty, notifyChangesFromSameWorkspace, del);
                subscriptions.Add(id, res);
                return new Subscription(id, workspaceId);
            }
        }

        /// <summary>
        /// Creates subscription to object changes for specific property
        /// </summary>
        /// <param name="workspaceId">Workspace identifier which is making a subscription</param>
        /// <param name="instanceId">Instance Id which changes are going to be notified</param>
        /// <param name="propertyName">Property name which changes are going to be notified</param>
        /// <param name="notifyChangesFromSameWorkspace">Defines if changes from same workspace should be notified</param>
        /// <param name="del">Delegate to be called</param>
        /// <returns>Subscription object</returns>
        public Subscription Create(Guid workspaceId, Guid instanceId, string propertyName, bool notifyChangesFromSameWorkspace, EventHandler<ObjectChangedEventArgs> del)
        {
            lock (subscriptions)
            {
                var id = Guid.NewGuid();
                var typeId = typesService.GetInstanceTypeId(instanceId);
                var memberId = typesService.GetTypeMemberId(typeId, propertyName);

                if (memberId.Equals(Guid.Empty))
                {
                    throw new ArgumentException("Property " + propertyName + " not found in type " + typesService.GetTypeFromId(typeId));
                }

                var res = new SubscriptionElement(id, workspaceId, instanceId, memberId, notifyChangesFromSameWorkspace, del);
                subscriptions.Add(id, res);
                return new Subscription(id, workspaceId);
            }
        }

        /// <summary>
        /// Removes subscription
        /// </summary>
        /// <param name="subscription">Subscription to remove</param>
        public void Remove(Subscription subscription)
        {
            lock (subscriptions)
            {
                subscriptions.Remove(subscription.SubscriptionId);
            }
        }

        /// <summary>
        /// Invokes events for object change subscriptions
        /// </summary>
        /// <param name="workspaceId">Workspace Id which is making the changes</param>
        /// <param name="commitResult">Commit result to process</param>
        public void InvokeEvents(Guid workspaceId, CommitResult<Guid> commitResult)
        {
            lock (subscriptions)
            {
                Collection<Guid> removalList = new Collection<Guid>();

                foreach (var item in subscriptions.Values)
                {
                    // Was the object changed in the commit?
                    if (commitResult.Mapping.ContainsKey(item.InstanceId))
                    {
                        // Is the change originating from different workspace OR we wanted events from same workspace 
                        bool doThrowEvent = !item.WorkspaceId.Equals(workspaceId) || item.NotifyChangesFromSameWorkspace;

                        // If property was specified
                        if (doThrowEvent && !item.MemberId.Equals(Guid.Empty))
                        {
                            var isScalarMember = typesService.IsScalarType(typesService.GetMemberTypeId(item.MemberId));

                            if (isScalarMember)
                            {
                                // Get scalar property values
                                var oldValue = objectInstancesService.GetScalarInstanceMember(item.InstanceId, item.MemberId);
                                var newValue = objectInstancesService.GetScalarInstanceMember(commitResult.Mapping[item.InstanceId], item.MemberId);

                                // Throw event if they are not the same
                                doThrowEvent = Comparer.Default.Compare(oldValue, newValue) != 0;
                            }
                            else
                            {
                                bool isPermanent;
                                // Get reference property values
                                var oldValue = objectInstancesService.GetReferenceInstanceMember(item.InstanceId, item.MemberId, out isPermanent);
                                var newValue = objectInstancesService.GetReferenceInstanceMember(commitResult.Mapping[item.InstanceId], item.MemberId, out isPermanent);

                                // Throw event if they are not pointing to the same Ids
                                doThrowEvent = Comparer.Default.Compare(oldValue, newValue) != 0;
                            }
                        }
                        
                        if (doThrowEvent)
                        {
                            var args = new ObjectChangedEventArgs(commitResult.ResultSnapshotId, item.InstanceId, commitResult.Mapping[item.InstanceId], new Subscription(item.SubscriptionId, item.WorkspaceId));
                            // Throw event
                            item.Del(this, args);

                            if (args.RenewSubscription)
                            {
                                // Renew subscription to new version
                                item.InstanceId = commitResult.Mapping[item.InstanceId];
                            }
                            else
                            {
                                // Register for removal
                                removalList.Add(item.SubscriptionId);
                            }
                        }
                        else
                        {
                            // Automatically renew subscriptions when processing own changes
                            item.InstanceId = commitResult.Mapping[item.InstanceId];
                        }
                    }
                }

                // Remove items which are not renewed
                foreach (var item in removalList)
                {
                    subscriptions.Remove(item);
                }
            }
        }
    }
}
