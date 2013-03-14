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
using System.IO;
using Execom.IOG.Storage;
using Execom.IOG.Attributes;

namespace Execom.IOG.ImmutableDB
{
    public interface IProduct
    {
        string Name { get; set; }
        int Price { get; set; }
    }

    public interface IOrder
    {
        DateTime Date { get; set; }
        
        [Immutable] // This means that once order is created no changes to the user are reflected!
        IUser User { get; set; }

        [Immutable] // This means that once order is created no changes to the product are reflected!
        IProduct Product { get; set; }
    }

    public interface IUser
    {
        string Username { get; set; }
        int Age { get; set; }
    }

    public interface IDataModel
    {
        ICollection<IUser> Users { get; set; }
        ICollection<IProduct> Products { get; set; }
        ICollection<IOrder> Orders { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var file = new FileStream("data.dat", FileMode.OpenOrCreate);
            var storage = new IndexedFileStorage(file, 256, true);
            Context ctx = new Context(typeof(IDataModel), null, storage);

            // Initialize data for the first time creating 100 users and 100 products
            using (var ws = ctx.OpenWorkspace<IDataModel>(IsolationLevel.Exclusive))
            {
                if (ws.Data.Users == null)
                {
                    Console.WriteLine("Creating data for the first time");

                    // Create 100 users
                    ws.Data.Users = ws.New<ICollection<IUser>>();
                    for (int i = 0; i < 100; i++)
                    {
                        IUser usr = ws.New<IUser>();
                        usr.Username = "User" + i;
                        usr.Age = i;

                        ws.Data.Users.Add(usr);
                    }

                    //Create 100 products
                    ws.Data.Products = ws.New<ICollection<IProduct>>();
                    for (int i = 0; i < 100; i++)
                    {
                        IProduct prd = ws.New<IProduct>();
                        prd.Name = "Product" + i;
                        prd.Price = i;

                        ws.Data.Products.Add(prd);
                    }

                    // Create 100 orders
                    ws.Data.Orders = ws.New<ICollection<IOrder>>();
                    for (int i = 0; i < 100; i++)
                    {
                        // Use LINQ to find user with appropriate age
                        var user = ws.Data.Users.Single(u => u.Age == i);

                        // Use LINQ to find product with appropriate price
                        var product = ws.Data.Products.Single(p => p.Price.Equals(i));

                        var order = ws.New<IOrder>();
                        order.Date = DateTime.UtcNow;
                        order.Product = product;
                        order.User = user;

                        ws.Data.Orders.Add(order);
                    }

                    ws.Commit();
                }
                else
                {
                    Console.WriteLine("Using data from file");
                }
            }

            // We set all product prices to zero to test if orders are still ok
            using (var ws = ctx.OpenWorkspace<IDataModel>(IsolationLevel.Exclusive))
            {
                foreach (var product in ws.Data.Products)
                {
                    product.Price = 0;
                }

                ws.Commit();
            }            

            // List all orders which have been made by users of age greater than 75            
            using (var ws = ctx.OpenWorkspace<IDataModel>(IsolationLevel.Exclusive))
            {
                // We list all products with prices
                Console.WriteLine("Product\tPrice");
                foreach (var item in ws.Data.Products)
                {
                    Console.WriteLine("{0}\t{1}", item.Name, item.Price);
                }

                var orders = ws.Data.Orders.Where(o => o.User.Age > 75);
                Console.WriteLine("Date\tProduct\tUser\tPrice");
                foreach (var item in orders)
                {
                    Console.WriteLine("{0}\t{1}\t{2}\t{3}", item.Date, item.Product.Name, item.User.Username, item.Product.Price);
                }
            }

            // Dispose the IOG storage making a clean shutdown
            storage.Dispose();

            // Dispose the context
            ctx.Dispose();
        }
    }
}
