
module "Table element"

test "CompareTo Method", () ->
  te1 = new TableElement("s1", 1)
  te2 = new TableElement("s3", 1)
  te3 = new TableElement("s4", 2)
  te4 = new TableElement("s5", 0)

  ok te1.compareTo(te2) == 0, "Objects are same"

  ok te1.compareTo(te3) < 0, "te1 object is smaller"

  ok te1.compareTo(te4) > 0, "te4 object is bigger"
