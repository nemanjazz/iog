namespace execom.iog.name, (exports) ->
  class exports.ProxyBuilder
    @IOG_TYPE = "iogType"
    constructor: () ->
    
    @GenerateClass: (classname) ->
      return class
        constructor: (@__facade__, @__instanceId__, @__readOnly__) ->
    
    @Generate: (className, properties, type) ->
      C = @GenerateClass(className)
      C.prototype["Equals"] = -> return exports.StaticProxyFacade.get().AreEqual(this, obj)
      C.prototype["GetHashCode"] = -> return exports.StaticProxyFacade.get().GetProxyHashCode(this)

      C.primaryKeyMemberId = null
      C.propertyIdName = null
      C.propertyIsScalarName = null
      C.typeId = null
      C.type = type
      
      # Method is used for making getter for field
      # 
      # proxy - is proxy that we are making
      # return Get function for given proxy
      C["GetBuilder"] = (proxy, propID, propScalar, propType) ->
          memberId = propID
          scalar = propScalar
          if(scalar == false)
            () -> 
              value = this.__facade__.GetInstanceMemberValue(this.__instanceId__, 
                  new exports.Guid(memberId), scalar, this.__readOnly__)
              if(value?)
                return value

              if(value == null && (exports.UTILS.IsCollectionType(propType)))
                # if there is not collection create one
                newCollection = this.__facade__.CreateCollection(propType)
                # set field with new collection
                this.__facade__.SetInstanceMemberValue(this.__instanceId__, 
                  new exports.Guid(memberId), newCollection, 
                  scalar, this.__readOnly__)
                # return value
                return newCollection
                  
              if(value == null && (exports.UTILS.IsDictionaryType(propType)))
                # if there is not dictionary create one
                newDictionary = this.__facade__.CreateDictionary(propType)
                # set field with new collection
                this.__facade__.SetInstanceMemberValue(this.__instanceId__, 
                  new exports.Guid(memberId), newDictionary, 
                  scalar, this.__readOnly__)
                # return value
                return newDictionary
              return value
          else
            if(exports.UTILS.equals(propType, exports.DateTimeType) ||
                exports.UTILS.equals(propType, exports.TimeSpanType))
              return () ->
                return this.__facade__.GetInstanceMemberValue(
                  this.__instanceId__, new exports.Guid(memberId), 
                  scalar,  this.__readOnly__)
            else
              return () ->
                value = this.__facade__.GetInstanceMemberValue(
                  this.__instanceId__, new exports.Guid(memberId), scalar, 
                  this.__readOnly__)
                if(value == null) 
                  return null
                else 
                  return value.value
      
      C["SetBuilder"] = (proxy, propID, propScalar, propType) ->
        memberId = propID
        scalar = propScalar
        if(scalar == true)
          return (value) ->
            this.__facade__.SetInstanceMemberValue(this.__instanceId__, 
              new exports.Guid(memberId), 
              exports.IOGType.CreateScalar( exports.UTILS.GetType(propType.name)
                , value), scalar, this.__readOnly__)
        else
          return (value) ->
            this.__facade__.SetInstanceMemberValue(this.__instanceId__, 
              new exports.Guid(memberId), value, scalar, this.__readOnly__)
              
      C["Initializer"] = () -> 
        
        C.typeId = type.id
        proxyFacade = exports.StaticProxyFacade.get()
        C.typeId = proxyFacade.typesService.GetTypeId(C.type)
        C.primaryKeyMemberId = proxyFacade.GetTypePrimaryKeyMemberId(C.typeId)

        #making static fields for every propertie

        for prop in properties
          if(exports.ProxyBuilder.IsRevisionIdProperty(prop))
            throw "Not implemented!"
          else
            if(exports.ProxyBuilder.IsDataProperty(prop))

              C[prop.name + exports.Constants.PropertyMemberIdSufix] = 
                proxyFacade.GetTypeMemberId(C.typeId, new 
                exports.IOGString(prop.name))
              C[prop.name + exports.Constants.PropertyIsScalarSufix] = 
                proxyFacade.IsScalarMember(C[prop.name + 
                  exports.Constants.PropertyMemberIdSufix])

              propMemberIdSufix =
                C[prop.name + exports.Constants.PropertyMemberIdSufix].value
              propMemberIsScalarSufix = 
                C[prop.name + exports.Constants.PropertyIsScalarSufix]

              propType = prop.GetPropertyType()
              C.prototype["Get" + prop.name] = 
                C.GetBuilder(C, propMemberIdSufix,
                  propMemberIsScalarSufix, propType)
              C.prototype["Set" + prop.name] =
                C.SetBuilder(C, propMemberIdSufix,
                  propMemberIsScalarSufix, propType)
        C.prototype["equals"] = (object) ->
          return exports.StaticProxyFacade.get().AreEqual(this, obj)

        C.prototype["GetHashCode"] = ->
          return exports.StaticProxyFacade.get().GetProxyHashCode(this)

      return C
    
    

    @IsRevisionIdProperty: (propertyInfo) ->
      return propertyInfo.canRead and not propertyInfo.canWrite and propertyInfo.GetCustomAttributes("RevisionIdAttribute") and exports.UTILS.equals(propertyInfo.GetPropertyType(),exports.GuidType);

    @IsDataProperty: (propertyInfo) ->
      return propertyInfo.canRead and propertyInfo.canWrite


  class exports.TypeBuilder
    constructor: () ->

    @DefineType: (typeName) ->
      return new exports.Type(exports.Guid.Create(), typeName, false, false, false, false, false, null, [])

