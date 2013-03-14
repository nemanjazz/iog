module('Events test');
asyncTest('asynchronous notification test', function () {

    $.connection.hub.stateChanged(function (change) {
        if (change.newState === $.signalR.connectionState.connected &&
                    (context == null || iog == null)) {
            var eventHappend = false
            var iog = window.execom.iog;
            var context = new iog.ClientContext('localhost:3681', "ServerContext.asmx");
            var workspace = context.OpenWorkspace(iog.IsolationLevel.Exclusive);

            var driver = workspace.New(iog.IDriverType);
            var car = workspace.New(iog.ICarType);
            car.SetModel("Model");
            driver.SetName("Driver");
            driver.SetCar(car);

            workspace.Data().SetDriver(driver);

            workspace.Commit();
            workspace.CreateSubscription(workspace.Data().GetDriver(), function (msg) {
                eventHappend = true; 
            });
            workspace.CloseWorkspace();

            var ws = context.OpenWorkspace(iog.IsolationLevel.Exclusive);
            ws.Data().GetDriver().GetCar().SetModel("Changed!");

            ws.Commit();
            ws.CloseWorkspace();
            setTimeout(function () {
                ok(eventHappend);
                start();
            }, 250);
        }
    });


});