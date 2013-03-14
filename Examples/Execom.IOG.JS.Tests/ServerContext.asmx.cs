using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Execom.IOG.Distributed;
using System.Web.Script.Services;
using Execom.IOG;
using Execom.IOG.Graph;
using System.Web.Script.Serialization;
using Execom.IOG.Server.Metadata;
using Execom.IOG.Properties;
using Execom.IOG.JS.Tests.Metadata;
using Execom.IOG.Events;
using SignalR;

namespace Execom.IOG.JS.Tests
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.None)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class ServerContext : System.Web.Services.WebService
    {
        //string that are used in code
        protected static String RESULT = "result";
        protected static String PREVIOUS = "previous";
        protected static String VALUE = "value";
        protected static String NODE_TYPE = "nodeType";
        protected static String COMMITED = "commited";
        protected static String DATA = "data";
        protected static String VALUES = "values";
        protected static String ARRAY = "array";

        protected static String KEY = "key";
        protected static String EDGES = "edges";


        /// <summary>
        /// Creates new instance of ServerContext type
        /// </summary>
        public ServerContext()
        {
            var temp = ServerContextSingleton.Instance;
        }

        /// <summary>
        /// Service is used for checking if snapshot isolation is enabled
        /// </summary>
        /// <returns>Dictionary that has key "result" and value for that key is 
        /// true if snapshot isolation is enabled, else false </returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string SnapshotIsolationEnabled()
        {
            Dictionary<String, Boolean> result = new Dictionary<string, Boolean>();
            result.Add(RESULT, ServerContextSingleton.Instance.ServerContext.SnapshotIsolationEnabled);
            return result.ToJSON();
        }

        /// <summary>
        /// Service is used for getting default workspace timeout.
        /// </summary>
        /// <returns>Dictionary that has key "result" and value for that key is 
        /// number of ticks in miliseconds.</returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string DefaultWorkspaceTimeout()
        {
            Dictionary<String, long> result = new Dictionary<string, long>();
            result.Add(RESULT, ServerContextSingleton.Instance.ServerContext.DefaultWorkspaceTimeout.Ticks);
            return result.ToJSON();
        }

        /// <summary>
        /// Service is used for getting exclusive lock.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void EnterExclusiveLock()
        {
            ServerContextSingleton.Instance.ServerContext.EnterExclusiveLock();
        }

        /// <summary>
        /// Service is used for getting types that server is using.
        /// </summary>
        /// <returns>Array of TypeMetadata objects in json string form.</returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string EntityTypes()
        {
            Execom.IOG.Server.IOGServerContext<IDatabase> si = ServerContextSingleton.Instance.ServerContext;
            return TypeSingleton.GetInstance<IDatabase>().GetTypes().ToJSON();
        }

        /// <summary>
        /// Service is used for putting a Node on server.
        /// </summary>
        /// <param name="identifier">Guid that uniquely represent Node</param>
        /// <param name="node">Object is representing a Node object that is being put on server.</param>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void SetNode(string identifier, object node)
        {
            Guid guidIdentifier = Guid.Parse(identifier);

            Node<Guid, Object, EdgeData> serverNode = NodeWrapper.ParseNode(node);
            ServerContextSingleton.Instance.ServerContext.SetNode(guidIdentifier, serverNode);
        }

        /// <summary>
        /// Service is used for getting a Node object from server.
        /// </summary>
        /// <param name="nodeId">Guid that uniquely represent Node object that we want to obtain.</param>
        /// <param name="access">value from enum NodeAccess</param>
        /// <returns>Dictionary that has key "result" and value for that key is 
        /// node that is transformed in NodeWrapper object, so now is ready for sending.</returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetNode(string nodeId, int access)
        {
            Guid nodeGuid = Guid.Parse(nodeId);
            NodeAccess nodeAccess = (NodeAccess)access;
            Dictionary<String, NodeWrapper> result = new Dictionary<string, NodeWrapper>();
            var node = ServerContextSingleton.Instance.ServerContext.GetNode(nodeGuid, nodeAccess);
            var nodeWraped = NodeWrapper.TransformeNode(node);
            result.Add(RESULT, nodeWraped);
            return result.ToJSON();
        }

        /// <summary>
        /// Service is used for checking if node with given identifier exist.
        /// </summary>
        /// <param name="identifier">Guid identifier that is uniquely represents node object</param>
        /// <returns>Dictionary that has key "result" and value for that key is 
        /// true if database contains node with given identifier, else false.</returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Contains(string identifier)
        {
            Guid id = Guid.Parse(identifier);

            Dictionary<String, Boolean> result = new Dictionary<string, Boolean>();
            result.Add(RESULT, ServerContextSingleton.Instance.ServerContext.Contains(id));
            return result.ToJSON();
        }

        /// <summary>
        /// Service is used for removing node with given identifier from database.
        /// </summary>
        /// <param name="identifier">Guid identifier that is uniquely represents node object</param>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void Remove(String identifier)
        {
            Guid guidIdentifier = Guid.Parse(identifier);

            // TODO (nsabo) Allow this ?
            ServerContextSingleton.Instance.ServerContext.Remove(guidIdentifier);
        }

        /// <summary>
        /// Service is used for getting enumerable with all identifiers of nodes on server.
        /// </summary>
        /// <returns>Dictionary that has key "result" and value for that key is 
        /// array of identifiers (Guids) that are uniquely represents nodes on server.</returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string EnumerateNodes()
        {
            Dictionary<String, System.Collections.IEnumerable> result = new Dictionary<string, System.Collections.IEnumerable>();
            result.Add(RESULT, ServerContextSingleton.Instance.ServerContext.EnumerateNodes());
            return result.ToJSON();
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void Clear()
        {
            // TODO (nsabo) Allow this ?
            ServerContextSingleton.Instance.ServerContext.Clear();
        }

        /// <summary>
        /// Service is used for commiting data on server
        /// </summary>
        /// <param name="workspaceId">Guid identifier that is representig workspace</param>
        /// <param name="changeSet">changes that need to be done. 
        /// This is IsolatedChangeSet<Guid, object, EdgeData> when this object is tranformed.</param>
        /// <returns>Dictionary that has key "result" and value for that key is 
        /// CommitResult object that is transformed into json object.</returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Commit(string workspaceId, object changeSet)
        {
            Guid guidWorkspaceId = Guid.Parse(workspaceId);
            //transform changeSet into IsolatedChangeSet object
            IsolatedChangeSet<Guid, object, EdgeData> serverChangeSet = ChangeSetParser.Parse(changeSet);
            //commit changes
            CommitResult<Guid> commitResult =
                ServerContextSingleton.Instance.ServerContext.Commit(guidWorkspaceId, serverChangeSet);

            String resultSnapshotId = commitResult.ResultSnapshotId.ToString();
            Dictionary<string, string> mapping = new Dictionary<string, string>();
            foreach (KeyValuePair<Guid, Guid> mapObject in commitResult.Mapping)
            {
                mapping.Add(mapObject.Key.ToString(), mapObject.Value.ToString());
            }

            CommitResult<String> commitResultString = new CommitResult<string>(resultSnapshotId, mapping);

            Dictionary<String, CommitResult<String>> rez = new Dictionary<string, CommitResult<string>>();
            rez.Add(RESULT, commitResultString);
            return rez.ToJSON();
        }

        /// <summary>
        /// Service is used for opening workspace.
        /// </summary>
        /// <param name="workspaceId">Guid identifier of workspace</param>
        /// <param name="snapshotId">Guid identifier of snapshot</param>
        /// <param name="isolationLevel">number that is representing value from enumeration IsolationLevel</param>
        /// <param name="timeout">number in miliseconds that is representing timeout</param>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void OpenWorkspace(string workspaceId, string snapshotId, int isolationLevel, long timeout)
        {
            Guid workspaceIdGuid = Guid.Parse(workspaceId);
            Guid snapshotIdGuid = Guid.Parse(snapshotId);
            IsolationLevel isoLevel = (IsolationLevel)isolationLevel;
            TimeSpan timeoutConverted = new TimeSpan(timeout);
            ServerContextSingleton.Instance.ServerContext.OpenWorkspace(workspaceIdGuid, snapshotIdGuid, isoLevel, timeoutConverted);
        }

        /// <summary>
        /// Service is used for closing given workspace.
        /// </summary>
        /// <param name="workspaceId">Guid identifier that is representing workspace.</param>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void CloseWorkspace(Guid workspaceId)
        {
            ServerContextSingleton.Instance.ServerContext.CloseWorkspace(workspaceId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="snapshotId"></param>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void UpdateWorkspace(string workspaceId, string snapshotId)
        {
            Guid guidWorkspaceId = Guid.Parse(workspaceId);
            Guid guidSnapshotId = Guid.Parse(snapshotId);
            ServerContextSingleton.Instance.ServerContext.UpdateWorkspace(guidWorkspaceId, guidSnapshotId);
        }

        /// <summary>
        /// Service is used for getting changes between two snapshots.
        /// </summary>
        /// <param name="oldSnapshotId">Guid identifier for old snapshot</param>
        /// <param name="newSnapshotId">Guid identifier for new snapshot</param>
        /// <returns>Dictionary that has key "result" and value for that key is 
        /// Dictionary that is representing changes that happends.</returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string ChangesBetween(string oldSnapshotId, string newSnapshotId)
        {
            Guid guidOldSnapshotId = Guid.Parse(oldSnapshotId);
            Guid guidNewSnapshotId = Guid.Parse(newSnapshotId);
            Dictionary<String, String> changes =
                ServerContextSingleton.Instance.ServerContext.ChangesBetween(guidOldSnapshotId, guidNewSnapshotId).
                ToDictionary(k => k.Key.ToString(), k => k.Value.ToString()); ;
            Dictionary<String, Dictionary<String, String>> rez = new Dictionary<string, Dictionary<String, String>>();
            rez.Add(RESULT, changes);
            return rez.ToJSON();
        }
        
        /// <summary>
        /// Service is used for removing subscription on some event
        /// </summary>
        /// <param name="subscriptionId">ID of subscription</param>
        /// <param name="workspaceId">ID of workspace</param>
        /// <param name="callerId">connection id from signalR library</param>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void RemoveSubscription(Guid subscriptionId, Guid workspaceId, string callerId)
        {
            Subscription subscription = new Subscription(subscriptionId, workspaceId);
            MemberManager.RemoveSubscription(callerId, subscriptionId.ToString());
            ServerContextSingleton.Instance.ServerContext.RemoveSubscription(subscription);
        }

        /// <summary>
        /// Service is used for getting Guid of root object in given shanpshot
        /// </summary>
        /// <param name="snapshotId">Guid identifier of snaphot</param>
        /// <returns>Dictionary that has key "result" and value for that key is 
        /// Guid identifier of root object.</returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetRootObjectId(string snapshotId)
        {
            Guid snapshotIdParsed = Guid.Parse(snapshotId);
            Guid rootId = ServerContextSingleton.Instance.ServerContext.GetRootObjectId(snapshotIdParsed);
            Dictionary<String, Guid> result = new Dictionary<string, Guid>();
            result.Add(RESULT, rootId);
            return result.ToJSON();
        }

        /// <summary>
        /// Service is used for getting last snaphot identifier.
        /// </summary>
        /// <returns>Dictionary that has key "result" and value for that key is 
        /// Guid identifier of last snapshot.</returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string LastSnapshotId()
        {
            Dictionary<String, Guid> result = new Dictionary<string, Guid>();
            result.Add(RESULT, ServerContextSingleton.Instance.ServerContext.LastSnapshotId());
            return result.ToJSON();
        }

        /// <summary>
        /// Service is used for returning Exclusive lock.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void ExitExclusiveLock()
        {
            ServerContextSingleton.Instance.ServerContext.ExitExclusiveLock();
        }

        /// <summary>
        /// This service is not implemented yet.
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="isolationLevel"></param>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void ChangeWorkspaceIsolationLevel(Guid workspaceId, IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Service is used for getting root type.
        /// </summary>
        /// <returns>Dictionary that has key "result" and value for that key is 
        /// TypeMetadata object that is representing root type object.</returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetRootType()
        {
            Type rootType = ServerContextSingleton.Instance.ServerContext.GetRootType();

            TypeMetadata root = TypeSingleton.ExtractTypeMetadata(rootType);

            Dictionary<String, TypeMetadata> result = new Dictionary<string, TypeMetadata>();
            result.Add(RESULT, root);
            return result.ToJSON();
        }

        /// <summary>
        /// Service is used for creating subscription to event.
        /// </summary>
        /// <param name="workspaceId">ID of workspace</param>
        /// <param name="instanceId">ID of instance</param>
        /// <param name="propertyName">name of property that we want to watch for changes</param>
        /// <param name="notifyChangesFromSameWorkspace">do we want to be notified for changes in same workspace</param>
        /// <param name="callerId">connection id from signalR</param>
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string CreateSubscription(Guid workspaceId, Guid instanceId, string propertyName, bool notifyChangesFromSameWorkspace, string callerId)//, EventHandler<Execom.IOG.Events.ObjectChangedEventArgs> del)
        {
            //EventHandler<Execom.IOG.Events.ObjectChangedEventArgs> delegate += 
            Subscription sub = null;
            if(propertyName == null)
            {
                sub = ServerContextSingleton.Instance.ServerContext.CreateSubscription(workspaceId, instanceId, notifyChangesFromSameWorkspace, JSEventHandler);
                MemberManager.AddSubscription(callerId, sub.SubscriptionId.ToString());
            }
            else 
            {
                sub = ServerContextSingleton.Instance.ServerContext.CreateSubscription(workspaceId, instanceId, propertyName, notifyChangesFromSameWorkspace, JSEventHandler);
                MemberManager.AddSubscription(callerId, sub.SubscriptionId.ToString());
            }

            Dictionary<String, Subscription> result = new Dictionary<String, Subscription>();
            result.Add(RESULT, sub);
            return result.ToJSON();
        }

        /// <summary>
        /// This handler is sending events to clients!
        /// </summary>
        /// <param name="sender">object that is sending event</param>
        /// <param name="e">data about event</param>
        public static void JSEventHandler(object sender, ObjectChangedEventArgs e)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<Event>();
            //finding user to which I need to send event
            string connectionId = MemberManager.FindConnection(e.Subscription.SubscriptionId.ToString());
            //sending event to specific user
            context.Clients[connectionId].sendEvent(e.ToJSON());
        }
    }
}
