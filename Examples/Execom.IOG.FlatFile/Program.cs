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
using System.Text;
using Execom.IOG.Storage;
using System.IO;

namespace Execom.IOG.FlatFile
{
    /// <summary>
    /// Simple data model with single string value
    /// </summary>
    public interface IDataModel
    {
        string StringValue { get; set; }
    }

    /// <summary>
    /// Main class
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Open or create new file stream to use as storage
            var file = new FileStream("data.dat", FileMode.OpenOrCreate);
            
            // Initialize IOG storage in the flat file using
            var storage = new IndexedFileStorage(file, 256, true);

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

            // Dispose the IOG storage making a clean shutdown
            storage.Dispose();

            // Dipose the context
            ctx.Dispose();
        }
    }
}
