using System;
using System.Collections.Generic;
using System.Text;

namespace Execom.IOG.Server.Metadata
{
    /// <summary>
    /// ObjectWrapper is representing Object that is transformed in that way that can be sent to 
    /// js client.
    /// </summary>
    /// <author>Ivan Vasiljevic</author>
    public class ObjectWrapper
    {

        protected static String IOG_TYPE = "iogType";
        protected static String VALUE = "value";
        protected static String NAME = "name";

        protected const String BOOLEAN = "Boolean";
        protected const String INT32 = "Int32";
        protected const String INT64 = "Int64";
        protected const String DOUBLE = "Double";
        protected const String STRING = "String";
        protected const String CHAR = "Char";
        protected const String BYTE = "Byte";
        protected const String DATE_TIME = "DateTime";
        protected const String TIME_SPAN = "TimeSpan";
        protected const String GUID_TYPE = "Guid";
        protected const String EMPTY_STRING = "";

        protected TypeProxy typeName;
        protected object data;

        public TypeProxy TypeName
        {
            get { return typeName; }
            set { typeName = value; }
        }

        public Object Data
        {
            get { return data; }
            set { data = value; }
        }

        private ObjectWrapper()
        {
        
        }

        /// <summary>
        /// Method is used for creating ObjectWrapper. This method is created because 
        /// DateTime and TimeSpan types. When we transforming this two object
        /// we need to get only number of ticks from them. All other cases we just
        /// need to put object to Data property and object will be transformed in a right way.
        /// </summary>
        /// <param name="typeOfdata">Type of data object</param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ObjectWrapper CreateObjectWrapper(Object data)
        {
            ObjectWrapper ow = new ObjectWrapper();

            if(data == null)
            {
                ow.TypeName = null;
                ow.Data = null;
                return ow;
            }

            Type typeOfdata = data.GetType();
            ow.TypeName = TypeSingleton.ExtractPropertyType(typeOfdata);//typeOfdata.Name;

            if (typeOfdata.Equals(typeof(DateTime)) )
            {
                ow.Data = ((DateTime)data).Ticks;
            }
            else
            {
                if (typeOfdata.Equals(typeof(TimeSpan)))
                {
                    ow.Data = ((TimeSpan)data).Ticks;
                }
                else
                {
                    ow.Data = data;
                }
            }

            return ow;
        }

        /// <summary>
        /// This method is used to get value from object that is got from js client.
        /// objectToParse need to have data about type of value that it contians.
        /// </summary>
        /// <param name="objectToParse">is Object that need to be parsed</param>
        /// <returns>value of spcified type, or null</returns>
        public static object ParseObjectWrapper(Object objectToParse)
        {
            if (objectToParse == null)
                return null;
            try
            {
                if (objectToParse is Dictionary<String, Object>)
                {
                    Dictionary<String, Object> dictionaryOfObject = (Dictionary<String, Object>)objectToParse;
                    if (dictionaryOfObject.ContainsKey(IOG_TYPE) &&
                        dictionaryOfObject[IOG_TYPE] is Dictionary<String, Object> &&
                        dictionaryOfObject.ContainsKey(VALUE))
                    {
                        Dictionary<String, Object> iogType = (Dictionary<String, Object>)dictionaryOfObject[IOG_TYPE];
                        if (iogType.ContainsKey(NAME) && iogType[NAME] is String)
                        {
                            String typeName = (String)iogType[NAME];
                            if (typeName == null)
                                return null;

                            switch (typeName)
                            {
                                case BOOLEAN:
                                    return Convert.ToBoolean(dictionaryOfObject[VALUE]);
                                case INT32:
                                    return Convert.ToInt32(dictionaryOfObject[VALUE]);
                                case INT64:
                                    return Convert.ToInt64( dictionaryOfObject[VALUE]);
                                case DOUBLE:
                                    var value = dictionaryOfObject[VALUE];

                                    return Convert.ToDouble(dictionaryOfObject[VALUE]);
                                    
                                case STRING:
                                    return (String)dictionaryOfObject[VALUE];
                                case CHAR:
                                    return Char.Parse((String)dictionaryOfObject[VALUE]);
                                case BYTE:
                                    return Convert.ToByte(dictionaryOfObject[VALUE]);
                                case DATE_TIME:
                                    long valueDateTime = Convert.ToInt64(dictionaryOfObject[VALUE]);
                                    return new DateTime(valueDateTime);
                                case TIME_SPAN:
                                    long valueTimeSpan = Convert.ToInt64(dictionaryOfObject[VALUE]);
                                    return new TimeSpan(valueTimeSpan);
                                case GUID_TYPE:
                                    Guid guid = new Guid((String)dictionaryOfObject[VALUE]);
                                    return guid;
                                case null:
                                case EMPTY_STRING:
                                    return null;
                                default:
                                    Type en = TypeSingleton.GetInstace().GetEnum(typeName);

                                    if (en == null)
                                    {
                                        throw new Exception("Unsupported type!");
                                    }

                                    object enumValue = null;
                                    if (en.IsEnum)
                                    {
                                         enumValue = Enum.ToObject(en , Convert.ToInt64(dictionaryOfObject[VALUE]));
                                         return enumValue;
                                    }
                                    throw new Exception("Unsupported type!");

                            }
                        }
                        else
                        {
                            throw new Exception("Object is not well formated!");
                        }
                    }
                    else
                    {
                        throw new Exception("Object is not well formated!");
                    }
                }
                else
                {
                    throw new Exception("Object is not well formated!");
                }
            }
            catch( Exception e)
            {
                e.ToString();
                throw e;
            }
        }
    }
}
