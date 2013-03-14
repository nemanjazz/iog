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
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Attribute which is set to reference-type properties which defines relation as immutable.
    /// Immutable relation will always point to same object sub-tree without regards to changes in future.
    /// It is used to mark historic data which should never be changed, always pointing to same data version.
    /// </summary>
    /// <author>Nenad Sabo</author>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class ImmutableAttribute : Attribute
    {
        public ImmutableAttribute()
        {
        }
    }
}
