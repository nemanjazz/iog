// -----------------------------------------------------------------------
// <copyright file="ImmutableAttribute.cs" company="Execom">
// Copyright 2011 EXECOM d.o.o
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
// -----------------------------------------------------------------------

namespace Execom.IOG.Attributes
{
    using System;

    /// <summary>
    /// Attribute which is set to reference-type properties which indicates that that child node will have parent node populated for this reference.
    /// Parents nodes are used as a level one circular reference support. When defined, child node will have parent node (node which has reference to child node) 
    /// added to the parent nodes structure which can be used to navigate up in the node tree structure.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class StoreParentNodesAttribute : Attribute
    {
        public StoreParentNodesAttribute()
        {
        }
    }
}
