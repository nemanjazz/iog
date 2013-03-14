namespace execom.iog.name, (exports) ->
  class exports.CollectionProxy
    # facade - - RuntimeProxyFacade
    # instanceId - - UUID
    # readOnly - - boolean
    constructor: (@__facade__, @__instanceId__, @__readOnly__, @type) ->
      @elementTypeId = exports.StaticProxyFacade.get().GetTypeId(@type)
      @genericArgumentType = @type.genericArguments[0]
      @genericArgumentTypeId = exports.StaticProxyFacade.get().
        GetTypeId(@genericArgumentType)
      @isScalar = exports.StaticProxyFacade.get().
        IsScalarType(@genericArgumentTypeId)

    ###
      This method is checking if key is scalara if it is of right type, and
      if it is not try to make it of right type. Also checking if proxy is
      proxy for rigth type. Return valid value.
    ###
    CheckValue: (value) ->
      valueType = @genericArgumentType
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

    Get: (index) ->
      if(exports.UTILS.IsNumber(index))
        enumerator = @__facade__.CollectionGetEnumerator(@genericArgumentTypeId, 
          @__instanceId__, @isScalar, @__readOnly__)
        i = 0
        while(i <= index)
          i += 1
          enumerator.MoveNext()

        return enumerator.Current()
      else
        return null;

    # item - - type
    Add: (item) ->
      if(@__readOnly__)
        throw "Operation not allowed for read only collection"
      item = this.CheckValue(item)
      @__facade__.CollectionAdd(@__instanceId__, 
        @genericArgumentTypeId, item, @isScalar)

    Clear: () ->
      if(@__readOnly__)
        throw "Operation not allowed for read only collection"

      @__facade__.CollectionClear(@__instanceId__)

    # item - - should be of type @type
    Contains: (item) ->
      item = this.CheckValue(item)
      return @__facade__.CollectionContains(@__instanceId__, item, @isScalar, 
        @__readOnly__)

    # array - - array of elements of type @type
    # arrayIndex - - int
    CopyTo: ( array, arrayIndex) ->
      @__facade__.CollectionCopyTo(Guid.EMPTY, @__instanceId__, @isScalar,
        @__readOnly__, array, arrayIndex)
      return

    Count: () ->
      @__facade__.CollectionCount(@__instanceId__)

    IsReadOnly: () ->
      retunr @__readOnly__

    # item - - item should be of type @type
    Remove: (item) ->
      if(@__readOnly__)
        throw "Operation not allowed for read only collection"
      item = this.CheckValue(item)
      return @__facade__.CollectionRemove(@__instanceId__, item, @isScalar)

    GetEnumerator: () ->
      return @__facade__.CollectionGetEnumerator(@genericArgumentTypeId, 
        @__instanceId__, @isScalar, @__readOnly__)

    # key - - object
    TryFindPrimaryKey: (key) ->
      rez = @__facade__.CollectionTryFindPrimaryKey(
        @genericArgumentTypeId, @__instanceId__,
        @isScalar, @IsReadOnly(), key)
      return rez.result

    # <return>element of type @type</return>
    FindByPrimaryKey: (key) ->
      return @__facade__.CollectionFindByPrimaryKey(@genericArgumentTypeId, 
        @__instanceId__, @isScalar, @IsReadOnly(), key)

    ContainsPrimaryKey: (key) ->
      return @__facade__.CollectionContainsPrimaryKey(@__instanceId__, key,
        @__readOnly__)