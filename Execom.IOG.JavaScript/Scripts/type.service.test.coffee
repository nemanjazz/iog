
module "Type Service"

test "GetTypeName test", () ->
  typeService = new TypesService()

  name = typeService.GetTypeName(window.BooleanType)

  ok name == "Boolean", "Name of BooleanType is Boolean"

  raises () ->
    typeService.GetTypeName(null)

  raises () ->
    typeService.GetTypeName({"lol":"aa"})
    return

  return

test "Initialize service with BooleanType", () ->
  typeService = new TypesService()
  typeService.InitializeTypeSystem(BooleanType)

  ok typeService.scalarTypesTable.Length() == 10, "TypeService added Boolean"

test "GetTypeId test", () ->
  typeService = new TypesService()
  typeService.InitializeTypeSystem(BooleanType)
  booleanGuid = typeService.GetTypeId(BooleanType)
  rez = typeService.provider.storage.dictionary.array[1].key

  ok  rez == booleanGuid, "Right boolean GUID"

  emptyGuid = typeService.GetTypeId({"name":"djklsajda"})

  ok emptyGuid == UUID.empty, "For object without guid there is EMPTY guid"

test "GetConstantValues BooleanType test", () ->
  typeService = new TypesService()

  list = typeService.GetConstantValues(window.BooleanType)

  ok list.length == 2, "Values of BooleanType is Boolean"

  ok list[0] == true and list[1] == false, "List is valid"

  enumType = new Type(UUID.create(), "ETask", false, false, false, true, false, Int32Type, [], [], [], ["OK", "FALSE"])

  list = typeService.GetConstantValues(enumType)

  ok list[0] == "OK" and list[1] == "FALSE", "List of values is valid"

  return


