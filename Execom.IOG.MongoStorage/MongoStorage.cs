// -----------------------------------------------------------------------
// <copyright file="MongoStorage.cs" company="Execom">
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
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Configuration;
using Execom.IOG.Storage;
using Execom.IOG.Graph;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json;

namespace Execom.IOG.MongoStorage
{
    public class MongoStorage : IKeyValueStorage<Guid, object>, ISerializingStorage
    {
        /// <summary>
        /// Database used to store the data
        /// </summary>
        private MongoDatabase database;

        /// <summary>
        /// Collection which stores the data
        /// </summary>
        private MongoCollection<BsonDocument> collection;

        /// <summary>
        /// Serializer to be injected by IOG
        /// </summary>
        public IObjectSerializer Serializer { get; set; }

        /// <summary>
        /// Creates new instance of MongoStorage type
        /// </summary>
        /// <param name="connParams">Connection parameters</param>    
        /// <param name="dbParams">Database</param>
        /// <param name="clParams">Collection</param>    
        public MongoStorage(String connParams, String dbParams, String clParams) 
        {
            MongoClient client = new MongoClient(connParams);
            MongoServer server = client.GetServer();
            database = server.GetDatabase(dbParams);
            collection = database.GetCollection(clParams);
        }
        
        /// <summary>
        /// Adds or updates value in the storage
        /// </summary>
        /// <param name="key">Key which identifies the data</param>
        /// <param name="value">Data to store</param>
        /// <returns>True if operation was success</returns>
        public bool AddOrUpdate(Guid key, object value)
        {          
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    Serializer.Serialize(value, bw);

                    byte[] data = new byte[ms.Length];

                    // Copy only the used portion of the buffer
                    Array.Copy(ms.GetBuffer(), data, ms.Length);

                    BsonDocument obj = new BsonDocument("_id", key);
                    obj.Add("value", data);

                    collection.Save(obj);

                    return true;
                }
            }
            
        }

        /// <summary>
        /// Reads data from storage
        /// </summary>
        /// <param name="key">Key which identifies the data</param>
        /// <returns>Storage data</returns>
        public object Value(Guid key)
        {            
            var query = Query.EQ("_id", key);

            foreach (var item in collection.FindAs<BsonDocument>(query))
            {
                BsonBinaryData doc = BsonSerializer.Deserialize<BsonBinaryData>(item.GetElement("value").Value.ToJson());

                byte[] array = doc.AsByteArray;

                using (MemoryStream ms = new MemoryStream(array))
                {
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        return Serializer.Deserialize(br);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Removes data from storage
        /// </summary>
        /// <param name="key">Key which identifies the data</param>
        /// <returns>True if data was present in the storage</returns>
        public bool Remove(Guid key)
        {
            var query = Query.EQ("_id", key);

            if (collection.FindOne(query) != null)
            {
                collection.Remove(query);
                return true;
            }

            return false;   
        }

        /// <summary>
        /// Determines if data is present in the storage
        /// </summary>
        /// <param name="key">Key which identifies the data</param>
        /// <returns>True if data is present</returns>
        public bool Contains(Guid key)
        {
            var query = Query.EQ("_id", key);

            if (collection.FindOne(query) != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the keys
        /// </summary>
        /// <returns>List of keys in the storage</returns>
        public System.Collections.IEnumerable ListKeys()
        {   
            List<Guid> listKeys = new List<Guid>();
            foreach (var item in collection.FindAll())
            {
                listKeys.Add((Guid)item.GetElement("_id").Value);
            }
            return listKeys;
        }

        /// <summary>
        /// Clears all data in the storage
        /// </summary>
        public void Clear()
        {
            collection.RemoveAll();
        }
    }

}
