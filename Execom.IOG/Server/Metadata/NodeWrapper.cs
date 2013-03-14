using System;
using System.Collections.Generic;
using System.Text;
using Execom.IOG.Graph;

namespace Execom.IOG.Server.Metadata
{
    /// <summary>
    /// This class is representing Node, that is formated for sending on js client.
    /// </summary>
    /// <author>Ivan Vasiljevic</author>
    public class NodeWrapper
    {

        protected static String KEY = "key";
        protected static String VALUE = "value";
        protected static String ARRAY = "array";

        protected static String PREVIOUS = "previous";
        protected static String NODE_TYPE = "nodeType";
        protected static String COMMITED = "commited";
        protected static String DATA = "data";
        protected static String VALUES = "values";
        protected static String EDGES = "edges";

        public NodeWrapper()
        {
            Edges = new List<KeyValueWrapper>();
            Values = new Dictionary<String, ObjectWrapper>();
        }

        public bool Commited
        {
            get;
            set;
        }

        public NodeType NodeType
        {
            get;
            set;
        }

        public Dictionary<String, ObjectWrapper> Values
        {
            get;
            set;
        }

        public List<KeyValueWrapper> Edges
        {
            get;
            set;
        }

        public ObjectWrapper Data
        {
            get;
            set;
        }

        public Guid Previous
        {
            get;
            set;
        }

        /// <summary>
        /// This method is used for transforming Node into NodeWrapper. 
        /// </summary>
        /// <param name="node"> Node that need to be tranformed into NodeWrapper</param>
        /// <returns>NodeWrapper that is being result of transformation of Node</returns>
        public static NodeWrapper TransformeNode(Node<Guid, object, EdgeData> node)
        {
            if (node == null)
            {
                return null;
            }
            NodeWrapper newNode = new NodeWrapper();

            newNode.Commited = node.Commited;
            newNode.NodeType = node.NodeType;
            newNode.Previous = node.Previous;

            ObjectWrapper newData = null;
            if (node.NodeType == NodeType.Type)
            {
                //getting name of type of node data.
                newData = ObjectWrapper.CreateObjectWrapper(node.Data);
                //this is needed because data contains string that is contain data that
                //identifies type in .NET. Because of that we need to change type of objectWrapper
                newData.TypeName = TypeSingleton.ExtractPropertyType(Type.GetType((String)node.Data));
                newNode.Data = newData;
            }
            else
            {
                //getting type name of data, in case node.Data is null type will be null
                newData = ObjectWrapper.CreateObjectWrapper(node.Data);

                newNode.Data = newData;
            }

            foreach (KeyValuePair<Guid, object> item in node.Values)
            {
                //making ObjectWrapper from object
                ObjectWrapper ow = ObjectWrapper.CreateObjectWrapper(item.Value);
                //adding new Guid-ObjectWrapper pair into Values
                newNode.Values.Add(item.Key.ToString(), ow);
            }

            foreach (KeyValuePair<EdgeData, Edge<Guid, EdgeData>> item in node.Edges)
            {
                //getting type of key and value
                Type typeOfObject = item.Key.GetType();
                Type edgeDataType = typeof(EdgeData);
                if (typeOfObject.Equals(edgeDataType))
                {
                    EdgeDataWrapper edgeDataKey = EdgeDataWrapper.TransformeEdgeData(item.Key);

                    EdgeWrapper edgeWraper = EdgeWrapper.TransformeEdge(item.Value);

                    newNode.Edges.Add(new KeyValueWrapper(edgeDataKey, edgeWraper));
                }
                else 
                {
                    throw new Exception("Unexpected type in node wraper!");
                }
            }
            return newNode;
        }

        /// <summary>
        /// Making KeyValuePair from objects that are representing Guid, and Object.
        /// </summary>
        /// <param name="key">object that is containing Guid data</param>
        /// <param name="value">object that is containing Object data</param>
        /// <returns></returns>
        public static KeyValuePair<Guid, Object> ParseValues(Object key, Object value)
        {
            //in case key is null, that should not happend
            if (key == null)
            {
                throw new ArgumentNullException();
            }
            try
            {
                Guid rezKey = (Guid)ObjectWrapper.ParseObjectWrapper(key);

                Object rezValue = ObjectWrapper.ParseObjectWrapper(value);

                //case when some data is not formated in right way
                if (rezKey == null || rezKey == Guid.Empty || rezValue == null)
                {
                    throw new ArgumentNullException();
                }

                return new KeyValuePair<Guid, object>(rezKey, rezValue);

            }
            catch(Exception e)
            {
                throw e;
            }

        }

        /// <summary>
        /// This method is used for parsing object into SortedList<EdgeData, Edge<Guid, EdgeData>>. 
        /// SortedList<EdgeData, Edge<Guid, EdgeData>> is used for representing Edges in node.
        /// </summary>
        /// <param name="objectToParse">object that need to be transformed into 
        /// SortedList<EdgeData, Edge<Guid, EdgeData>></param>
        /// <returns>SortedList<EdgeData, Edge<Guid, EdgeData>>, or if somthing is not right throws an Exception</returns>
        public static SortedList<EdgeData, Edge<Guid, EdgeData>> ParseEdges(Object objectToParse)
        {
            SortedList<EdgeData, Edge<Guid, EdgeData>> edges = new SortedList<EdgeData, Edge<Guid, EdgeData>>();
            if (objectToParse == null)
                return edges;
            Dictionary<String, Object> dictiomaryToParse = objectToParse as Dictionary<String, Object>;

            if (dictiomaryToParse.ContainsKey(ARRAY) && (dictiomaryToParse)[ARRAY] is Object[])
            {
                Object[] tempArray = (dictiomaryToParse)[ARRAY] as Object[];

                //case when there is not Edges in array
                if (tempArray.Length == 0)
                    return edges;

                foreach (Object o in tempArray)
                {
                    //transforming object into dictionary
                    if (o is Dictionary<String, Object>)
                    {
                        var tempDictionaryO = o as Dictionary<String, Object>;

                        // tempDictionaryO should continas KEY and VALUE
                        if (tempDictionaryO.ContainsKey(KEY) && tempDictionaryO.ContainsKey(VALUE))
                        {
                            Object objectEdgeData = tempDictionaryO[KEY];
                            if (objectEdgeData == null)
                                continue;

                            EdgeData edgeData = EdgeDataWrapper.ParseEdgeData(objectEdgeData);
                            Object objectEdge = tempDictionaryO[VALUE];
                            Edge<Guid, EdgeData> edge = null;

                            if (objectEdge == null)
                            {
                                edges.Add(edgeData, null);
                            }
                            else
                            {
                                edge = EdgeWrapper.ParseEdge(objectEdge);
                            }
                            //add new edge into edges dictionary
                            edges.Add(edgeData, edge);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }  

                }
                return edges;
            }
            else
            {
                throw new Exception("Object is not formated right!");
            }
        }

        /// <summary>
        /// ParseNode is method that is trying to transform an object into Node object
        /// </summary>
        /// <param name="objectToParse"> object that needs to be transformed</param>
        /// <returns>Node object or is throwing an Exception</returns>
        public static Node<Guid, Object, EdgeData> ParseNode(Object objectToParse)
        {
            if (objectToParse is Dictionary<String, Object>)
            {
                Dictionary<String, Object> nodeRepresentation = objectToParse as Dictionary<String, Object>;

                //checking if there is everything in objectToParse that should be there
                if (nodeRepresentation.ContainsKey(PREVIOUS) && 
                    nodeRepresentation[PREVIOUS] is Dictionary<string, object> && 
                    nodeRepresentation.ContainsKey(COMMITED) &&
                    nodeRepresentation[COMMITED] is Boolean &&
                    nodeRepresentation.ContainsKey(NODE_TYPE) &&
                    nodeRepresentation[NODE_TYPE] is int &&
                    nodeRepresentation.ContainsKey(DATA) &&
                    nodeRepresentation.ContainsKey(VALUES) &&
                    nodeRepresentation[VALUES] is Dictionary<String, Object> &&
                    nodeRepresentation.ContainsKey(EDGES))
                {
                    //getting Guid of previous Node
                    Dictionary<string, object> previousDict = nodeRepresentation[PREVIOUS] as Dictionary<string, object>;
                    if (previousDict.ContainsKey(VALUE) && previousDict[VALUE] is String)
                    {
                        Guid previous = new Guid(previousDict[VALUE] as String);
                    }
                    else
                    {
                        throw new Exception("Object is not formated in right way! Previous Guid is missing!");
                    }

                    Boolean commited = (Boolean)nodeRepresentation[COMMITED];
                    NodeType nodeType = (NodeType)((int)nodeRepresentation[NODE_TYPE]);

                    Object data = ObjectWrapper.ParseObjectWrapper(nodeRepresentation[DATA]);
                    Dictionary<Guid, Object> values = new Dictionary<Guid, object>();

                    //getting Dictionary that is contining Values dictionary
                    Dictionary<String, Object> valuesDict = (Dictionary<String, Object>)nodeRepresentation[VALUES];

                    if (valuesDict.ContainsKey(ARRAY) && valuesDict[ARRAY] is Object[])
                    {
                        Object[] tempList = (Object[])valuesDict[ARRAY];

                        foreach (Object o in tempList)
                        {
                            if (o is Dictionary<String, object>)
                            {
                                Dictionary<String, object> tempDict = o as Dictionary<String, object>;

                                if (tempDict.ContainsKey(KEY) && tempDict.ContainsKey(VALUE))
                                {
                                    Guid key = (Guid)ObjectWrapper.ParseObjectWrapper(tempDict[KEY]);
                                    Object value = ObjectWrapper.ParseObjectWrapper(tempDict[VALUE]);

                                    values.Add(key, value);
                                }
                            }
                            else
                            {
                                throw new Exception("Object is not formated in right way!");
                            }

                        }
                    }
                    else
                    {
                        throw new Exception("Object is not formated in right way!");
                    }

                    Object edges = nodeRepresentation[EDGES];

                    SortedList<EdgeData, Edge<Guid, EdgeData>> edgesSortedList = NodeWrapper.ParseEdges(edges);
                    Node<Guid, Object, EdgeData> serverNode = new Node<Guid, object, EdgeData>(nodeType, data, edgesSortedList, values);
                    return serverNode;
                }
                else
                {
                    throw new Exception("Object is not formated in right way!");
                }
            }
            else
            {
                throw new Exception("Object is not formated in right way!");
            }
        }

    }

    
}
