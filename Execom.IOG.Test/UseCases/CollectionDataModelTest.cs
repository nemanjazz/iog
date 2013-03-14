﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Attributes;

namespace Execom.IOG.Test.UseCases
{
    [TestClass]
    public class CollectionDataModelTest
    {
        private Context ctx = null;

        public interface IProduct
        {
            string Name { get; set; }
            int Price { get; set; }
        }

        public interface ICity
        {
            string Name { get; set; }
        }

        public interface IOrder
        {
            DateTime Date { get; set; }

            [Immutable]
            IUser User { get; set; }

            [Immutable]
            IProduct Product { get; set; }
        }

        public interface IUser
        {
            string Username { get; set; }
            int Age { get; set; }
            ICity City { get; set; }
        }

        public interface IState 
        {
            ICollection<ICity> Cities { get; set; }
            String Name { get; set; }
        }

        public interface IDatabase
        {
            ICollection<IUser> Users { get; set; }
            ICollection<IProduct> Products { get; set; }
            ICollection<IOrder> Orders { get; set; }
            [Immutable]
            ICollection<ICity> Cities { get; set; }
            ICollection<IState> States { get; set; }
        }

        [TestInitialize]
        public void SetUp()
        {
            ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                ICollection<ICity> tempCities = ws.New<ICollection<ICity>>();
                
                database.Users = ws.New<ICollection<IUser>>();
                IUser user;
                ICity city;
                for (int i = 0; i < 100; i++)
                {
                    user = ws.New<IUser>();
                    user.Username = "User" + i;
                    user.Age = i;

                    city = ws.New<ICity>();
                    city.Name = "City" + i;
                    tempCities.Add(city);
                    user.City = city;

                    database.Users.Add(user);
                }

                database.States = ws.New<ICollection<IState>>();
                IState state;
                for (int i = 0; i < 2; i++)
                {
                    state = ws.New<IState>();
                    state.Name = "State" + i;
                    state.Cities = tempCities;
                    database.States.Add(state);
                }

                database.Cities = tempCities;

                //Create 100 products
                database.Products = ws.New<ICollection<IProduct>>();
                IProduct product;
                for (int i = 0; i < 100; i++)
                {
                    product = ws.New<IProduct>();
                    product.Name = "Product" + i;
                    product.Price = i;

                    database.Products.Add(product);
                }

                // Create 100 orders
                database.Orders = ws.New<ICollection<IOrder>>();
                for (int i = 0; i < 100; i++)
                {
                    // Use LINQ to find user with appropriate age
                    user = database.Users.Single(u => u.Age == i);

                    // Use LINQ to find product with appropriate price
                    product = database.Products.Single(p => p.Price.Equals(i));

                    var order = ws.New<IOrder>();
                    order.Date = DateTime.UtcNow;
                    order.Product = product;
                    order.User = user;

                    database.Orders.Add(order);
                }

                ws.Commit();
            }
        }

        [TestMethod]
        public void TestReadData()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                IUser user;
                IOrder order;
                IProduct product;

                for (int i = 0; i < 100; i++)
                {
                    user = database.Users.Single(u => u.Username.Equals("User" + i));
                    Assert.IsNotNull(user);
                    Assert.AreEqual("User" + i, user.Username);
                    Assert.AreEqual("City" + i, user.City.Name);

                    order = database.Orders.Single(o => o.User.Username.Equals("User" + i));
                    Assert.IsNotNull(order);
                    user = database.Users.Single(u => u.Username == order.User.Username);
                    Assert.IsNotNull(user);
                    Assert.ReferenceEquals(order.User, user);

                    product = database.Products.Single(p => p.Price == order.Product.Price);
                    Assert.AreEqual(order.Product.Price, product.Price);
                    Assert.ReferenceEquals(order.Product, product);
                }

                ICity city = database.Cities.Single(c => c.Name.Equals("City30"));
                Assert.IsNotNull(city);
                IState state = database.States.Single(s => s.Name.Equals("State1"));
                Assert.IsNotNull(state);

                ICity tmpCity = state.Cities.Single(tc => tc.Name.Equals(city.Name));
                Assert.IsNotNull(tmpCity);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                IUser user;
                IOrder order;
                IProduct product;
                for (int i = 0; i < 100; i++)
                {
                    user = database.Users.Single(u => u.Username.Equals("User" + i));
                    Assert.IsNotNull(user);
                    Assert.AreEqual("User" + i, user.Username);
                    Assert.AreEqual("City" + i, user.City.Name);

                    order = database.Orders.Single(o => o.User.Username.Equals("User" + i));
                    Assert.IsNotNull(order);
                    user = database.Users.Single(u => u.Username == order.User.Username);
                    Assert.IsNotNull(user);
                    Assert.ReferenceEquals(order.User, user);

                    product = database.Products.Single(p => p.Price == order.Product.Price);
                    Assert.AreEqual(order.Product.Price, product.Price);
                    Assert.ReferenceEquals(order.Product, product);
                }

                ICity city = database.Cities.Single(c => c.Name.Equals("City30"));
                Assert.IsNotNull(city);
                IState state = database.States.Single(s => s.Name.Equals("State1"));
                Assert.IsNotNull(state);

                ICity tmpCity = state.Cities.Single(tc => tc.Name.Equals(city.Name));
                Assert.IsNotNull(tmpCity);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.InvalidOperationException))]
        public void TestModificationData()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                IUser user;
                IOrder order;
                ICity city;
                for (int i = 0; i < 100; i++)
                {
                    order = database.Orders.Single(o => o.User.Username.Equals("User" + i));
                    Assert.IsNotNull(order);
                    user = database.Users.Single(u => u.Username.Equals(order.User.Username));
                    Assert.IsNotNull(user);
                    Assert.ReferenceEquals(order.User, user);
                    city = database.Cities.Single(c => c.Name.Equals("City" + i));
                    Assert.IsNotNull(city);
                    Assert.AreEqual(user.City.Name, city.Name);
                }

                foreach (var product in database.Products)
                {
                    product.Price = -1;
                }

                var userCity1 = database.Users.Single(u => u.City.Name.Equals("City1"));
                Assert.IsNotNull(userCity1);
                Assert.AreEqual("City1", userCity1.City.Name);

                IState state = database.States.Single(s => s.Name.Equals("State1"));
                Assert.IsNotNull(state);

                Assert.ReferenceEquals(userCity1, state.Cities.Select(c => c.Name.Equals("City1")));

                userCity1.City.Name = "";

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                foreach (var product in database.Products)
                {
                    Assert.AreEqual(-1, product.Price);
                }

                foreach (var order in database.Orders)
                {
                    Assert.AreNotEqual(-1, order.Product.Price);
                }

                var user1 = database.Users.Single(u => u.City.Name.Equals("City1"));
                Assert.IsNull(user1);

                user1 = database.Users.Single(u => u.City.Name.Equals(""));
                Assert.IsNotNull(user1);

                var city = database.Cities.Single(c => c.Name.Equals("City1"));
                Assert.IsNotNull(city);

                IState state = database.States.Single(s => s.Name.Equals("State1"));
                Assert.IsNotNull(state);

                Assert.IsNull(state.Cities.Select(c => c.Name.Equals("")));
                Assert.IsNotNull(state.Cities.Select(c => c.Name.Equals("City1")));

            }
        }

        [TestMethod]
        public void TestModificationOfImmutable()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                IState state = database.States.Single(s => s.Name.Equals("State1"));
                Assert.IsNotNull(state);

                ICity city;
                ICity stateCity;
                for (int i = 0; i < 100; i++)
                {
                    city = database.Cities.Single(c => c.Name.Equals("City" + i));
                    Assert.IsNotNull(city);
                    stateCity = state.Cities.Single(sc => sc.Name.Equals("City" + i));
                    Assert.IsNotNull(stateCity);
                    Assert.ReferenceEquals(city, stateCity);
                }

                database.Cities = ws.New<ICollection<ICity>>();

                IUser user;
                for (int i = 0; i < 100; i++)
                {
                    user = database.Users.Single(u => u.Username.Equals("User" + i));
                    Assert.IsNotNull(user);
                    Assert.AreEqual("City" + i, user.City.Name);
                    stateCity = state.Cities.Single(sc => sc.Name.Equals("City" + i));
                    Assert.IsNotNull(stateCity);
                }
            }
        }

        [TestMethod]
        public void TestModifAndRollback()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                IProduct product;
                for (int i = 0; i < 100; i++)
                {
                    product = database.Products.Single(p => p.Price == i);
                    Assert.IsNotNull(product);
                }

                foreach (var prod in database.Products)
                {
                    prod.Price = -1;
                }

                foreach (var prod in database.Products)
                {
                    Assert.AreEqual(-1, prod.Price);
                }

                ICity city;
                for (int i = 0; i < 100; i++)
                {
                    city = database.Cities.Single(c => c.Name.Equals("City" + i));
                    Assert.IsNotNull(city);
                }

                IState state = database.States.Single(s => s.Name.Equals("State0"));
                Assert.IsNotNull(state);

                for (int i = 0; i < 100; i++)
                {
                    city = state.Cities.Single(c => c.Name.Equals("City" + i));
                    Assert.IsNotNull(city);
                }

                state.Cities = ws.New<ICollection<ICity>>();
                database.Cities = ws.New<ICollection<ICity>>();

                foreach (var cityIter in database.Cities)
                {
                    Assert.AreEqual("", cityIter.Name);
                }

                foreach (var stateCity in state.Cities)
                {
                    Assert.AreEqual("", stateCity.Name);
                }

                ws.Rollback();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                IProduct product;
                for (int i = 0; i < 100; i++)
                {
                    product = database.Products.Single(p => p.Price == i);
                    Assert.IsNotNull(product);
                }

                ICity city;
                for (int i = 0; i < 100; i++)
                {
                    city = database.Cities.Single(c => c.Name.Equals("City" + i));
                    Assert.IsNotNull(city);
                }

                IState state = database.States.Single(s => s.Name.Equals("State0"));
                Assert.IsNotNull(state);

                for (int i = 0; i < 100; i++)
                {
                    city = state.Cities.Single(c => c.Name.Equals("City" + i));
                    Assert.IsNotNull(city);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.InvalidOperationException))]
        public void TestCollectionContains()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                IUser user;
                for (int i = 0; i < 100; i++)
                {
                    user = database.Users.Single(u => u.Username.Equals("User" + i));
                    Assert.IsNotNull(user);
                    Assert.IsTrue(database.Cities.Contains(user.City));
                }

                user = ws.New<IUser>();
                user.City = database.Cities.Single(c => c.Name.Equals("City1"));
                user.Username = "User1";
                user.Age = 1;
                Assert.IsFalse(database.Users.Contains(user));

                IOrder order = database.Orders.Single(o => o.Product.Price.Equals(5));
                Assert.IsNotNull(order);
                user = order.User;
                Assert.IsNotNull(user);
                Assert.IsTrue(database.Users.Contains(user));

                user = ws.New<IUser>();
                user.City = database.Cities.Single(c => c.Name.Equals("City1"));
                user.Username = "User101";
                user.Age = 101;

                database.Users.Add(user);

                IState state = database.States.Single(s => s.Name.Equals("State0"));
                Assert.IsNotNull(state);

                Assert.IsNull(state.Cities.Single(sc => sc.Name.Equals("City666")));

                ICity city666 = ws.New<ICity>();
                city666.Name = "City666";

                database.Cities.Add(city666);
                state.Cities.Add(city666);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                IUser user = database.Users.Single(u => u.Username.Equals("User101"));
                Assert.IsNotNull(user);
                Assert.IsTrue(database.Users.Contains(user));

                IState state = database.States.Single(s => s.Name.Equals("State0"));
                Assert.IsNotNull(state);
                Assert.IsNotNull(state.Cities.Single(sc => sc.Name.Equals("City666")));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.InvalidOperationException))]
        public void TestCollectionClear()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                ICity city;
                for (int i = 0; i < 100; i++)
                {
                    city = database.Cities.Single(c => c.Name.Equals("City" + i));
                    Assert.IsNotNull(city);
                }

                ICollection<ICity> tmpCities = ws.New<ICollection<ICity>>();
                database.Cities = tmpCities;
                
                Assert.AreEqual(0, database.Cities.Count);

                IUser user = database.Users.Single(u => u.Username.Equals("User1"));
                Assert.AreEqual("City1", user.City.Name);

                IState state = database.States.Single(s => s.Name.Equals("State0"));
                Assert.IsNotNull(state);
                Assert.IsNotNull(state.Cities.Single(sc => sc.Name.Equals("City30")));

                state.Cities.Clear();

                Assert.AreEqual(0, state.Cities.Count);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                Assert.AreNotEqual(100, database.Cities.Count);
                Assert.AreEqual(0, database.Cities.Count);

                ICity city = database.Cities.Single(c => c.Name.Equals("City10"));
                Assert.IsNull(city);

                IUser user = database.Users.Single(u => u.Username.Equals("User1"));
                Assert.AreEqual("City1", user.City.Name);

                IState state = database.States.Single(s => s.Name.Equals("State0"));
                Assert.IsNotNull(state);
                Assert.IsNull(state.Cities.Single(sc => sc.Name.Equals("City30")));
                Assert.AreEqual(0, state.Cities.Count);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.InvalidOperationException))]
        public void TestCollectionRemove()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                ICity city;
                for (int i = 0; i < 100; i++)
                {
                    city = database.Cities.Single(c => c.Name.Equals("City" + i));
                    Assert.IsNotNull(city);
                }

                city = database.Cities.Single(c => c.Name.Equals("City10"));
                Assert.IsNotNull(city);
                Assert.IsTrue(database.Cities.Remove(city));

                Assert.AreEqual(99, database.Cities.Count);

                // Collection Cities is immutable, so user have reference to City10 in 
                // previous version, although collection is cleared
                IUser user = database.Users.Single(u => u.Username.Equals("User10"));
                Assert.AreEqual("City10", user.City.Name);

                IState state = database.States.Single(s => s.Name.Equals("State0"));
                Assert.IsNotNull(state);

                city = state.Cities.Single(sc => sc.Name.Equals("City50"));
                Assert.IsNotNull(city);
                Assert.IsTrue(database.Cities.Remove(city));

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                Assert.AreEqual(99, database.Cities.Count);

                ICity city = database.Cities.Single(c => c.Name.Equals("City10"));
                Assert.IsNull(city);

                IUser user = database.Users.Single(u => u.Username.Equals("User10"));
                Assert.AreEqual("City10", user.City.Name);

                IState state = database.States.Single(s => s.Name.Equals("State0"));
                Assert.IsNotNull(state);

                city = state.Cities.Single(sc => sc.Name.Equals("City50"));
                Assert.IsNull(city);
                Assert.IsTrue(database.Cities.Remove(city));
            }
        }
    }
}
