namespace execom.iog.name, (exports) ->
  class exports.IOGBaseType
    
namespace execom.iog.name, (exports) ->
  class exports.IOGObject
    
    equals: (other)->
      return exports.UTILS.equals(this, other)

namespace execom.iog.name, (exports) ->
  class exports.IOGEnum extends exports.IOGBaseType
  
    constructor: (@type) ->
      @values = []
      @name = @type.name
      firstValue = true
      for key, value of type.enumValues
        this[key] = new exports.IOGEnumLiteral(key, value, this)
        @values.push(this[key])
        if(firstValue)
          @first = this[key]
    
    fromInt: (intValue) ->
      for value in @values 
        if value.value == intValue
          return value
      throw "Value is not in range of enumeration #{@name}!"
      
    
    equals: (other)->
      return @name == other.name
 
namespace execom.iog.name, (exports) ->
  class exports.IOGEnumLiteral extends exports.IOGObject
  
    constructor: (@name, @value, @enumeration) ->
    
    compareTo: (other) ->
      return exports.IOGNumber.NumberCompare(this.value, other.value)
    
    equals: (other)->
      if(other.enumeration.name == @enumeration.name and 
      ( @name == other.name and @value == other.value))
        return true
      else
        return false
        
  exports.IOGEnumLiteral::toJSON = () ->
      json = {}
      json[exports.IOGJsonConstatns.IOG_TYPE] = @enumeration.type
      json[exports.IOGJsonConstatns.VALUE] = @value
      return json 
      
namespace execom.iog.name, (exports) ->
  class exports.IOGNumber extends exports.IOGBaseType


    @NumberCompare: (value1, value2) ->
      if( not (value1 instanceof exports.IOGNumber) and not (value2 instanceof exports.IOGNumber) )
        throw "One of objects are not number!"

      if( not exports.UTILS.equals(value1.iogType, value2.iogType) )
        throw "One of objects are not number!"

      x = new Number(value1.value)
      y = new Number(value2.value)

      if(isNaN(x) or isNaN(y))
        throw "One of objects are not number!"

      if x < y
        return -1
      else
        if x > y
          return 1

      return 0

    @NumberEqual: (value1, value2) ->
      if( not (value1 instanceof exports.IOGNumber) and not (value2 instanceof exports.IOGNumber) )
        throw false

      if( not exports.UTILS.equals(value1.iogType, value2.iogType))
        return false

      return exports.UTILS.equals(value1.value, value2.value) 


namespace execom.iog.name, (exports) ->
  class exports.Int32 extends exports.IOGNumber
    @MAX_VALUE =  2147483647
    @MIN_VALUE = -2147483648

    constructor: (value) ->
      x = Number(value)

      if(isNaN(x))
        throw "Entered value is not int32!"

      if(value < exports.Int32.MIN_VALUE or value > exports.Int32.MAX_VALUE)
        throw "Out of range!"

      @value = value
      @iogType = exports.IOGType.FindType(exports.ScalarName.Int32)

    compareTo: (other) ->
      exports.IOGNumber.NumberCompare(this, other)

    equals: (other) ->
      exports.IOGNumber.NumberEqual(this, other)


namespace execom.iog.name, (exports) ->
  class exports.Int64 extends exports.IOGNumber

    @MAX_VALUE =   9223372036854775807
    @MIN_VALUE =  -9223372036854775808

    constructor: (value) ->
      x = Number(value)

      if(isNaN(x))
        throw "Entered value is not int64!"

      if(value < exports.Int64.MIN_VALUE or value > exports.Int64.MAX_VALUE)
        throw "Out of range!"

      @value = value
      @iogType = exports.IOGType.FindType(exports.ScalarName.Int64)

    compareTo: (other) ->
      exports.IOGNumber.NumberCompare(this, other)

    equals: (other) ->
      exports.IOGNumber.NumberEqual(this, other)

namespace execom.iog.name, (exports) ->
  class exports.Double extends exports.IOGNumber
    @MAX_VALUE =   1.7976931348623157e+308
    @MIN_VALUE =  -1.7976931348623157e+308

    constructor: (value) ->
      x = Number(value)

      if(isNaN(x))
        throw "Entered value is not double!"

      if(value < exports.Double.MIN_VALUE or value > exports.Double.MAX_VALUE)
        throw "Out of range!"

      @value = value
      @iogType = exports.IOGType.FindType(exports.ScalarName.Double)

    compareTo: (other) ->
      exports.IOGNumber.NumberCompare(this, other)

    equals: (other) ->
      exports.IOGNumber.NumberEqual(this, other)

namespace execom.iog.name, (exports) ->
  class exports.Byte extends exports.IOGNumber
    @MAX_VALUE =   255
    @MIN_VALUE =   0

    constructor: (value) ->
      x = Number(value)

      if(isNaN(x))
        throw "Entered value is not byte!"

      if(value < exports.Byte.MIN_VALUE or value > exports.Byte.MAX_VALUE)
        throw "Out of range!"

      @value = value
      @iogType = exports.IOGType.FindType(exports.ScalarName.Byte)

    compareTo: (other) ->
      exports.IOGNumber.NumberCompare(this, other)

    equals: (other) ->
      exports.IOGNumber.NumberEqual(this, other)
      
namespace execom.iog.name, (exports) ->
  class exports.Char extends exports.IOGBaseType
    constructor: (value) ->

      if(value.length > 1)
        throw "Char cannot have more then one charachter!"
      @value = value
      @iogType = exports.IOGType.FindType(exports.ScalarName.Char)

    compareTo: (other) ->
      if(not other instanceof exports.Char)
        throw "Type of compering type is not right!"

      return this.value.compareTo(other.value)

    equals: (other) ->
      if(not other instanceof exports.Char)
        return false

      return this.value == other.value

namespace execom.iog.name, (exports) ->
  class exports.Guid extends exports.IOGBaseType
    constructor: (value) ->

      @value = UUID.tryParse(value)
      @iogType = exports.IOGType.FindType(exports.ScalarName.Guid)

    @Create: () ->
      new Guid(UUID.create())

    @EMPTY = new Guid(UUID.empty)

    @TryParse: (value) ->
      return new exports.Guid(UUID.tryParse(value))

    @Parse: () ->
      return new Guid(UUID.Parse(value))

    compareTo: (other) ->

      return this.value.compareTo(other.value)

    equals: (other) ->
      if(not other instanceof exports.Guid)
        return false

      return this.value == other.value

namespace execom.iog.name, (exports) ->
  class exports.IOGString extends exports.IOGBaseType
    constructor: (value) ->
      @value = value
      @iogType = exports.IOGType.FindType(exports.ScalarName.String)

    compareTo: (other) ->
      if(not other instanceof exports.IOGString)
        throw "Type of compering type is not right!"

      return this.value.compareTo(other.value)

    equals: (other) ->
      if(not other instanceof exports.IOGString)
        return false

      return this.value == other.value

  class exports.IOGBoolean extends exports.IOGBaseType
    constructor: (value) ->

      @value = Boolean(value)
      @iogType = exports.IOGType.FindType(exports.ScalarName.Boolean)

    compareTo: (other) ->
      if(not other instanceof exports.IOGBoolean)
        throw "Type of compering type is not right!"

      if(this.value)
        if(other.value)
          return 0
        else
          return 1
      else
        if(other.value)
          return -1
        else
          return 0

    equals: (other) ->
      if(not other instanceof exports.Boolean)
        return false

      return this.value == other.value


