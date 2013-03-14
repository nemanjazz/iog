// -----------------------------------------------------------------------
// <copyright file="Subscription.cs" company="Execom">
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

using System;
using System.Collections.Generic;
using System.Text;

namespace Execom.IOG.Events
{
    /// <summary>
    /// Event arguments for object change notifications.
    /// </summary>
    [Serializable]
    public class ObjectChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Defines snapshot ID where the change happened
        /// </summary>
        public Guid SnapshotId;
        
        /// <summary>
        /// Defines old instance revision ID
        /// </summary>
        public Guid OldRevisionId;

        /// <summary>
        /// Defines new instance revision ID
        /// </summary>
        public Guid NewRevisionId;

        /// <summary>
        /// Subscrition which triggered the notification
        /// </summary>
        public Subscription Subscription;

        /// <summary>
        /// Defines if subscription should be renewed after notification is thrown
        /// </summary>
        public bool RenewSubscription = true;

        /// <summary>
        /// Creates new instance of ObjectChangedEventArgs type
        /// </summary>
        /// <param name="snapshotId">Defines snapshot ID where the change happened</param>
        /// <param name="oldRevisionId">Defines old instance revision ID</param>
        /// <param name="newRevisionId">Defines new instance revision ID</param>
        public ObjectChangedEventArgs(Guid snapshotId, Guid oldRevisionId, Guid newRevisionId, Subscription subscription)
        {
            this.SnapshotId = snapshotId;
            this.OldRevisionId = oldRevisionId;
            this.NewRevisionId = newRevisionId;
            this.Subscription = subscription;
        }
    }

    /// <summary>
    /// Defines a subscription to object change events.
    /// </summary>
    /// <author>Nenad Sabo</author>
    [Serializable]
    public class Subscription
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
        /// Creates new instance of Subscription type
        /// </summary>
        /// <param name="subscriptionId">Unique ID of subscription</param>
        /// <param name="workspaceId">Workspace identifier which made the subscription</param>
        public Subscription(Guid subscriptionId, Guid workspaceId)
        {
            this.SubscriptionId = subscriptionId;
            this.WorkspaceId = workspaceId;
        }

    }
}
