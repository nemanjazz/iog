using System;
using System.Collections.Generic;
using System.Text;
using Execom.IOG.Graph;

namespace Execom.IOG.Server.Metadata
{
    /// <summary>
    /// This class is used for getting ready EdgeData for sending on client side.
    /// </summary>
    /// <author>Ivan Vasiljevic</author>
    public class EdgeDataWrapper
    {
        protected static String SEMANTIC = "semantic";
        protected static String FLAGS = "flags";
        protected static String DATA = "data";

        public EdgeType Semantic
        {
            get;
            set;
        }

        public EdgeFlags Flags
        {
            get;
            set;
        }

        /// <summary>
        /// ObjectWrapper is representing Object that is ready for sending on client side
        /// </summary>
        public ObjectWrapper Data
        {
            get;
            set;
        }

        /// <summary>
        /// TransformeEdgeData is transforming EdgeData to EdgeDataWrapper
        /// </summary>
        /// <param name="edgeData">data that need to transformed</param>
        /// <returns></returns>
        public static EdgeDataWrapper TransformeEdgeData(EdgeData edgeData)
        {
            EdgeDataWrapper edgeDataWraped = new EdgeDataWrapper();
            edgeDataWraped.Flags = edgeData.Flags;
            edgeDataWraped.Semantic = edgeData.Semantic;

            ObjectWrapper objectWithType = ObjectWrapper.CreateObjectWrapper(edgeData.Data);
            //type of object is needed so that clent know what is type of data that is received
            edgeDataWraped.Data = objectWithType;

            return edgeDataWraped;
        }

        /// <summary>
        /// This method is used for getting EdgeData from object that should be EdgeData.
        /// </summary>
        /// <param name="objectToParse">object that need to be transformed to EdgeData</param>
        /// <returns>EdgeData that was in form of object</returns>
        public static EdgeData ParseEdgeData(Object objectToParse)
        {
            if (objectToParse == null)
                return null;

            if (objectToParse is Dictionary<String, Object>)
            {
                Dictionary<String, Object> dictionaryRepresentationOfObject = (Dictionary<String, Object>)objectToParse;

                if (dictionaryRepresentationOfObject.ContainsKey(FLAGS) &&
                    dictionaryRepresentationOfObject.ContainsKey(SEMANTIC) &&
                    dictionaryRepresentationOfObject.ContainsKey(DATA))
                {
                    int flagsValue = (int)dictionaryRepresentationOfObject[FLAGS];
                    EdgeType semnatic = (EdgeType)dictionaryRepresentationOfObject[SEMANTIC];
                    EdgeFlags flags = (EdgeFlags)flagsValue;
                    Object data = ObjectWrapper.ParseObjectWrapper(dictionaryRepresentationOfObject[DATA]);
                    EdgeData edgeData = new EdgeData(semnatic, flags, data);
                    return edgeData;
                }
                else
                {
                    throw new Exception("Object is not formated right!");
                }
            }
            else
            {
                throw new Exception("Object is not formated right!");
            }
        }
    }
}
