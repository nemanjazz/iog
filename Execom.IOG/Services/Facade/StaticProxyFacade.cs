// -----------------------------------------------------------------------
// <copyright file="ProxyFacade.cs" company="Execom">
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

namespace Execom.IOG.Services.Facade
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Execom.IOG.Services.Data;
    using Execom.IOG.Services.Runtime;

    /// <summary>
    /// Defines methods visible to generated proxy objects.
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal class StaticProxyFacade
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static StaticProxyFacade instance;

        /// <summary>
        /// Service for type manipulation
        /// </summary>
        private TypesService typesService;
                

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static StaticProxyFacade Instance
        {
            get
            {
                if (instance == null)
                {
                    throw new ArgumentException("ProxyFacade not initialized");
                }

                return instance;
            }
        }

        /// <summary>
        /// Initializes a new instance of the ProxyFacade class
        /// </summary>
        private StaticProxyFacade()
        {

        }

        /// <summary>
        /// Initializes the ProxyFacade class
        /// </summary>
        /// <param name="typesService">Service for type manipulation</param>
        public static void Initialize(TypesService typesService)
        {
            if (typesService == null)
            {
                throw new ArgumentNullException("typesService");
            }            

            StaticProxyFacade inst = new StaticProxyFacade();

            inst.typesService = typesService;            

            instance = inst; // Instance initialized
        }

        /// <summary>
        /// Returns type identifier for a given type
        /// </summary>
        /// <param name="type">Type to identify</param>
        /// <returns>Type ID, or empty Guid if not found</returns>
        public Guid GetTypeId(Type type)
        {
            return typesService.GetTypeIdCached(type);
        }

        /// <summary>
        /// Returns member identifier for a given type and member name
        /// </summary>
        /// <param name="typeId">Type to search in</param>
        /// <param name="propertyName">Name of member</param>
        /// <returns>Member ID, or empty Guid if not found</returns>
        public Guid GetTypeMemberId(Guid typeId, string propertyName)
        {
            return typesService.GetTypeMemberId(typeId, propertyName);
        }

        /// <summary>
        /// Determines if type with given ID is scalar type
        /// </summary>
        /// <param name="typeId">Type ID</param>
        /// <returns>True if type is scalar</returns>
        public bool IsScalarType(Guid typeId)
        {
            return typesService.IsScalarType(typeId);
        }

        /// <summary>
        /// Determines if member with given ID is of scalar type
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <returns>True if member type is scalar</returns>
        public bool IsScalarMember(Guid memberId)
        {
            Guid typeId = typesService.GetMemberTypeId(memberId);
            return typesService.IsScalarType(typeId);
        }

        public Guid GetTypePrimaryKeyMemberId(Guid typeId)
        {
            return typesService.GetTypePrimaryKeyMemberId(typeId);
        }

        /// <summary>
        /// Returns if two proxies are equal
        /// </summary>
        /// <param name="proxy1">Proxy which is calling the Equals()</param>
        /// <param name="proxy2">Proxy which is used to compare to</param>
        /// <returns>True if proxies have same instance ID</returns>
        public static bool AreEqual(object proxy1, object proxy2)
        {
            if (proxy2 == null)
            {
                return false;
            }

            if (!Utils.HasItemId(proxy2))
            {
                return false;
            }

            return Utils.GetItemId(proxy1).Equals(Utils.GetItemId(proxy2));
        }

        /// <summary>
        /// Calculates hashcode for the proxy
        /// </summary>
        /// <param name="instance">Proxy instance</param>
        /// <returns>Hash code of instance ID</returns>
        public static int GetProxyHashCode(object instance)
        {
            return Utils.GetItemId(instance).GetHashCode();
        }

        
    }
}
