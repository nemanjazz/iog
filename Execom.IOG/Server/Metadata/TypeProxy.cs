using System;
using System.Collections.Generic;
using System.Text;

namespace Execom.IOG.Server.Metadata
{
    /// <summary>
    /// This class is made for one reason. When in some class we have property with type of some other
    /// user defined type. Then if we would use TypeMetadata we would have circular dependency and 
    /// cleint script couldn't find right type. Because of that we are using PropertyTypeProxy
    /// that is containing just name of type. For now this is good enough.
    /// </summary>
    /// <author>Ivan Vasiljevic</author>
    public class TypeProxy
    {
        public TypeProxy()
        {
            genericAgrumentsTypeName = new List<TypeProxy>();
        }

        /// <summary>
        /// Name of type that PropertyTypeProxy reoresent.
        /// </summary>
        protected String nameOfType;
        /// <summary>
        /// List of PropertyTypeProxy in case that type has generic arguments.
        /// </summary>
        protected List<TypeProxy> genericAgrumentsTypeName;

        public String NameOfType
        {
            get { return nameOfType; }
            set { nameOfType = value; }
        }

        public List<TypeProxy> GenericAgrumentsTypeName
        {
            get { return genericAgrumentsTypeName; }
            set { genericAgrumentsTypeName = value; }
        }
    }
}
