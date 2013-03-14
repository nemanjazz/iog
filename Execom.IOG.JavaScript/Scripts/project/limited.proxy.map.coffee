namespace execom.iog.name, (exports) ->
  class exports.LimitedProxyMap
    constructor: (@minElements = 50000, @maxElements = 100000) ->
      @table = new exports.SortedList()

    # Add new proxy instance to map
    # instanceId - Instance ID - UUID
    # proxy - Proxy to add - object
    AddProxy: (instanceId, proxy) ->
      if (proxy == null)
        throw "proxy object is null"

      if (@table.Length() > @maxElements)
        this.Cleanup()

      if (not @table.Contains(instanceId))
        @table.Add(instanceId, new exports.TableElement(proxy, 0))
      else
        (@table[instanceId]).accessCount++

    # Gets a proxy from the map if it exists
    # instanceId - Instance ID - Guid
    # <returns>result - True if proxy was found, else false, value - Proxy result, null if not found - object</returns>proxy -
    TryGetProxy: (instanceId) ->
      if (@table.Contains(instanceId))
        element = @table.Get(instanceId)
        proxy = element.target
        element.accessCount++
        return {
        "result": proxy != null
        "value": proxy
        }
      else
        return {
        "result": false
        "value": null
        }

    Cleanup: () ->
      if (@table.Length() > @minElements)

        int nrToRemove = table.Length() - @minElements

        for key in @table.Keys()
          @table.Remove(key)

          nrToRemove--
          if(nrToRemove == 0)
            break

        for item in @table.Array()
          item.accessCount = 0
    # Upgrades identifiers for existing proxies to new instance versions from the changeset
    # mapping - Change set mapping - Dictionary
    UpgradeProxies: (mapping) ->
      keys = []

      for key in @table.Keys()
        keys.push(key)

      for key in keys
        if mapping.Contains(key)
          element = @table.Get(key)
          reference = element.target
          newKey = mapping.Get(key)
          exports.UTILS.SetItemId(reference, newKey)
          @table.Add(newKey, element)
          @table.Remove(key)

      this.Cleanup()


    # Makes proxies unusable by clearing instance ID
    # instances - Identifiers for instances to be cleared - Array
    InvalidateProxies: (instances) ->
      if(instances?)
        for key in @table.Keys()
          if instances.Contains(key)
            target = (table.Get(key)).target
            if target?
              exports.UTILS.SetItemId(target, Guid.EMPTY)
      else
        for key in @table.Keys()
          target = @table.Get(key).taget
          if target?
            exports.UTILS.SetItemId(target, Guid.Empty)