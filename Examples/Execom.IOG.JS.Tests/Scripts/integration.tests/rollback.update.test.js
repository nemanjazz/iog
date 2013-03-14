module('Rollback and update test');
test("Rollback test", function () {

    var iog = window.execom.iog;

    var ctx = new iog.ClientContext('localhost:3681', "ServerContext.asmx");
    var ws = ctx.OpenWorkspace(iog.IsolationLevel.Exclusive);

    var database = ws.Data();

    var person = ws.New(iog.IDriverType);
    var car = ws.New(iog.ICarType);
    car.SetModel("model");

    person.SetName("John Connor");
    person.SetCar(car);

    database.SetDriver(person);

    ws.Commit();
    ws.CloseWorkspace();

    ws = ctx.OpenWorkspace(iog.IsolationLevel.Exclusive);

    var newInstance = ws.New(iog.IDriverType);
    newInstance.Name = "new";
    ws.Data().SetDriver(newInstance);

    ws.Rollback();

    var exception = false;

    try {
        var name = newInstance.GetName();
    }
    catch (ArgumentException) {
        exception = true;
    }

    ok(exception, 'Rollback is working!');

    ws.CloseWorkspace();

    ws = ctx.OpenWorkspace(iog.IsolationLevel.Exclusive);
    var person = ws.Data().GetDriver();
    person.SetName("changed");

    ws.Rollback();

    ok(person.GetName() == "John Connor", "Name are equal!");

    person.SetName("changed");

    ok("changed" == person.GetName(), "Name changed!");

    ws.Commit();
    ws.CloseWorkspace();

});

test('Update to old', function () {
    var iog = window.execom.iog;
    var ctx = new iog.ClientContext('localhost:3681', "ServerContext.asmx");
    var ws = ctx.OpenWorkspace(iog.IsolationLevel.Exclusive)

    var database = ws.Data();
    var person = ws.New(iog.IDriverType);
    var car = ws.New(iog.ICarType);
    car.SetModel("model");

    person.SetName("John Connor");
    person.SetCar(car);

    database.SetDriver(person);

    oldSnapshotId = ws.SnapshotId();

    ws.Commit();
    ws.CloseWorkspace();

    var ws1 = ctx.OpenWorkspace(iog.IsolationLevel.Snapshot);
    var ws2 = ctx.OpenWorkspace(iog.IsolationLevel.Snapshot);

    ws1.Data().GetDriver().SetName("Changed");
    ws1.Commit();

    ok("John Connor" == ws2.Data().GetDriver().GetName());

    ws2.Data().GetDriver().GetCar().SetModel("Changed");
    ws2.Update(oldSnapshotId);

    ok("John Connor" == ws2.Data().GetDriver().GetName());
    ok("Changed" == ws2.Data().GetDriver().GetCar().GetModel());

    ws1.CloseWorkspace();
    ws2.CloseWorkspace();
});

test('Update test', function () {

    var iog = window.execom.iog;

    var ctx = new iog.ClientContext('localhost:3681', "ServerContext.asmx");

    var ws = ctx.OpenWorkspace(iog.IsolationLevel.Exclusive)

    var database = ws.Data();
    var person = ws.New(iog.IDriverType);
    var car = ws.New(iog.ICarType);
    car.SetModel("model");

    person.SetName("John Connor");
    person.SetCar(car);

    database.SetDriver(person);

    ws.Commit();
    ws.CloseWorkspace();

    var ws1 = ctx.OpenWorkspace(iog.IsolationLevel.Snapshot);
    var ws2 = ctx.OpenWorkspace(iog.IsolationLevel.Snapshot);

    ws1.Data().GetDriver().SetName("Changed");
    ws1.Commit();

    ok("John Connor" == ws2.Data().GetDriver().GetName());

    ws2.Data().GetDriver().GetCar().SetModel("Changed");
    ws2.Update();

    ok("Changed" == ws2.Data().GetDriver().GetName());
    ok("Changed" == ws2.Data().GetDriver().GetCar().GetModel());

    ws1.CloseWorkspace();
    ws2.CloseWorkspace();
});

test("Rollback and update test", function () {


    var iog = window.execom.iog;

    var ctx = new iog.ClientContext('localhost:3681', "ServerContext.asmx");
    var ws = ctx.OpenWorkspace(iog.IsolationLevel.Exclusive)

    var database = ws.Data();
    var person = ws.New(iog.IDriverType);
    var car = ws.New(iog.ICarType);
    car.SetModel("model");

    person.SetName("John Connor");
    person.SetCar(car);

    database.SetDriver(person);

    ws.Commit();
    ws.CloseWorkspace();


    var ws1 = ctx.OpenWorkspace(iog.IsolationLevel.Snapshot);
    var ws2 = ctx.OpenWorkspace(iog.IsolationLevel.Snapshot);

    ws1.Data().GetDriver().SetName("Changed");
    ws1.Commit();

    ok("John Connor" == ws2.Data().GetDriver().GetName(), "Name in ws2 is same as name in ws1");

    ws2.Data().GetDriver().GetCar().SetModel("Changed");
    ws2.Rollback();
    ws2.Update(ws1.SnapshotId());

    ok("Changed" == ws2.Data().GetDriver().GetName());
    ok("model" == ws2.Data().GetDriver().GetCar().GetModel());

    ws1.CloseWorkspace();
    ws2.CloseWorkspace();
});