namespace execom.iog.name, (exports) ->
  class exports.Utils

    @IOG_TYPE = 'iogType'
    @VALUE = 'value'

    constructor: () ->
    
    equals :(first, second) -> 
      other = second
      if first instanceof exports.IOGEnum or second instanceof exports.IOGEnumLiteral
        return first.equals(other)
        
      if second instanceof exports.IOGEnum or second instanceof exports.IOGEnumLiteral
        return second.equals(first)
        
      if(first == undefined || second == undefined)
        return false;
      
      for own key, value of first
        if( other?)
          hasOwnProperty = other.hasOwnProperty(key)
        else
          hasOwnProperty = false
        if not other? or not hasOwnProperty
          return false
        if(typeof(other[key])=='undefined')
          return false

      for own key, value of first
        if (first[key])
          switch typeof(first[key])
            when 'object'
              if (not exports.UTILS.equals(first[key], other[key]))
                return false
            when 'function'
              if (typeof(other[key])=='undefined' or (key != 'equals' and first[key].toString() != other[key].toString()))
                return false
            else
              if (first[key] != other[key])
                return false
        else
          if (other[key])
            return false

      for own key, value of  other
        if(typeof(first[key])=='undefined')
          return false

      return true
    
    IsCollectionType: (type) ->
      if(type.isCollectionType == true)
        return type

      return null

    IsDictionaryType: (type) ->
      if(type.isDictionaryType == true)
        return type

      return null

    ExtractProperties: (type, props) ->
      for p in type.properties
        exist = false

        for pr in props
          if(pr.name == p.name)
            exist = true
            break

        if(not exist)
          props.push(p)


      for baseType in type.interfaces
        interfaceType = exports.IOGType.FindType(baseType.name)
        if(interfaceType?)
          exports.UTILS.ExtractProperties(interfaceType, props)

      return props

    GetType: (typeName) ->
      if(typeName instanceof exports.IOGType)
        return typeName

      for type in exports.types
        if type.name == typeName
          return type

    SetItemId: (item, instanceId) ->
      item[exports.Constants.InstanceIdFieldName] = instanceId

    GetItemId: (item) ->
      return item["#{exports.Constants.InstanceIdFieldName}"]

    HasItemId: (item) ->
      return item.hasOwnProperty(exports.Constants.InstanceIdFieldName)

    GetItemPrimaryKeyId: (value) ->
      if(value[exports.Constants.PrimaryKeyIdFieldName]?)
        return value[exports.Constants.PrimaryKeyIdFieldName]
      else
        return exports.Guid.EMPTY

    IsPermanentEdge: (edge) ->
      return  edge.data.semantic == exports.EdgeType.OfType or edge.toNodeId == exports.Constants.NullReferenceNodeId or ((edge.data.flags == exports.EdgeFlags.permanent) == exports.EdgeFlags.permanent)

    IsInstaceOfScalar: (value, scalarType) ->
      if(value.hasOwnProperty(Utils.IOG_TYPE) and value.hasOwnProperty(Utils.VALUE))
        valueType = value[Utils.IOG_TYPE]
        if(exports.UTILS.equals(valueType, scalarType))
          return true
        else
          return false
      else
        return false

    # this is taken from jQuery library
    IsNumber: (n) ->
      return $.isNumeric(n)


namespace execom.iog.name, (exports) ->
  exports.UTILS = new exports.Utils()
  exports.equals = exports.UTILS.equals
