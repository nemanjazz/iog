module "BaseType tests"

test "Int32 test", () ->
  a = new Int32(4)
  b = new Int32(5)
  c = new Int32(6)
  d = new Int32(5)

  ok d.equals(b) == true, "Objects are same"
  ok d.equals(a) == false, "Objects are not same"

  ok b.compareTo(a) > 0, "b is bigger"
  ok b.compareTo(c) < 0, "b is smaller"
  ok b.compareTo(d) == 0, "b is equal to d"