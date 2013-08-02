using System;
using System.Collections.Generic;
using System.Text;
using Execom.IOG.Graph;
using System.Collections;

namespace Execom.IOG.Services.Data
{
    public class ViewDataStructure
    {
        public string DataStructureName { get; set; }
        public ViewDataStructure SubDataStructure { get; set; }
        public IDictionary<string, object> ScalarValues { get; set; }

        public ViewDataStructure()
        {
            ScalarValues = new Dictionary<string, object>();
        }
    }
    
    internal class ViewDataService
    {
        private INodeProvider<Guid, object, EdgeData> nodes;
        private TypesService typesService;

        private ViewDataStructure structure = new ViewDataStructure();

        public ViewDataService(INodeProvider<Guid, object, EdgeData> nodes, TypesService typesService)
        {
            this.nodes = nodes;
            this.typesService = typesService;
        }

        public ViewDataStructure GetDataForSelectedNode(Node<Guid, object, EdgeData> startingNode)
        {
            if (startingNode != null)
            {
                getDataRecursive(startingNode);
            }

            return structure;
        }

        private void getDataRecursive(Node<Guid, object, EdgeData> node)
        {
            /* Use this Node Types:
             * Scalar, Object, Collection, Dictionary, TypeMember
             */

            /* Use this Edge Types:
             * OfType - B node defines the type of node A
             * Property - A node contains B node as a property
             * ListItem - A node contains a list of B nodes
             */

            if (node.Values.Count > 0)
            {
                foreach (var value in node.Values)
                {
                    if (typesService.IsScalarType(typesService.GetMemberTypeId(value.Key)))
                    {
                        foreach (var edge in node.Edges.Values)
                        {
                            if (edge.Data.Semantic == EdgeType.OfType)
                            {
                                structure.DataStructureName = typesService.GetTypeFromId(edge.ToNodeId).Name;
                            }
                        }
                        string memberName = typesService.GetMemberName(typesService.GetMemberTypeId(value.Key), value.Key);
                        structure.ScalarValues.Add(memberName, value.Value);
                    }
                }
            }
        }

        private void addSubDataStructure(ViewDataStructure viewDataStructure)
        {
            ViewDataStructure currentViewDataStructure = structure;

            while (currentViewDataStructure != null)
            {
                currentViewDataStructure = currentViewDataStructure.SubDataStructure;
            }

            currentViewDataStructure = viewDataStructure;
        }
    }
}
