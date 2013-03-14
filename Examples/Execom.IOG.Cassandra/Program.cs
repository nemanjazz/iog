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
using Execom.IOG.AquilesStorage;

namespace Execom.IOG.Cassandra
{
    /// <summary>
    /// Simple data model with single string value
    /// </summary>
    public interface IDataModel
    {
        string StringValue { get; set; }
    }

    /// <summary>
    /// This example demonstrates usage of AquilesStorage to connect Apache Cassandra as the underlying storage.
    /// Cassandra instance must be installed and configured to have IOGKeyspace and IOGFamily, and named TestCluster.    
    /// </summary>
    /// <author>Nenad Sabo</author>
    class Program
    {
        static void Main(string[] args)
        {            
            // Initialize IOG storage in the Cassandra storage
            // See App.config for host/port settings of Cassandra instance
            var storage = new AquilesStorage.AquilesStorage("Test Cluster", "IOGKeyspace", "IOGFamily");

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
                data.StringValue = "Hello world!";

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

            ctx.Dispose();
        }
    }
}
