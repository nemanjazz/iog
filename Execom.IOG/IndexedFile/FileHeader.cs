// -----------------------------------------------------------------------
// <copyright file="FileHeader.cs" company="Execom">
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
    using System.IO;

    /// <summary>
    /// Represents data in the indexed file header
    /// </summary>
    public class FileHeader
    {
        /// <summary>
        /// File identification
        /// </summary>
        public string Magic;
        /// <summary>
        /// File format version
        /// </summary>
        public int Version;
        /// <summary>
        /// Cluster size in bytes
        /// </summary>
        public int ClusterSize;
        /// <summary>
        /// Defines number of cluster which stores index data. -1 value means there is no cluster.
        /// </summary>
        public int IndexCluster;

        /// <summary>
        /// Writes header to stream
        /// </summary>
        /// <param name="stream">Stream to write to</param>
        internal void Write(System.IO.Stream stream)
        {
            // TODO (nsabo) binary writer is disposable but cannot dispose here because it closes the stream also
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write(Magic);
            bw.Write(Version);
            bw.Write(ClusterSize);
            bw.Write(IndexCluster);
        }

        /// <summary>
        /// Reads header from stream
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        internal void Read(Stream stream)
        {
            // TODO (nsabo) binary reader is disposable but cannot dispose here because it closes the stream also
            BinaryReader br = new BinaryReader(stream);
            Magic = br.ReadString();
            Version = br.ReadInt32();
            ClusterSize = br.ReadInt32();
            IndexCluster = br.ReadInt32();
        }
    }
}
