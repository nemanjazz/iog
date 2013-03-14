module "SortedList"

test "Adding test", () ->
  dict = new SortedList()

  dict.Add("1", "a");
  len = dict.Length()
  ok len == 1, "Adding test"

  dict.Add("3", "b");

  dict.Add("2", "s");

  ok dict.keys[1] == "2", "Element is added in sorted way"


test "Removing test", () ->
  dict = new SortedList()

  dict.Add("1", "a");

  dict.Add("3", "b");

  dict.Add("2", "s");

  dict.Remove("2")

  ok dict.Contains("2") == false, "Element is removed"

  ok dict.Length() == 2, "SortedList size is 2"

test "Index of test", () ->

  dict = new SortedList()

  dict.Add("1", "a");

  dict.Add("3", "b");

  dict.Add("2", "s");

  ok dict.IndexOf("1") == 0, "First element found"

  ok dict.IndexOf("2") == 1, "Second element found"

  ok dict.IndexOf("3") == 2, "Third element found"

  ok dict.IndexOf("s") == -1, "Element is not found"




