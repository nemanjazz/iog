namespace execom.iog.name, (exports) ->
  class exports.StaticProxyFacade
    _instance = undefined

    @get: () -> # Must be a static method
      if(_instance == undefined or _instance == null)
        throw "ProxyFacade not initialized"
      #_instance = new _StaticProxyFacade
      return _instance

    @Initialize: (typesService) ->
      if (typesService == null)
        throw "typesService"

      inst = new _StaticProxyFacade()

      inst.typesService = typesService

      _instance = inst

  class _StaticProxyFacade
    constructor: () ->
      @typesService = null

    GetTypeId: (type) ->
      return @typesService.GetTypeIdCached(type)

    GetTypeMemberId: (typeId, propertyName) ->
      return @typesService.GetTypeMemberId(typeId, propertyName)

    IsScalarType: (typeId) ->
      return @typesService.IsScalarType(typeId)

    IsScalarMember: (memberId) ->
      typeId = @typesService.GetMemberTypeId(memberId);
      return @typesService.IsScalarType(typeId)

    GetTypePrimaryKeyMemberId: (typeId) ->
      return @typesService.GetTypePrimaryKeyMemberId(typeId)

    AreEqual: (proxy1, proxy2) ->
      if (proxy2 == null)
        return false

      if (not exports.UTILS.HasItemId(proxy2))
        return false;

      return exports.UTILS.equals(exports.UTILS.GetItemId(proxy1), 
        exports.UTILS.GetItemId(proxy2))

    GetProxyHashCode: (instance) ->
      return exports.UTILS.GetItemId(instance).GetHashCode()