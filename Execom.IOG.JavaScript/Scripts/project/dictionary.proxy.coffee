namespace execom.iog.name, (exports) ->
  class exports.DictionaryProxy

    @VALUE = 'value'
    @KEY = 'key'

    constructor: (@__facade__, @__instanceId__, @__readOnly__, @type) ->
      @elementTypeId = exports.StaticProxyFacade.get().GetTypeId(@type)
      @isScalar = exports.StaticProxyFacade.get().IsScalarType(@elementTypeId)
      if(@type.genericArguments[0] != null)
        @keyIsScalar = @type.genericArguments[0].isScalar
      else
        @keyIsScalar = false

    First: () ->
      enumerator = this.GetEnumerator()

      hasFirst = enumerator.MoveNext()

      if(hasFirst)
        return enumerator.Current()
      else
        return null;

    Last: () ->
      enumerator = this.GetEnumerator()
      size = this.Count()
      i = 0
      while(i < size)
        i+= 1
        enumerator.MoveNext()
      return enumerator.Current()


    Add: (key, value) ->
      newKey = null
      newValue = null
      if (@__readOnly__)
        throw "Operation not allowed for read only dictionary"
      if(not value? and key.hasOwnProperty(DictionaryProxy.KEY) and  key.hasOwnProperty(DictionaryProxy.VALUE))
        value = key[DictionaryProxy.VALUE]
        key = key[DictionaryProxy.KEY]

      newKey = this.CheckKey(key)
      newValue = this.CheckValue(value)

      @__facade__.DictionaryAdd(@__instanceId__, exports.Guid.EMPTY, @isScalar,
        newKey, newValue)

      return

    ###
      This method is checking if key is scalara if it is of right type, and
      if it is not try to make it of right type. Also checking if proxy is
      proxy for rigth type. Return valid key.
    ###
    CheckKey: (key) ->
      keyType = @type.genericArguments[0]
      if(keyType.isScalar )
        #checking if key is of right type
        if(exports.UTILS.IsInstaceOfScalar(key, keyType))
          newKey = key
        else
          newKey = exports.IOGType.CreateScalar(keyType, key)
      else
        newKey = key
        #TODO checki if proxy is of right type
      return newKey

    ###
      This method is checking if key is scalara if it is of right type, and
      if it is not try to make it of right type. Also checking if proxy is
      proxy for rigth type. Return valid value.
    ###
    CheckValue: (value) ->
      if(valueType.isScalar)
        #checking if value is of right type
        if(exports.UTILS.IsInstaceOfScalar(value, valueType))
          newValue = value
        else
          newValue = exports.IOGType.CreateScalar(valueType, value)
      else
        newValue = value
        #TODO checki if proxy is of right type
      return newValue

    ContainsKey: (key) ->
      newKey = this.CheckKey(key)
      return @__facade__.DictionaryContainsKey(@__instanceId__, newKey, @__readOnly__)

    Keys: () ->
      return @__facade__.DictionaryKeys(@__instanceId__, @__readOnly__)

    Remove: (key) ->
      if (__readOnly__)
        throw "Operation not allowed for read only dictionary"

      newKey = this.CheckKey(key)
      return @__facade__.DictionaryRemove(@__instanceId__, newKey)

    # <return>result - - boolean, value - value for given key - @type</return>
    TryGetValue: (key) ->
      newKey = this.CheckKey(key)
      valueType = @type.genericArguments[1]

      returnValue = @__facade__.DictionaryTryGetValue(@__instanceId__, 
        exports.StaticProxyFacade.get().GetTypeId(@type.genericArguments[1]), 
        @isScalar, @__readOnly__, newKey)

      if(valueType.isScalar)
        result = {}
        result[DictionaryProxySealed.RESULT] = returnValue.result
        result[DictionaryProxySealed.VALUE] = returnValue.value.value
        return result
      else
        return returnValue

    Values: () ->
      return @__facade__.DictionaryValues(@__instanceId__, 
        exports.StaticProxyFacade.get().GetTypeId(@type.genericArguments[1]),
        @isScalar, @__readOnly__)

    Get: (key) ->
      newKey = this.CheckKey(key)
      valueType = @type.genericArguments[1]

      returnValue = @__facade__.DictionaryGetValue(@__instanceId__, 
        exports.StaticProxyFacade.get().GetTypeId(@type.genericArguments[1]), 
        @isScalar, @__readOnly__, newKey)

      if(valueType.isScalar)
        return returnValue.value
      else
        return returnValue

    Set: (key, value) ->
      if(@__readOnly__)
        throw "Operation not allowed for read only dictionary"
      newKey = this.CheckKey(key)
      newValue = this.CheckValue(value)
      @__facade__.DictionarySetValue(@__instanceId__, 
        exports.StaticProxyFacade.get().GetTypeId(@type.genericArguments[1]), 
        @isScalar, @__readOnly__, newKey, newValue)

    Clear: () ->
      if (__readOnly__)
        throw "Operation not allowed for read only dictionary"
      @__facade__.DictionaryClear(@__instanceId__)
      return

    # item - - is javascript object that has properties key and value
    Contains: (item) ->
      return @__facade__.DictionaryContains(@__instanceId__, item, @__readOnly__)

    # array is array of javascripts objects, where every object h
    # as properties key and value
    CopyTo: (array, arrayIndex) ->
      @__facade__.DictionaryCopyTo(@__instanceId__, 
        exports.StaticProxyFacade.get().GetTypeId(@type.genericArguments[1]),
        @isScalar, @__readOnly__, array, arrayIndex);
      return

    Count: () ->
      return @__facade__.DictionaryCount(@__instanceId__, @__readOnly__)

    IsReadOnly: () ->
      return @__readOnly__

    # item - - javascript object that has properties key and  value
    Remove: (item) ->
      if (@__readOnly__)
        throw "Operation not allowed for read only dictionary"
      return @__facade__.DictionaryRemove(@__instanceId__, item)

    GetEnumerator: () ->
      return @__facade__.DictionaryGetEnumerator(@__instanceId__,
          exports.StaticProxyFacade.get().GetTypeId(@type.genericArguments[1]), 
          @isScalar, @readOnly, @keyIsScalar)