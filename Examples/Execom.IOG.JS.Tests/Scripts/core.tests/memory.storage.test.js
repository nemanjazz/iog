// Generated by CoffeeScript 1.3.3
(function() {

  module("Memory storage");

  test("AddOrUpdate method test", function() {
    var iog = window.execom.iog;
    var memory;
    memory = new iog.MemoryStorage();
    memory.AddOrUpdate("1", "a");
    ok(memory.dictionary.Array().compare(["a"]), "Add is working");
    memory.AddOrUpdate("1", "c");
    return ok(memory.dictionary.Array().compare(["c"]), "Update is working");
  });

  test("Value method test", function() {
    var iog = window.execom.iog;
    var a, c, memory, s;
    memory = new iog.MemoryStorage();
    memory.AddOrUpdate("1", "a");
    a = [];
    a["1"] = "a";
    memory.AddOrUpdate("1", "c");
    a["1"] = "c";
    c = memory.Value("1");
    ok(c === "c", "Value method is working, c and c are equal");
    s = memory.Value("32");
    return ok(s === null, "Value method is working with keys that are not in memory");
  });

  test("Contains method test", function() {
    var a, c, memory, s;
    var iog = window.execom.iog;
    memory = new iog.MemoryStorage();
    memory.AddOrUpdate("1", "a");
    a = [];
    a["1"] = "a";
    memory.AddOrUpdate("1", "c");
    a["1"] = "c";
    c = memory.Contains("1");
    ok(c === true, "Contains method is working for key that is in memory");
    s = memory.Contains("32");
    return ok(s === false, "Contains method is working for key that is not in memory, returning false");
  });

  test("Remove method test", function() {
    var iog = window.execom.iog;
    var a, c, memory, s;
    memory = new iog.MemoryStorage();
    memory.AddOrUpdate("1", "a");
    a = [];
    a["1"] = "a";
    memory.AddOrUpdate("1", "c");
    a["1"] = "c";
    c = memory.Remove("1");
    ok(c === true, "Remove method removing for key");
    c = memory.Value(1);
    ok(c === null, "Valeu after removing is null");
    s = memory.Remove("32");
    return ok(s === false, "Contains method is working for key that is not in memory, returning false");
  });

  test("Remove method test", function() {
    var iog = window.execom.iog;
    var a, c, memory, s;
    memory = new iog.MemoryStorage();
    memory.AddOrUpdate("1", "a");
    a = [];
    a["1"] = "a";
    memory.AddOrUpdate("1", "c");
    a["1"] = "c";
    c = memory.Remove("1");
    ok(c === true, "Remove method removing for key");
    c = memory.Value(1);
    ok(c === null, "Valeu after removing is null");
    s = memory.Remove("32");
    return ok(s === false, "Remove method is working for key that is not in memory, returning false");
  });

  test("Clear method test", function() {
    var iog = window.execom.iog;
    var a, memory;
    memory = new iog.MemoryStorage();
    memory.AddOrUpdate("1", "a");
    a = [];
    a["1"] = "a";
    memory.AddOrUpdate("1", "c");
    a["1"] = "c";
    memory.Clear();
    return ok(memory.dictionary.Array().compare([]), "Clear is clearing memory");
  });

  test("ListKeys method test", function() {
    var iog = window.execom.iog;
    var list, memory, rezList;
    memory = new iog.MemoryStorage();
    memory.AddOrUpdate("a", "aa");
    memory.AddOrUpdate("b", "cc");
    list = memory.ListKeys();
    rezList = ["a", "b"];
    ok(list.compare(rezList), "ListKeys is returning keys");
    memory = new iog.MemoryStorage();
    list = memory.ListKeys();
    return ok(list.compare([]), "ListKeys for empty array");
  });

  test("Get Values", function() {
    var iog = window.execom.iog;
    var array, memory, rezArray;
    array = ["aa", "cc"];
    memory = new iog.MemoryStorage();
    memory.AddOrUpdate("a", "aa");
    memory.AddOrUpdate("b", "cc");
    rezArray = memory.Values();
    return ok(iog.equals(array, rezArray), "Values method is working");
  });

}).call(this);
