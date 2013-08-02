using System;
using System.Collections.Generic;
using System.Text;
using Execom.IOG.Graph;

namespace Execom.IOG.Services.Data
{
    internal class ViewDataService
    {
        private INodeProvider<Guid, object, EdgeData> nodes;
        private TypesService typesService;

        public ViewDataService(INodeProvider<Guid, object, EdgeData> nodes, TypesService typesService)
        {
            this.nodes = nodes;
            this.typesService = typesService;
        }
    }
}
