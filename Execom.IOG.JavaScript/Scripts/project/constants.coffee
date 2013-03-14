
namespace execom.iog.name, (exports) ->
  TimeSpan.ZERO = new TimeSpan()
  DateTime.MIN_DATE = new DateTime()
  DateTime.MIN_DATE.span = TimeSpan.ZERO

  exports.IsolationLevel =
    ReadOnly:  0
    Snapshot:  1
    Exclusive: 2


  exports.EdgeType =
    Contains:   0
    OfType:     1
    Property:   2
    ListItem:   3
    RootObject: 4
    Special:    5


  exports.EdgeFlags =
    None:       0
    Permanent:  1


  exports.NodeType =
    TypesRoot:      0
    SnapshotsRoot:  1
    Snapshot:       2
    Type:           3
    Scalar:         4
    Object:         5
    Collection:     6
    Dictionary:     7
    TypeMember:     8
    TreeInternal:   9
    TreeLeaf:       10

  exports.NodeState =
    None:           0
    Created:        1
    Modified:       2
    Removed:        3


  exports.NodeAccess =
    Read:           0
    ReadWrite:      1

  exports.Constants =
    TypesNodeId:                    new exports.Guid("22DD35BD-071B-4429-837D-4F5D2C201580")
    SnapshotsNodeId:                new exports.Guid("52138911-0016-4C08-A685-9487617FD664")
    ExclusiveWriterLockId:          new exports.Guid("7EB5139E-72C2-4029-9EFD-1CD514775832")
    NullReferenceNodeId:            new exports.Guid("FFCE2840-A5D7-4C1F-81F4-A8AC7FC61F92")
    InstanceIdFieldName:            "__instanceId__"
    PrimaryKeyIdFieldName:          "__keyId__"
    TypeIdFieldName:                "__typeId__"
    FacadeFieldName:                "__facade__"
    ReadOnlyFieldName:              "__readOnly__"
    ProxyTypeSufix:                 "ProxyTypeSufix"
    GeneratedAssemblyName:          "IOG.RuntimeProxy"
    PropertyMemberIdSufix:          "_MemberID_"
    PropertyIsScalarSufix:          "_IsScalar_"
    TypeMemberPrimaryKeyId:         new exports.Guid("67B21654-1E2D-4565-A4AE-33A7E1D43AF2")


  exports.MemberTypes =
    Constructor:        0
    Event:              1
    Field:              2
    Method:             3
    Property:           4
    TypeInfo:           5
    Custom:             6
    NestedType:         7
    All:                8

  exports.TypeConstants =
    ID:                               "ID"
    Name:                             "Name"
    IsCollectionType:                 "IsCollectionType"
    IsDictionaryType:                 "IsDictionaryType"
    IsInterface:                      "IsInterface"
    IsEnum:                           "IsEnum"
    IsGenericType:                    "IsGenericType"
    CustomAttributes:                 "CustomAttributes"
    Interfaces:                       "Interfaces"
    EnumValues:                       "EnumValues"
    GenericArguments:                 "GenericArguments"
    IsScalar:                         "IsScalar"
    GenericType:                      "GenericType"
    Properties:                         "Properties"

  exports.ProperiteConstants =
    CanRead:                          "CanRead"
    CanWrite:                         "CanWrite"
    Name:                             "Name"
    IsStatic:                         "IsStatic"
    CustomAttributes:                 "CustomAttributes"
    PropertyType:                     "PropertyType"
    DeclaringType:                    "DeclaringType"

  exports.IOGTypeConstants = 
    Collection:                      "Collection"
    Dictionary:                      "Dictionary"

  exports.IOGJsonConstatns = 
    IOG_TYPE:                        "iogType"
    VALUE:                           "value"
  
