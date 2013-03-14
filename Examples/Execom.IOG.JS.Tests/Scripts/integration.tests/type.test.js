('Type test');
test("Type test", function () {
    //$(document).ready(function () {
    var iog = window.execom.iog;
    var context = new iog.ClientContext('localhost:3681', "ServerContext.asmx");
    var workspace = context.OpenWorkspace(iog.IsolationLevel.Exclusive);

    var typeProxy = workspace.Data().GetTypeTestData();

    if (typeProxy == null) {
        typeProxy = workspace.New(iog.ITypeTestType);
        workspace.Data().SetTypeTestData(typeProxy);
        typeProxy = null;
    }

    typeProxy = workspace.Data().GetTypeTestData();

    ok(typeProxy != null, 'Type proxy is not NULL!');

    var int32 = iog.IOGType.CreateScalar(iog.Int32Type, 5);
    var int64 = iog.IOGType.CreateScalar(iog.Int64Type, 555);
    var double = iog.IOGType.CreateScalar(iog.DoubleType, iog.Double.MIN_VALUE);
    var string = iog.IOGType.CreateScalar(iog.StringType, 'ZLO');
    //var char = IOGType.CreateScalar(window.CharType, 'c');
    var boolean = iog.IOGType.CreateScalar(iog.BooleanType, true);
    var byte = iog.IOGType.CreateScalar(iog.ByteType, 1); ;
    var guid = iog.Guid.Create();
    var date = iog.IOGType.CreateScalar(iog.DateTimeType, 5555);
    var timespan = iog.IOGType.CreateScalar(iog.TimeSpanType, 5555);

    typeProxy.SetInt32Type(int32);
    typeProxy.SetInt64Type(int64);
    typeProxy.SetBooleanType(boolean);
    typeProxy.SetStringType(string);
    typeProxy.SetDoubleType(double);
    typeProxy.SetByteType(byte);
    //typeProxy.SetCharType(char);
    typeProxy.SetGuidType(guid);
    typeProxy.SetDateTimeType(date);
    typeProxy.SetTimeSpanType(timespan);

    workspace.Commit();
    workspace.CloseWorkspace();
    typeProxy = null;

    var snapshotId = context.LastSnapshotId();
    workspace = context.OpenWorkspace(iog.IsolationLevel.Snapshot, snapshotId);
    typeProxy = workspace.Data().GetTypeTestData();

    ok(typeProxy != null, 'Type proxy is not NULL!')

    var sInt32 = typeProxy.GetInt32Type();
    var sInt64 = typeProxy.GetInt64Type();
    var sDouble = typeProxy.GetDoubleType()
    var sBoolean = typeProxy.GetBooleanType();
    var sString = typeProxy.GetStringType();
    var sByte = typeProxy.GetByteType();
    var sGuid = typeProxy.GetGuidType();
    var sDateTime = typeProxy.GetDateTimeType();
    var sTimeSpan = typeProxy.GetTimeSpanType();

    ok(int32.value === sInt32, 'Int32 value is ok!');
    ok(int64.value === sInt64, 'Int64 value is ok!');
    ok(double.value === sDouble, "Double value is ok!");
    ok(string.value === sString, 'String value is ok!');
    ok(boolean.value === sBoolean, 'Boolean value is ok!');
    ok(byte.value === sByte, 'Byte value is ok!');
    ok(guid.value === sGuid, 'Guid value is ok!');
    ok(date.equals(sDateTime), 'DateTime value is ok!');
    ok(timespan.equals(sTimeSpan), 'TimeSpan value is ok!');

    var newInt32 = iog.IOGType.CreateScalar(iog.Int32Type, 7);
    var newInt64 = iog.IOGType.CreateScalar(iog.Int64Type, 7777);
    var newDouble = iog.IOGType.CreateScalar(iog.DoubleType, 5.5);
    var newString = iog.IOGType.CreateScalar(iog.StringType, 'DOBRO');
    //var char = IOGType.CreateScalar(window.CharType, 'c');
    var newBoolean = iog.IOGType.CreateScalar(iog.BooleanType, false);
    var newByte = iog.IOGType.CreateScalar(iog.ByteType, 254); ;
    var newGuid = iog.Guid.Create();
    var newDate = iog.IOGType.CreateScalar(iog.DateTimeType, 77777);
    var newTimespan = iog.IOGType.CreateScalar(iog.TimeSpanType, 7766);

    typeProxy.SetInt32Type(7);
    typeProxy.SetInt64Type(7777);
    typeProxy.SetBooleanType(false);
    typeProxy.SetStringType('DOBRO');
    typeProxy.SetDoubleType(5.5);
    typeProxy.SetByteType(254);
    //typeProxy.SetCharType(char);
    typeProxy.SetGuidType(newGuid);
    typeProxy.SetDateTimeType(newDate);
    typeProxy.SetTimeSpanType(newTimespan);

    workspace.Commit();
    workspace.CloseWorkspace();
    typeProxy = null;

    workspace = context.OpenWorkspace(iog.IsolationLevel.ReadOnly);

    typeProxy = workspace.Data().GetTypeTestData();

    ok(typeProxy != null, 'Type proxy is not NULL!')

    sInt32 = typeProxy.GetInt32Type();
    sInt64 = typeProxy.GetInt64Type();
    sDouble = typeProxy.GetDoubleType()
    sBoolean = typeProxy.GetBooleanType();
    sString = typeProxy.GetStringType();
    sByte = typeProxy.GetByteType();
    sGuid = typeProxy.GetGuidType();
    sDateTime = typeProxy.GetDateTimeType();
    sTimeSpan = typeProxy.GetTimeSpanType();

    ok(newInt32.value === sInt32, 'Int32 value is ok!');
    ok(newInt64.value === sInt64, 'Int64 value is ok!');
    ok(newDouble.value === sDouble, "Double value is ok!");
    ok(newString.value === sString, 'String value is ok!');
    ok(newBoolean.value === sBoolean, 'Boolean value is ok!');
    ok(newByte.value === sByte, 'Byte value is ok!');
    ok(newGuid.value === sGuid, 'Guid value is ok!');
    ok(newDate.equals(sDateTime), 'DateTime value is ok!');
    ok(newTimespan.equals(sTimeSpan), 'TimeSpan value is ok!');

    workspace.CloseWorkspace();
    typeProxy = null;
});
//});