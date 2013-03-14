module "CollectionInstancesService"

test "NewInstance test", () ->

  typesService = new TypesService()
  typesService.InitializeTypeSystem(BooleanType)

  collectionService = new CollectionInstancesService(new DirectNodeProvider(), typesService)

  stringId = typesService.GetTypeId(StringType)

  collectionId = collectionService.NewInstance(stringId)

  ok collectionService.provider.Length() == 1, "Collection of type string made"

  ok collectionService.Count(collectionId) == 0, "String collection is empty"

  ok collectionService.GetInstanceTypeId(collectionId) == stringId, "Returned right type for given collection"

  ok collectionService.GetInstanceTypeId(UUID.create) == UUID.empty, "For UUID that does not exist we got UUID.empty"