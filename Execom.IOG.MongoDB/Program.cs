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
using Execom.IOG.Attributes;

namespace Execom.IOG.MongoDB
{
    public interface ISupplier
    {
        string Name { get; set; }
    }

    public interface IProduct
    {
        string Name { get; set; }
        int Price { get; set; }
        ISupplier Supplier { get; set; }
    }

    public interface IOrder
    {
        DateTime Date { get; set; }

        [Immutable] // This means that once an order is created, no changes to the user are reflected!
        IUser User { get; set; }

        [Immutable] // This means that once an order is created, no changes to the product are reflected!
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
            // Initialize IOG storage in the MongoDB storage 
            // with connection parameters
            var storage = new MongoStorage.MongoStorage("mongodb://ws014:27017/test", "test", "test");
            storage.Clear();

            // Create an IOG context in memory which has the data model of IDataModel type
            // Use the created storage for the context data
            Context ctx = new Context(typeof(IDataModel), null, storage);

            // Initialize data for the first time creating 100 users and 100 products
            using (var ws = ctx.OpenWorkspace<IDataModel>(IsolationLevel.Exclusive))
            {
                Console.WriteLine("Generating data");

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
                    prd.Price = i * 10;
                    ISupplier sup = ws.New<ISupplier>();
                    sup.Name = "Supplier" + i;
                    prd.Supplier = sup;

                    ws.Data.Products.Add(prd);
                }

                // Create 100 orders
                ws.Data.Orders = ws.New<ICollection<IOrder>>();
                for (int i = 0; i < 100; i++)
                {
                    // Use LINQ to find user with appropriate age
                    var user = ws.Data.Users.Single(u => u.Age == i);

                    // Use LINQ to find product with appropriate price
                    var product = ws.Data.Products.Single(p => p.Price.Equals(i * 10));

                    var order = ws.New<IOrder>();
                    order.Date = DateTime.UtcNow;
                    order.Product = product;
                    order.User = user;

                    ws.Data.Orders.Add(order);
                }
                ws.Commit();
            }

            // We set all product prices to zero to test if the orders are still ok
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
                Console.WriteLine("\nProduct\t\tPrice\tSupplier\n");
                foreach (var item in ws.Data.Products)
                {
                    Console.WriteLine("{0}\t{1}\t{2}", item.Name, item.Price, item.Supplier.Name);
                }

                var orders = ws.Data.Orders.Where(o => o.User.Age > 75);
                Console.WriteLine("\nOrders: ");
                Console.WriteLine("Date\t\t\tProduct\t\tUser\tPrice\n");
                foreach (var item in orders)
                {
                    Console.WriteLine("{0}\t{1}\t{2}\t{3}", item.Date, item.Product.Name, item.User.Username, item.Product.Price);
                }
            }
            ctx.Dispose();
            
            while (Console.ReadKey().Key != ConsoleKey.Escape)
                Console.Read();
        }
    }
}















































/*
Guid id = Guid.NewGuid();
Node<Guid, object, EdgeData> node = new Node<Guid, object, EdgeData>(NodeType.Object, "NODE");

node.Edges.Add(new EdgeData(EdgeType.Property, "EDGED"), 
    new Edge<Guid, EdgeData>(Guid.NewGuid(), 
        new EdgeData(EdgeType.Property, "Non-connected edge")));
           
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

            
//Generate 20 nodes
for (int i = 0; i < 20; i++)
{
    Guid key = Guid.NewGuid();
    String val = "Node " + (i + 1);
    Node<Guid, object, EdgeData> value =
        new Node<Guid, object, EdgeData>(NodeType.Object, val);
    Edge<Guid, EdgeData> edge = 
        new Edge<Guid, EdgeData>(Guid.NewGuid(), new EdgeData(EdgeType.Contains, key));
    value.Edges.Add(new EdgeData(EdgeType.Property, "Edge"), edge);
    //Add nodes to storage
    storage.AddOrUpdate(key, value);
}
            
//Print data from the storage
foreach (Guid key in storage.ListKeys()) 
{
    Node<Guid, object, EdgeData> temp = (Node<Guid, object, EdgeData>) storage.Value(key);
    Console.WriteLine("Node: {0}; Edges: ", temp.Data);
    foreach (Edge<Guid, EdgeData> edge in temp.Edges.Values)
    {
        Console.WriteLine("\t\tEdge: {0}", edge.Data);
    }
}
*/