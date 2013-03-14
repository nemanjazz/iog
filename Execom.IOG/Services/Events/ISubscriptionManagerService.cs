// -----------------------------------------------------------------------
// <copyright file="ISubscriptionManagerService.cs" company="Execom">
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

    /// <summary>
    /// Service which contains and manages event subscriptions
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal interface ISubscriptionManagerService
    {
        /// <summary>
        /// Creates subscription to object changes
        /// </summary>
        /// <param name="workspaceId">Workspace identifier which is making a subscription</param>
        /// <param name="instanceId">Instance Id which changes are going to be notified</param>
        /// <param name="notifyChangesFromSameWorkspace">Defines if changes from same workspace should be notified</param>
        /// <param name="del">Delegate to be called</param>
        /// <returns>Subscription object</returns>
        Subscription Create(Guid workspaceId, Guid instanceId, bool notifyChangesFromSameWorkspace, EventHandler<ObjectChangedEventArgs> del);

        /// <summary>
        /// Creates subscription to object changes for specific property
        /// </summary>
        /// <param name="workspaceId">Workspace identifier which is making a subscription</param>
        /// <param name="instanceId">Instance Id which changes are going to be notified</param>
        /// <param name="propertyName">Property name which changes are going to be notified</param>
        /// <param name="notifyChangesFromSameWorkspace">Defines if changes from same workspace should be notified</param>
        /// <param name="del">Delegate to be called</param>
        /// <returns>Subscription object</returns>
        Subscription Create(Guid workspaceId, Guid instanceId, string propertyName, bool notifyChangesFromSameWorkspace, EventHandler<ObjectChangedEventArgs> del);

        /// <summary>
        /// Removes subscription
        /// </summary>
        /// <param name="subscription">Subscription to remove</param>
        void Remove(Subscription subscription);

        /// <summary>
        /// Invokes events for object change subscriptions
        /// </summary>
        /// <param name="workspaceId">Workspace Id which is making the changes</param>
        /// <param name="commitResult">Commit result to process</param>
        void InvokeEvents(Guid workspaceId, CommitResult<Guid> commitResult);
    }
}
