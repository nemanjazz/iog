// -----------------------------------------------------------------------
// <copyright file="ProxySample.cs" company="Execom">
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

namespace Execom.IOG
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Execom.IOG.Services;
    using Execom.IOG.Services.Runtime;
    using System.Reflection;
    using Execom.IOG.Services.Facade;

    /// <summary>
    /// Represents a sample of how the proxy should look like, serves during development of library only.
    /// </summary>
    internal class ProxySample
    {
        private Guid instanceId;
        private bool readOnly;
        private IRuntimeProxyFacade facade;

        private static Guid typeId;
        private static Guid primaryKeyMemberId;
        private static Guid propertyIdName;
        private static bool propertyIsScalarName;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification="Temporary class")]
        static ProxySample()
        {
            typeId = MethodInfo.GetCurrentMethod().DeclaringType.GUID;
            StaticProxyFacade proxyFacade = StaticProxyFacade.Instance;
            primaryKeyMemberId = proxyFacade.GetTypePrimaryKeyMemberId(typeId);

            propertyIdName = proxyFacade.GetTypeMemberId(typeId, "Name");
            propertyIsScalarName = proxyFacade.IsScalarMember(propertyIdName);
        }

        public ProxySample(IRuntimeProxyFacade facade, Guid instanceId, Boolean readOnly)
        {
            this.instanceId = instanceId;
            this.readOnly = readOnly;
            this.facade = facade;
        }

        ~ProxySample()
        {
            facade.ProxyCollected(this);
        }

        public String Name 
        { 
            get
            {
                return (string)facade.GetInstanceMemberValue(instanceId, propertyIdName, propertyIsScalarName, readOnly);
            }

            set
            {
                facade.SetInstanceMemberValue(instanceId, propertyIdName, value, propertyIsScalarName, readOnly);
            } 
        }

        public Guid ID
        {
            get
            {
                return instanceId;
            }
        }

        public override bool Equals(object obj)
        {
            return StaticProxyFacade.AreEqual(this, obj);
        }

        public override int GetHashCode()
        {
            return StaticProxyFacade.GetProxyHashCode(this);
        }
    }
}
