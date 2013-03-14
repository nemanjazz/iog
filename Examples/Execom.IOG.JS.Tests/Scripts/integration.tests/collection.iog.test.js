('Collection test');
test("Dictinary Add test", function () {
    //$(document).ready(function () {
    var iog = window.execom.iog;
    var n = 5;
    var context = new iog.ClientContext('localhost:3681', "ServerContext.asmx");
    var workspace = context.OpenWorkspace(iog.IsolationLevel.Exclusive);

    var rootProxy = workspace.Data().GetDictionaryTestData();

    if (rootProxy == null) {
        rootProxy = workspace.New(iog.IDictionaryTestType);
        workspace.Data().SetDictionaryTestData(rootProxy);
        rootProxy = null;
    }

    rootProxy = workspace.Data().GetDictionaryTestData();
    //ok(rootProxy != null, 'Root proxy for this test is not NULL!');

    var personCollection = workspace.New(iog.IOGType.CreateCollectionWithGenericType(iog.IPersonType));
    var personDictionary = workspace.New(iog.IOGType.CreateDictionaryWithGenericTypes(iog.Int32Type, iog.IPersonType));

    rootProxy.SetPersonCollection(personCollection);
    rootProxy.SetPersonDictionary(personDictionary);

    for (i = 0; i < n; i++) {
        var person = workspace.New(iog.IPersonType);
        person.SetName("Person" + i);
        person.SetAge(i);
        personCollection.Add(person);

    }

    personDictionary.Add(0, personCollection.First());

    workspace.Commit();

    workspace.CloseWorkspace();
    rootProxy = null;

    var ws1 = context.OpenWorkspace(iog.IsolationLevel.Snapshot);
    ws1.Data().GetDictionaryTestData().
        SetIntCollection(ws1.New(iog.IOGType.CreateCollectionWithGenericType(iog.Int32Type)));

    var ws2 = context.OpenWorkspace(iog.IsolationLevel.Snapshot);

    var lastInCollection = ws2.Data().GetDictionaryTestData().GetPersonCollection().Last();
    var ws2PersonDictProxy = ws2.Data().GetDictionaryTestData().GetPersonDictionary()
    ws2PersonDictProxy.Add(1, lastInCollection);
    var ws2PersonCollectionProxy = ws2.Data().GetDictionaryTestData().GetPersonCollection();

    var firstPerson = ws2PersonDictProxy.Get(0);
    var secondPerson = ws2PersonDictProxy.Get(1);


    var firstName = firstPerson.GetName();
    var lastName = secondPerson.GetName();

    firstPerson.SetName("Changed");

    secondPerson.SetName("Changed");

    ws1.Commit();
    ws2.Commit();


    ws1.CloseWorkspace();
    ws2.CloseWorkspace();

    var ws = context.OpenWorkspace(iog.IsolationLevel.ReadOnly);
    /*var perDict = ws.Data().GetDictionaryTestData().GetPersonDictionary();
    var personProxy = perDict.Get(0);
    name = personProxy.GetName();*/
    ok(ws.Data().GetDictionaryTestData().GetPersonDictionary().Count() == 2,
        'Number of persons in dictionary is 2.'); //Assert.AreEqual(2, ws.Data.PersonDictionary.Count);

    var person1Name = ws.Data().GetDictionaryTestData().GetPersonDictionary().
        Get(0).GetName()

    ok(person1Name == "Changed",
        'Person 0 name is ok!'); //Assert.AreEqual("Changed", ws.Data.PersonDictionary[0].Name);

    var person2Name = ws.Data().GetDictionaryTestData().GetPersonDictionary().
        Get(1).GetName();

    ok(person2Name == "Changed",
        'Person 1 name is ok!'); //Assert.AreEqual("Changed", ws.Data.PersonDictionary[1].Name);

    var person1NameColl = ws.Data().GetDictionaryTestData().GetPersonDictionary().Get(0);
    var person2NameColl = ws.Data().GetDictionaryTestData().GetPersonDictionary().Get(1);

    ok(person1NameColl.GetName() == "Changed",
        'Person 0 name is ok!'); //Assert.AreEqual("Changed", ws.Data.PersonDictionary[0].Name);
    ok(person2NameColl.GetName() == "Changed",
        'Person 1 name is ok!'); //Assert.AreEqual("Changed", ws.Data.PersonDictionary[1].Name);

    ws.CloseWorkspace();


});

test("Dictinary Remove test", function () {
    var iog = window.execom.iog;
    var n = 5;
    var context = new iog.ClientContext('localhost:3681', "ServerContext.asmx");
    var workspace = context.OpenWorkspace(iog.IsolationLevel.Exclusive);

    var rootProxy = workspace.Data().GetDictionaryTestData();

    if (rootProxy == null) {
        rootProxy = workspace.New(iog.IDictionaryTestType);
        workspace.Data().SetDictionaryTestData(rootProxy);
        rootProxy = null;
    }
    //getting root proxy for this test
    rootProxy = workspace.Data().GetDictionaryTestData();

    var personCollection = workspace.New(iog.IOGType.CreateCollectionWithGenericType(iog.IPersonType));
    var personDictionary = workspace.New(iog.IOGType.CreateDictionaryWithGenericTypes(iog.Int32Type, iog.IPersonType));

    rootProxy.SetPersonCollection(personCollection);
    rootProxy.SetPersonDictionary(personDictionary);
    //loading initial data
    for (i = 0; i < n; i++) {
        var person = workspace.New(iog.IPersonType);
        person.SetName("Person" + i);
        person.SetAge(i);
        personCollection.Add(person);

    }

    personDictionary.Add(0, personCollection.First());

    workspace.Commit();
    workspace.CloseWorkspace();

    var ws1 = context.OpenWorkspace(iog.IsolationLevel.Snapshot);
    ws1.Data().GetDictionaryTestData().
        SetIntCollection(ws1.New(iog.IOGType.CreateCollectionWithGenericType(iog.Int32Type)));

    var ws2 = context.OpenWorkspace(iog.IsolationLevel.Snapshot);

    var lastInCollection = ws2.Data().GetDictionaryTestData().GetPersonCollection().Last();
    var ws2PersonDictProxy = ws2.Data().GetDictionaryTestData().GetPersonDictionary()
    ws2PersonDictProxy.Add(1, lastInCollection);
    var ws2PersonCollectionProxy = ws2.Data().GetDictionaryTestData().GetPersonCollection();

    var firstPerson = ws2PersonCollectionProxy.First();
    var secondPerson = ws2PersonCollectionProxy.Last()


    var firstName = firstPerson.GetName();
    var lastName = secondPerson.GetName();

    firstPerson.SetName("Changed");

    secondPerson.SetName("Changed");

    var personCollectionEnumeration = ws2.Data().GetDictionaryTestData().GetPersonCollection().GetEnumerator();
    //ws2.Data.PersonCollection.Remove(ws2.Data.PersonCollection.Single(o => o.Name == "Person2"));
    var personForRemoving = ws2.Data().GetDictionaryTestData().GetPersonCollection().Get(2);

    ws2.Data().GetDictionaryTestData().GetPersonCollection().Remove(personForRemoving);

    ws2.Commit();
    ws1.Commit();

    ws1.CloseWorkspace();
    ws2.CloseWorkspace();

    //checking tested data
    var ws = context.OpenWorkspace(iog.IsolationLevel.ReadOnly);

    ok(ws.Data().GetDictionaryTestData().GetPersonDictionary().Count() == 2,
        'Number of persons in dictionary is 2.'); //Assert.AreEqual(2, ws.Data.PersonDictionary.Count);

    var person1Name = ws.Data().GetDictionaryTestData().GetPersonDictionary().
        Get(0).GetName()

    ok(person1Name == "Changed",
        'Person 0 name is ok!'); //Assert.AreEqual("Changed", ws.Data.PersonDictionary[0].Name);

    var person2Name = ws.Data().GetDictionaryTestData().GetPersonDictionary().
        Get(1).GetName();

    ok(person2Name == "Changed",
        'Person 1 name is ok!'); //Assert.AreEqual("Changed", ws.Data.PersonDictionary[1].Name);

    var person1NameColl = ws.Data().GetDictionaryTestData().GetPersonCollection().First();
    var person2NameColl = ws.Data().GetDictionaryTestData().GetPersonCollection().Last();

    ok(person1NameColl.GetName() == "Changed",
        'Person 0 name is ok!'); //Assert.AreEqual("Changed", ws.Data.PersonDictionary[0].Name);
    ok(person2NameColl.GetName() == "Changed",
        'Person 1 name is ok!'); //Assert.AreEqual("Changed", ws.Data.PersonDictionary[1].Name);

    var numeberOfCollection = ws.Data().GetDictionaryTestData().GetPersonCollection().Count();
    ok(numeberOfCollection == n - 1,
        'Number of elements in collection is appropriate!')
    ws.CloseWorkspace();



});