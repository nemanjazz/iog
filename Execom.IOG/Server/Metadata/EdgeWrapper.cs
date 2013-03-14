using System;
using System.Collections.Generic;
using System.Text;
using Execom.IOG.Graph;

namespace Execom.IOG.Server.Metadata
{
    /// <summary>
    /// This class is used for getting ready Edge for sending on client side.
    /// </summary>
    /// <author>Ivan Vasiljevic</author>
    public class EdgeWrapper
    {
        protected static String TO_NODE_ID = "toNodeId";
        protected static String DATA = "data";

        public Guid ToNodeId
        {
            get;
            set;
        }

        public EdgeDataWrapper Data
        {
            get;
            set;
        }

        /// <summary>
        /// This method is used for making EdgeWrapper object that can be sent on clint, from Edge<Guid, EdgeData>.
        /// </summary>
        /// <param name="edge">data that need to be sent to client</param>
        /// <returns>EdgeWrapper object that is prepared for sending</returns>
        public static EdgeWrapper TransformeEdge(Edge<Guid, EdgeData> edge)
        {
            EdgeWrapper edgeDataWraper = new EdgeWrapper();
            edgeDataWraper.ToNodeId = edge.ToNodeId;
            edgeDataWraper.Data = EdgeDataWrapper.TransformeEdgeData(edge.Data);
            return edgeDataWraper;
        }

        /// <summary>
        /// This method is used for getting Edge<Guid, EdgeData> from object that should be Edge.
        /// </summary>
        /// <param name="objectToParse">object that need to be transformed into Edge, 
        /// if this is not possible this method throw an Exception</param>
        /// <returns>Edge<Guid, EdgeData> that represent transformed object from objectToParse.
        /// If objectToParse is null, this method will return null</returns>
        public static Edge<Guid, EdgeData> ParseEdge(Object objectToParse)
        {
            if (objectToParse == null)
            {
                return null;
            }
            if (objectToParse is Dictionary<String, Object>)
            {
                Dictionary<String, Object> dictionaryToParse = objectToParse as Dictionary<String, Object>;
                if(dictionaryToParse.ContainsKey(TO_NODE_ID) && dictionaryToParse.ContainsKey(DATA))
                {
                    Guid toNodeId = (Guid)ObjectWrapper.ParseObjectWrapper(dictionaryToParse[TO_NODE_ID]);
                    EdgeData edgeData = (EdgeData)EdgeDataWrapper.ParseEdgeData(dictionaryToParse[DATA]);

                    Edge<Guid, EdgeData> edge = new Edge<Guid, EdgeData>(toNodeId, edgeData);

                    return edge;
                }
                else
                {
                    throw new Exception("Edge object is not formated in right way!");
                }
            }
            else
            {
                throw new Exception("Edge object is not formated in right way!");
            }
        }
    }

    
}
