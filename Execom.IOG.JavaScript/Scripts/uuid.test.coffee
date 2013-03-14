module "UUID"

test "UUID empty", () ->
	ok UUID.empty == "00000000-0000-0000-0000-000000000000", "Empty UUID is valid"

test "UUID parse", () ->
	ok UUID.parse("52138911-0016-4C08-A685-9487617FD664"), "Parsing SnapshotsNodeId"

	ok UUID.parse("22DD35BD-071B-4429-837D-4F5D2C201580"), "Parsing TypesNodeId"

	ok UUID.parse("7EB5139E-72C2-4029-9EFD-1CD514775832"), "Parsing ExclusiveWriterLockId"

	ok UUID.parse("FFCE2840-A5D7-4C1F-81F4-A8AC7FC61F92"), "Parsing NullReferenceNodeId"

	ok UUID.parse("67B21654-1E2D-4565-A4AE-33A7E1D43AF2"), "Parsing TypeMemberPrimaryKeyId"

	ok UUID.parse("53F11357-62B7-430F-B446-9EC8F9702406"), "Parsing EdgeData.MAX_VALUE"

	ok UUID.parse("76367091-B69D-4BDF-A643-779032AF3503"), "Parsing EdgeData.MIN_VALUE"
	
	ok UUID.parse(UUID.create()), "Parsing random value"

test "UUID random", () ->

	uuid1 = UUID.random()

	uuid2 = UUID.random()

	ok uuid1 != uuid2, "UUIDs are not the same"

	uuid3 = UUID.create()

	ok uuid1 != uuid3 and uuid3 != uuid2, "Created uuid is not the same as random"