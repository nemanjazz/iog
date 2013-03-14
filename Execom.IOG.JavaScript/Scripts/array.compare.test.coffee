module "Array compare"

test "Comparing two empty arrays", () ->
	a1 = []
	a2 = []

	ok a1.compare(a2), "Two empty arrays are equal"

test "Comparing two arrays that are not empty and are equal", () ->
	a1 = [1, 12]
	a2 = [1, 12]

	ok a1.compare(a2), "Two not empty arrays are equal"

test "Comparing two arrays that are not empty and are not equal", () ->
	a1 = [1, 2]
	a2 = [1, 12]

	ok not a1.compare(a2), "Two not empty arrays are not equal"

test "Array that is used for dictionary", () ->
	a1 = []
	a1["a"] = "1"
	a1["b"] = "2"

	a2 = []
	a2["a"] = "1"
	a2["b"] = "2"

	ok a1.compare(a2), "Two arrays are equeal"

	a1 = []
	a1["1"] = "1"
	a1["2"] = "2"

	a2 = []
	a2["1"] = "4"
	a2["2"] = "2"

	ok not a1.compare(a2), "Two arrays are not equeal"

	a1 = []
	a1["1"] = "1"
	a1["2"] = "2"

	a2 = []
	a2["3"] = "1"
	a2["2"] = "2"

	ok not a1.compare(a2), "Two arrays are not equeal"

	a1 = []
	a1["1"] = "1"
	a1["2"] = "2"

	a2 = []
	a2["3"] = "4"
	a2["2"] = "2"

	ok not a1.compare(a2), "Two arrays are not equeal"

test "Array enumerator", () ->
  a = ['red', 'green', 'blue']

  en = a.GetEnumerator()

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
