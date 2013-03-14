using System;
using System.Collections.Generic;
using System.Text;

namespace Execom.IOG.Server.Metadata
{
    /// <summary>
    /// This class represent PropertyInfo class. There is no every property from PropertyInfo, but 
    /// just property that are needed for js client. 
    /// </summary>
    /// <author>Ivan Vasiljevic</author>
    public class PropertiesMetadata
    {

        private Boolean canRead;
        private Boolean canWrite;
        private String name;
        private Boolean isStatic;
        private List<String> customAttributes = new List<string>();
        private TypeProxy propertyType;
        private TypeProxy declaringType;

        public Boolean CanRead
        {
            get { return canRead; }
            set { canRead = value; }
        }

        public Boolean CanWrite
        {
            get { return canWrite; }
            set { canWrite = value; }
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public Boolean IsStatic
        {
            get { return isStatic; }
            set { isStatic = value; }
        }

        public List<String> CustomAttributes
        {
            get { return customAttributes; }
            set { customAttributes = value; }
        }

        public TypeProxy PropertyType
        {
            get { return propertyType; }
            set { propertyType = value; }
        }

        public TypeProxy DeclaringType
        {
            get { return declaringType; }
            set { declaringType = value; }
        }


    }
}
