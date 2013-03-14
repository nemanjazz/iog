module "Memory storage"

test "AddOrUpdate method test", () ->
	
	memory = new MemoryStorage()

	memory.AddOrUpdate("1", "a")

	ok memory.dictionary.Array().compare(["a"]), "Add is working"

	memory.AddOrUpdate("1", "c")

	ok memory.dictionary.Array().compare(["c"]), "Update is working"

test "Value method test", () ->
	
	memory = new MemoryStorage()

	memory.AddOrUpdate("1", "a")

	a = []
	a["1"] = "a"

	memory.AddOrUpdate("1", "c")
	a["1"] = "c"
	
	c = memory.Value("1")

	ok c == "c", "Value method is working, c and c are equal"

	s = memory.Value("32")

	ok s == null, "Value method is working with keys that are not in memory"

test "Contains method test", () ->
	
	memory = new MemoryStorage()

	memory.AddOrUpdate("1", "a")

	a = []
	a["1"] = "a"

	memory.AddOrUpdate("1", "c")
	a["1"] = "c"
	
	c = memory.Contains("1")

	ok c == true, "Contains method is working for key that is in memory"

	s = memory.Contains("32")

	ok s == false, "Contains method is working for key that is not in memory, returning false"

test "Remove method test", () ->
	
	memory = new MemoryStorage()

	memory.AddOrUpdate("1", "a")

	a = []
	a["1"] = "a"

	memory.AddOrUpdate("1", "c")
	a["1"] = "c"
	
	c = memory.Remove("1")

	ok c == true, "Remove method removing for key"

	c = memory.Value(1)

	ok c == null, "Valeu after removing is null"

	s = memory.Remove("32")

	ok s == false, "Contains method is working for key that is not in memory, returning false"

test "Remove method test", () ->
	
	memory = new MemoryStorage()

	memory.AddOrUpdate("1", "a")

	a = []
	a["1"] = "a"

	memory.AddOrUpdate("1", "c")
	a["1"] = "c"
	
	c = memory.Remove("1")

	ok c == true, "Remove method removing for key"

	c = memory.Value(1)

	ok c == null, "Valeu after removing is null"

	s = memory.Remove("32")

	ok s == false, "Remove method is working for key that is not in memory, returning false"

test "Clear method test", () ->
	memory = new MemoryStorage()

	memory.AddOrUpdate("1", "a")

	a = []
	a["1"] = "a"

	memory.AddOrUpdate("1", "c")
	a["1"] = "c"

	memory.Clear()

	ok memory.dictionary.Array().compare([]), "Clear is clearing memory"

test "ListKeys method test", () ->
	memory = new MemoryStorage()

	memory.AddOrUpdate("a", "aa")

	memory.AddOrUpdate("b", "cc")

	list = memory.ListKeys()
	rezList = ["a", "b"]

	ok list.compare(rezList), "ListKeys is returning keys"

	memory = new MemoryStorage()

	list = memory.ListKeys()

	ok list.compare([]), "ListKeys for empty array"

test "Get Values", () ->
  array = ["aa", "cc"]

  memory = new MemoryStorage()

  memory.AddOrUpdate("a", "aa")

  memory.AddOrUpdate("b", "cc")

  rezArray = memory.Values()
  ok array.equals(rezArray), "Values method is working"