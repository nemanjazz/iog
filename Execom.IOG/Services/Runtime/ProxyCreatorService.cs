// -----------------------------------------------------------------------
// <copyright file="ProxyCreatorService.cs" company="Execom">
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

namespace Execom.IOG.Services.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Execom.IOG.Services.Facade;
    using System.Reflection;

    /// <summary>
    /// Service which creates and manages proxy instances and proxy types
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal class ProxyCreatorService
    {
        /// <summary>
        /// Mapping from (interface entity type)->(generated proxy type)
        /// </summary>
        private Dictionary<Type, Type> proxyTypesFromInterfaces = new Dictionary<Type,Type>();

        /// <summary>
        /// Mapping from (type identifier)->(generated proxy type)
        /// </summary>
        private Dictionary<Guid, Type> proxyTypesFromIDs = new Dictionary<Guid, Type>();

        /// <summary>
        /// Creates new instance of ProxyCreatorService class
        /// </summary>
        /// <param name="types">List of types to register</param>
        /// <param name="interfaceToTypeIdMapping">Mapping to registered type id</param>
        /// <param name="interfaceToGeneratedMapping">Mapping to generated proxy types</param>
        public ProxyCreatorService(IEnumerable<Type> types, Dictionary<Type, Guid> interfaceToTypeIdMapping, Dictionary<Type, Type> interfaceToGeneratedMapping)
        {
            foreach (var type in types)
            {
                if (type.IsInterface)
                {
                    RegisterTypeMapping(type, interfaceToGeneratedMapping[type], interfaceToTypeIdMapping[type]);
                }
            }
        }

        /// <summary>
        /// Creates new proxy instance for a given instance id
        /// </summary>
        /// <typeparam name="T">Type of the entity interface</typeparam>
        /// <param name="facade">Runtime facade visible to proxy</param>
        /// <param name="instanceId">Instance ID</param>
        /// <param name="readOnly">Defines if proxy should be read only</param>
        /// <returns>Instance of the proxy</returns>
        public T NewObject<T>(IRuntimeProxyFacade facade, Guid instanceId, bool readOnly)
        {
            Type proxyType = proxyTypesFromInterfaces[typeof(T)];
            return (T)Activator.CreateInstance(proxyType, new object[] { facade, instanceId, readOnly });
        }

        /// <summary>
        /// Creates new proxy instance for a given instance id
        /// </summary>
        /// <param name="facade">Runtime facade visible to proxy</param>
        /// <param name="instanceId">Instance ID</param>
        /// <param name="typeId">Type ID</param>
        /// <param name="readOnly">Defines if proxy should be read only</param>
        /// <returns>Instance of the proxy</returns>
        public object NewObject(IRuntimeProxyFacade facade, Guid typeId, Guid instanceId, bool readOnly)
        {
            Type proxyType = proxyTypesFromIDs[typeId];

            return Activator.CreateInstance(proxyType, new object[] { facade, instanceId, readOnly });

            // TODO (nsabo) Optimize if newer framework is used by using the Expression:
            /*
            Func<IRuntimeProxyFacade, Guid, bool, object> constructor = null;

            if (constructors.ContainsKey(typeId))
            {
                constructor=(Func<IRuntimeProxyFacade, Guid, bool, object>)constructors[typeId];
            }
            else
            {
                constructor = GetConstructor(proxyType);
                constructors.Add(typeId, constructor);
            }

            return constructor(facade, instanceId, readOnly);
             */            
        }

        /*
        private Dictionary<Guid, object> constructors = new Dictionary<Guid,object>();

        private Func<IRuntimeProxyFacade, Guid, bool, object> GetConstructor(Type proxyType)
        {
            var parameters = new ParameterExpression[] { Expression.Parameter(typeof(IRuntimeProxyFacade), "facade"), Expression.Parameter(typeof(Guid), "instanceId"), Expression.Parameter(typeof(bool), "readOnly") };
            ConstructorInfo ci = proxyType.GetConstructor(new Type[] { typeof(IRuntimeProxyFacade), typeof(Guid), typeof(bool) });
            return Expression.Lambda<Func<IRuntimeProxyFacade, Guid, bool, object>>(Expression.New(ci, new Expression[] { parameters[0], parameters[1], parameters[2] }),
               parameters).Compile();
        }
         * */

        /// <summary>
        /// Registers mapping between interface type and proxy type
        /// </summary>
        /// <param name="interfaceType">Type of the entity interface</param>
        /// <param name="proxyType">Type of generated proxy</param>
        /// <param name="typeId">Type id in data</param>
        private void RegisterTypeMapping(Type interfaceType, Type proxyType, Guid typeId)
        {
            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException("Interface type expected : " + interfaceType.AssemblyQualifiedName);
            }

            proxyTypesFromIDs.Add(typeId, proxyType);
            proxyTypesFromInterfaces.Add(interfaceType, proxyType);
        }
    }
}
