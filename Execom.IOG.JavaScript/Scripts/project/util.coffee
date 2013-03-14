
String::GetHashcode = () ->
  hash = 0
  len = this.length
  if (this.length == 0)
    return hash
  for charachter, index in this
    hash = ((hash<<5)-hash)+charachter;
    hash = hash & hash; # Convert to 32bit integer


  return hash

Array::compare = (testArray) ->
  if(this.length != testArray.length)
    return false
  for element, i in this
    if(typeof element == "Array")
      if(!element.compare(testArray[i]))
        return false
    if(element != testArray[i])
      return false

  return true;

String::compareTo = (other) ->
  otherType = typeof(other)
  if( otherType != "string")
    throw "Cannot compare this two objects!"
  for char, index in this
    if other[index] == char
      continue
    else
      if other[index] < char
        return 1
      else
        return -1

  return 0
###
Number::compareTo = (other) ->
  x = new Number(other)
  y = new Number(this)

  if(isNaN(x) or isNaN(y))
    throw "One of objects are not number!"

  if this.valueOf() < other.valueOf()
    return -1
  else
    if this.valueOf() > other.valueOf()
      return 1

  return 0
  
Number::equals = (other)->
  x = new Number(other)
  y = new Number(this)

  if(isNaN(x) or isNaN(y))
    throw "One of objects are not number!"

  if other.valueOf() == this.valueOf()
    return true

  return false
###
  
namespace execom.iog.name, (exports) ->  
  DateTime::toJSON = () ->
    json = {}
    json[exports.IOGJsonConstatns.IOG_TYPE] = exports.DateTimeType
    json[exports.IOGJsonConstatns.VALUE] = this.span._millis
    return json
    
  DateTime::toDate = () ->
    return new Date(this.year(), this.month() - 1, this.day(), this.hour(), 
      this.minute(), this.second(), this.millisecond());

  TimeSpan::toJSON = () ->
    json = {}
    json[exports.IOGJsonConstatns.IOG_TYPE] = exports.TimeSpanType
    json[exports.IOGJsonConstatns.VALUE] = this._millis
    return json

  
