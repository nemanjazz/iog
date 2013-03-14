// -----------------------------------------------------------------------
// <copyright file="StorageServicesSingleton.cs" company="Execom">
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

namespace Execom.IOG.Webshop.Service
{
    /// <summary>
    /// Represents storage services singleton which is shared among pages
    /// </summary>
    public static class StorageServicesSingleton
    {
        private static StorageServices instance = new StorageServices(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar);

        public static StorageServices Instance { get { return instance;  } }
    }
}
