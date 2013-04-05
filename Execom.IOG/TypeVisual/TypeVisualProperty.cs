using System;
using System.Collections.Generic;
using System.Text;

namespace Execom.IOG.TypeVisual
{

    public enum PropertyAttribute
    {
        None,
        ImmutableProperty,
        PrimaryKeyProperty,
        PrimaryKeyAndImmutableProperty
    }

    public enum PropertyCollectionType
    {
        NotACollection,
        ICollection,
        IScalarSet,
        IOrderedCollection,
        IIndexedCollection,
        IDictionary

    }

    public class TypeVisualProperty
    {
        private string name;
        private string type;
        private PropertyAttribute attribute;
        private PropertyCollectionType collectionType;
        private string collectionKey;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        
        public string Type
        {
            get { return type; }
            set { type = value; }
        }
        
        public PropertyAttribute Attribute
        {
            get { return attribute; }
            set { attribute = value; }
        }
        
        public PropertyCollectionType CollectionType
        {
            get { return collectionType; }
            set { collectionType = value; }
        }
        
        public string CollectionKey
        {
            get { return collectionKey; }
            set { collectionKey = value; }
        }

        //public TypeVisualProperty()
        //{

        //}

        public TypeVisualProperty(string name, string type, PropertyAttribute attribute, PropertyCollectionType collectionType, string collectionKey)
        {
            this.name = name;
            this.type = type;
            this.attribute = attribute;
            this.collectionType = collectionType;
            this.collectionKey = collectionKey;

        }

        public override string ToString()
        {
            return name + " : " + type;
        }


    }
}
