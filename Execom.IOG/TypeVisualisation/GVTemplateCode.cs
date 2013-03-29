using System;
using System.Collections.Generic;
using System.Text;

namespace Execom.IOG.TypeVisualisation
{
    public partial class GVTemplate
    {
        List<TypeVisualisationUnit> typeUnits;

        public GVTemplate(List<TypeVisualisationUnit> typeUnits)
        {
            this.typeUnits = typeUnits;
        }

    }
}
