namespace execom.iog.name, (exports) ->
  class exports.CollectionEnumerator
  #
  # elementType - -  UUID
  # edgeEnumerator - - Enumerator
  # isScalar - - boolean
  # isReadOnly - - boolean
  # objectInstancesService - - ObjectInstancesService
  # proxyFacade - - RuntimeProxyFacade
    constructor: (@elementType, @edgeEnumerator, @isScalar, @isReadOnly,
      @objectInstancesService, @proxyFacade) ->

    Current: () ->
      if(@isScalar)
        currentValue = @objectInstancesService.GetScalarInstanceValue(
          @edgeEnumerator.Current().toNodeId)
        if(currentValue?)
          return currentValue.value
        else
          return currentValue
      else
        return @proxyFacade.GetProxyInstance(@isReadOnly,
          @edgeEnumerator.Current().toNodeId, @elementType)

    MoveNext: () ->
      return @edgeEnumerator.MoveNext()

    Reset: () ->
      return @edgeEnumerator.Reset()

  class exports.DictionaryEnumerator
    constructor: (@elementType, @edgeEnumerator, @isScalar, @isReadOnly,
      @objectInstancesService, @proxyFacade, @keyIsScalar) ->

    Current: () ->
      if(@isScalar)
        value = @objectInstancesService.GetScalarInstanceValue(
          @edgeEnumerator.Current().toNodeId)
        if(value?)
          value = value.value
        else
          value = null
      else
        value = @proxyFacade.GetProxyInstance(@isReadOnly, 
          @edgeEnumerator.Current().toNodeId, @elementType)

      if(@keyIsScalar)
        key = @edgeEnumerator.Current().data.data.value
      else
        key = @edgeEnumerator.Current().data.data

      return {
        "key": key
        "value": value
      }

    MoveNext: () ->
      return @edgeEnumerator.MoveNext()

    Reset: () ->
      return @edgeEnumerator.Reset()

  class exports.RuntimeProxyFacade

    constructor: (@typesService, @objectInstancesService, @immutableInstancesService, @collectionInstancesService, @immutableCollectionInstancesService, @dictionaryInstancesService,@immutableDictionaryInstancesService, @mutableProxyMap, @immutableProxyMap, @proxyCreatorService) ->
    
    CreateCollection: (type) ->
      newType = iog.IOGType.CreateCollectionWithGenericType(
        type.genericArguments[0].GetType())
      typeId = @typesService.GetTypeIdCached(newType)
      if(@typesService.IsCollectionType(typeId))
        instanceId = @collectionInstancesService.NewInstance(typeId)
        proxy = @proxyCreatorService.NewObject(this, 
          instanceId, false, newType)
        @mutableProxyMap.AddProxy(instanceId, proxy)
        return proxy;
      else 
        return null
        
      CreateDictionary: (type) ->
        newType = iog.IOGType.CreateDictionaryWithGenericTypes(
          type.genericArguments[0].GetType(), type.genericArguments[1].GetType())
        typeId = @typesService.GetTypeIdCached(newType)
        if(@typesService.IsDictionaryType(typeId))
          instanceId = @dictionaryInstancesService.NewInstance(typeId)
          proxy = @proxyCreatorService.NewObject(this, 
            instanceId, false, newType)
          @mutableProxyMap.AddProxy(instanceId, proxy)
          return proxy;
        else 
          return null
    
      # Returns value of a property:
      # - If property is scalar, returns the scalar value
      # - If property is not scalar, returns the proxy object
      # instanceId - Instance ID - UUID
      # memberId - Member ID - UUID
      # isScalar - Determines if member is scalar - bool
      # isReadOnly - Determines if instance is read only - bool
      # <return>Scalar value, or a proxy</return>
    GetInstanceMemberValue: (instanceId, memberId, isScalar, isReadOnly) ->
      if(exports.UTILS.equals(instanceId, exports.Guid.EMPTY))
        throw "Instance was accessed outside of workspace scope or it was rolled back."

      if(isScalar)
        if(isReadOnly)
          return @immutableInstancesService.GetScalarInstanceMember(instanceId,
            memberId)
        else
          return @objectInstancesService.GetScalarInstanceMember(instanceId, 
            memberId)
      else
        isPermanent = false
        referenceId = exports.Guid.EMPTY

        if (isReadOnly)
          #Get ID of referenced instance
          rez = @immutableInstancesService.GetReferenceInstanceMember(
            instanceId, memberId)
          referenceId = rez.guid
        else
          # Get ID of referenced instance
          rez = @objectInstancesService.GetReferenceInstanceMember(
            instanceId, memberId)
          referenceId = rez.guid

        return this.GetProxyInstance(isReadOnly or rez.isPermanet, 
          referenceId, exports.Guid.EMPTY)

    GetProxyInstance: (isReadOnly, referenceId, typeId) ->
      if( exports.UTILS.equals(referenceId, exports.Constants.NullReferenceNodeId))
        return null
      else
        proxy = null
        map = @mutableProxyMap
        collectionService = @collectionInstancesService
        objectService = @objectInstancesService

        if(isReadOnly)
          map = @immutableProxyMap
          collectionService = @immutableCollectionInstancesService
          objectService = @immutableInstancesService

        rez = map.TryGetProxy(referenceId)
        if (not rez.result)
          if (exports.UTILS.equals(typeId, exports.Guid.EMPTY))
            if (collectionService.IsCollectionInstance(referenceId))
              typeId = collectionService.GetInstanceTypeId(referenceId)
            else
              typeId = objectService.GetInstanceTypeId(referenceId)
          # Create proxy object
          proxy = @proxyCreatorService.NewObject(this, referenceId, isReadOnly, 
            null, typeId)
          # Add to proxy map
          map.AddProxy(referenceId, proxy)
          return proxy
        else
          return rez.value

    SetInstanceMemberValue: (instanceId, memberId, value, isScalar, isReadOnly) ->
      if(exports.UTILS.equals(instanceId, exports.Guid.EMPTY))
        throw "Instance was accessed outside of workspace scope or it was rolled back."

      if(isReadOnly)
        throw "Setting property on read only instance not allowed"

      if(isScalar)
        @objectInstancesService.SetScalarInstanceMember(instanceId, memberId, value)
      else
        referenceId = exports.Constants.NullReferenceNodeId;

        if (value != null)
          if (!exports.UTILS.HasItemId(value))
            throw "Object set is not a valid IOG proxy"

          referenceId = exports.UTILS.GetItemId(value)

        @objectInstancesService.SetReferenceInstanceMember(instanceId, memberId, 
          referenceId)

    CollectionAdd: (instanceId, valueTypeId, value, isScalar) ->
      this.PerformAdd(instanceId, valueTypeId, value, isScalar, false, false)

    CollectionAddOrdered: (instanceId, valueTypeId, value, isScalar) ->
      this.PerformAdd(instanceId, valueTypeId, value, isScalar, false, true)

    PerformAdd: (instanceId, valueTypeId, value, isScalar, isSet, isOrdered) ->
      if(exports.UTILS.equals(instanceId, exports.Guid.EMPTY))
        throw "Instance was accessed outside of workspace scope or it was rolled back."

      if(isScalar)
        if(not isSet)
          if(isOrdered)
            maxId = @collectionInstancesService.MaxOrderedIdentifier(instanceId)
            @collectionInstancesService.AddScalar(instanceId, valueTypeId, value,
              maxId + 1)
          else
            @collectionInstancesService.AddScalar(instanceId, valueTypeId, value)
        else
          @collectionInstancesService.AddScalar(instanceId, valueTypeId, value, 
            value)
      else
        referenceId = exports.Constants.NullReferenceNodeId
        primaryKey = null

        if(value?)
          if (not exports.UTILS.HasItemId(value))
            throw "Object set is not a valid IOG proxy"

          referenceId = exports.UTILS.GetItemId(value)
          primaryKeyId = exports.UTILS.GetItemPrimaryKeyId(value)

          if(not exports.UTILS.equals(primaryKeyId, exports.Guid.EMPTY))
            primaryKey = @objectInstancesService.GetScalarInstanceMember(
              referenceId, primaryKeyId)

          if(isOrdered)
            maxId = @collectionInstancesService.MaxOrderedIdentifier(instanceId)
            @collectionInstancesService.AddReference(instanceId, referenceId,
              maxId + 1)
          else
            if(not primaryKey?)
              @collectionInstancesService.AddReference(instanceId, referenceId)
            else
              @collectionInstancesService.AddReference(instanceId, referenceId,
                primaryKey)

    CollectionClear: (instanceId) ->
      if (exports.UTILS.equals(instanceId, exports.Guid.EMPTY))
        throw "Instance was accessed outside of workspace scope or it was rolled back."

      @collectionInstancesService.Clear(instanceId)
      return

    CollectionContains: ( instanceId, value, isScalar, isReadOnly) ->
      return this.PerformContains(instanceId, value, isScalar, false,
        isReadOnly, false)

    CollectionContainsOrdered: (instanceId, value, isScalar, isReadOnly) ->
      return this.PerformContains(instanceId, value, isScalar, false,
        isReadOnly, true)

    PerformContains: (instanceId, value, isScalar, isSet, isReadOnly, isOrdered) ->
      if (exports.UTILS.equals(instanceId, exports.Guid.EMPTY))
        throw "Instance was accessed outside of workspace scope or it was rolled back."

      service = @collectionInstancesService

      if (isReadOnly)
        service = @immutableCollectionInstancesService

      if(isScalar)
        if(not isSet)
          return service.ContainsScalar(instanceId, value)
        else
          return service.ContainsScalar(instanceId, value, value)
      else
        referenceId = Constants.NullReferenceNodeId
        primaryKey = null
        if (value?)
          if (not exports.UTILS.HasItemId(value))
            throw "Object set is not a valid IOG proxy"

          referenceId = exports.UTILS.GetItemId(value)
          primaryKeyId = exports.UTILS.GetItemPrimaryKeyId(value)

          if (not exports.UTILS.equals(primaryKeyId, exports.Guid.EMPTY))
            primaryKey = @objectInstancesService.GetScalarInstanceMember(
              referenceId, primaryKeyId)

        if (primaryKey == null or isOrdered)
          return service.ContainsReference(instanceId, referenceId)
        else
          return service.ContainsReference(instanceId, referenceId, primaryKey)

    CollectionCopyTo: (elementTypeId, instanceId, isScalar, isReadOnly, array, arrayIndex) ->
      if(exports.UTILS.equals(instanceId, exports.Guid.EMPTY))
        throw "Instance was accessed outside of workspace scope or it was rolled back."
      #TODO check if here is missing type
      enumerator = this.CollectionGetEnumerator(elementTypeId, instanceId, 
        isScalar, isReadOnly)
      index = arrayIndex

      while (enumerator.MoveNext())
        array.SetValue(enumerator.Current, index)
        index++

    CollectionCount: (instanceId) ->
      if (exports.UTILS.equals(instanceId, exports.Guid.EMPTY))
        throw "Instance was accessed outside of workspace scope or it was rolled back."

      return this.collectionInstancesService.Count(instanceId)

    CollectionRemove: (instanceId, value, isScalar) ->
      return this.PerformRemove(instanceId, value, isScalar, false, false)

    CollectionRemoveOrdered: (instanceId, value, isScalar) ->
      return this.PerformRemove(instanceId, value, isScalar, false, true)

    PerformRemove: (instanceId, value, isScalar, isSet, isOrdered) ->

      if(exports.UTILS.equals(instanceId, exports.Guid.EMPTY))
        throw "Invalid instance revision ID"

      if(isScalar)
        if(not isSet)
          return @collectionInstancesService.RemoveScalar(instanceId, value)
        else
          return @collectionInstancesService.RemoveScalar(instanceId, value,
            value)
      else
        referenceId = exports.Constants.NullReferenceNodeId;
        primaryKey = null;

        if (value != null)
          if (not exports.UTILS.HasItemId(value))
            throw "Object set is not a valid IOG proxy"

          referenceId = exports.UTILS.GetItemId(value);

          primaryKeyId = exports.UTILS.GetItemPrimaryKeyId(value);

          if (not exports.UTILS.equals(primaryKeyId, exports.Guid.EMPTY))
            primaryKey = @objectInstancesService.GetScalarInstanceMember(
              referenceId, primaryKeyId)


        # Ordered collection ignores primary key
        if (primaryKey == null or isOrdered)
          return @collectionInstancesService.RemoveReference(instanceId, 
            referenceId)
        else
          return @collectionInstancesService.RemoveReference(instanceId,
            referenceId, primaryKey)

    IsScalarType: (typeId) ->
      return @typesService.IsScalarType(typeId)

    GetTypeId: (type) ->
      return @typesService.GetTypeId(type)

    # elementType - - UUID
    # instanceId - - UUID
    # isScalar - - boolean
    # isReadOnly - - boolean
    CollectionGetEnumerator: (elementType, instanceId, isScalar, isReadOnly) ->
      if(isReadOnly)
        var1 = @immutableCollectionInstancesService.GetEnumerator(instanceId)
      else
        var1 = @collectionInstancesService.GetEnumerator(instanceId)

      if(isReadOnly)
        var2 = @immutableInstancesService
      else
        var2 = @objectInstancesService

      enumeration = new exports.CollectionEnumerator(elementType, var1 , 
        isScalar, isReadOnly, var2, this)

      return enumeration
    # elementType - - UUID
    # instanceId - - UUID
    # isScalar - - boolean
    # isReadOnly - - boolean
    # key - - object
    # <return>result that is boolean, value object value</return>
    CollectionTryFindPrimaryKey: (elementType, instanceId, isScalar, isReadOnly, key) ->
      referenceId = exports.Guid.EMPTY
      rez = @collectionInstancesService.TryFindReferenceByKey(instanceId, key)
      if(rez.result)
        value = this.GetProxyInstance(isReadOnly, referenceId, elementType)
        return {
        "result": true
        "value": value
        }
      else
        return {
        "result": false
        "value": exports.UTILS.Default(value)
        }
    # elementType - - UUID
    # instanceId - - UUID
    # isScalar - -  boolean
    # isReadOnly - - boolean
    # key - - object
    CollectionFindByPrimaryKey: (elementType, instanceId, isScalar, isReadOnly, key) ->
      referenceId = exports.Guid.EMPTY
      rez = @collectionInstancesService.TryFindReferenceByKey(instanceId, key)
      if (rez.result)
        return this.GetProxyInstance(isReadOnly, rez.value, elementType);
      else
        throw "Key not found"

    CollectionContainsPrimaryKey: (instanceId, key, isReadOnly) ->
      referenceId = exports.Guid.EMPTY
      if (isReadOnly)
        return @immutableCollectionInstancesService.TryFindReferenceByKey(
          instanceId, key).result
      else
        return @collectionInstancesService.TryFindReferenceByKey(instanceId, 
          key).result

    DictionaryAdd: (instanceId, elementType, isScalar, key, value) ->
      if (exports.UTILS.equals(instanceId, exports.Guid.EMPTY))
        throw "Instance was accessed outside of workspace scope or it was rolled back."

      if(isScalar)
        @dictionaryInstancesService.AddScalar(instanceId, elementType, key, value)
      else
        referenceId = exports.Constants.NullReferenceNodeId;

        if (value != null)

          if (not exports.UTILS.HasItemId(value))
            throw "Object set is not a valid IOG proxy"

          referenceId = exports.UTILS.GetItemId(value)

        @dictionaryInstancesService.AddReference(instanceId, key, referenceId)

    DictionaryContainsKey: (instanceId, key,  readOnly) ->
      if(readOnly)
        return @immutableDictionaryInstancesService.Contains(instanceId, key)
      else
        return @dictionaryInstancesService.ContainsKey(instanceId, key)

    DictionaryKeys: (instanceId, readOnly) ->
      service = @dictionaryInstancesService

      if(readOnly)
        service = @immutableDictionaryInstancesService

      keys = []

      enumerator = service.GetEnumerator(instanceId)

      while (enumerator.MoveNext())
        keys.Add(enumerator.Current().data.data)

      return keys

    DictionaryRemove: (instanceId, key) ->
      @dictionaryInstancesService.Remove(instanceId, key)

    # <return>result boolean, value object</return>
    DictionaryTryGetValue: (instanceId, elementTypeId, isSclar, readOnly, key, type) ->
      if (isSclar)
        rez = @dictionaryInstancesService.TryGetScalar(instanceId, key)
        return rez
      else
        referenceId = exports.Guid.EMPTY
        value = exports.UTILS.Default(type);

        rez = @dictionaryInstancesService.TryGetReference(instanceId, key)
        if ( rez.result)
          value =  @GetProxyInstance(readOnly, rez.value, elementTypeId)
          return {
          "result": true
          "value": value
          }
        else
          return {
          "result": false
          "value": null
          }

    DictionaryValues: (instanceId, elementTypeId, isSclar, readOnly) ->

      values = []

      enumerator = @dictionaryInstancesService.GetEnumerator(instanceId)

      while (enumerator.MoveNext())
        values.Add(@DictionaryGetValue(instanceId, elementTypeId, isSclar, 
          readOnly, enumerator.Current().data.data))

      return values

    DictionaryGetValue: (instanceId, elementTypeId, isSclar, readOnly, key) ->
      if(isSclar)
        rez = @dictionaryInstancesService.TryGetScalar(instanceId, key)
        scalarValue = null
        if(rez.result)
          return rez.value
        else
          throw "Element not found with given key"
      else
        referenceId = exports.Guid.EMPTY
        rez = @dictionaryInstancesService.TryGetReference(instanceId, key)
        if (rez.result)
          return @GetProxyInstance(readOnly, rez.value, elementTypeId)
        else
          throw "Element not found with given key"

    DictionarySetValue: (instanceId, elementTypeId, isSclar, readOnly, key, value) ->
      if(isSclar)
        @dictionaryInstancesService.SetScalar(instanceId, elementTypeId, key,
          value)
      else
        if (not exports.UTILS.HasItemId(value))
          throw "Object set is not a valid IOG proxy"

        @dictionaryInstancesService.SetReference(instanceId, key, 
          exports.UTILS.GetItemId(value))

    DictionaryClear: (instanceId) ->
      @dictionaryInstancesService.Clear(instanceId)
      return

    # item - - KeyValuePair
    DictionaryContains: (instanceId, item, readOnly) ->
      if(readOnly)
        return @immutableDictionaryInstancesService.ContainsKey(instanceId,
          item.key)
      else
        return @dictionaryInstancesService.ContainsKey(instanceId, item.key)

    # array - - LeyPairArray
    DictionaryCopyTo: (instanceId, elementTypeId, isSclar, readOnly, array,
      arrayIndex) ->
      index = arrayIndex;
      enumerator = @dictionaryInstancesService.GetEnumerator(instanceId)

      while(enumerator.MoveNext())
        key = enumerator.Current().data.data;
        array[arrayIndex] = {
        "key": key,
        "value": @DictionaryGetValue(instanceId, elementTypeId, isSclar,
          readOnly, key)
        }

        arrayIndex++
        return

    DictionaryCount: (instanceId, readOnly) ->
      if(readOnly)
        return @immutableDictionaryInstancesService.Count(instanceId)
      else
        return @dictionaryInstancesService.Count(instanceId)

    # item - - KeyValuePair
    DictionaryRemove: (instanceId, item) ->
      return @dictionaryInstancesService.Remove(instanceId, item.key)

    DictionaryGetEnumerator: (instanceId, elementTypeId, isSclar, readOnly,
        keyIsScalar) ->
      if(readOnly)
        val1 = @immutableDictionaryInstancesService.GetEnumerator(instanceId)
      else
        val1 = @dictionaryInstancesService.GetEnumerator(instanceId)

      if(readOnly)
        val2 = @immutableInstancesService
      else
        val2 = @objectInstancesService

      return new exports.DictionaryEnumerator(elementTypeId, val1, 
        isSclar, readOnly, val2, this, keyIsScalar)

    SetAdd: (instanceId, itemTypeId,  item, isScalar) ->
      @PerformAdd(instanceId, itemTypeId, item, isScalar, true, false)
      return

    SetContains: (instanceId, item, isScalar, isReadOnly) ->
      return @PerformContains(instanceId, item, isScalar, true, isReadOnly,
        false)

    SetRemove: (instanceId, item, isScalar) ->
      return @PerformRemove(instanceId, item, isScalar, true, false)