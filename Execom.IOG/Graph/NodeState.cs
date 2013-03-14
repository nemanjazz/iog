using System;
using System.Collections.Generic;
using System.Text;

namespace Execom.IOG.Graph
{
    /// <summary>
    /// Defines possible node change states
    /// </summary>
    public enum NodeState
    {
        /// <summary>
        /// This means node has not been touched
        /// </summary>
        None,
        
        /// <summary>
        /// Defines new node
        /// </summary>
        Created,

        /// <summary>
        /// Defines node which exists in parent provider and was changed
        /// </summary>
        Modified,

        /// <summary>
        /// Defines removed node
        /// </summary>
        Removed
    }
}
