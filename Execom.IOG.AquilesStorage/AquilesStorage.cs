// -----------------------------------------------------------------------
// <copyright file="AquilesStorage.cs" company="Execom">
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
using Execom.IOG.Storage;
using Aquiles.Core.Cluster;
using Aquiles.Cassandra10;
using Aquiles.Helpers.Encoders;
using Apache.Cassandra;
using Aquiles.Helpers;
using System.IO;

namespace Execom.IOG.AquilesStorage
{
    /// <summary>
    /// Implementation of key value storage via Aquiles connector to Apache Cassandra.
    /// Cassandra cluster must be installed and configured according to Apache Cassandra documentation.
    /// IOG stores data in predefined keyspace and column family which need to be pre-created by the cassandra-cli or other external tool.
    /// Aquiles configuration must be setup in configuration file, according to aquiles.codeplex.com documentation. These setting would also define connection parameters, such as host and port of cassandra instance.
    /// </summary>
    /// <author>Nenad Sabo</author>
    public class AquilesStorage : IKeyValueStorage<Guid, object>, ISerializingStorage
    {
        /// <summary>
        /// Reference to Cassandra cluster interface
        /// </summary>
        private ICluster cluster;

        /// <summary>
        /// Key space used to store the data
        /// </summary>
        private string keySpace;

        /// <summary>
        /// Column family which stores the data
        /// </summary>
        private string columnFamily;

        /// <summary>
        /// Creates new instance of AquilesStorage type
        /// </summary>
        /// <param name="clusterName">Cluster name</param>
        /// <param name="keySpace">Key space name used to store IOG data</param>
        /// <param name="columnFamily">Column family name used to store IOG data</param>
        public AquilesStorage(string clusterName, string keySpace, string columnFamily)
        {
            AquilesHelper.Initialize();
            cluster = AquilesHelper.RetrieveCluster(clusterName);            

            this.keySpace = keySpace;
            this.columnFamily = columnFamily;
        }

        /// <summary>
        /// Serializer to be injected by IOG
        /// </summary>
        public IObjectSerializer Serializer { get; set; }

        /// <summary>
        /// Adds or updates value in the storage
        /// </summary>
        /// <param name="key">Key which identifies the data</param>
        /// <param name="value">Data to store</param>
        /// <returns>True if operation was success</returns>
        public bool AddOrUpdate(Guid key, object value)
        {
            // Insert statement
            byte[] keyEncoded = key.ToByteArray();

            ColumnParent columnParent = new ColumnParent();
            columnParent.Column_family = columnFamily;

            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {

                    Serializer.Serialize(value, bw);

                    byte[] data = new byte[ms.Length];

                    // Copy only the used portion of the buffer
                    Array.Copy(ms.GetBuffer(), data, ms.Length);

                    Column column = new Column()
                    {
                        Name = ByteEncoderHelper.UTF8Encoder.ToByteArray("Value"),
                        Timestamp = UnixHelper.UnixTimestamp,
                        Value = data,
                    };

                    cluster.Execute(new ExecutionBlock(delegate(Cassandra.Client client)
                    {
                        client.insert(keyEncoded, columnParent, column, ConsistencyLevel.ONE);
                        return null;
                    }), keySpace);
                }
            }

            return true;
        }

        /// <summary>
        /// Reads data from storage
        /// </summary>
        /// <param name="key">Key which identifies the data</param>
        /// <returns>Storage data</returns>
        public object Value(Guid key)
        {
            // Get statement
            byte[] keyEncoded = key.ToByteArray();
            ColumnPath columnPath = new ColumnPath()
            {
                Column = ByteEncoderHelper.UTF8Encoder.ToByteArray("Value"),
                Column_family = columnFamily,
            };

            ColumnOrSuperColumn column = (ColumnOrSuperColumn)cluster.Execute(new ExecutionBlock(delegate(Cassandra.Client client)
            {
                return client.get(keyEncoded, columnPath, ConsistencyLevel.ONE);
            }), keySpace);

            var data = column.Column.Value;

            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    return Serializer.Deserialize(br);
                }
            }
        }

        /// <summary>
        /// Removes data from storage
        /// </summary>
        /// <param name="key">Key which identifies the data</param>
        /// <returns>True if data was present in the storage</returns>
        public bool Remove(Guid key)
        {
            // Get statement
            byte[] keyEncoded = key.ToByteArray();
            ColumnPath columnPath = new ColumnPath()
            {
                Column = ByteEncoderHelper.UTF8Encoder.ToByteArray("Value"),
                Column_family = columnFamily,
            };

            cluster.Execute(new ExecutionBlock(delegate(Cassandra.Client client)
            {
                client.remove(keyEncoded, columnPath, UnixHelper.UnixTimestamp, ConsistencyLevel.ONE);
                return null;
            }), keySpace);

            return true;
        }

        /// <summary>
        /// Determines if data is present in the storage
        /// </summary>
        /// <param name="key">Key which identifies the data</param>
        /// <returns>True if data is present</returns>
        public bool Contains(Guid key)
        {
            // Get statement
            byte[] keyEncoded = key.ToByteArray();

            ColumnParent columnParent = new ColumnParent();
            columnParent.Column_family = columnFamily;

            var predicate = new SlicePredicate();
            //predicate.Column_names = new List<byte[]>();
            //predicate.Column_names.Add(ByteEncoderHelper.UTF8Encoder.ToByteArray("Value"));

            bool result = (bool)cluster.Execute(new ExecutionBlock(delegate(Cassandra.Client client)
            {
                return client.get_count(keyEncoded, columnParent, predicate, ConsistencyLevel.ONE) > 0;
            }), keySpace);

            return result;
        }

        /// <summary>
        /// Returns the keys
        /// </summary>
        /// <returns>List of keys in the storage</returns>
        public System.Collections.IEnumerable ListKeys()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clears all data in the storage
        /// </summary>
        public void Clear()
        {
            throw new NotImplementedException();
        }
    }
}
