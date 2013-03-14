// -----------------------------------------------------------------------
// <copyright file="IsolationLevel.cs" company="Execom">
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

    /// <summary>
    /// Represents workspace isolation level
    /// </summary>
    /// <author>Nenad Sabo</author>
    public enum IsolationLevel
    {
        /// <summary>
        /// All workspace operations are restricted to data reading
        /// </summary>
        ReadOnly,

        /// <summary>
        /// Workspace data can be read and modified concurrently with other workspaces
        /// </summary>
        Snapshot, 

        /// <summary>
        /// Workspace data can be modified exclusive to other workspaces. Reading is still possible from other workspaces.
        /// </summary>
        Exclusive
    }
}
