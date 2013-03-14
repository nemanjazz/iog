﻿// -----------------------------------------------------------------------
// <copyright file="RevisionIdAttribute.cs" company="Execom">
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
    /// Attribute which classifies property as exposing internal revision ID.
    /// Attribute may be applied to entity interface property which must be of Guid type and read only.
    /// </summary>
    /// <author>Nenad Sabo</author>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class RevisionIdAttribute : Attribute
    {
        public RevisionIdAttribute()
        {
        }
    }
}
