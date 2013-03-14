// -----------------------------------------------------------------------
// <copyright file="IObjectSerializer.cs" company="Execom">
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

namespace Execom.IOG.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;

    /// <summary>
    /// Defines methods for object serialization / deserialization
    /// </summary>
    public interface IObjectSerializer
    {
        /// <summary>
        /// Serializes object as byte array
        /// </summary>
        /// <param name="value">Object to serialize</param>
        void Serialize(object value, BinaryWriter bw);

        /// <summary>
        /// Deserializes object from byte array
        /// </summary>
        /// <param name="stream">Stream to deserialize</param>
        /// <returns>Object</returns>
        object Deserialize(BinaryReader br);
    }
}
