module "ObjectInstanceService"

test "NewInstance test", () ->
  typesService = new TypesService()
  typesService.InitializeTypeSystem(BooleanType)

  objectInstanceService = new ObjectInstancesService(new DirectNodeProvider(), typesService)
  booleanUuid = typesService.GetTypeId(BooleanType)

  newInstaceUuid = objectInstanceService.NewInstance(booleanUuid)
  ok objectInstanceService.provider.Contains(newInstaceUuid), "Made new instace of boolean type"
