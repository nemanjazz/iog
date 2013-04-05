using System;
using System.Collections.Generic;
using System.Text;

namespace Execom.IOG.TypeVisual
{
    public partial class GVTemplate
    {
        private ICollection<TypeVisualUnit> typeUnits;
        private int minLen = 2;
        private int labelDistance = 2;

        public int MinLen
        {
            get { return minLen; }
            set { minLen = value; }
        }

        public int LabelDistance
        {
            get { return labelDistance; }
            set { labelDistance = value; }
        }


        public GVTemplate(ICollection<TypeVisualUnit> typeUnits)
        {
            this.typeUnits = typeUnits;
        }

        private string printNodesAndEdges()
        {   
            string retVal = "";
    
            foreach (TypeVisualUnit unit in typeUnits)
            {
                retVal +=unit.Name + "Node [label=\"<p1>" + unit.Name + "|";
                if(unit.ScalarProperties.Count == 0)
                     retVal +="\\n";
                else
                     retVal +="<p2>";
                
                //Nodes
  	            foreach(TypeVisualProperty scalarProperty in unit.ScalarProperties)
		        {
                    string propertyType;
                    if (scalarProperty.CollectionType == PropertyCollectionType.NotACollection)
                        propertyType = scalarProperty.Type;
                    else if (scalarProperty.CollectionType == PropertyCollectionType.IDictionary)
                        propertyType = PropertyCollectionType.IDictionary.ToString() + "\\<" + scalarProperty.CollectionKey
                               + "," + scalarProperty.Type + "\\>";
                    else
                        propertyType = scalarProperty.CollectionType.ToString() + "\\<" + scalarProperty.Type + "\\>";

			        retVal +="+ " + scalarProperty.Name + " : " + propertyType + " ";
                    PropertyAttribute attribute = scalarProperty.Attribute;
		   	        if(attribute == PropertyAttribute.PrimaryKeyAndImmutableProperty)
                        retVal += "[PK][IM]";
				    else if(attribute == PropertyAttribute.PrimaryKeyProperty)
				        retVal += "[PK]";
			        else if(attribute == PropertyAttribute.ImmutableProperty)
			           retVal += "[IM]";
			        
                    retVal += "\\l";
		        }

		        retVal += "\"];\r\n";

                //Edges
                foreach (TypeVisualProperty nonScalarProperty in unit.NonScalarProperties)
                {
                    string propertyType = nonScalarProperty.Type;
			        propertyType = propertyType.Replace("<","\\<");
			        propertyType = propertyType.Replace(">","\\>");
                    retVal += unit.Name + "Node -> " + propertyType + "Node [label=\"" + nonScalarProperty.Name; 

                    PropertyAttribute attribute = nonScalarProperty.Attribute;
					if(attribute == PropertyAttribute.PrimaryKeyAndImmutableProperty)
						retVal +="[PK][IM]\",color=\"red";
				    else if(attribute == PropertyAttribute.PrimaryKeyProperty)
						retVal +="[PK]";
					else if(attribute == PropertyAttribute.ImmutableProperty)
                        retVal += "[IM]\",color=\"red";

                    retVal += "\",headlabel=";
                    if (nonScalarProperty.CollectionType == PropertyCollectionType.NotACollection)
                        retVal += "\"1\"";
                    else
                        retVal += "\"*\"";
                    retVal += "];";

                    retVal += "\r\n";

                }

                retVal += "\r\n";
            }

            return retVal;
        }

    }
}
