using System;
using System.Collections.Generic;
using System.Text;
using Execom.IOG.Services.Runtime;
using System.Reflection;
using System.Collections.ObjectModel;

namespace Execom.IOG.Server.Metadata
{
    /// <summary>
    /// TypeSingleton is class that is continaing all data about types that
    /// are used on IOG server.
    /// </summary>
    /// <author>Ivan Vasiljevic</author>
    public class TypeSingleton
    {
        private static TypeSingleton instance;

        private static Object locker = new Object();
        /// <summary>
        /// types contains key guid, that is guid of Type that TypeMetadata represents
        /// and value TypeMetadata that represent one Type.
        /// </summary>
        private Dictionary<Guid, TypeMetadata> types = new Dictionary<Guid, TypeMetadata>();
        private Dictionary<String, Type> enums = new Dictionary<string, Type>();

        public static TypeSingleton GetInstance<RootType>()
        {
            lock (locker)
            {
                if (instance == null)
                {
                    instance = new TypeSingleton();
                    instance.Initialize<RootType>();
                }
            }
            return instance;
        }

        public static TypeSingleton GetInstace()
        {
            lock (locker)
            {
                if (instance == null)
                {
                    throw new ArgumentNullException("TypeSingleton is not initialised!");
                }

                return instance;
            }
        }

        /// <summary>
        /// Method is used for getting enumeration type that can be used 
        /// in iog library.
        /// </summary>
        /// <param name="name">it is name of type</param>
        /// <returns>Type that has name that is equal </returns>
        public Type GetEnum(String name)
        {
            Type value = null;
            enums.TryGetValue(name, out value);
            return value;
        }

        protected TypeSingleton()
        {
            
        }

        /// <summary>
        /// This method initialize TypeSingleton
        /// </summary>
        protected void Initialize<RootType>()
        {
            foreach (Type type in ScalarTypes.SCALARS_TYPE)
            {
                TypeMetadata newMetadata = ExtractTypeMetadata(type);
                this.SetType(newMetadata);
            }
            List<Type> result = new List<Type>();
            RecursivelyExtractTypeMetadata(typeof(RootType), result);

            foreach (Type type in result)
            {
                if (types.ContainsKey(type.GUID))
                {
                    continue;
                }

                if (type.IsEnum && !enums.ContainsKey(type.Name))
                {
                    enums.Add(type.Name, type);
                }
                
                TypeMetadata tm = TypeSingleton.ExtractTypeMetadata(type);
                this.SetType(tm);
            }
        }

        /// <summary>
        /// Method is used for getting all enumeration types that 
        /// can be used in database.
        /// </summary>
        /// <returns>List of types that represents Enumearations</returns>
        public List<Type> GetEnumerations()
        {
            List<Type> result = new List<Type>();
            foreach (var en in enums.Values)
            {
                result.Add(en);
            }

            return result;
        }

        public List<String> GetEnumerationsName()
        {
            List<String> result = new List<String>();
            foreach (var en in enums.Keys)
            {
                result.Add(en);
            }

            return result;
        }

        /// <summary>
        /// Method is used for setting specific type in Dicitionary types
        /// </summary>
        /// <param name="type">type that need to be added. Will be added if it is not already in dictionary</param>
        public void SetType(TypeMetadata type)
        {
            if (!types.ContainsKey(type.ID))
            {
                types.Add(type.ID, type);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>List of all types that are registred</returns>
        public List<TypeMetadata> GetTypes()
        {
            List<TypeMetadata> resultList = new List<TypeMetadata>();

            foreach (TypeMetadata tm in types.Values)
            {
                resultList.Add(tm);
            }

            return resultList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">name of type</param>
        /// <returns>corresponding type, that is represented with TypeMetadata object, or null
        /// if there is no corresponding type.</returns>
        public TypeMetadata GetType(String name)
        {
            foreach(var entry in types)
            {
                if (entry.Value.Name.Equals(name))
                {
                    return entry.Value;
                }
            }

            return null;
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid">guid that is uniquely represent one type</param>
        /// <returns>corresponding type, that is represented with TypeMetadata object, or null
        /// if there is no corresponding type.</returns>
        public TypeMetadata GetType(Guid guid)
        {
            if (types.ContainsKey(guid))
            {
                return types[guid];
            }

            return null;
        }

        /// <summary>
        /// This method is used for finding all property types. It is used in way, in begining you should
        /// give this method RootType as type, and return list will contain all types that root type contains.
        /// </summary>
        /// <param name="type"> type that need to be processed</param>
        /// <param name="result"> list in which we will add all types that are found</param>
        /// <returns>list of all finded types</returns>
        public static List<Type> RecursivelyExtractTypeMetadata(Type type, List<Type> result)
        {
            if (type.GetInterfaces() != null && type.GetInterfaces().Length != 0)
            {
                foreach (Type t in type.GetInterfaces())
                {
                    if (!result.Contains(t))
                        RecursivelyExtractTypeMetadata(t, result);
                        if (!result.Contains(t))
                            result.Add(t);
                }
                    
            }

            Collection<PropertyInfo> propertiesOfType = new Collection<PropertyInfo>();
            Utils.ExtractProperties(type, propertiesOfType);
            result.Insert(0, type);
            foreach (PropertyInfo pi in propertiesOfType)
            {
                Type propertyType = null;
                Type collectionType = null;
                Type dictionaryType = null;

                if(Utils.IsCollectionType(pi.PropertyType, ref collectionType))
                {
                    if (collectionType.IsGenericType)
                    {
                        propertyType = collectionType.GetGenericArguments()[0];
                        if (!result.Contains(propertyType) && !ScalarTypes.IsScalarType(propertyType))
                        {
                            TypeSingleton.RecursivelyExtractTypeMetadata(propertyType, result);
                        }

                        result.Add(collectionType);
                        
                    }
                }
                else
                {
                    if (Utils.IsDictionaryType(pi.PropertyType, ref dictionaryType))
                    {
                        Type firstType = dictionaryType.GetGenericArguments()[0];
                        Type secondType = dictionaryType.GetGenericArguments()[1];

                        if (!result.Contains(firstType) && !ScalarTypes.IsScalarType(propertyType))
                        {
                            TypeSingleton.RecursivelyExtractTypeMetadata(firstType, result);
                        }
                        if (!result.Contains(secondType) && !ScalarTypes.IsScalarType(propertyType))
                        {
                            TypeSingleton.RecursivelyExtractTypeMetadata(secondType, result);
                        }

                        result.Add(dictionaryType);
                        
                    }
                    else
                    {
                        propertyType = pi.PropertyType;
                        if (!result.Contains(propertyType) && !ScalarTypes.IsScalarType(propertyType))
                        {
                            TypeSingleton.RecursivelyExtractTypeMetadata(propertyType, result);
                        }
                    }
                }

                
                
            }
            

            return result;
        }

        /// <summary>
        /// This method extract TypeMetadata object from Type
        /// </summary>
        /// <param name="type">type that need to be processed</param>
        /// <returns>TypeMetadata object that is representing given type</returns>
        public static TypeMetadata ExtractTypeMetadata(Type type)
        {
            TypeMetadata typeMetadata = new TypeMetadata();
            Type collectionType = null;
            Type dictionaryType = null;

            typeMetadata.Name = type.Name;
            typeMetadata.ID = type.GUID;
            typeMetadata.IsScalar = ScalarTypes.IsScalarType(type);
            typeMetadata.IsInterface = type.IsInterface;
            typeMetadata.IsGenericType = type.IsGenericType;
            typeMetadata.IsCollectionType = Utils.IsCollectionType(type, ref collectionType);
            typeMetadata.IsDictionaryType = Utils.IsDictionaryType(type, ref dictionaryType);
            typeMetadata.IsEnum = type.IsEnum;
            typeMetadata.EnumValues = ExtractEnumValues(type);
            typeMetadata.GenericType = type.IsGenericType? type.GetGenericTypeDefinition().Name : null;
            typeMetadata.CustomAttributes = ExtractCustomAttribute(type);
            typeMetadata.Interfaces = ExtractInterfaces(type);
            typeMetadata.GenericArguments = ExtractGenericAttributes(type);
            if (!typeMetadata.IsScalar && !typeMetadata.IsDictionaryType && !typeMetadata.IsCollectionType)
            {
                typeMetadata.Properties = ExtractPropertiesMetadata(type);
            }
            

            return typeMetadata;
        }

        /// <summary>
        /// Extract property type proxy from type.
        /// </summary>
        /// <param name="type">type that need to be processed</param>
        /// <returns>PropertyTypeProxy that is representing type of property</returns>
        public static TypeProxy ExtractPropertyType(Type type)
        {
            TypeProxy ptp = new TypeProxy();
            Type collectionType = null;
            Type dictionaryType = null;

            //first we need to check if this is dictionary type
            //this is because Dictionary is also Collection
            if (Utils.IsDictionaryType(type, ref dictionaryType))
            {
                ptp = new TypeProxy();
                ptp.NameOfType = typeof(IDictionary<object, object>).Name;
                Type firstGenericArgument = type.GetGenericArguments()[0];
                Type secondGenericArgument = type.GetGenericArguments()[1];
                //getting generic arguments
                ptp.GenericAgrumentsTypeName.Add(ExtractPropertyType(firstGenericArgument));
                ptp.GenericAgrumentsTypeName.Add(ExtractPropertyType(secondGenericArgument));
            }
            else
            {
                if (Utils.IsCollectionType(type, ref collectionType))
                {
                    ptp = new TypeProxy();
                    ptp.NameOfType = typeof(ICollection<object>).Name;
                    Type genericArgument = type.GetGenericArguments()[0];
                    //getting getting argument
                    ptp.GenericAgrumentsTypeName.Add(ExtractPropertyType(genericArgument));
                }
                else
                {
                    ptp = new TypeProxy();
                    ptp.NameOfType = type.Name;

                    //getting all generic arguments for given type
                    foreach (Type generic in type.GetGenericArguments())
                    {
                        ptp.GenericAgrumentsTypeName.Add(ExtractPropertyType(generic));
                    }
                }
            }

            return ptp;


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">type that need to be processed</param>
        /// <returns>list of properties</returns>
        public static List<PropertiesMetadata> ExtractPropertiesMetadata(Type type)
        {
            List<PropertiesMetadata> propertiesMetadata = new List<PropertiesMetadata>();

            Type[] inheretedTypes = type.GetInterfaces();
            List<Type> allTypes = new List<Type>(inheretedTypes);
            allTypes.Add(type);

            foreach (Type inheretedType in allTypes)
            {
                foreach (PropertyInfo p in inheretedType.GetProperties())
                {
                    PropertiesMetadata pm = new PropertiesMetadata();
                    pm.CanRead = p.CanRead;
                    pm.CanWrite = p.CanWrite;
                    pm.CustomAttributes = ExtractCustomAttribute(p);


                    pm.DeclaringType = ExtractPropertyType(p.DeclaringType);
                    pm.IsStatic = p.GetGetMethod().IsStatic;
                    pm.Name = p.Name;

                    pm.PropertyType = ExtractPropertyType(p.PropertyType);

                    propertiesMetadata.Add(pm);
                }
            }
            
                
            return propertiesMetadata;
        }

        /// <summary>
        /// Extract generic arguments from type
        /// </summary>
        /// <param name="type">type that need to be processed</param>
        /// <returns>list of name of generic arguments name</returns>
        public static List<String> ExtractGenericAttributes(Type type)
        {
            List<String> attributes = new List<string>();

            foreach (Type attType in type.GetGenericArguments())
            {
                attributes.Add(attType.Name);
            }

            return attributes;
        }

        /// <summary>
        /// Extract enumeration values from given enum type
        /// </summary>
        /// <param name="type">type that need to be processed</param>
        /// <returns>dictionary in which key is int value of enum value, 
        /// and value is string representation of enum value</returns>
        public static Dictionary<string, Int64> ExtractEnumValues(Type type)
        {

            Dictionary<string, Int64> result = new Dictionary<string, Int64>();
            //checking if type is enum
            if (!type.IsEnum)
            {
                return result;
            }
            //getting enum values
            foreach (var value in Enum.GetValues(type))
            {
                result.Add(Convert.ToString(value), Convert.ToInt64(value));//(string)Convert.ChangeType(value, type));
            }

            return result;
        }

        /// <summary>
        /// Extract custom attributes from type
        /// </summary>
        /// <param name="type">type that need to be processed</param>
        /// <returns>list of name of custom attributes</returns>
        public static List<String> ExtractCustomAttribute(Type type)
        {
            List<String> customAttributes = new List<string>();
            foreach (object attribute in type.GetCustomAttributes(false))
            {
                customAttributes.Add(attribute.GetType().Name);
            }

            return customAttributes;
        }

        /// <summary>
        /// Extract custom attributes from property info
        /// </summary>
        /// <param name="type">PropertyInfo that need to be processed</param>
        /// <returns>list of custom attributes name of PropertyInfo</returns>
        public static List<String> ExtractCustomAttribute(PropertyInfo type)
        {
            List<String> customAttributes = new List<string>();
            foreach (object attribute in type.GetCustomAttributes(false))
            {
                customAttributes.Add(attribute.GetType().Name);
            }

            return customAttributes;
        }

        /// <summary>
        /// Extract interfaces from type
        /// </summary>
        /// <param name="type">type that need to precessed</param>
        /// <returns>list of interfaces name</returns>
        public static List<String> ExtractInterfaces(Type type)
        {
            List<String> interfaces = new List<string>();

            foreach (var inter in type.GetInterfaces())
            {
                interfaces.Add(inter.Name);
            }

            return interfaces;
        }
    }
}
