using System;
using System.Collections.Generic;
using System.Text;
using Execom.IOG.Graph;
using Execom.IOG.Providers;
using Execom.IOG.Storage;

namespace Execom.IOG.Server.Metadata
{
    /// <summary>
    /// This class is used for parsing data from client. Data is representing IsolatedChangeSet, and this data 
    /// is sent when Commit operation occure.
    /// </summary>
    /// <author>Ivan Vasiljevic</author>
    public class ChangeSetParser
    {
        protected static String SOURCE_SNAPSHOT_ID = "sourceSnapshotId";
        protected static String VALUE = "value";
        protected static String KEY = "key";
        protected static String NODE_STATES = "nodeStates";
        protected static String ARRAY = "array";
        protected static String NODES = "nodes";
        protected static String STORAGE = "storage";
        protected static String DICTIONARY = "dictionary";

        /// <summary>
        /// Method is used for getting NodeStates from ChangeSet object
        /// </summary>
        /// <param name="dictForParsing">Dictinary that should contain NodeStates</param>
        /// <returns></returns>
        private static Dictionary<Guid, NodeState> ParseNodeStates(Dictionary<String, Object> dictForParsing)
        {
            Dictionary<Guid, NodeState> nodeState = new Dictionary<Guid, NodeState>();
            Dictionary<String, Object> dictFromObject = dictForParsing;
            //checking if there is dictionary with node_states data
            if (dictFromObject.ContainsKey(NODE_STATES))
            {
                Object oNodeStates = dictFromObject[NODE_STATES];

                if (oNodeStates is Dictionary<String, Object>)
                {
                    Dictionary<String, Object> dNodeStates = oNodeStates as Dictionary<String, Object>;
                    //this is needed because js client is not parsing data at the momment. He has Dictionary
                    //class that is containg ARRAY property. Because we didn't want to format data on client 
                    //side, this need to be done here
                    if (dNodeStates.ContainsKey(ARRAY))
                    {
                        Object oArray = dNodeStates[ARRAY];
                        if (oArray is Object[])
                        {
                            //array is containg Dictionary objects
                            Object[] lArray = oArray as Object[];
                            foreach (Object o in lArray)
                            {
                                if (o is Dictionary<String, Object>)
                                {
                                    Dictionary<String, Object> dNodeState = o as Dictionary<String, Object>;
                                    //checking if Dictionary continas right properties
                                    if (dNodeState.ContainsKey(KEY) && dNodeState.ContainsKey(VALUE))
                                    {
                                        //parsing KEY and VALUE data
                                        Guid key = Guid.Empty;//new Guid((string)dNodeState[KEY]);
                                        if (dNodeState[KEY] is Dictionary<String, Object>)
                                        {
                                            Dictionary<String, Object> dKey = dNodeState[KEY] as Dictionary<String, Object>;
                                            if (dKey.ContainsKey(VALUE))
                                            {
                                                key = new Guid((String)dKey[VALUE]);
                                            }
                                        }
                                        //making new NodeState data
                                        NodeState currentNodeState = (NodeState)dNodeState[VALUE];
                                        //adding new NodeState data to NodeStates dictionary
                                        nodeState.Add(key, currentNodeState);
                                    }

                                }
                            }
                        }
                    }

                }
            }
            else
            {
                throw new Exception("Node object is not valid!");
            }

            return nodeState;
        }

        /// <summary>
        /// Method is used for parsing Nodes in change set.
        /// </summary>
        /// <param name="dictForParsing">Dictionary from ChangeSet which should contain Nodes</param>
        /// <returns></returns>
        private static DirectNodeProviderSafe<Guid, object, EdgeData> ParseNodes(Dictionary<String, Object> dictForParsing)
        {
            DirectNodeProviderSafe<Guid, object, EdgeData> nodes =
                        new DirectNodeProviderSafe<Guid, object, EdgeData>(new MemoryStorageSafe<Guid, object>(), false);
            Dictionary<String, Object> dictFromObject = dictForParsing;
            //parsing nodes from objectForParsing
                if (dictFromObject.ContainsKey(NODES))
                {
                    Object oNodes = dictFromObject[NODES];

                    if (oNodes is Dictionary<String, Object>)
                    {
                        Dictionary<String, Object> dNodes = oNodes as Dictionary<String, Object>;

                        if (dNodes.ContainsKey(STORAGE))
                        {
                            Object oStorage = dNodes[STORAGE];

                            if (oStorage is Dictionary<String, Object>)
                            {
                                Dictionary<String, Object> dStorage = oStorage as Dictionary<String, Object>;
                                Object oDictionary = dStorage[DICTIONARY];

                                if (oDictionary is Dictionary<String, Object>)
                                {
                                    Dictionary<String, Object> dDictionary = oDictionary as Dictionary<String, Object>;
                                    Object oArray = dDictionary[ARRAY];

                                    if (oArray is Object[])
                                    {
                                        Object[] dArray = oArray as Object[];

                                        foreach (Object o in dArray)
                                        {
                                            if (o is Dictionary<String, Object>)
                                            {
                                                Dictionary<String, Object> dNode = o as Dictionary<String, Object>;
                                                if (dNode.ContainsKey(KEY) && dNode.ContainsKey(VALUE))
                                                {
                                                    Guid key = Guid.Empty;
                                                    if (dNode[KEY] is Dictionary<String, Object>)
                                                    {
                                                        Dictionary<String, Object> dKey =
                                                            (dNode[KEY] as Dictionary<String, Object>);
                                                        if (dKey.ContainsKey(VALUE) && dKey[VALUE] is String)
                                                        {
                                                            key = new Guid((String)dKey[VALUE]);
                                                        }
                                                        else
                                                        {
                                                            //case when there is not right properties in Dictionary
                                                            continue;
                                                        }
                                                       
                                                    }
                                                    else
                                                    {
                                                        //case when dNode[KEY] is not Dictionary
                                                        continue;
                                                    }
                                                    Node<Guid, object, EdgeData> node = NodeWrapper.ParseNode(dNode[VALUE]);

                                                    nodes.SetNode(key, node);
                                                }
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("Node object is not valid!");
                }

                return nodes;
        }

        /// <summary>
        /// Method is used for getting data from object to IsolatedChangeSet.
        /// Object containing dictionaries that contain data. If object is not 
        /// formated in right way, this method is throwing exception.
        /// </summary>
        /// <param name="objectForParsing">object that is containg data that is commited from client</param>
        /// <returns>IsolatedChangeSet that is created from objectForParsing</returns>
        public static IsolatedChangeSet<Guid, object, EdgeData> Parse(Object objectForParsing)
        {
            Guid sourceSnapshotId = Guid.Empty;
            if (objectForParsing == null)
                throw new Exception("Object is not valid!");

            Dictionary<Guid, NodeState> nodeState = null;
            DirectNodeProviderSafe<Guid, object, EdgeData> nodes = null;

            if (objectForParsing is Dictionary<String, Object>)
            {
                Dictionary<String, Object> dictFromObject = (Dictionary<String, Object>)objectForParsing;
                
                //finding sourceSnapshotId property from dictionary
                if (dictFromObject.ContainsKey(SOURCE_SNAPSHOT_ID))
                {
                    Object objectSourceId = dictFromObject[SOURCE_SNAPSHOT_ID];
                    if (objectSourceId is Dictionary<String, Object>)
                    {
                        Dictionary<String, Object> dSourceId = objectSourceId as Dictionary<String, Object>;
                        if (dSourceId != null)
                        {
                            Object sSourceId = dSourceId[VALUE];
                            //checking if object is string. If it is then this is guid
                            if (sSourceId is String)
                            {
                                sourceSnapshotId = new Guid(sSourceId as String);
                            }
                            else
                            {
                                //sSourceId is not string so this is not right type and object that is being
                                //parsed is not formated as he should.
                                throw new Exception("Field SourceSnapshotId is not in right format!");
                            }

                        }
                        else
                        {
                            throw new Exception("Field SourceSnapshotId is not in right format!");
                        }
                    }
                }
                else
                {
                    //case when SourceSnapshotId is missing in objectForParsing
                    throw new Exception("There is not field SourceSnapshotId!");
                }

                nodes = ParseNodes(dictFromObject);
                nodeState = ParseNodeStates(dictFromObject);


            }
            else
                throw new Exception("Object is not valid!");

            //in case some data is missing here we  will throw exception
            if (sourceSnapshotId == null || nodes == null || nodeState == null)
            {
                throw new ArgumentNullException();
            }

            // creating IsolatedChangeSet object with parsed data
            IsolatedChangeSet<Guid, object, EdgeData> result = 
                new IsolatedChangeSet<Guid, object, EdgeData>(sourceSnapshotId, nodes, nodeState);
            return result;
        }
    }
}
