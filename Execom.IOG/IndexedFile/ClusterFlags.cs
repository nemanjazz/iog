// -----------------------------------------------------------------------
// <copyright file="ClusterFlags.cs" company="Execom">
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

namespace Execom.IOG.IndexedFile
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Defines flags which are set to clusters
    /// </summary>
    /// <author>Nenad Sabo</author>
    [Flags]
    internal enum ClusterFlags : byte
    {
        /// <summary>
        /// Empty block
        /// </summary>
        Empty = 0,          
        /// <summary>
        /// Header cluster of multi-cluster stream
        /// </summary>
        StreamHeader = 1,   
        /// <summary>
        /// Data cluster of multi-cluster stream
        /// </summary>
        StreamData = 2,    
        /// <summary>
        /// Safe copy of original data while editing
        /// </summary>
        SafeCopy = 4       
    }
}
