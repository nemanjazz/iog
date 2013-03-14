module "Utils"

test "Testing ExtractProperties", () ->
  props = []
  UTILS.ExtractProperties(booleanType, props)

  ok props.length == 1, "Number of properties is 1"

  ok props[0] == "value", "There is only value proeprties"