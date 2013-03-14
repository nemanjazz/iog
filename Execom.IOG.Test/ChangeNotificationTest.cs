using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Events;

namespace Execom.IOG.Test
{
    [TestClass]
    public class ChangeNotificationTest
    {
        public interface ICar
        {
            string Model { get; set; }
        }

        public interface IPerson
        {            
            string Name { get; set; }
            ICar Car { get; set; }
        }

        public interface IDatabase
        {
            IPerson Person { get; set; }
        }

        [TestMethod]
        public void TestEvent()
        {
            Context ctx = new Context(typeof(IDatabase));

            bool eventThrown = false;

            EventHandler<Events.ObjectChangedEventArgs> del = new EventHandler<Events.ObjectChangedEventArgs>((sender, args) =>
            {
                eventThrown = true;
            });

            // Create data
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                var person = ws.New<IPerson>();
                var car = ws.New<ICar>();
                car.Model = "Model";

                person.Name = "John Connor";
                person.Car = car;

                database.Person = person;

                ws.Commit();

                ws.CreateSubscription(ws.Data.Person, del);
            }

            // Make changes
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                database.Person.Name = "Changed";
                ws.Commit();
            }

            Assert.IsTrue(eventThrown);
        }

        [TestMethod]
        public void TestEventOnScalarProperty()
        {
            Context ctx = new Context(typeof(IDatabase));

            bool eventThrown = false;

            EventHandler<Events.ObjectChangedEventArgs> del = new EventHandler<Events.ObjectChangedEventArgs>((sender, args) =>
            {
                eventThrown = true;
            });

            // Create data
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                var person = ws.New<IPerson>();
                var car = ws.New<ICar>();
                car.Model = "Model";

                person.Name = "John Connor";
                person.Car = car;

                database.Person = person;

                ws.Commit();

                ws.CreateSubscription(ws.Data.Person, "Name", del);
            }

            // Make changes
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                database.Person.Name = "Changed";
                ws.Commit();
            }

            Assert.IsTrue(eventThrown);
        }

        [TestMethod]
        public void TestEventOnScalarProperty2()
        {
            Context ctx = new Context(typeof(IDatabase));

            bool eventThrown = false;

            EventHandler<Events.ObjectChangedEventArgs> del = new EventHandler<Events.ObjectChangedEventArgs>((sender, args) =>
            {
                eventThrown = true;
            });

            // Create data
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                var person = ws.New<IPerson>();
                var car = ws.New<ICar>();
                car.Model = "Model";

                person.Name = "John Connor";
                person.Car = car;

                database.Person = person;

                ws.Commit();

                ws.CreateSubscription(ws.Data.Person, "Name", del);
            }

            // Make changes
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                database.Person.Car.Model = "Changed";
                ws.Commit();
            }

            Assert.IsFalse(eventThrown);
        }

        [TestMethod]
        public void TestEventOnReferenceProperty()
        {
            Context ctx = new Context(typeof(IDatabase));

            bool eventThrown = false;

            EventHandler<Events.ObjectChangedEventArgs> del = new EventHandler<Events.ObjectChangedEventArgs>((sender, args) =>
            {
                eventThrown = true;
            });

            // Create data
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                var person = ws.New<IPerson>();
                var car = ws.New<ICar>();
                car.Model = "Model";

                person.Name = "John Connor";
                person.Car = car;

                database.Person = person;

                ws.Commit();

                ws.CreateSubscription(ws.Data.Person, "Car", del);
            }

            // Make changes
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                database.Person.Car = null;
                ws.Commit();
            }

            Assert.IsTrue(eventThrown);
        }

        [TestMethod]
        public void TestEventOnReferenceProperty2()
        {
            Context ctx = new Context(typeof(IDatabase));

            bool eventThrown = false;

            EventHandler<Events.ObjectChangedEventArgs> del = new EventHandler<Events.ObjectChangedEventArgs>((sender, args) =>
            {
                eventThrown = true;
            });

            // Create data
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                var person = ws.New<IPerson>();
                var car = ws.New<ICar>();
                car.Model = "Model";

                person.Name = "John Connor";
                person.Car = car;

                database.Person = person;

                ws.Commit();

                ws.CreateSubscription(ws.Data.Person, "Car", del);
            }

            // Make changes
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                database.Person.Car.Model = "Changed";
                ws.Commit();
            }

            Assert.IsTrue(eventThrown);
        }

        [TestMethod]
        public void TestEventOnReferenceProperty3()
        {
            Context ctx = new Context(typeof(IDatabase));

            bool eventThrown = false;

            EventHandler<Events.ObjectChangedEventArgs> del = new EventHandler<Events.ObjectChangedEventArgs>((sender, args) =>
            {
                eventThrown = true;
            });

            // Create data
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                var person = ws.New<IPerson>();
                

                person.Name = "John Connor";
                database.Person = person;

                ws.Commit();

                ws.CreateSubscription(ws.Data.Person, "Car", del);
            }

            // Make changes
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                var car = ws.New<ICar>();
                car.Model = "Model";
                database.Person.Car = car;
                ws.Commit();
            }

            Assert.IsTrue(eventThrown);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestEventInvalidProperty()
        {
            Context ctx = new Context(typeof(IDatabase));

            bool eventThrown = false;

            EventHandler<Events.ObjectChangedEventArgs> del = new EventHandler<Events.ObjectChangedEventArgs>((sender, args) =>
            {
                eventThrown = true;
            });

            // Create data
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                var person = ws.New<IPerson>();


                person.Name = "John Connor";
                database.Person = person;

                ws.Commit();

                ws.CreateSubscription(ws.Data.Person, "Car213", del);
            }            
        }

        [TestMethod]
        public void TestEventSpawn()
        {
            Context ctx = new Context(typeof(IDatabase));

            EventHandler<Events.ObjectChangedEventArgs> del = new EventHandler<Events.ObjectChangedEventArgs>((sender, args) =>
            {
                using (var ws = ctx.OpenWorkspace<IDatabase>(args.SnapshotId, IsolationLevel.ReadOnly))
                {
                    var oldPerson = ws.Spawn<IPerson>(args.OldRevisionId);
                    var newPerson = ws.Spawn<IPerson>(args.NewRevisionId);

                    Assert.AreEqual("John Connor", oldPerson.Name);
                    Assert.AreEqual("Changed", newPerson.Name);
                }
            });

            // Create data
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                var person = ws.New<IPerson>();
                var car = ws.New<ICar>();
                car.Model = "Model";

                person.Name = "John Connor";
                person.Car = car;

                database.Person = person;

                ws.Commit();

                ws.CreateSubscription(ws.Data.Person, del);
            }

            // Make changes
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                database.Person.Name = "Changed";
                ws.Commit();
            }
        }

        [TestMethod]
        public void TestEventImplicit()
        {
            Context ctx = new Context(typeof(IDatabase));

            bool eventThrown = false;

            EventHandler<Events.ObjectChangedEventArgs> del = new EventHandler<Events.ObjectChangedEventArgs>((sender, args) =>
            {
                eventThrown = true;
            });

            // Create data
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                var person = ws.New<IPerson>();
                var car = ws.New<ICar>();
                car.Model = "Model";

                person.Name = "John Connor";
                person.Car = car;

                database.Person = person;

                ws.Commit();

                ws.CreateSubscription(ws.Data.Person, del);
            }

            // Make changes
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                database.Person.Car.Model = "Changed";
                ws.Commit();
            }

            Assert.IsTrue(eventThrown);
        }

        [TestMethod]
        public void TestEventNotRenewed()
        {
            Context ctx = new Context(typeof(IDatabase));

            bool eventThrown = false;

            EventHandler<Events.ObjectChangedEventArgs> del = new EventHandler<Events.ObjectChangedEventArgs>((sender, args) =>
            {
                eventThrown = true;
                args.RenewSubscription = false;
            });

            // Create data
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                var person = ws.New<IPerson>();
                var car = ws.New<ICar>();
                car.Model = "Model";

                person.Name = "John Connor";
                person.Car = car;

                database.Person = person;

                ws.Commit();

                ws.CreateSubscription(ws.Data.Person, del);
            }

            // Make changes
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                database.Person.Name = "Changed";
                ws.Commit();
            }

            Assert.IsTrue(eventThrown);

            eventThrown = false;

            // Make changes
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                database.Person.Name = "Changed 2";
                ws.Commit();
            }

            Assert.IsFalse(eventThrown);
        }

        [TestMethod]
        public void TestEventRenew()
        {
            Context ctx = new Context(typeof(IDatabase));

            bool eventThrown = false;

            EventHandler<Events.ObjectChangedEventArgs> del = new EventHandler<Events.ObjectChangedEventArgs>((sender, args) =>
            {
                eventThrown = true;
            });

            // Create data
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                var person = ws.New<IPerson>();
                var car = ws.New<ICar>();
                car.Model = "Model";

                person.Name = "John Connor";
                person.Car = car;

                database.Person = person;

                ws.Commit();

                ws.CreateSubscription(ws.Data.Person, del);
            }

            // Make changes
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                database.Person.Name = "Changed";
                ws.Commit();
            }

            Assert.IsTrue(eventThrown);

            eventThrown = false;

            // Make changes
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                database.Person.Name = "Changed 2";
                ws.Commit();
            }

            Assert.IsTrue(eventThrown);
        }

        [TestMethod]
        public void TestEventUnsubscribe()
        {
            Context ctx = new Context(typeof(IDatabase));

            bool eventThrown = false;

            EventHandler<Events.ObjectChangedEventArgs> del = new EventHandler<Events.ObjectChangedEventArgs>((sender, args) =>
            {
                eventThrown = true;
            });

            Subscription subscription = null;

            // Create data
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                var person = ws.New<IPerson>();
                var car = ws.New<ICar>();
                car.Model = "Model";

                person.Name = "John Connor";
                person.Car = car;

                database.Person = person;

                ws.Commit();

                subscription = ws.CreateSubscription(ws.Data.Person, del);
            }

            // Make changes
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                database.Person.Name = "Changed";
                ws.Commit();
            }

            Assert.IsTrue(eventThrown);

            eventThrown = false;

            // Unsubscribe
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                ws.RemoveSubscription(subscription);
            }


            // Make changes
            using (var ws = ctx.OpenWorkspace<IDatabase>(IsolationLevel.Exclusive))
            {
                IDatabase database = ws.Data;
                database.Person.Name = "Changed 2";
                ws.Commit();
            }

            Assert.IsFalse(eventThrown);
        }
    }
}
