
module "Dictionary"

test "Creating empty dictionary", () ->
    dict = new Dictionary()
    len = dict.Length()
    ok len == 0, "Empty dictionary"

test "Adding test", () ->
    dict = new Dictionary()
    dict.Add("1", "a");
    len = dict.Length()
    ok len == 1, "Adding test"

test "Getting key test", () ->
  dict = new Dictionary()
  dict.Add("1", "a");
  len = dict.Length()
  ok len == 1, "Adding test"

  a = dict.Get("1")

  ok a == "a", "Getting key test done"

  dict = new Dictionary()

  dict = new Dictionary()
  complexData = {
  "1": "a"
  "b": "c"
  "2": "d"
  }
  dict.Add(complexData, "s1")

  complexData2 = {
  "11": "w"
  "12": "q"
  "13": "y"
  }
  dict.Add(complexData2, "s2")

  rez = dict.Get(complexData2)

  ok rez.equals("s2"), "Complex key Get method is working"



test "Contains key test", () ->
  dict = new Dictionary()
  dict.Add("1", "a");
  len = dict.Length()
  ok len == 1, "Adding test"

  contains = dict.Contains("1")

  ok contains == true, "Key that exist, contains method passing right value"

  contains = dict.Contains("2")

  ok contains == false, "Key that doesn't exist, contains method passing right value"

  dict = new Dictionary()
  complexData = {
    "1": "a"
    "b": "c"
    "2": "d"
  }
  dict.Add(complexData, "s1")

  complexData2 = {
  "11": "w"
  "12": "q"
  "13": "y"
  }
  dict.Add(complexData2, "s2")

  contains = dict.Contains(complexData2)

  ok contains == true, "Complex key found"


test "Removing key", () ->
     dict = new Dictionary()
     dict.Add("1", "a")
     len = dict.Length()
     ok len == 1, "Adding test"

     dict.Add("2", "b")
     len = dict.Length()
     ok len == 2, "Adding test"

     dict.Remove("1")
     len = dict.Length()

     ok dict.array[0].value == "b" and dict.keys[0] == "2", "Total recrating of list whene removing is working"

     ok len == 1, "Removing key test succeeded"

     dict.Remove("3")
     len = dict.Length()

     ok len == 1, "Removing didn't happend because key do not exist"

     dict.Add("4", "s")
     dict.Add("5", "d")
     dict.Add("6", "f")
     dict.Add("7", "g")

     dict.Remove("5")
     len = dict.Length()

     ok len == 4, "Lengt is correct of removing in middle"
     ok dict.array[2].value == "f" and dict.keys[2] == "6", "Correctly reorganized arrays after deleting item"

     dict.Remove("7")
     len = dict.Length()
     ok len == 3, "Lengt is correct of removing on the end"
     ok dict.array[2].value == "f" and dict.keys[2] == "6", "Correctly reorganized arrays after deleting item"

test "Setting value", () ->
     dict = new Dictionary()
     dict.Add("1", "a")
     len = dict.Length()
     ok len == 1, "Adding test"

     dict.Add("2", "b")
     len = dict.Length()
     ok len == 2, "Adding test"

     dict.Set("1", "c")
     
     c = dict.Get("1")

     ok c == "c", "Setting value test succeeded"

     dict.Set("2", "d")

     d = dict.Get("2")

     ok d == "d", "Setting value test succeeded"

     dict.Set("3", "f")

     f = dict.Get("3")

     ok  not f?, "Setting value test succeeded"

test "Array method", () ->
    dict = new Dictionary()

    arr = dict.Array()
    testArray = []

    ok arr.length == testArray.length, "Array method is returning array"

test "Clear all items", () ->
    
    dict = new Dictionary()
    dict.Add("1", "a")

    dict.Add("2", "b")

    dict.Clear()

    retArray = dict.Array()
    shouldBeArray = []
    testRez = (retArray.compare(shouldBeArray))
    ok testRez, "Dictionary clear method is working"