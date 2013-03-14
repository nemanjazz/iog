// -----------------------------------------------------------------------
// <copyright file="StorageServices.cs" company="Execom">
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
using Execom.IOG.Storage;
using Execom.IOG.Webshop.Data;
using Execom.IOG.Types;

namespace Execom.IOG.Webshop.Service
{
    /// <summary>
    /// Storage services
    /// </summary>
    public class StorageServices : IDisposable
    {
        private Context context;
        private FileStream dataFileStream;
        private IKeyValueStorage<Guid, object> storage;
        private bool isDisposed;

        public Context Context { get { return context; } }

        /// <summary>
        /// Storage services constructor
        /// </summary>
        /// <param name="dataFilePath">Path to data file</param>
        public StorageServices(String dataFilePath) 
        {
            // Initialize in-memory context
            context = new Context(typeof(SystemData), null, ConfigureStorage(dataFilePath));
            using (var iogWorkspace = context.OpenWorkspace<SystemData>(IsolationLevel.Exclusive))
            {
                // Initialize context with dummy data
                InitilazieContextWithData(iogWorkspace);

                iogWorkspace.Commit();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        private void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                isDisposed = true;
                if (disposing)
                {
                    if (storage != null)
                    {
                        storage = null;
                    }
                    if (dataFileStream != null)
                    {
                        dataFileStream.Flush();
                        dataFileStream.Dispose();
                        dataFileStream = null;
                    }
                }
            }
        }

        #endregion

        private IKeyValueStorage<Guid, object> ConfigureStorage(String dataFilePath)
        {
            string fileName = dataFilePath + "data.dat";
            var file = new FileStream(fileName, FileMode.Create);
            storage = new IndexedFileStorage(file, 256, true);
            return storage;
        }

        private void InitilazieContextWithData(Workspace<SystemData> iogWorkspace)
        {
            iogWorkspace.Data.Products = iogWorkspace.New<IIndexedCollection<Product>>();
            iogWorkspace.Data.Orders = iogWorkspace.New<IIndexedCollection<Order>>();
            iogWorkspace.Data.Categories = iogWorkspace.New<IIndexedCollection<Category>>();

            #region Categories initial dummy data

            Category cat1 = iogWorkspace.New<Category>();
            cat1.ID = Guid.NewGuid();
            cat1.Name = "Keyboard";

            iogWorkspace.Data.Categories.Add(cat1);

            Category cat2 = iogWorkspace.New<Category>();
            cat2.ID = Guid.NewGuid();            
            cat2.Name = "Mouse";

            iogWorkspace.Data.Categories.Add(cat2);

            Category cat3 = iogWorkspace.New<Category>();
            cat3.ID = Guid.NewGuid();
            cat3.Name = "HDD";

            iogWorkspace.Data.Categories.Add(cat3);

            Category cat4 = iogWorkspace.New<Category>();
            cat4.ID = Guid.NewGuid();
            cat4.Name = "Motherboard";

            iogWorkspace.Data.Categories.Add(cat4);

            Category cat5 = iogWorkspace.New<Category>();
            cat5.ID = Guid.NewGuid();
            cat5.Name = "Graphic card";

            iogWorkspace.Data.Categories.Add(cat5);

            #endregion

            #region Products initial dummy data
          
            Product p1 = iogWorkspace.New<Product>();
            p1.ID = Guid.NewGuid();
            p1.Name = "PS/2 US Genius Slimstar 110";
            p1.Photo = "Images/Products/Slimstar_110.jpg";
            p1.Price = 6;
            p1.Category = cat1;

            iogWorkspace.Data.Products.Add(p1);

            Product p2 = iogWorkspace.New<Product>();
            p2.ID = Guid.NewGuid();
            p2.Name = "PS/2 Genius NetScroll 120";
            p2.Photo = "Images/Products/Genius_NetScroll_120.jpg";
            p2.Price = 4.5;
            p2.Category = cat2;

            iogWorkspace.Data.Products.Add(p2);

            Product p3 = iogWorkspace.New<Product>();
            p3.ID = Guid.NewGuid();
            p3.Name = "Seagate Barracuda Green ST100";
            p3.Photo = "Images/Products/Seagate_Barracuda_Green.jpg";
            p3.Price = 100;
            p3.Category = cat3;

            iogWorkspace.Data.Products.Add(p3);

            Product p4 = iogWorkspace.New<Product>();
            p4.ID = Guid.NewGuid();
            p4.Name = "MSI 970A-G46";
            p4.Photo = "Images/Products/MSI_970A-G46.jpg";
            p4.Price = 82;
            p4.Category = cat4;

            iogWorkspace.Data.Products.Add(p4);

            Product p5 = iogWorkspace.New<Product>();
            p5.ID = Guid.NewGuid();
            p5.Name = "GeForce GTX550-Ti EVGA";
            p5.Photo = "Images/Products/GeForce_GTX550-Ti.jpg";
            p5.Price = 122;
            p5.Category = cat5;

            iogWorkspace.Data.Products.Add(p5);

            #endregion            
        }
    }
}
