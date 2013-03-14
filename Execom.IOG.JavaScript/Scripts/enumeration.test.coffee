module "Enumeration"

test "Enumeration test", () ->
  en = new Enumerator(['red', 'green', 'blue'])
  array = en.array

  ok en.MoveNext() == true, "First MoveNext succeeded"

  ok en.Current() == 'red', "Current element is red"

  ok en.MoveNext() == true, "Second MoveNext succeeded"

  ok en.Current() == 'green', "Current element is red"

  ok en.MoveNext() == true, "Third MoveNext succeeded"

  ok en.Current() == 'blue', "Current element is red"

  ok en.MoveNext() == false, "Fourth MoveNext is false"

  en.Reset()

  ok en.MoveNext() == true, "First MoveNext succeeded"

  ok en.Current() == 'red', "Current element is red"

  ok en.MoveNext() == true, "Second MoveNext succeeded"

  ok en.Current() == 'green', "Current element is red"

  ok en.MoveNext() == true, "Third MoveNext succeeded"

  ok en.Current() == 'blue', "Current element is red"

  ok en.MoveNext() == false, "Fourth MoveNext is false"



