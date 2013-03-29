using System;
using System.Collections.Generic;
using System.Text;

namespace Execom.IOG.TypeVisualisation
{

    public enum PropertyAttribute
    {
        None,
        ImmutableProperty,
        PrimaryKeyProperty,
        PrimaryKeyAndImmutableProperty
    }

    public class TypeVisualisationUnit
    {

        private string name;
        private List<string> propertyScalarNames;
        private List<string> propertyScalarTypes;
        private List<PropertyAttribute> propertyScalarAttributes;
        private List<string> propertyNonScalarNames;
        private List<string> propertyNonScalarTypes;
        private List<PropertyAttribute> propertyNonScalarAttributes;

        
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        
        public List<string> PropertyScalarNames
        {
            get { return propertyScalarNames; }
            set { propertyScalarNames = value; }
        }

        public List<string> PropertyScalarTypes
        {
            get { return propertyScalarTypes; }
            set { propertyScalarTypes = value; }
        }

        public List<PropertyAttribute> PropertyScalarAttributes
        {
            get { return propertyScalarAttributes; }
            set { propertyScalarAttributes = value; }
        }

        public List<string> PropertyNonScalarNames
        {
            get { return propertyNonScalarNames; }
            set { propertyNonScalarNames = value; }
        }

        public List<string> PropertyNonScalarTypes
        {
            get { return propertyNonScalarTypes; }
            set { propertyNonScalarTypes = value; }
        }

        public List<PropertyAttribute> PropertyNonScalarAttributes
        {
            get { return propertyNonScalarAttributes; }
            set { propertyNonScalarAttributes = value; }
        }


        public TypeVisualisationUnit()
        {
            propertyScalarNames = new List<string>();
            propertyScalarTypes = new List<string>();
            propertyScalarAttributes = new List<PropertyAttribute>();
            propertyNonScalarNames = new List<string>();
            propertyNonScalarTypes = new List<string>();
            propertyNonScalarAttributes = new List<PropertyAttribute>();
        }

        public TypeVisualisationUnit(string name)
        {
            this.name = name;
            propertyScalarNames = new List<string>();
            propertyScalarTypes = new List<string>();
            propertyScalarAttributes = new List<PropertyAttribute>();
            propertyNonScalarNames = new List<string>();
            propertyNonScalarTypes = new List<string>();
            propertyNonScalarAttributes = new List<PropertyAttribute>();
        }

        public TypeVisualisationUnit(string name, List<string> propertyScalarNames, List<string> propertyScalarTypes, List<PropertyAttribute> propertyScalarAttributes, 
            List<string> propertyNonScalarNames, List<string> propertyNonScalarTypes, List<PropertyAttribute> propertyNonScalarAttributes)
        {
            this.name = name;
            this.propertyScalarNames = propertyScalarNames;
            this.propertyScalarTypes = propertyScalarTypes;
            this.propertyScalarAttributes = propertyScalarAttributes;
            this.propertyNonScalarNames = propertyNonScalarNames;
            this.propertyNonScalarTypes = propertyNonScalarTypes;
            this.propertyNonScalarAttributes = propertyNonScalarAttributes;
        }

    }
}
