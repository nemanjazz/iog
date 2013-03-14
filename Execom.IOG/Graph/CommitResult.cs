// -----------------------------------------------------------------------
// <copyright file="CommitResult.cs" company="Execom">
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

namespace Execom.IOG.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Defines an entity which represents result data of the commit operation
    /// </summary>
    /// <author>Nenad Sabo</author>
    /// <typeparam name="TIdentifier">Identifier type</typeparam>
    [Serializable]
    public class CommitResult<TIdentifier>
    {
        /// <summary>
        /// Defines result snapshot Id
        /// </summary>
        public TIdentifier ResultSnapshotId;

        /// <summary>
        /// Defines mapping between old->new id defined from the originating snapshot Id
        /// </summary>
        public Dictionary<TIdentifier, TIdentifier> Mapping;

        /// <summary>
        /// Creates new instance of CommitResult type
        /// </summary>
        /// <param name="resultSnapshotId">Defines result snapshot Id</param>
        /// <param name="mapping">Defines mapping between old->new id defined from the originating snapshot Id</param>
        public CommitResult(TIdentifier resultSnapshotId, Dictionary<TIdentifier, TIdentifier> mapping)
        {
            this.ResultSnapshotId = resultSnapshotId;
            this.Mapping = mapping;
        }
    }
}
