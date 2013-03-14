// -----------------------------------------------------------------------
// <copyright file="ConcurrentAttribute.cs" company="Execom">
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

namespace Execom.IOG.Attributes
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Defines behaviors for entity conflict resolution
    /// </summary>
    public enum ConcurrentBehavior
    {
        /// <summary>
        /// Defines that behavior is defined per type
        /// </summary>
        Static,

        /// <summary>
        /// Defines that resolution is performed per instance
        /// </summary>
        Dynamic
    }

    public interface IUserDefinedMergeResolver
    {
    }

    /// <summary>
    /// This attribute is set on entity interface types. It marks type as allowed to be
    /// changed from different workspaces at the same time.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
    public sealed class ConcurrentAttribute : Attribute
    {
        public ConcurrentBehavior Behavior;
        public Type Resolver;

        public ConcurrentAttribute()
        {
            this.Behavior = ConcurrentBehavior.Static;
        }

        public ConcurrentAttribute(ConcurrentBehavior behavior, Type resolver)
        {
            this.Behavior = behavior;

            if (behavior == ConcurrentBehavior.Dynamic)
            {
                if (resolver.GetInterfaces().Length != 1 ||
                    resolver.GetInterfaces()[0] != typeof(IUserDefinedMergeResolver))
                {
                    throw new ArgumentException("User defined resolver type missing or not derived from IUserDefinedMergeResolver");
                }

                this.Resolver = resolver;
            }
        }
    }

}
