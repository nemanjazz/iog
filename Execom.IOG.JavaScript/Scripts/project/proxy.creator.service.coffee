namespace execom.iog.name, (exports) ->
  class exports.ProxyCreatorService
  # Creates new instance of ProxyCreatorService class
  # types - List of types to register - Array
  # interfaceToTypeIdMapping - Mapping to registered type id - Dictionary
  # interfaceToGeneratedMapping - Mapping to generated proxy types -  Dictionary
    constructor: (types = [], interfaceToTypeIdMapping = new exports.Dictionary(), interfaceToGeneratedMapping = new exports.Dictionary()) ->
      @proxyTypesFromInterfaces = new exports.Dictionary()
      @proxyTypesFromIDs = new exports.Dictionary()
      @typeIdToType = new exports.Dictionary()
      for type in types
        if type.isInterface
          this.RegisterTypeMapping(type, 
            interfaceToGeneratedMapping.Get(type), 
            interfaceToTypeIdMapping.Get(type))

    # Creates new proxy instance for a given instance id
    # facade - Runtime facade visible to proxy - RuntimeProxyFacade
    # instanceId - Instance ID - UUID
    # readOnly - Defines if proxy should be read only - bool
    # type - type of object that need to be created - Type
    # <returns>Instance of the proxy</returns>
    NewObject: (facade, instanceId, readOnly, type, typeId) ->

      if(typeId? and not exports.UTILS.equals(typeId, exports.Guid.EMPTY))
        proxyType = @proxyTypesFromIDs.Get(typeId)
        type = @typeIdToType.Get(typeId)
        if(proxyType.hasOwnProperty('Initializer'))
          proxyType.Initializer(proxyType.type.id)
        return new proxyType(facade, instanceId, readOnly, type)
      else
        proxyType = @proxyTypesFromInterfaces.Get(type)
        if(proxyType.hasOwnProperty('Initializer'))
          proxyType.Initializer(proxyType.type.id)
        return new proxyType(facade, instanceId, readOnly, type)

    # Registers mapping between interface type and proxy type
    # interfaceType - Type of the entity interface - Type
    # proxyType - Type of generated proxy - Type
    # typeId - Type id in data - UUID
    RegisterTypeMapping: ( interfaceType, proxyType, typeId) ->
      if (not interfaceType.isInterface)
        throw "Interface type expected : " + interfaceType.name

      @proxyTypesFromIDs.Add(typeId, proxyType);
      @proxyTypesFromInterfaces.Add(interfaceType, proxyType)
      @typeIdToType.Add(typeId, interfaceType)