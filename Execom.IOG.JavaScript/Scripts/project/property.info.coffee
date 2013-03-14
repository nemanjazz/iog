namespace execom.iog.name, (exports) ->
  class exports.PropertyInfo
    constructor: (@canRead, @canWrite, @declaringType, @name, @propertyType, 
      @isStatic, @customAttributes = []) ->

    GetCustomAttributes: (name) ->
      attributes = []
      rezBoolean = false

      for attr in @customAttributes
        if(attr.name == name)
          rezBoolean = true
          attributes.push(attr)


      return {
        result: rezBoolean
        value: attributes
      }

    GetPropertyType: () ->
      type = exports.IOGType.FindType(@propertyType.nameOfType)
      if(exports.UTILS.IsCollectionType(type))
        type = exports.IOGType.CreateCollectionWithGenericType( 
          @propertyType.genericArgumentsName[0])

      if(exports.UTILS.IsDictionaryType(type))
        type = exports.IOGType.CreateDictionaryWithGenericTypes(
          @propertyType.genericArgumentsName[0], 
          @propertyType.genericArgumentsName[1])

      return type