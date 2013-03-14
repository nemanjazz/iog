// -----------------------------------------------------------------------
// <copyright file="IndexedFile.cs" company="Execom">
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
    using System.Diagnostics;
    using System.Collections.ObjectModel;
    using System.Collections;    
    
    /// <summary>
    /// Represents key-value storage in a single file, able to store multiple small files in a large one.
    /// Key is of type Guid, value is byte array of variable length. It is assumed that reading/writing is done in one read/write operation, thus limiting usage to reasonably small data chunks, with not good support for incomming / streaming data.
    /// Index of keys is in memory for fast retreival of file locations, it is rebuit on startup, therefore the slower startup time for longer files. 
    /// Memory usage for the index is roughly 60 MB per 1 million items.
    /// Access is thread safe from multiple threads, all calls are exclusive. 
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal class IndexedFile : IDisposable
    {       
        /// <summary>
        /// Represents information contained in the data cluster
        /// </summary>
        private class DataCluster
        {
            /// <summary>
            /// Cluster flags which describe the status
            /// </summary>
            public ClusterFlags Flags;

            /// <summary>
            /// Cluster sequence number
            /// </summary>
            public int ClusterNumber;

            /// <summary>
            /// Next cluster
            /// </summary>
            public DataCluster NextCluster;        
    
            /// <summary>
            /// Creates new instance of DataCluster type
            /// </summary>
            /// <param name="flags">Initial flags</param>
            /// <param name="clusterNumber">Assigned cluster number</param>
            /// <param name="nextCluster">Next cluster</param>
            public DataCluster(ClusterFlags flags, int clusterNumber, DataCluster nextCluster)
            {
                this.Flags = flags;
                this.ClusterNumber = clusterNumber;
                this.NextCluster = nextCluster;
            }

            /// <summary>
            /// Defines number of bytes in the cluster header (Flags = 1, NextCluster = 4)
            /// </summary>
            public static int HeaderBytes = 5;
        }

        /// <summary>
        /// Represents information contained in the data cluster
        /// </summary>
        private class HeaderCluster : DataCluster
        {
            /// <summary>
            /// Defines key which is stored in cluster
            /// </summary>
            public Guid Key;

            /// <summary>
            /// Defines total data length stored in cluster, and optional data clusters that follow
            /// </summary>
            public int DataLength;

            /// <summary>
            /// Creates new instance of HeaderCluster type
            /// </summary>
            /// <param name="flags">Initial flags</param>
            /// <param name="clusterNumber">Assigned cluster number</param>
            /// <param name="nextCluster">Next cluster</param>
            /// <param name="key">Key which is stored</param>
            /// <param name="dataLength">Total data length</param>
            public HeaderCluster(ClusterFlags flags, Guid key, int clusterNumber, int dataLength, DataCluster nextCluster) :
                base(flags, clusterNumber, nextCluster)
            {
                this.Key = key;
                this.DataLength = dataLength;
            }

            /// <summary>
            /// Defines number of bytes in the cluster header (Flags = 1, Key = 16, DataLength = 4, NextCluster = 4)
            /// </summary>
            public static new int HeaderBytes = 25;
        }

        private static int GrowthRate = Properties.Settings.Default.IndexedFileGrowthRateBlocks;

        /// <summary>
        /// Key which is used for storing the index
        /// </summary>
        private Guid indexKey = new Guid("{748DF1EF-952F-4025-9C19-F79E97F301D7}");

        /// <summary>
        /// Indicates that shutdown was performed
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Synchronization object
        /// </summary>
        private object sync = new object();

        /// <summary>
        /// Underlying file
        /// </summary>
        private Stream fs;

        /// <summary>
        /// Index of existing keys
        /// </summary>
        private Hashtable index = new Hashtable();

        /// <summary>
        /// List of empty cluster ids
        /// </summary>
        private Queue<int> emptyClusters = new Queue<int>();

        /// <summary>
        /// File header information
        /// </summary>
        private FileHeader header = new FileHeader();        

        /// <summary>
        /// Header reserved bytes
        /// </summary>
        private const int ReservedHeaderLength = 512;

        /// <summary>
        /// Size of individual cluster
        /// </summary>
        private int clusterSize;

        /// <summary>
        /// Determines if writes should be done with power failure safety
        /// </summary>
        private bool safeWrites;

        /// <summary>
        /// Creates new instance of IndexedFile type
        /// </summary>
        /// <param name="file">File stream to use</param>
        /// <param name="clusterSize">Preffered cluster size for new files. Existing files will use their original size.</param>
        /// <param name="safeWrites">Defines if safe writes are used</param>
        public IndexedFile(Stream file, int clusterSize, bool safeWrites)
        {
            Init(file, clusterSize, safeWrites, Properties.Settings.Default.IndexedFileMagic);
        }

        /// <summary>
        /// Creates new instance of IndexedFile type
        /// </summary>
        /// <param name="file">File stream to use</param>
        /// <param name="clusterSize">Preffered cluster size for new files. Existing files will use their original size.</param>
        /// <param name="safeWrites">Defines if safe writes are used</param>
        /// <param name="headerMagic">Expected header value</param>
        public IndexedFile(Stream file, int clusterSize, bool safeWrites, string headerMagic)
        {
            Init(file, clusterSize, safeWrites, headerMagic);
        }

        private void Init(Stream file, int clusterSize, bool safeWrites, string headerMagic)
        {
            if (clusterSize <= HeaderCluster.HeaderBytes)
            {
                throw new ArgumentException("Cluster size must be greater than " + HeaderCluster.HeaderBytes);
            }

            this.fs = file;
            this.clusterSize = clusterSize;
            this.safeWrites = safeWrites;

            lock (sync)
            {
                if (file.Length > 0)
                {
                    if (!ReadFileHeader(headerMagic))
                    {
                        throw new InvalidOperationException("File header not recognized");
                    }
                    // Assume cluster size from the header
                    clusterSize = header.ClusterSize;

                    if (header.IndexCluster != -1)
                    {
                        LoadIndex();
                    }
                    else
                    {
                        ReconstructIndex();
                    }
                }
                else
                {
                    InitializeFileHeader(headerMagic);
                }
            }
        }

        /// <summary>
        /// Write data to file
        /// </summary>
        /// <param name="key">Key which identifies the data. If the key already exists, previous data will be overwritten.</param>
        /// <param name="data">Data bytes to store</param>
        public void Write(Guid key, byte[] data, int offset, int length)
        {
            lock (sync)
            {
                CheckDisposed();
                GrowFile(data.Length == 0 ? 1 : length);

                HeaderCluster entry = null;
                int safeCopyClusterNumber = -1;

                if (index.ContainsKey(key))
                {
                    safeCopyClusterNumber = (int)index[key];
                    entry = AllocateNewHeader(key, length);
                    index[key] = entry.ClusterNumber;

                    MarkAsSafeCopy(safeCopyClusterNumber);
                }
                else
                {
                    entry = AllocateNewHeader(key, length);
                    index.Add(key, entry.ClusterNumber);
                }

                // Write to allocated head
                int dataOffset = offset;
                DataCluster cluster = entry;
                byte[] buffer = new byte[clusterSize];

                do
                {
                    int headerBytes = (cluster is HeaderCluster ? HeaderCluster.HeaderBytes : DataCluster.HeaderBytes);
                    int remainingBytes = clusterSize - headerBytes;
                    // Position to start of the stream
                    PositionToCluster(cluster.ClusterNumber);
                    // Write cluster header information to prepared buffer
                    WriteCluster(cluster, buffer);
                    // See how many bytes we are writing
                    int writtenBytes = Math.Min(remainingBytes, length - (dataOffset - offset));
                    // Copy the data from the incomming buffer to prepared buffer
                    Array.Copy(data, dataOffset, buffer, headerBytes, writtenBytes);
                    // Write the prepared buffer
                    fs.Write(buffer, 0, clusterSize);
                    // Advance the dataOffset
                    dataOffset += remainingBytes;
                    // Move to next cluster
                    cluster = cluster.NextCluster;
                }
                while ((dataOffset - offset) < length);

                if (safeCopyClusterNumber != -1)
                {
                    DeleteChain(safeCopyClusterNumber, true);
                }
            }
        }        

        /// <summary>
        /// Reads the data from file
        /// </summary>
        /// <param name="key">Key which identifies the data. If key does not exist, exception is thrown.</param>
        /// <returns>Data bytes</returns>
        public byte[] Read(Guid key)
        {
            lock (sync)
            {
                CheckDisposed();
                if (!index.ContainsKey(key))
                {
                    throw new ArgumentException("Key not found");
                }

                int clusterNumber = (int)index[key];

                return Read(clusterNumber);
            }
        }

        /// <summary>
        /// Reads the data from file
        /// </summary>
        /// <param name="clusterNumber">Cluster of data location.</param>
        /// <returns>Data bytes</returns>
        private byte[] Read(int clusterNumber)
        {
            byte[] result = null;
            int dataOffset = 0;
            int dataLength = 1;
            byte[] buffer = new byte[clusterSize];
            while (dataOffset < dataLength)
            {
                PositionToCluster(clusterNumber);
                fs.Read(buffer, 0, clusterSize);

                ClusterFlags flags = (ClusterFlags)buffer[0];
                int headerBytes = 0;

                if ((flags & ClusterFlags.StreamHeader) != 0)
                {
                    clusterNumber = BitConverter.ToInt32(buffer, 1);
                    dataLength = BitConverter.ToInt32(buffer, 21);                    
                    headerBytes = HeaderCluster.HeaderBytes;

                    if (result == null)
                    {
                        result = new byte[dataLength];
                    }
                    else
                    {
                        throw new InvalidOperationException("Unexpected second header in chain");
                    }

                }
                else
                {
                    if ((flags & ClusterFlags.StreamData) != 0)
                    {
                        clusterNumber = BitConverter.ToInt32(buffer, 1);
                        headerBytes = DataCluster.HeaderBytes;
                    }
                    else
                    {
                        throw new InvalidDataException();
                    }
                }

                if (result == null)
                {
                    throw new InvalidOperationException("Header not found in chain");
                }

                int remainingBytes = clusterSize - headerBytes;
                Array.Copy(buffer, headerBytes, result, dataOffset, Math.Min(remainingBytes, dataLength - dataOffset));
                dataOffset += remainingBytes;
            }
            return result;
        }

        /// <summary>
        /// Deletes data from file
        /// </summary>
        /// <param name="key">Key which identifies the data. If key does not exist, exception is thrown.</param>
        /// <returns>True if deletion was successful</returns>
        public bool Delete(Guid key)
        {
            lock (sync)
            {
                CheckDisposed();


                if (!index.ContainsKey(key))
                {
                    return false;
                }
                else
                {

                    DeleteChain((int)index[key], true);

                    index.Remove(key);

                    return true;
                }
            }
        }

        /// <summary>
        /// Determines if key is present in the file
        /// </summary>
        /// <param name="key">Key which identifies the data</param>
        /// <returns>True if data exists</returns>
        public bool Contains(Guid key)
        {
            lock (sync)
            {
                CheckDisposed();
                return index.ContainsKey(key);
            }
        }

        /// <summary>
        /// Lists all keys in the file
        /// </summary>
        /// <returns>List of keys</returns>
        public IEnumerable ListKeys()
        {
            lock (sync)
            {
                CheckDisposed();
                return index.Keys;
            }
        }

        /// <summary>
        /// Deletes all entries in the file
        /// </summary>
        public void Clear()
        {
            lock (sync)
            {
                CheckDisposed();
                foreach (Guid key in index.Keys)
                {
                    Delete(key);
                }
            }
        }

        /// <summary>
        /// Generates report about data fragmentation
        /// </summary>
        /// <returns>List of present bytestream lengths separated by newline</returns>
        public string FragmentationReport()
        {
            StringBuilder sb = new StringBuilder();
            byte[] buffer = new byte[clusterSize];

            foreach (Guid item in index.Keys)
            {
                int clusterNumber = (int)index[item];    
                PositionToCluster(clusterNumber);
                fs.Read(buffer, 0, clusterSize);

                ClusterFlags flags = (ClusterFlags)buffer[0];

                if ((flags & ClusterFlags.StreamHeader) != 0)
                {
                    int dataLength = BitConverter.ToInt32(buffer, 21);
                    sb.AppendLine(dataLength.ToString());
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Performs clean file shutdown
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (sync)
            {
                CheckDisposed();
                if (disposing)
                {
                    StoreIndex();
                    fs.Flush();
                }

                disposed = true;
            }
        }

        /// <summary>
        /// Stores index to file
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private void StoreIndex()
        {
            // Store index information to byte stream
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(index.Count);

                    foreach (Guid key in index.Keys)
                    {
                        bw.Write(key.ToByteArray());
                        bw.Write((int)index[key]);
                    }

                    bw.Write((int)emptyClusters.Count);

                    foreach (int number in emptyClusters)
                    {
                        bw.Write(number);
                    }

                    if (index.ContainsKey(indexKey))
                    {
                        throw new InvalidDataException("Index entry should not be here");
                    }

                    Write(indexKey, ms.GetBuffer(), 0, (int)ms.Length);
                }
            }

            // Store the index cluster in file header
            header.IndexCluster = (int)index[indexKey];
            fs.Position = 0;
            header.Write(fs);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private void LoadIndex()
        {
            if (header.IndexCluster != -1)
            {
                int indexCluster = header.IndexCluster;
                byte[] data = Read(indexCluster);
                using (MemoryStream ms = new MemoryStream(data))
                {
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        int nrEntries = br.ReadInt32();
                        for (int i = 0; i < nrEntries; i++)
                        {
                            index.Add(new Guid(br.ReadBytes(16)), br.ReadInt32());
                        }

                        int nrEmpty = br.ReadInt32();

                        for (int i = 0; i < nrEmpty; i++)
                        {
                            int nr = br.ReadInt32();
#if DEBUG
                            Debug.Assert(!emptyClusters.Contains(nr));
#endif
                            emptyClusters.Enqueue(nr);
                        }
                    }
                }

                // Clear the index cluster location
                header.IndexCluster = -1;
                fs.Position = 0;
                header.Write(fs);

                // Delete index data chain
                DeleteChain(indexCluster, false);
            }
        }

        /// <summary>
        /// Deletes chain identified by the head
        /// </summary>
        /// <param name="clusterNumber">Cluster number</param>
        /// <param name="setToEmpty">Defines if chain clusters should be set to empty</param>
        private void DeleteChain(int clusterNumber, bool setToEmpty)
        {
            do
            {
                PositionToCluster(clusterNumber);
                fs.WriteByte((byte)ClusterFlags.Empty);

                if (setToEmpty)
                {
#if DEBUG
                    Debug.Assert(!emptyClusters.Contains(clusterNumber));
#endif
                    emptyClusters.Enqueue(clusterNumber);
                }

                byte[] intBuffer = new byte[4];
                fs.Read(intBuffer, 0, 4);
                int nextCluster = BitConverter.ToInt32(intBuffer, 0);
                clusterNumber = nextCluster;
            }
            while (clusterNumber != -1);
        }

        /// <summary>
        /// Writes cluster information to byte array
        /// </summary>
        /// <param name="cluster">Cluster to write</param>
        /// <param name="buffer">Byte array</param>
        private static void WriteCluster(DataCluster cluster, byte[] buffer)
        {
            if (cluster is HeaderCluster)
            {
                HeaderCluster head = cluster as HeaderCluster;
                // Store information in format [Flags][Next][Key][Length]
                buffer[0] = (byte)cluster.Flags;
                Array.Copy(BitConverter.GetBytes(head.NextCluster == null ? -1 : head.NextCluster.ClusterNumber), 0, buffer, 1, 4);
                Array.Copy(head.Key.ToByteArray(), 0, buffer, 5, 16);
                Array.Copy(BitConverter.GetBytes(head.DataLength), 0, buffer, 21, 4);                
            }
            else
            {
#if DEBUG
                Debug.Assert(cluster is DataCluster);
#endif
                buffer[0] = (byte)cluster.Flags;
                Array.Copy(BitConverter.GetBytes(cluster.NextCluster == null ? -1 : cluster.NextCluster.ClusterNumber), 0, buffer, 1, 4);
            }
        }

        /// <summary>
        /// Marks cluster as safe copy
        /// </summary>
        /// <param name="clusterNumber">Cluster to mark</param>
        private void MarkAsSafeCopy(int clusterNumber)
        {
            if (safeWrites)
            {
                PositionToCluster(clusterNumber);
                fs.WriteByte((byte)(ClusterFlags.StreamHeader | ClusterFlags.SafeCopy));
            }
        }

        /// <summary>
        /// Takes empty clusters and reserves a header cluster
        /// </summary>
        /// <param name="key">Key for the data</param>
        /// <param name="length">Bytes required</param>
        /// <returns>Head allocation</returns>
        private HeaderCluster AllocateNewHeader(Guid key, int length)
        {
            HeaderCluster head = new HeaderCluster(ClusterFlags.StreamHeader,
                                                   key,
                                                   emptyClusters.Dequeue(),
                                                   length,
                                                   null);

            int remainingLength = length - (clusterSize - HeaderCluster.HeaderBytes);
            DataCluster prev = head;

            while (remainingLength > 0)
            {
                DataCluster data = new DataCluster(ClusterFlags.StreamData,
                                                          emptyClusters.Dequeue(),
                                                          null);

                remainingLength -= (clusterSize - DataCluster.HeaderBytes);
                prev.NextCluster = data;
                prev = data;
            }

            return head;
        }        

        /// <summary>
        /// Calculates maximum theoretical stream which can be stored in existing structure
        /// </summary>
        /// <returns>Available bytes</returns>
        private int AvailableSpace()
        {
            if (emptyClusters.Count == 0)
            {
                return 0;
            }
            else
            {
                return (clusterSize - HeaderCluster.HeaderBytes) + (emptyClusters.Count - 1) * (clusterSize - DataCluster.HeaderBytes);
            }
        }

        /// <summary>
        /// Grows the file to accomodate for required space, by allocating new space
        /// </summary>
        /// <param name="requiredSpace">Number of free bytes required</param>
        private void GrowFile(int requiredSpace)
        {
            while (requiredSpace > AvailableSpace())
            {
                if (((fs.Length - ReservedHeaderLength) % clusterSize) != 0)
                {
                    throw new InvalidDataException("Incorrect file size");
                }

                int oldCount = (int)((fs.Length - ReservedHeaderLength) / clusterSize);
                fs.SetLength(fs.Length + GrowthRate * clusterSize);

                for (int i = oldCount; i < oldCount + GrowthRate; i++)
                {
#if DEBUG
                    Debug.Assert(!emptyClusters.Contains(i));
#endif
                    emptyClusters.Enqueue(i);
                }
            }
        }

        /// <summary>
        /// Writes file header at file beginning
        /// </summary>
        private void InitializeFileHeader(string headerMagic)
        {
            fs.SetLength(ReservedHeaderLength);
            fs.Position = 0;
            header.Magic = headerMagic;
            header.Version = Properties.Settings.Default.IndexedFileVersion;
            header.ClusterSize = clusterSize;
            header.IndexCluster = -1;
            header.Write(fs);
        }

        /// <summary>
        /// Reads and collects clusters in index
        /// </summary>
        private void ReconstructIndex()
        {
            byte[] buffer = new byte[HeaderCluster.HeaderBytes];
            for (int i = 0; i < (fs.Length - ReservedHeaderLength) / clusterSize; i++)
            {
                PositionToCluster(i);
                fs.Read(buffer, 0, buffer.Length);

                ClusterFlags flags = (ClusterFlags)buffer[0];

                if (flags != ClusterFlags.Empty)
                {                    
                    if ((flags & ClusterFlags.StreamHeader) != 0)
                    {
                        Guid id = new Guid(new byte[] { buffer[5], buffer[6], buffer[7], buffer[8], buffer[9], buffer[10], buffer[11], buffer[12], buffer[13], buffer[14], buffer[15], buffer[16], buffer[17], buffer[18], buffer[19], buffer[20] });

                        if (!id.Equals(indexKey))
                        {

                            if ((flags & ClusterFlags.SafeCopy) != 0)
                            {
                                // Write not safe copy status
                                PositionToCluster(i);
                                fs.WriteByte((byte)ClusterFlags.StreamHeader);

                                // Key already in index ?
                                if (index.ContainsKey(id))
                                {
                                    // Delete previous chain and free space
                                    DeleteChain((int)index[id], true);
                                    // Remove from index
                                    index.Remove(id);
                                }
                            }

                            if (!index.ContainsKey(id))
                            {
                                index.Add(id, i);
                            }
                            else
                            {
                                // Delete duplicate chain and free space
                                DeleteChain(i, true);
                            }
                        }
                        else
                        {
                            // Index entry found at this point means it was corrupted
                            DeleteChain(i, false);
                        }
                    }
                }
                else
                {
#if DEBUG
                    Debug.Assert(!emptyClusters.Contains(i));
#endif
                    emptyClusters.Enqueue(i);
                }
            }
        }

        /// <summary>
        /// Sets stream position to start of given cluster number
        /// </summary>
        /// <param name="clusterNumber">Cluster number to position to</param>
        private void PositionToCluster(int clusterNumber)
        {
            fs.Position = ReservedHeaderLength + (long)clusterNumber * clusterSize;
        }

        /// <summary>
        /// Reads the chain of clusters
        /// </summary>
        /// <param name="clusterNumber">Cluster number</param>
        /// <returns>Cluster chain</returns>
        private DataCluster ReadChain(int clusterNumber)
        {
            if (clusterNumber == -1)
            {
                return null;
            }

            // Position to start of cluster
            PositionToCluster(clusterNumber);

            byte[] buffer = new byte[HeaderCluster.HeaderBytes];
            fs.Read(buffer, 0, buffer.Length);

            ClusterFlags flags = (ClusterFlags)buffer[0];

            if ((flags & ClusterFlags.StreamHeader) != 0)
            {
                HeaderCluster cluster = new HeaderCluster(
                flags,                
                new Guid(new byte[] { buffer[5], buffer[6], buffer[7], buffer[8], buffer[9], buffer[10], buffer[11], buffer[12], buffer[13], buffer[14], buffer[15], buffer[16], buffer[17], buffer[18], buffer[19], buffer[20] }),                
                clusterNumber,
                BitConverter.ToInt32(buffer, 21),
                ReadChain(BitConverter.ToInt32(buffer, 1))
                );

                return cluster;
            }
            else
            {
                if ((flags & ClusterFlags.StreamData) != 0)
                {

                    DataCluster cluster = new DataCluster(
                    flags,
                    clusterNumber,
                    ReadChain(BitConverter.ToInt32(buffer, 1)));

                    return cluster;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Reads the file header
        /// </summary>
        /// <returns>True if compatible header is detected</returns>
        private bool ReadFileHeader(string headerMagic)
        {
            try
            {
                fs.Position = 0;
                header.Read(fs);
                return header.Magic == headerMagic && header.Version == Properties.Settings.Default.IndexedFileVersion;
            }
            catch
            {
                return false;
            }            
        }

        /// <summary>
        /// Throws exception if object is disposed
        /// </summary>
        private void CheckDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("IndexedFile");
            }
        }
    }
}
