namespace execom.iog.name, (exports) ->
  #in this enumerator I didn't implemented immutability
  class exports.Enumerator
    array = []
    currentNumber = -1
    #array - Array for enumerator - Array
    constructor: (array = []) ->
      this.array = array
      this.currentNumber = -1

    MoveNext: () ->
      if(@array.length > @currentNumber + 1)
        @currentNumber = @currentNumber + 1
        return true

      return false

    Reset: () ->
      @currentNumber = -1
      return

    Current: () ->
      if(@currentNumber >= 0 and @currentNumber < @array.length)
        return @array[@currentNumber]
      return null

  Array::GetEnumerator = () ->
    return new exports.Enumerator(this)
