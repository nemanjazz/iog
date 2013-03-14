using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Attributes;

namespace Execom.IOG.Test.UseCases
{
    [TestClass]
    public class DictionaryDataModelTest
    {
        private Context ctx = null;

        public interface IProduct
        {
            int ProductID { get; set; }
            string Name { get; set; }
            int Price { get; set; }
        }

        public interface ICity
        {
            string Name { get; set; }
        }

        public interface IOrder
        {
            int OrderID { get; set; }

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
            string Name { get; set; }
            IDictionary<String, ICity> Cities { get; set; }
        }

        public interface IDatabase
        {
            IDictionary<String, IUser> Users { get; set; }
            IDictionary<Int32, IProduct> Products { get; set; }
            IDictionary<Int32, IOrder> Orders { get; set; }
            [Immutable]
            IDictionary<String, ICity> Cities { get; set; }
            IDictionary<String, IState> States { get; set; }
        }

        [TestInitialize]
        public void SetUp()
        {
            ctx = new Context(typeof(IDatabase));

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                IDictionary<String, ICity> tempCities = ws.New<IDictionary<String, ICity>>();
                // Create 100 users
                database.Users = ws.New<IDictionary<String, IUser>>();
                IUser user;
                ICity city;
                for (int i = 0; i < 100; i++)
                {
                    user = ws.New<IUser>();
                    user.Username = "User" + (i < 10? "00" + i : "0" + i);
                    user.Age = i;

                    city = ws.New<ICity>();
                    city.Name = "City" + (i < 10 ? "00" + i : "0" + i);
                    tempCities.Add(city.Name, city);
                    user.City = city;

                    database.Users.Add(user.Username, user);
                }

                database.States = ws.New<IDictionary<String, IState>>();
                IState state;
                for (int i = 0; i < 2; i++)
                {
                    state = ws.New<IState>();
                    state.Name = "State" + (i < 10 ? "00" + i : "0" + i);
                    state.Cities = tempCities;
                    database.States.Add(state.Name, state);
                }

                database.Cities = tempCities;

                //Create 100 products
                database.Products = ws.New<IDictionary<Int32, IProduct>>();
                IProduct product;
                for (int i = 0; i < 100; i++)
                {
                    product = ws.New<IProduct>();
                    product.Name = "Product" + (i < 10 ? "00" + i : "0" + i);
                    product.ProductID = i;
                    product.Price = i;

                    database.Products.Add(product.ProductID, product);
                }

                // Create 100 orders
                database.Orders = ws.New<IDictionary<Int32, IOrder>>();
                for (int i = 0; i < 100; i++)
                {
                    // Use LINQ to find user with appropriate age
                    user = database.Users.Single(u => u.Value.Age == i).Value;

                    // Use LINQ to find product with appropriate price
                    product = database.Products.Single(p => p.Value.Price.Equals(i)).Value;

                    var order = ws.New<IOrder>();
                    order.OrderID = i;
                    order.Date = DateTime.UtcNow;
                    order.Product = product;
                    order.User = user;

                    database.Orders.Add(order.OrderID, order);
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
                    String userKey = (i < 10 ? "00" + i : "0" + i);
                    user = database.Users.Single(u => u.Key.Equals("User" + userKey)).Value;
                    Assert.IsNotNull(user);
                    Assert.AreEqual("User" + userKey, user.Username);
                    Assert.AreEqual("City" + userKey, user.City.Name);

                    order = database.Orders.Single(o => o.Value.User.Username.Equals("User" + userKey)).Value;
                    Assert.IsNotNull(order);
                    user = database.Users.Single(u => u.Key.Equals(order.User.Username)).Value;
                    Assert.IsNotNull(user);
                    Assert.ReferenceEquals(order.User, user);

                    product = database.Products.Single(p => p.Value.Price == order.Product.Price).Value;
                    Assert.AreEqual(order.Product.Price, product.Price);
                    Assert.ReferenceEquals(order.Product, product);
                }

                ICity city = database.Cities.Single(c => c.Key.Equals("City030")).Value;
                Assert.IsNotNull(city);
                IState state = database.States.Single(s => s.Key.Equals("State001")).Value;
                Assert.IsNotNull(state);

                ICity tmpCity = state.Cities.Single(tc => tc.Key.Equals(city.Name)).Value;
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
                    String userKey = (i < 10 ? "00" + i : "0" + i);
                    user = database.Users.Single(u => u.Key.Equals("User" + userKey)).Value;
                    Assert.IsNotNull(user);
                    Assert.AreEqual("User" + userKey, user.Username);
                    Assert.AreEqual("City" + userKey, user.City.Name);

                    order = database.Orders.Single(o => o.Value.User.Username.Equals("User" + userKey)).Value;
                    Assert.IsNotNull(order);
                    user = database.Users.Single(u => u.Key.Equals(order.User.Username)).Value;
                    Assert.IsNotNull(user);
                    Assert.ReferenceEquals(order.User, user);

                    product = database.Products.Single(p => p.Value.Price == order.Product.Price).Value;
                    Assert.AreEqual(order.Product.Price, product.Price);
                    Assert.ReferenceEquals(order.Product, product);
                }

                ICity city = database.Cities.Single(c => c.Key.Equals("City030")).Value;
                Assert.IsNotNull(city);
                IState state = database.States.Single(s => s.Key.Equals("State001")).Value;
                Assert.IsNotNull(state);

                ICity tmpCity = state.Cities.Single(tc => tc.Key.Equals(city.Name)).Value;
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
                    String userKey = (i < 10 ? "00" + i : "0" + i);
                    order = database.Orders.Single(o => o.Value.User.Username.Equals("User" + userKey)).Value;
                    Assert.IsNotNull(order);
                    user = database.Users.Single(u => u.Key.Equals(order.User.Username)).Value;
                    Assert.IsNotNull(user);
                    Assert.ReferenceEquals(order.User, user);
                    city = database.Cities.Single(c => c.Key.Equals("City" + userKey)).Value;
                    Assert.IsNotNull(city);
                    Assert.AreEqual(user.City.Name, city.Name);
                }

                foreach (var product in database.Products)
                {
                    product.Value.Price = -1;
                }

                var userCity1 = database.Users.Single(u => u.Value.City.Name.Equals("City001")).Value;
                Assert.IsNotNull(userCity1);
                Assert.AreEqual("City001", userCity1.City.Name);

                IState state = database.States.Single(s => s.Key.Equals("State001")).Value;
                Assert.IsNotNull(state);
                ICity city2 = state.Cities.Single(c => c.Key.Equals("City001")).Value;
                Assert.ReferenceEquals(userCity1, city2);

                userCity1.City.Name = "";

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                foreach (var product in database.Products)
                {
                    Assert.AreEqual(-1, product.Value.Price);
                }

                foreach (var order in database.Orders)
                {
                    Assert.AreNotEqual(-1, order.Value.Product.Price);
                }

                var user1 = database.Users.Single(u => u.Value.City.Name.Equals("City001")).Value;
                Assert.IsNull(user1);

                user1 = database.Users.Single(u => u.Value.City.Name.Equals("")).Value;
                Assert.IsNotNull(user1);

                var city = database.Cities.Single(c => c.Value.Name.Equals("City001")).Value;
                Assert.IsNotNull(city);

                IState state = database.States.Single(s => s.Value.Name.Equals("State001")).Value;
                Assert.IsNotNull(state);

                Assert.IsNull(state.Cities.Select(c => c.Value.Name.Equals("")));
                Assert.IsNotNull(state.Cities.Select(c => c.Value.Name.Equals("City001")));
            }
        }

        [TestMethod]
        public void TestModificationOfImmutable()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;

                IState state = database.States.Single(s => s.Key.Equals("State001")).Value;
                Assert.IsNotNull(state);

                ICity city;
                ICity stateCity;
                for (int i = 0; i < 100; i++)
                {
                    String userKey = (i < 10 ? "00" + i : "0" + i);
                    city = database.Cities.Single(c => c.Key.Equals("City" + userKey)).Value;
                    Assert.IsNotNull(city);
                    stateCity = state.Cities.Single(sc => sc.Value.Name.Equals("City" + userKey)).Value;
                    Assert.IsNotNull(stateCity);
                    Assert.ReferenceEquals(city, stateCity);
                }

                database.Cities = ws.New<IDictionary<String, ICity>>();

                IUser user;
                for (int i = 0; i < 100; i++)
                {
                    String userKey = (i < 10 ? "00" + i : "0" + i);
                    user = database.Users.Single(u => u.Key.Equals("User" + userKey)).Value;
                    Assert.IsNotNull(user);
                    Assert.AreEqual("City" + userKey, user.City.Name);
                    stateCity = state.Cities.Single(sc => sc.Key.Equals("City" + userKey)).Value;
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
                    product = database.Products.Single(p => p.Value.Price == i).Value;
                    Assert.IsNotNull(product);
                }

                foreach (var prod in database.Products)
                {
                    prod.Value.Price = -1;
                }

                foreach (var prod in database.Products)
                {
                    Assert.AreEqual(-1, prod.Value.Price);
                }

                ICity city;
                for (int i = 0; i < 100; i++)
                {
                    String userKey = (i < 10 ? "00" + i : "0" + i);
                    city = database.Cities.Single(c => c.Key.Equals("City" + userKey)).Value;
                    Assert.IsNotNull(city);
                }

                IState state = database.States.Single(s => s.Key.Equals("State000")).Value;
                Assert.IsNotNull(state);

                for (int i = 0; i < 100; i++)
                {
                    String userKey = (i < 10 ? "00" + i : "0" + i);
                    city = state.Cities.Single(c => c.Key.Equals("City" + userKey)).Value;
                    Assert.IsNotNull(city);
                }

                state.Cities = ws.New<IDictionary<String, ICity>>();
                database.Cities = ws.New<IDictionary<String, ICity>>();

                foreach (var cityIter in database.Cities)
                {
                    Assert.AreEqual("", cityIter.Key);
                }

                foreach (var stateCity in state.Cities)
                {
                    Assert.AreEqual("", stateCity.Value.Name);
                }

                ws.Rollback();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                IProduct product;
                for (int i = 0; i < 100; i++)
                {
                    product = database.Products.Single(p => p.Value.Price == i).Value;
                    Assert.IsNotNull(product);
                }

                ICity city;
                for (int i = 0; i < 100; i++)
                {
                    String userKey = (i < 10 ? "00" + i : "0" + i);
                    city = database.Cities.Single(c => c.Key.Equals("City" + userKey)).Value;
                    Assert.IsNotNull(city);
                }

                IState state = database.States.Single(s => s.Key.Equals("State000")).Value;
                Assert.IsNotNull(state);

                for (int i = 0; i < 100; i++)
                {
                    String userKey = (i < 10 ? "00" + i : "0" + i);
                    city = state.Cities.Single(c => c.Key.Equals("City" + userKey)).Value;
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
                    String userKey = (i < 10 ? "00" + i : "0" + i);
                    user = database.Users.Single(u => u.Key.Equals("User" + userKey)).Value;
                    Assert.IsNotNull(user);
                    Assert.IsTrue(database.Cities.Contains(new KeyValuePair<String, ICity>(user.City.Name, user.City)));
                }

                user = ws.New<IUser>();
                user.City = database.Cities.Single(c => c.Key.Equals("City002")).Value;
                user.Username = "User001";
                user.Age = 1;
                Assert.IsTrue(database.Users.ContainsKey(user.Username));

                IOrder order = database.Orders.Single(o => o.Value.Product.Price.Equals(5)).Value;
                Assert.IsNotNull(order);
                user = order.User;
                Assert.IsNotNull(user);
                Assert.IsTrue(database.Users.ContainsKey(user.Username));

                user = ws.New<IUser>();
                user.City = database.Cities.Single(c => c.Key.Equals("City001")).Value;
                user.Username = "User101";
                user.Age = 101;

                database.Users.Add(user.Username, user);

                IState state = database.States.Single(s => s.Key.Equals("State000")).Value;
                Assert.IsNotNull(state);

                Assert.IsNull(state.Cities.Single(sc => sc.Key.Equals("City666")));

                ICity city666 = ws.New<ICity>();
                city666.Name = "City666";

                database.Cities.Add(city666.Name, city666);
                state.Cities.Add(city666.Name, city666);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                IUser user = database.Users.Single(u => u.Key.Equals("User101")).Value;
                Assert.IsNotNull(user);
                Assert.IsTrue(database.Users.Contains(new KeyValuePair<String, IUser>(user.Username, user)));

                IState state = database.States.Single(s => s.Key.Equals("State000")).Value;
                Assert.IsNotNull(state);
                Assert.IsNotNull(state.Cities.Single(sc => sc.Value.Name.Equals("City666")));
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
                    String userKey = (i < 10 ? "00" + i : "0" + i);
                    city = database.Cities.Single(c => c.Key.Equals("City" + userKey)).Value;
                    Assert.IsNotNull(city);
                }

                IDictionary<String, ICity> tmpCities = ws.New<IDictionary<String, ICity>>();
                database.Cities = tmpCities;

                Assert.AreEqual(0, database.Cities.Count);

                IUser user = database.Users.Single(u => u.Key.Equals("User001")).Value;
                Assert.AreEqual("City001", user.City.Name);

                IState state = database.States.Single(s => s.Key.Equals("State000")).Value;
                Assert.IsNotNull(state);
                Assert.IsNotNull(state.Cities.Single(sc => sc.Value.Name.Equals("City030")));

                state.Cities.Clear();

                Assert.AreEqual(0, state.Cities.Count);

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                Assert.AreNotEqual(100, database.Cities.Count);
                Assert.AreEqual(0, database.Cities.Count);

                ICity city = database.Cities.Single(c => c.Key.Equals("City010")).Value;
                Assert.IsNull(city);

                IUser user = database.Users.Single(u => u.Key.Equals("User001")).Value;
                Assert.AreEqual("City1", user.City.Name);

                IState state = database.States.Single(s => s.Key.Equals("State000")).Value;
                Assert.IsNotNull(state);
                Assert.IsNull(state.Cities.Single(sc => sc.Value.Name.Equals("City030")));
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
                    String userKey = (i < 10 ? "00" + i : "0" + i);
                    city = database.Cities.Single(c => c.Key.Equals("City" + userKey)).Value;
                    Assert.IsNotNull(city);
                }

                city = database.Cities.Single(c => c.Key.Equals("City010")).Value;
                Assert.IsNotNull(city);
                Assert.IsTrue(database.Cities.Remove(city.Name));

                Assert.AreEqual(99, database.Cities.Count);

                // Collection Cities is immutable, so user have reference to City10 in 
                // previous version, although collection is cleared
                IUser user = database.Users.Single(u => u.Key.Equals("User010")).Value;
                Assert.AreEqual("City010", user.City.Name);

                IState state = database.States.Single(s => s.Key.Equals("State000")).Value;
                Assert.IsNotNull(state);

                city = state.Cities.Single(sc => sc.Value.Name.Equals("City050")).Value;
                Assert.IsNotNull(city);
                Assert.IsTrue(database.Cities.Remove(city.Name));

                ws.Commit();
            }

            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                Assert.AreEqual(99, database.Cities.Count);

                ICity city = database.Cities.Single(c => c.Key.Equals("City010")).Value;
                Assert.IsNull(city);

                IUser user = database.Users.Single(u => u.Key.Equals("User010")).Value;
                Assert.AreEqual("City010", user.City.Name);

                IState state = database.States.Single(s => s.Key.Equals("State000")).Value;
                Assert.IsNotNull(state);

                city = state.Cities.Single(sc => sc.Value.Name.Equals("City050")).Value;
                Assert.IsNull(city);
                Assert.IsTrue(database.Cities.Remove(city.Name));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestKeyChange()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                IUser user = database.Users.Single(u => u.Key.Equals("User010")).Value;
                Assert.IsNotNull(user);

                // Assign user key to unused key
                user.Username = "User666";

                Assert.IsNotNull(database.Users.Single(u => u.Key.Equals("User666")));
                Assert.IsNull(database.Users.Single(u => u.Key.Equals("User010")));

                // Assign user key to existing key
                user.Username = "User002";

                Assert.AreNotEqual("User002", user.Username);
                Assert.AreNotEqual("User666", user.Username);

                IState state = database.States.Single(s => s.Key.Equals("State000")).Value;
                Assert.IsNotNull(state);

                ICity city = state.Cities.Single(sc => sc.Value.Name.Equals("City050")).Value;
                Assert.IsNotNull(city);

                city.Name = "City666";

                Assert.IsNotNull(state.Cities.Single(sc => sc.Value.Name.Equals("City066")));
                Assert.IsNull(state.Cities.Single(sc => sc.Value.Name.Equals("City050")));

                city.Name = "City001";

                Assert.AreNotEqual("City001", city.Name);
                Assert.AreNotEqual("City666", city.Name);
            }
        }

        [TestMethod]
        public void TestFindByKey()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;
                IOrder order1;
                IOrder order2;
                for (int i = 0; i < 100; i++)
                {
                    order1 = database.Orders.Single(o => o.Key == i).Value;
                    Assert.IsNotNull(order1);
                    Assert.IsTrue(database.Orders.TryGetValue(i, out order2));
                    Assert.IsNotNull(order2);
                    Assert.ReferenceEquals(order1, order2);
                }

                IUser user30 = database.Users.Single(u => u.Key.Equals("User030")).Value;
                Assert.IsTrue(database.Orders.TryGetValue(30, out order1));
                Assert.AreEqual(user30.Username, order1.User.Username);

                ICity city;
                database.Cities.TryGetValue("City040", out city);
                Assert.IsNotNull(city);
                Assert.AreEqual("City040", city.Name);

                IState state;
                Assert.IsTrue(database.States.TryGetValue("State000", out state));
                Assert.IsNotNull(state);

                state.Cities.TryGetValue("City040", out city);
                Assert.IsNotNull(city);
                Assert.AreEqual("City040", city.Name);
            }
        }

        [TestMethod]
        public void TestCopyTo()
        {
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.ReadOnly))
            {
                IDatabase database = ws.Data;

                KeyValuePair<String, IUser>[] usersArray = new KeyValuePair<string, IUser>[100];
                database.Users.CopyTo(usersArray, 0);

                Assert.AreEqual(database.Users.Count, usersArray.Length);

                int i = 0;
                foreach (var val in usersArray)
                {
                    String userKey = (i < 10 ? "00" + i : "0" + i);
                    Assert.AreEqual("User" + userKey, val.Key);
                    i++;
                }
            }
        }
    }
}
