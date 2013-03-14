using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Collections;
using Execom.IOG.Distributed.Model;

namespace Execom.IOG.Distributed.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting client");

            BinaryServerFormatterSinkProvider serverProv = new BinaryServerFormatterSinkProvider();
            serverProv.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
            BinaryClientFormatterSinkProvider clientProv = new BinaryClientFormatterSinkProvider();

            IDictionary props = new Hashtable();
            props["port"] = 0;
            props["name"] = "clientChannelName";

            var channel = new HttpChannel(props, clientProv, serverProv);
            ChannelServices.RegisterChannel(channel, false);

            try
            {
                var ctxServer = (IServerContext)RemotingServices.Connect(typeof(IServerContext), "http://localhost:5656" + "/ctxServer");

                var ctxClient = new ClientContext(ctxServer, new Execom.IOG.Storage.MemoryStorageUnsafe<Guid, object>());
                
                ConsoleKeyInfo pressedKey = new ConsoleKeyInfo();
                do
                {
                    Console.Clear();                    
                    Console.WriteLine("Press ESC to exit");

                    if (Console.KeyAvailable)
                    {
                        pressedKey = Console.ReadKey();
                    }                    

                    if (pressedKey.Key != ConsoleKey.Escape)
                    {
                        using (var ws = ctxClient.OpenWorkspace<Execom.IOG.Distributed.Model.IDataModel>(IsolationLevel.Snapshot))
                        {
                            for (int i = 0; i < 1000; i++)
                            {
                                var user = ws.New<IUser>();
                                user.Username = "New guy";

                                ws.Data.Users.Add(user);
                            }
                            ws.Commit();
                        }
                    }

                }
                while (pressedKey.Key != ConsoleKey.Escape);                
            }
            finally
            {                
                ChannelServices.UnregisterChannel(channel);
            }

            
        }
        
    }
}
