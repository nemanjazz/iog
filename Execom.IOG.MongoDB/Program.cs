// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Execom">
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
using System.Configuration;
using Execom.IOG.MongoStorage;
using Execom.IOG.Graph;
using Execom.IOG.Storage;
using Execom.IOG.Providers;

namespace Execom.IOG.MongoDB
{   
    /// <summary>
    /// Simple data model with single string value
    /// </summary>
    public interface IDataModel
    {
        String StringValue { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Initialize IOG storage in the MongoDB storage 
            // with connection parameters
            var storage = new MongoStorage.MongoStorage("mongodb://ws014:27017/test", "test", "test5");
            storage.Clear();

            // Create an IOG context in memory which has the data model of IDataModel type
            // Use the created storage for the context data
            Context ctx = new Context(typeof(IDataModel), null, storage);
            
            // Open workspace for writing
            using (var workspace = ctx.OpenWorkspace<IDataModel>(IsolationLevel.Exclusive))
            {
                // Access the data model via the current workspace
                IDataModel data = workspace.Data;

                // Write out the current data:
                // When running for the first time, data will be empty string
                // When running for the second time, data will be loaded from file
                Console.WriteLine(data.StringValue);

                // Set the value in data model
                data.StringValue = "Hello world!\n";

                // Commit the change
                workspace.Commit();
            }

            // Open workspace for reading
            using (var workspace = ctx.OpenWorkspace<IDataModel>(IsolationLevel.ReadOnly))
            {
                // Access the data model via the current workspace
                IDataModel data = workspace.Data;

                // Write out the current data
                Console.WriteLine(data.StringValue);
            }

            Guid id = Guid.NewGuid();
            Node<Guid, object, EdgeData> node = new Node<Guid, object, EdgeData>(NodeType.Object, "NODE");

            node.Edges.Add(new EdgeData(EdgeType.Property, "EDGED"), new Edge<Guid, EdgeData>(Guid.NewGuid(), new EdgeData(EdgeType.Property, "EDGES")));
           
            storage.AddOrUpdate(id, node);

            Node<Guid, object, EdgeData> read = (Node<Guid, object, EdgeData>)storage.Value(id);

            Console.WriteLine("Node data: {0}", read.Data);

            read.Data = "UPDATED NODE";

            Console.WriteLine("\nUpdating node\n");

            storage.AddOrUpdate(id, read);

            Node<Guid, object, EdgeData> read1 = (Node<Guid, object, EdgeData>)storage.Value(id);

            Console.WriteLine("Node data: {0}", read1.Data);

            foreach(Edge<Guid, EdgeData> edge in read.Edges.Values)
            {
                Console.WriteLine("\nEdge data: {0}\n", edge.Data);
            }

            Console.WriteLine("\nKeys: ");

            foreach (var key in storage.ListKeys())
            {
                Console.WriteLine(key);
            }

            //storage.Clear();
            Console.WriteLine("\n--------------------------------------\n");

            /*
            //Generate 20 nodes
            for (int i = 0; i < 20; i++)
            {
                Guid key = Guid.NewGuid();
                String val = "Node " + (i + 1);
                Node<Guid, object, EdgeData> value = new Node<Guid, object, EdgeData>(NodeType.Object, val);
                String edgeVal = "Edge " + (i + 1);
                Edge<Guid, EdgeData> edge = new Edge<Guid, EdgeData>(Guid.NewGuid(), new EdgeData(EdgeType.Property, edgeVal));
                value.Edges.Add(new EdgeData(EdgeType.Property, "Edge"), edge);
                storage.AddOrUpdate(key, value);
            }
            */
            foreach (Guid key in storage.ListKeys()) 
            {
                Node<Guid, object, EdgeData> temp = (Node<Guid, object, EdgeData>) storage.Value(key);
                Console.WriteLine("Node: {0}; Edges: ", temp.Data);
                foreach (Edge<Guid, EdgeData> edge in temp.Edges.Values)
                {
                    Console.WriteLine("\t\tEdge: {0}", edge.Data);
                }
            }
           
            ctx.Dispose();
            
            while (Console.ReadKey().Key != ConsoleKey.Escape)
                Console.Read();
        }
    }
}
