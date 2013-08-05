using System;
using System.Collections.Generic;
using System.Text;
using Execom.IOG.Graph;
using System.Collections;

namespace Execom.IOG.Services.Data
{
    /// <summary>
    /// Represents data structure for IOG model
    /// </summary>
    public class IOGDataStructure
    {
        /// <summary>
        /// Gets or sets name of the data structure
        /// </summary>
        public string DataStructureName { get; set; }

        /// <summary>
        /// Gets or sets sub-data structure
        /// </summary>
        public IOGDataStructure SubDataStructure { get; set; }

        /// <summary>
        /// Gets or sets dictionary (property name, value) of scalar values of this data structure
        /// </summary>
        public IDictionary<string, object> ScalarValues { get; set; }

        public IOGDataStructure()
        {
            ScalarValues = new Dictionary<string, object>();
        }
    }
    
    /// <summary>
    /// Service for creating data structure for IOG model
    /// </summary>
    internal class IOGDataValuesService
    {
        private INodeProvider<Guid, object, EdgeData> nodes;
        private TypesService typesService;

        private IOGDataStructure structure = new IOGDataStructure();

        /// <summary>
        /// Creates a new intance of ViewDataService
        /// </summary>
        /// <param name="nodes">Node provider instance</param>
        /// <param name="typesService">Types provider instance</param>
        public IOGDataValuesService(INodeProvider<Guid, object, EdgeData> nodes, TypesService typesService)
        {
            this.nodes = nodes;
            this.typesService = typesService;
        }

        /// <summary>
        /// Generates IOG data structure starting from a given node
        /// </summary>
        /// <param name="startingNode"></param>
        /// <returns></returns>
        public IOGDataStructure GetDataForSelectedNode(Node<Guid, object, EdgeData> startingNode)
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
                //foreach (var value in node.Values)
                //{
                //    if (typesService.IsScalarType(typesService.GetMemberTypeId(value.Key)))
                //    {
                //        foreach (var edge in node.Edges.Values)
                //        {
                //            if (edge.Data.Semantic == EdgeType.OfType)
                //            {
                //                structure.DataStructureName = typesService.GetTypeFromId(edge.ToNodeId).Name;
                //            }
                //        }
                //        string memberName = typesService.GetMemberName(typesService.GetMemberTypeId(value.Key), value.Key);
                //        structure.ScalarValues.Add(memberName, value.Value);
                //    }
                //}

                addScalarValuesToDataStructure(node);
            }
        }

        private void addScalarValuesToDataStructure(Node<Guid, object, EdgeData> node)
        {
            foreach (var value in node.Values)
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
