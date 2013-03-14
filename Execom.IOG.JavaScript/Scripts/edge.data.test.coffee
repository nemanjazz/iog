module "EdgeData"

test "EdgeData compareTo test", () ->

  e1 = new EdgeData(EdgeType.Property, "bb")
  e2 = new EdgeData(EdgeType.Property, "bb")

  ok e1.compareTo(e2) == 0, "EdgeData objects are equals"

  e3 = new EdgeData(EdgeType.Property, "bc")

  ok e1.compareTo(e3)  < 0, "EdgeData object e1 is smaller"

  e4 = new EdgeData(EdgeType.Property, "ba")

  ok e1.compareTo(e4)  > 0, "EdgeData object e4 is bigger"

  e5 = new EdgeData(EdgeType.Contains, "bb")

  ok e1.compareTo(e5) > 0, "EdgeData object e1 is bigger"

  e6 = new EdgeData(EdgeType.RootObject, "bb")

  ok e1.compareTo(e6) < 0, "EdgeData object e1 is smaller"


