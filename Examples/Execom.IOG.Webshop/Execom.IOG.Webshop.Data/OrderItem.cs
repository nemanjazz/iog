﻿// -----------------------------------------------------------------------
// <copyright file="OrderItem.cs" company="Execom">
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Execom.IOG.Attributes;

namespace Execom.IOG.Webshop.Data
{
    /// <summary>
    /// Basic entity for order item
    /// </summary>
    public interface OrderItem : BaseEntity
    {
        [Immutable]
        Product Product { get; set; }
        int Quantity { get; set; }
    }
}