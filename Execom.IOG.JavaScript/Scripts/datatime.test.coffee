module "DateTime and TimeSpan"

test "Constructor test", () ->
  #alert(TimeSpan.ZERO.toString())

  ok TimeSpan.ZERO?, "TimeSpan.Zero exist"

  #alert(DateTime.MIN_DATE.format("dd.MM.yyyy hh:mm:ss"))

  ok DateTime.MIN_DATE?, "DateTime.MIN_DATE exist"