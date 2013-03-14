// -----------------------------------------------------------------------
// <copyright file="AttributeBasedMergeRuleProvider.cs" company="Execom">
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

namespace Execom.IOG.Services.Merging
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Execom.IOG.Services.Data;
    using Execom.IOG.Services.Runtime;
    using Execom.IOG.Attributes;
    using System.Reflection;

    /// <summary>
    /// Provider of merge rules based on type attributes
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal class AttributeBasedMergeRuleProvider : IMergeRuleProvider
    {
        private TypesService typesService;

        private class TypeMergeRule
        {
            public bool IsConcurrent;
            public bool IsStaticConcurrency;
            public Type DynamicResolverType;
            public Dictionary<Guid, bool> IsMemberOverride = new Dictionary<Guid, bool>();
        }

        private Dictionary<Guid, TypeMergeRule> typeMergeRules = new Dictionary<Guid, TypeMergeRule>();

        public AttributeBasedMergeRuleProvider(TypesService typesService)
        {
            this.typesService = typesService;

            Initialize();
        }

        private void Initialize()
        {
            foreach (var typeId in typesService.GetRegisteredTypes())
            {
                if (!typesService.IsScalarType(typeId))
                {
                    var type = typesService.GetTypeFromId(typeId);

                    var rule = new TypeMergeRule();

                    Type collectionType = null;
                    Type dictionaryType = null;

                    if (!Utils.IsCollectionType(type, ref collectionType) &&
                        !Utils.IsDictionaryType(type, ref dictionaryType))
                    {
                        var attributes = type.GetCustomAttributes(typeof(ConcurrentAttribute), true);

                        if (attributes.Length == 1)
                        {
                            rule.IsConcurrent = true;
                            rule.IsStaticConcurrency = ((ConcurrentAttribute)attributes[0]).Behavior == ConcurrentBehavior.Static;
                            if (!rule.IsStaticConcurrency)
                            {
                                rule.DynamicResolverType = ((ConcurrentAttribute)attributes[0]).Resolver;
                            }
                            else
                            {
                                foreach (var edge in typesService.GetTypeEdges(typeId))
                                {
                                    if (edge.Data.Semantic == Graph.EdgeType.Property)
                                    {
                                        var memberId = edge.ToNodeId;
                                        //var memberTypeId = typesService.GetMemberTypeId(memberId);
                                        var memberName = typesService.GetMemberName(typeId, memberId);

                                        PropertyInfo propertyInfo = FindPropertyInfo(type, memberName);

                                        bool isOverride = propertyInfo.GetCustomAttributes(typeof(OverrideAttribute), false).Length == 1;

                                        rule.IsMemberOverride.Add(memberId, isOverride);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        rule.IsConcurrent = true;
                        rule.IsStaticConcurrency = true;
                    }

                    // Add rule for the type in cache
                    typeMergeRules.Add(typeId, rule);
                }
            }
        }

        public bool IsConcurrent(Guid typeId)
        {
            return typeMergeRules[typeId].IsConcurrent;
        }

        public bool IsMemberOverride(Guid typeId, Guid memberId)
        {
            return typeMergeRules[typeId].IsMemberOverride[memberId];
        }

        public bool IsStaticConcurrency(Guid typeId)
        {
            return typeMergeRules[typeId].IsStaticConcurrency;
        }

        /// <summary>
        /// Returns property info from given type according to member name. 
        /// Member name represents name of the property which property info will be returned.
        /// All properties of given type and all properties which are part of inherited interfaces are checked.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        private PropertyInfo FindPropertyInfo(Type type, string memberName)
        {
            PropertyInfo propertyInfo = type.GetProperty(memberName);
            if (propertyInfo != null)
            {
                return propertyInfo;
            }

            foreach (var item in type.GetInterfaces())
            {
                propertyInfo = FindPropertyInfo(item, memberName);
                if (propertyInfo != null)
                {
                    return propertyInfo;
                }
            }

            return null;
        }
    }
}
