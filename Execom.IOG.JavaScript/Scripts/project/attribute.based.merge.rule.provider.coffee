class TypeMergeRule

  # isConcurrent - - bool
  # isStaticConcurrency - - bool
  # dynamicResolverType - - type
  # isMemberOverride - - Dictionary
  constructor: (@isConcurrent, @isStaticConcurrency, @dynamicResolverType, @isMemberOverride = new Dictionary()) ->

class window.AttributeBasedMergeRuleProvider
  constructor: (@typeService) ->
    this.Initialize()

  Initialize: () ->
    for typeId in @typesService.GetRegisteredTypes()
      if(not @typesService.IsScalarType(typeId))
        type = @typesService.GetTypeFromId(typeId)

        rule = new TypeMergeRule()

        collectionType = null
        dictionaryType = null
        customAttrubutes = null
        if( (not (collectionType = UTILS.IsCollectionType(type))?) and (not (dictionaryType = UTILS.IsDictionaryType(type))?) )
          if( (customAttrubutes = type.GetCustomAttributes("ConcurrentAttribute")).result == true)
            rule.isConcurrent = true
            rule.isStaticConcurrency = customAttrubutes.value[0].behavior == ConcurrentBehavior.Static

            if(not rule.isStaticConcurrency)
              rule.dynamicResolverType = (customAttrubutes.value[0]).Resolver
            else
              for edge in @typesService.GetTypeEdges(typeId)

                if(edge.data.semantic == EdgeType.Property)
                  memberId = edge.toNodeId
                  memberName = @typesService.GetMemberName(typeId, memberId)
                  propertyInfo = type.GetProperty(memberName)
                  if(propertyInfo == null)
                    throw "Propertie does not exist!"
                  isOverride = propertyInfo.GetCustomAttributes("OverrideAttribute").result

                  rule.isMemberOverride.Add(memberId, isOverride)
        else
          rule.isConcurrent = true
          rule.isStaticConcurrency = true

        typeMergeRules.Add(typeId, rule)

  IsConcurrent: (typeId) ->
    return typeMergeRules[typeId].isConcurrent

  IsMemberOverride: (typeId, memberId) ->
    return typeMergeRules[typeId].isMemberOverride[memberId]

  IsStaticConcurrency: (typeId) ->
    return typeMergeRules[typeId].isStaticConcurrency




