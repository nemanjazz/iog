module "Object Comparable"

test "String test", () ->

  a = "test"

  b = "test"

  ok a.compareTo(b) == 0, "Strings are same"

  a = "taaaa"

  b = "tbbb"

  ok a.compareTo(b) == -1, "String a is smaler"

  a = "tbbbbb"

  b = "taaaa"

  ok a.compareTo(b) == 1, "String a is greater"

test "Number test", () ->

  a = 1

  b = 2

  ok a.compareTo(b) == -1, "Var a is smaller"

  ok b.compareTo(a) == 1, "Var b is bigger"

  a = 2

  b = 2

  ok a.compareTo(b) == 0, "Numbers are equal"