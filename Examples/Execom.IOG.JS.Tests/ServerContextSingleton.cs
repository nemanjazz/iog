using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Execom.IOG.Server;
using Execom.IOG.JS.Tests.Metadata;
using Execom.IOG.Storage;
using System.IO;

namespace Execom.IOG.JS.Tests
{
    public sealed class ServerContextSingleton : IDisposable
    {
        private static ServerContextSingleton instance;
        private IOGServerContext<IDatabase> serverContext;
        private IndexedFileStorage storage;

        public static ServerContextSingleton Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ServerContextSingleton();
                }
                return instance;
            }
        }

        public IOGServerContext<IDatabase> ServerContext
        {
            get
            {
                if (serverContext == null)
                {
                    // Open or create new file stream to use as storage
                    var file = new FileStream("D:\\data.dat", FileMode.OpenOrCreate);
            
                    // Initialize IOG storage in the flat file using
                    storage = new IndexedFileStorage(file, 256, true);
                    serverContext = new IOGServerContext<IDatabase>(storage);
                }
                return serverContext;
            }
        }

        private ServerContextSingleton() { }

        public void Dispose()
        {
            storage.Dispose();
        }
    }
}