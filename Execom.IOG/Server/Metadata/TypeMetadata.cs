using System;
using System.Collections.Generic;
using System.Text;

namespace Execom.IOG.Server.Metadata
{
    /// <summary>
    /// This class represent Type class, but with only that filds that are need for
    /// js client to work.
    /// </summary>
    /// <author>Ivan Vasiljevic</author>
    public class TypeMetadata
    {
        private Guid id;
        private String name;
        private Boolean isCollectionType;
        private Boolean isDictionaryType;
        private Boolean isInterface;
        private Boolean isEnum;
        private Boolean isGenericType;
        private String genericType;
        private List<String> customAttributes = new List<string>();
        private List<PropertiesMetadata> properties = new List<PropertiesMetadata>();
        private List<String> interfaces = new List<string>();
        private Dictionary<string, Int64> enumValues = new Dictionary<string,Int64>();
        private List<String> genericArguments = new List<string>();
        private Boolean isScalar;

        public Guid ID
        {
            get { return id; }
            set { id = value; }
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public Boolean IsCollectionType
        {
            get { return isCollectionType; }
            set { isCollectionType = value; }
        }

        public Boolean IsDictionaryType
        {
            get { return isDictionaryType; }
            set { isDictionaryType = value; }
        }

        public Boolean IsInterface
        {
            get { return isInterface; }
            set { isInterface = value; }
        }

        public Boolean IsEnum
        {
            get { return isEnum; }
            set { isEnum = value; }
        }

        public Boolean IsGenericType
        {
            get { return isGenericType; }
            set { isGenericType = value; }
        }

        public List<String> CustomAttributes
        {
            get { return customAttributes; }
            set { customAttributes = value; }
        }

        public List<PropertiesMetadata> Properties
        {
            get { return properties; }
            set { properties = value; }
        }

        public List<String> Interfaces
        {
            get { return interfaces; }
            set { interfaces = value; }
        }

        public Dictionary<string, Int64> EnumValues
        {
            get { return enumValues; }
            set { enumValues = value; }
        }

        public List<String> GenericArguments
        {
            get { return genericArguments; }
            set { genericArguments = value; }
        }

        public Boolean IsScalar
        {
            get { return isScalar; }
            set { isScalar = value; }
        }

        public String GenericType
        {
            get { return genericType; }
            set { genericType = value; }
        }


    }
}
