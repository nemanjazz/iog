namespace execom.iog.name, (exports) ->
  class exports.GenerationService
    constructor: (@typesService) ->

    # tb - Builder of type - type
    @AddEqualsHashcode: (tb) ->

    # tb - Builder of type - type
    # property - - PropertyInfo
    # propertyName - Property name - String
    # TODO find how to solve fbinstaceId,fbTypeId, fbReadOnly, fbFacade
    @GenerateDataProperty: (tb, property, propertyName, fbinstaceId,fbTypeId, fbReadOnly, fbFacade) ->
      throw "Changed implementation"

    # tb - Builder of type - type
    # property - - PropertyInfo
    # TODO find how to solve fbinstaceId
    @GenerateRevisionIdProperty: (tb, property, fbinstaceId) ->
      throw "Changed implementation"

    # Generates entity proxy type for given entity interface
    # type - - Type
    # <returns>Generated proxy type - - Type</returns>
    GenerateProxyType: (type) ->
      saveAssemblyToDisk = false
      assemblyFileName = "a"

      dictionaryType = null

      if( (dictionaryType = exports.UTILS.IsDictionaryType(type))?)
        return this.GenerateDictionaryProxyType(type)

      collectionType = null

      if( (collectionType = exports.UTILS.IsCollectionType(type))?)
        return this.GenerateCollectionProxyType(type)

      if(not type.isInterface)
        throw "Type should be an interface:" + type.name

      typeId = @typesService.GetTypeId(type)

      if(exports.UTILS.equals(typeId, exports.Guid.EMPTY))
        throw "Type not registered:" + type.name

      properties = []
      exports.UTILS.ExtractProperties(type, properties)

      newType = exports.ProxyBuilder.Generate(type.name + 
        exports.Constants.ProxyTypeSufix, properties, type)

      return newType


      #  propertyInfo - - PropertyInfo
    IsRevisionIdProperty: (propertyInfo) ->
      return propertyInfo.canRead and not propertyInfo.canWrite and propertyInfo.GetCustomAttributes(RevisionIdAttribute) and exports.UTILS.equals(propertyInfo.GetPropertyType(), exports.GuidType)

    # propertyInfo - - PropertyInfo
    IsDataProperty: (propertyInfo) ->
      return propertyInfo.canRead and propertyInfo.canWrite

    # collectionType - - Type
    GenerateCollectionProxyType: (collectionType) ->
      elementType = collectionType.genericArguments
      # TODO I am not sure, but think I don't need genericArguments At the momment 
      #if(elementType.length != 1)
      #  throw "Collection type should specify element type."

      baseListType = null
      isSet = false
      isOrdered = false
      for inter in collectionType.interfaces
        if(inter == "IScalarSet")
          isSet = true
        if(inter == "IOrderedCollection")
          isOrdered = true

      if(not @typesService.IsSealedType(elementType))
        if(not isSet)
          if(isOrdered)
            throw "Not yet supported"
          else
            baseListType = exports.CollectionProxy
        else
          throw "Not yet supported"

      else
        if(not isSet)
          if(isOrdered)
            throw "Not yet supported"
          else
            baseListType = exports.CollectionProxySealed
        else
          throw "Not yet supported"

      return baseListType
    # dictionaryType - - Type
    GenerateDictionaryProxyType: (dictionaryType) ->
      elementType = dictionaryType.genericType
      # TODO similar like with collection
      #if(elementType.length != 2)
      #  throw "Dictionary type should specify key and element type."

      if (not @typesService.IsSealedType(elementType[0]) )
        baseDictionaryType = exports.DictionaryProxy
      else
        baseDictionaryType = exports.DictionaryProxySealed

      return baseDictionaryType

    # Generates entity proxy types for given entity interfaces
    # types - Interface types, they must be registered previously with the types service - Array or enumerator of Type
    # <returns>Mapping between interface types and generated proxy types</returns>
    GenerateProxyTypes: (types) ->
      saveAssemblyToDisk = false
      assemblyFileName = "a"

      mapping = new exports.Dictionary()

      for type in types
        if(type.isInterface)
          generatedType = this.GenerateProxyType(type)
          mapping.Add(type, generatedType)

      return mapping

