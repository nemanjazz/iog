﻿// -----------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="Execom">
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

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Execom.IOG")]
[assembly: AssemblyDescription("iog.codeplex.com")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Execom")]
[assembly: AssemblyProduct("Execom.IOG")]
[assembly: AssemblyCopyright("Copyright © Execom 2011")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("a7180ec3-0008-4f59-afee-fc3b143d50c7")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("0.8.5.0")]
[assembly: AssemblyFileVersion("0.8.5.0")]


[assembly: InternalsVisibleTo("IOG.RuntimeProxy")]
[assembly: InternalsVisibleTo("Execom.IOG.Test")]
[assembly: InternalsVisibleTo("Execom.IOG.StorageBenchmark")]

[assembly: CLSCompliant(true)]
