module "Object"

test "Obejct equals test", () ->

  a = {
    "a": 1
    "b": 2
    "c": 3
  }

  b = {
  "a": 1
  "b": 2
  "c": 3
  }

  ok a.equals(b) == true, "Objects are equal"

  a = {
  "a": 1
  "d": 2
  "c": 3
  }

  b = {
  "a": 1
  "b": 2
  "c": 3
  }

  ok a.equals(b) == false, "Objects are not equal"

  a = {
  "a": 1
  "b": 3
  "c": 3
  }

  b = {
  "a": 1
  "b": 2
  "c": 3
  }

  ok a.equals(b) == false, "Objects are not equal"

  a = {
  "a": 1
  "d": 2
  "c": 3
  }

  b = {
  "a": 1
  "h": 2
  "c": 3
  }

  ok a.equals(b) == false, "Objects are not equal"