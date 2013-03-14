module "Number test"

test "Number test value", () ->

  x = new Number(4)
  y = new Number(5)
  w = new Number(4)

  ok x.equals(y) == false, "Values are not same!"
  ok x.equals(w) == true, "Values are same!"

  return