using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Runtime.InteropServices;
using System.Threading;
using Execom.IOG.Distributed.Model;
using System.Collections;
using Execom.IOG.Storage;
using System.IO;

namespace Execom.IOG.Distributed.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing local context data");

            var file = new FileStream("data.dat", FileMode.OpenOrCreate);
            using (var storage = new IndexedFileStorage(file, 256, true))
            {

                using (var ctxLocal = new Context(typeof(IDataModel), null, storage))
                {

                    using (var ws = ctxLocal.OpenWorkspace<IDataModel>(IsolationLevel.Exclusive))
                    {
                        ws.Data.Users = ws.New<ICollection<IUser>>();

                        for (int i = 0; i < 10; i++)
                        {
                            var user = ws.New<IUser>();
                            user.Username = "User" + i;
                            user.Age = i;
                            ws.Data.Users.Add(user);
                        }

                        ws.Commit();
                    }

                    var ctxServer = new ServerContext(ctxLocal);

                    Console.WriteLine("Starting server...");
                    BinaryServerFormatterSinkProvider serverProv = new BinaryServerFormatterSinkProvider();
                    serverProv.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
                    BinaryClientFormatterSinkProvider clientProv = new BinaryClientFormatterSinkProvider();

                    IDictionary props = new Hashtable();
                    props["port"] = 5656;
                    props["name"] = "serverChannelName";

                    HttpChannel channel = new HttpChannel(props, clientProv, serverProv);
                    ChannelServices.RegisterChannel(channel, false);
                    try
                    {
                        RemotingServices.Marshal(ctxServer, "ctxServer");

                        do
                        {
                            while (!Console.KeyAvailable)
                            {
                                Console.Clear();
                                Console.WriteLine("Press ESC to stop the server.");

                                using (var ws = ctxLocal.OpenWorkspace<Execom.IOG.Distributed.Model.IDataModel>(IsolationLevel.ReadOnly))
                                {
                                    Console.WriteLine(ws.Data.Users.Count + " USERS ");
                                }

                                Thread.Sleep(5000);
                            }
                        }
                        while (Console.ReadKey().Key != ConsoleKey.Escape);
                    }
                    finally
                    {
                        RemotingServices.Disconnect(ctxServer);
                        ChannelServices.UnregisterChannel(channel);
                    }
                }
            }
        }
    }
}
