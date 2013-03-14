// -----------------------------------------------------------------------
// <copyright file="Constants.cs" company="Execom">
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

namespace Execom.IOG.Services.Data
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Shared constants
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal static class Constants
    {
        /// <summary>
        /// Id for types root node
        /// </summary>
        public static Guid TypesNodeId = new Guid("{22DD35BD-071B-4429-837D-4F5D2C201580}");

        /// <summary>
        /// Id for snapshots node
        /// </summary>
        public static Guid SnapshotsNodeId = new Guid("{52138911-0016-4C08-A685-9487617FD664}");

        /// <summary>
        /// Id for exclusive writer lock
        /// </summary>
        public static Guid ExclusiveWriterLockId = new Guid("{7EB5139E-72C2-4029-9EFD-1CD514775832}");

        /// <summary>
        /// ID for null reference node
        /// </summary>
        public static Guid NullReferenceNodeId = new Guid("{FFCE2840-A5D7-4C1F-81F4-A8AC7FC61F92}");

        /// <summary>
        /// Name of generated field for instance ID
        /// </summary>
        public static string InstanceIdFieldName = "__instanceId__";

        /// <summary>
        /// Name of generated field for primary key ID
        /// </summary>
        public static string PrimaryKeyIdFieldName = "__keyId__";

        /// <summary>
        /// Name of generated field for type ID
        /// </summary>
        public static string TypeIdFieldName = "__typeId__";

        /// <summary>
        /// Name of generated field for facade
        /// </summary>
        public static string FacadeFieldName = "__facade__";

        /// <summary>
        /// Name of generated field for read only status of proxy
        /// </summary>
        public static string ReadOnlyFieldName = "__readOnly__";

        /// <summary>
        /// Sufix for generated type name
        /// </summary>
        public static string ProxyTypeSufix = "_Proxy_";

        /// <summary>
        /// Name of generated assembly, which is different from file name where assembly is saved
        /// </summary>
        public static string GeneratedAssemblyName = "IOG.RuntimeProxy";

        /// <summary>
        /// Sufix for fields generated for each property member ID
        /// </summary>
        public static string PropertyMemberIdSufix = "_MemberID_";

        /// <summary>
        /// Sufix for fields generated for each property scalar info
        /// </summary>
        public static string PropertyIsScalarSufix = "_IsScalar_";

        /// <summary>
        /// Determines identifier of value for type members which defines a primary key
        /// </summary>
        public static Guid TypeMemberPrimaryKeyId = new Guid("{67B21654-1E2D-4565-A4AE-33A7E1D43AF2}");
    }
}
