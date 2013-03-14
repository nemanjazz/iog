using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Execom.IOG.Storage;

namespace Execom.IOG.Server
{
    ///<summary>
    /// LocalContextSingleton is used so that IOGServerContext can access Context of the server.
    /// In this class we are holding what is root type for this server instance and context of the server.
    /// 
    ///</summary>
    ///<author>Ivan Vasiljevic</author>
    public class LocalContextSingleton 
    {
        private static LocalContextSingleton instance;

        private Context localContext;
        private static Object locker = new Object();
        /// <summary>
        /// storage is IKeyValueStorage object that will be used for saving nodes.
        /// </summary>
        private IKeyValueStorage<Guid, object> storage;
        /// <summary>
        /// DEFAULT_PATH is default path for making file in which will be saved database data.
        /// </summary>
        private string DEFAULT_PATH = Path.GetTempPath() + Path.DirectorySeparatorChar + "data.dat";//"C:\\Temp\\data.dat";

        public Context LocalContext
        {
            get { return localContext; }
        }

        /// <summary>
        /// Method is used for geting instace of localContextSingelton.
        /// </summary>
        /// <typeparam name="RootType">Type that will be used for initialization of Context</typeparam>
        /// <returns></returns>
        public static LocalContextSingleton GetInstance<RootType>()
        {
            lock (locker)
            {
                if (instance == null)
                {
                    instance = new LocalContextSingleton(typeof(RootType));
                }
            }
            return instance;
        }

        /// <summary>
        /// Method is used for geting instace of localContextSingelton.
        /// </summary>
        /// <typeparam name="RootType">Type that will be used for initialization of Context</typeparam>
        /// <param name="newStorage">This object will be used for new storage if LocalContextSingleton 
        /// is not yet initialized</param>
        /// <returns></returns>
        public static LocalContextSingleton GetInstance<RootType>(IKeyValueStorage<Guid, object> newStorage)
        {
            lock (locker)
            {
                if (instance == null)
                {
                    instance = new LocalContextSingleton(typeof(RootType), newStorage);
                }
            }
            return instance;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rootType">type that will be used as root type for Context construction.</param>
        protected LocalContextSingleton(Type rootType)
        {
            // if storage object is not supplied we will used default storage object, for making Context class
            if(storage == null)
            {
                var file = new FileStream(DEFAULT_PATH, FileMode.OpenOrCreate);
                storage = new IndexedFileStorage(file, 256, true);
            }
            
            //TODO check when will storage be deleted!
            var ctxLocal = new Context(rootType, null, storage);
            localContext = ctxLocal;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rootType">type that will be used as root type for Context construction.</param>
        /// <param name="newStorage">This object will be used for new storage if LocalContextSingleton 
        /// is not yet initialized</param>
        protected LocalContextSingleton(Type rootType, IKeyValueStorage<Guid, object> newStorage)
        {
            // if storage object is not supplied we will used default storage object, for making Context class
            if (newStorage != null)
            {
                storage = newStorage;
            }
            else
            {
                if (storage == null)
                {
                    var file = new FileStream(DEFAULT_PATH, FileMode.OpenOrCreate);
                    storage = new IndexedFileStorage(file, 256, true);
                }
            }

            var ctxLocal = new Context(rootType, null, storage);
            localContext = ctxLocal;
        }
    }
}
