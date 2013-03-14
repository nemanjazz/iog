using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace Execom.IOG.Server.Metadata
{
    /// <summary>
    /// This class is made in order to have all scalar types name and types in one array.
    /// Also there is static method for checking if type is scalar.
    /// </summary>
    /// <author>Ivan Vasiljevic</author>
    public class ScalarTypes
    {
        public static String BOOLEAN = "Boolean";
        public static String INT32 = "Int32";
        public static String INT64 = "Int64";
        public static String DOUBLE = "Double";
        public static String DATE_TIME = "DateTime";
        public static String GUID = "Guid";
        public static String BYTE = "Byte";
        public static String CHAR = "Char";
        public static String TIME_SPAN = "TimeSpan";
        public static String STRING = "String";

        public static readonly String[] SCALARS = { BOOLEAN, INT32, INT64, DOUBLE, DATE_TIME, GUID, BYTE, CHAR, 
                                                       TIME_SPAN, STRING};

        public static readonly Type[] SCALARS_TYPE = {typeof(Boolean), typeof(Int32), typeof(Int64),
                                                     typeof(Double), typeof(DateTime), typeof(Guid),
                                                     typeof(Byte), typeof(TimeSpan), typeof(String), typeof(Char), 
                                                     typeof(IDictionary<object, object>), typeof(ICollection<object>)};

        /// <summary>
        /// This method is checking if type is scalar!
        /// </summary>
        /// <param name="type">type that need to be checked</param>
        /// <returns>true if type is scalar, elese false</returns>
        public static bool IsScalarType(Type type)
        {
            String name = type.Name;
            foreach (var typeName in SCALARS)
            {
                if (name.Equals(typeName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
