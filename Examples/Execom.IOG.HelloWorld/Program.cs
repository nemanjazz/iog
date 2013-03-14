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

namespace Execom.IOG.HelloWorld
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
            // Create an IOG context in memory which has the data model of IDataModel type
            Context ctx = new Context(typeof(IDataModel));

            // Open workspace for writing
            using (var workspace = ctx.OpenWorkspace<IDataModel>(IsolationLevel.Exclusive))
            {
                // Access the data model via the current workspace
                IDataModel data = workspace.Data;
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
                // Display the message
                Console.Write(data.StringValue);
            }

            // Dispose the context
            ctx.Dispose();
        }
    }
}
