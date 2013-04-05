using System;
using System.Collections.Generic;
using System.Text;
using Execom.IOG.TypeVisual;

namespace Execom.IOG.TypeVisual
{



    /// <summary>
    /// Class for holding information of a type (its name and all its properties, both scalar and non-scalar).
    /// </summary>
    public class TypeVisualUnit
    {

        private string name;
        private ICollection<TypeVisualProperty> scalarProperties;
        private ICollection<TypeVisualProperty> nonScalarProperties;
        
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public ICollection<TypeVisualProperty> ScalarProperties
        {
            get { return scalarProperties; }
            set { scalarProperties = value; }
        }

        public ICollection<TypeVisualProperty> NonScalarProperties
        {
            get { return nonScalarProperties; }
            set { nonScalarProperties = value; }
        }

        
        public TypeVisualUnit()
        {
            scalarProperties = new List<TypeVisualProperty>();
            nonScalarProperties = new List<TypeVisualProperty>();
        }

        public TypeVisualUnit(string name)
        {
            this.name = name;
            scalarProperties = new List<TypeVisualProperty>();
            nonScalarProperties = new List<TypeVisualProperty>();
        }

        public TypeVisualUnit(string name, ICollection<TypeVisualProperty> scalarProperties, ICollection<TypeVisualProperty> nonScalarProperties)
        {
            this.name = name;
            this.scalarProperties = scalarProperties;
            this.nonScalarProperties = nonScalarProperties;
        }

        public override string ToString()
        {
            return name;
        }

    }
}
