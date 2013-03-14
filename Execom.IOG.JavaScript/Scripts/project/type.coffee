
namespace execom.iog.name, (exports) ->
  exports.ScalarName =
    Boolean:                          "Boolean"
    Int32:                            "Int32"
    Int64:                            "Int64"
    Double:                           "Double"
    String:                           "String"
    Char:                             "Char"
    Byte:                             "Byte"
    DateTime:                         "DateTime"
    TimeSpan:                         "TimeSpan"
    Guid:                             "Guid"

namespace execom.iog.name, (exports) ->
  class exports.IOGType
    @NAME = "name"
    @IOG_TYPE = "iogType"

    constructor: (@id, @name, @isCollectionType, @isDictionaryType, @isInterface, @isEnum, @isGenericType, @genericType, @customAttributes, @properties = [], @interfaces = [], @enumValues = [], @genericArguments = [], @isScalar = false) ->

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

    @CopyConstructor: (iogType) ->
      if(iogType instanceof IOGType)
        return new IOGType(iogType.id, iogType.name, iogType.isCollectionType,
          iogType.isDictionaryType, iogType.isInterface, iogType.isEnum, 
          iogType.isGenericType, iogType.genericType, iogType.customAttributes,
          iogType.properties, iogType.interfaces, iogType.enumValues, 
          iogType.GenericArguments, iogType.isScalar)


    GetProperty: (name) ->
      for prop in properties
        if(prop.name == name)
          return prop

      return null

    @CreateScalar: (type, value) ->
      if(IOGType.CheckValue(type, value))
        return value

      if(!type.isScalar and !type.isEnum)
        return null
      
      if(type.isEnum)
        enumObject = exports.typeToEnums.Get(type)
        if(value instanceof exports.IOGEnumLiteral )
          if(value.enumeration.equals(enumObject))
            return value
          else
            throw "Wrong enumeration!"
        else
          if(enumObject[value]?)
            return enumObject[value]?
          else
            return enumObject.first
        
      
      switch type.name
        when exports.ScalarName.Guid
          tempRez = new exports.Guid(value)
          return tempRez
        when exports.ScalarName.String
          tempRez = new exports.IOGString(value)
          return tempRez
        when exports.ScalarName.Boolean
          tempRez = new exports.IOGBoolean(value)
          return tempRez
        when exports.ScalarName.Byte
          tempRez = new exports.Byte(value)
          return tempRez
        when exports.ScalarName.Char
          tempRez = new exports.Char(value)
          return tempRez
        when exports.ScalarName.DateTime
          if(value instanceof DateTime)
            tempRez = value
          else
            tempRez = new DateTime(value)
          #tempRez._millis = value
          tempRez[IOGType.IOG_TYPE] = exports.DateTimeType
          return tempRez
        when exports.ScalarName.Double
          tempRez = new exports.Double(value)
          return tempRez
        when exports.ScalarName.Int32
          tempRez = new exports.Int32(value)
          return tempRez
        when exports.ScalarName.Int64
          tempRez = new exports.Int64(value)
          return tempRez
        when exports.ScalarName.TimeSpan
          if(value instanceof TimeSpan)
            tempRez = value
          else
            tempRez = new TimeSpan(value)
          #tempRez._millis = 
          tempRez[IOGType.IOG_TYPE] = exports.TimeSpanType
          return tempRez
        else
          throw "Type is not scalar!"

    # Check if value is already of needed type.
    # Return true if value is instance of type, return false else.
    @CheckValue: (type, value) ->
      if((value instanceof exports.IOGBaseType or value instanceof DateTime or value instanceof TimeSpan) and exports.UTILS.equals(value.iogType, type))      
        return true
      else
        return false

    @FindScalar: (type) ->      
      if(!type.isScalar)
        return null

      switch type.name
        when exports.ScalarName.Guid
          exports.GuidType = type
          return
        when exports.ScalarName.String
          exports.StringType = type
          return
        when exports.ScalarName.Boolean
          exports.BooleanType = type
          return
        when exports.ScalarName.Byte
          exports.ByteType = type
          return
        when exports.ScalarName.Char
          exports.CharType = type
          return
        when exports.ScalarName.DateTime
          exports.DateTimeType = type
          return
        when exports.ScalarName.Double
          exports.DoubleType = type
          return
        when exports.ScalarName.Int32
          exports.Int32Type = type
          return
        when exports.ScalarName.Int64
          exports.Int64Type = type
          return
        when exports.ScalarName.TimeSpan
          exports.TimeSpanType = type
          return
        else
          throw "Type is not scalar!"

    @FindType: (name) ->
      for type in exports.types
        if(type.name == name)
          return type
      return null


    AddGenericArgument: (type, clear) ->
      if(clear? and clear == true)
        genericArguments = []
      genericArguments.push(type)

    ClearGenericArguments: () ->
      genericArguments = []

    @CreateCollectionWithGenericType: (type) ->
      newCollection = IOGType.CopyConstructor(exports.ArrayType)
      newCollection.ClearGenericArguments()
      newCollection.genericArguments.push(type)
      return newCollection

    @CreateDictionaryWithGenericTypes: (type1, type2) ->
      newCollection = IOGType.CopyConstructor(exports.DictionaryType)
      newCollection.ClearGenericArguments()
      newCollection.genericArguments.push(type1)
      newCollection.genericArguments.push(type2)
      return newCollection

  exports.IOGType::toJSON = ()->
    json = {}
    json[exports.IOGType.NAME] = @name
    return json
  
  
  exports.IOGType::equals = (other)->
    exports.UTILS.equals(this, other)
    
namespace execom.iog.name, (exports) ->
  class exports.TypeProxy
    constructor: (@nameOfType, @genericArgumentsName = []) ->

    GetType: () ->
      type = exports.IOGType.FindType(@nameOfType)
      type = exports.IOGType.CopyConstructor(type)
      type.ClearGenericArguments()

      for argument in @genericArgumentsName
        type.genericArguments.push(argument.GetType())
      return type
    

  exports.BooleanType = new exports.IOGType(UUID.create(), 
    exports.ScalarName.Boolean, false, false, false, false, false, null, [])
  exports.StringType = new exports.IOGType(UUID.create(), 
    exports.ScalarName.String, false, false, false, false, false, null, [])
  exports.Int32Type = new exports.IOGType(UUID.create(), 
    exports.ScalarName.Int32, false, false, false, false, false, null, [])
  exports.Int64Type = new exports.IOGType(UUID.create(), 
    exports.ScalarName.Int64, false, false, false, false, false, null, [])
  exports.DoubleType = new exports.IOGType(UUID.create(), 
    exports.ScalarName.Double, false, false, false, false, false, null, [])
  exports.DateTimeType = new exports.IOGType(UUID.create(), 
    exports.ScalarName.DateTime, false, false, false, false, false, null, [])
  exports.GuidType = new exports.IOGType(UUID.create(), 
    exports.ScalarName.Guid, false, false, false, false, false, null, [])
  exports.TimeSpanType = new exports.IOGType(UUID.create(), 
    exports.ScalarName.TimeSpan, false, false, false, false, false, null, [])
  exports.ByteType = new exports.IOGType(UUID.create(), 
    exports.ScalarName.Byte, false, false, false, false, false, null, [])
  exports.CharType = new exports.IOGType(UUID.create(), 
    exports.ScalarName.Char, false, false, false, false, false, null, [])
  exports.ArrayType = null
  exports.DictionaryType = null
  exports.RootType = null

  exports.types = []
  exports.enums = {}
  exports.typeToEnums = new exports.Dictionary()