module('Banking test');
test("Banking test", function () {
    var iog = window.execom.iog;
    var context = new iog.ClientContext('localhost:3681', "ServerContext.asmx");
    var workspace = context.OpenWorkspace(iog.IsolationLevel.Exclusive);

    //getting bank proxy object
    var bankProxy = workspace.Data().GetBank();

    if (bankProxy == null) {
        bankProxy = workspace.New(iog.IBankDataType);
        workspace.Data().SetBank(bankProxy);
        bankProxy = null;
    }

    bankProxy = workspace.Data().GetBank();

    ok(bankProxy != null, 'Bank proxy is not NULL!');

    var nrThreads = 10;
    var nrRepeats = 3; //should be 50, but it is too slow now!
    var nrUsers = 5;
    var nrAccounts = 5;

    var newUsers = workspace.New(iog.IOGType.CreateCollectionWithGenericType(iog.IUserType));
    bankProxy.SetUsers(newUsers);

    for (usr = 0; usr < nrUsers; usr++) {
        var user = workspace.New(iog.IUserType);
        user.SetID(iog.Guid.Create());
        user.SetName("User" + usr);
        user.SetIndex(usr);
        var newAccounts = workspace.New(iog.IOGType.CreateCollectionWithGenericType(iog.IAccountType));
        user.SetAccounts(newAccounts);
        bankProxy.GetUsers().Add(user);

        for (acc = 0; acc < nrAccounts; acc++) {
            var account = workspace.New(iog.IAccountType);
            account.SetID(iog.Guid.Create());
            account.SetIndex(acc);
            var newTransactions = workspace.New(iog.IOGType.CreateCollectionWithGenericType(iog.ITransactionType));
            account.SetTransactions(newTransactions);
            user.GetAccounts().Add(account);
        }

        workspace.Commit();
    }

    workspace.CloseWorkspace();
    bankProxy = null;


    //Make modifications
    for (i = 0; i < nrRepeats; i++) {
        //Random r = new Random();

        var snapshotId = context.LastSnapshotId();

        for (iter = 0; iter < nrThreads; iter++) {
            var ws = context.OpenWorkspace(iog.IsolationLevel.Snapshot, snapshotId);
            bankProxy = ws.Data().GetBank();

            ok(bankProxy != null, 'Bank proxy is not NULL!');

            var actual = 0;

            // enumerator is used because in js there is no foreach, like in 
            // c#
            var userEnumerator = bankProxy.GetUsers().GetEnumerator();

            while (userEnumerator.MoveNext()) {
                actual++;
            }
            ok(nrUsers == actual,
                'Number of user that are found in database is equal to number of created user!');
            //if (nrUsers != actual)
            //    alert("Number of user that are found in database is not right!");

            var firstUserIndex = Math.floor((Math.random() * nrUsers)); //r.Next(nrUsers);
            var secondUserIndex = Math.floor((Math.random() * nrUsers)); //r.Next(nrUsers);

            while (firstUserIndex == secondUserIndex) {
                firstUserIndex = Math.floor((Math.random() * nrUsers)); //r.Next(nrUsers);
                secondUserIndex = Math.floor((Math.random() * nrUsers)); //r.Next(nrUsers);
            }


            var firstUser = null;
            userEnumerator.Reset();
            while (userEnumerator.MoveNext()) {
                var user = userEnumerator.Current();
                var index = user.GetIndex();
                if (index == firstUserIndex) {

                    firstUser = user;
                    break;
                }
            }
            //ws.Data().GetUsers().First(o => o.Index.Equals(firstUserIndex));
            var secondUser = null; //ws.Data().GetUsers().First(o => o.Index.Equals(secondUserIndex));
            userEnumerator.Reset();
            while (userEnumerator.MoveNext()) {
                var user = userEnumerator.Current();
                var index = user.GetIndex();
                if (index == secondUserIndex) {
                    secondUser = user;
                    break;
                }
            }

            var firstAccountIndex = Math.floor((Math.random() * nrAccounts)); //r.Next(nrAccounts);
            var secondAccountIndex = Math.floor((Math.random() * nrAccounts)); //r.Next(nrAccounts);

            var firstUserAccountsEnumerator = firstUser.GetAccounts().GetEnumerator();
            var secondUserAccountsEnumerator = secondUser.GetAccounts().GetEnumerator();

            var firstAccount = null; //firstUser.Accounts.First(o => o.Index.Equals(firstAccountIndex));
            var secondAccount = null; //secondUser.Accounts.First(o => o.Index.Equals(secondAccountIndex));

            while (firstUserAccountsEnumerator.MoveNext()) {
                var account = firstUserAccountsEnumerator.Current();
                var index = account.GetIndex();
                if (index == firstAccountIndex) {
                    firstAccount = account;
                    break;
                }
            }

            while (secondUserAccountsEnumerator.MoveNext()) {
                var account = secondUserAccountsEnumerator.Current();
                var index = account.GetIndex();
                if (index == secondAccountIndex) {
                    secondAccount = account;
                    break;
                }
            }

            var transaction = ws.New(iog.ITransactionType);
            transaction.SetID(iog.Guid.Create());
            transaction.SetAmount(1);
            transaction.SetFrom(firstAccount);
            transaction.SetTo(secondAccount);

            firstAccount.GetTransactions().Add(transaction);
            secondAccount.GetTransactions().Add(transaction);

            ws.Commit();

            ws.CloseWorkspace();
            bankProxy = null;

        }
    }

    var ws = context.OpenWorkspace(iog.IsolationLevel.ReadOnly);
    bankProxy = ws.Data().GetBank();

    ok(bankProxy != null, 'Bank proxy is not NULL!');

    var totalSum = 0;

    for (usr = 0; usr < nrUsers; usr++) {
        var user = null;
        var userEnumerator = bankProxy.GetUsers().GetEnumerator(); //.First(o => o.Index.Equals(usr));

        while (userEnumerator.MoveNext()) {
            var currentUser = userEnumerator.Current();
            if (currentUser.GetIndex() == usr) {
                user = currentUser;
                break;
            }
        }
        ok(user != null, 'Right user is found!');
        //if (user == null) {
        //    alert("User is not found!");
        //    return;
        //}
        ok(user.GetName() == "User" + usr, 'Founded user is right!');
        /*if (user.GetName() != "User" + usr) {
        alert("Founded user is not right!");
        }*/
        ok(user.GetIndex() == usr, 'Founded user is right!');
        /*if (user.GetIndex() != usr) {
        alert("Founded user is not right!");
        }*/

        //Assert.AreEqual(user.Name, "User" + usr);
        //Assert.AreEqual(user.Index, usr);

        var userSum = 0;

        for (acc = 0; acc < nrAccounts; acc++) {
            var account = null; //user.Accounts.First(o => o.Index.Equals(acc));
            var accountEnum = user.GetAccounts().GetEnumerator();
            while (accountEnum.MoveNext()) {
                var currentAccount = accountEnum.Current();
                if (currentAccount.GetIndex() == acc) {
                    account = currentAccount;
                    break;
                }
            }
            //Assert.AreEqual(account.Index, acc);
            ok(account.GetIndex() == acc, 'Founded account is right!');
            /*if (account.GetIndex() != acc) {
            alert("Founded account is not right!");
            }*/
            var accountSum = 0;

            var accountEnum = account.GetTransactions().GetEnumerator();

            while (accountEnum.MoveNext()) {
                var trans = accountEnum.Current();
                var from = trans.GetFrom();
                var to = trans.GetTo();

                if (from.GetID() != to.GetID() ) {
                    if (trans.GetFrom().GetID() == account.GetID()) {
                        accountSum -= trans.GetAmount();
                    }
                    else {
                        if (trans.GetTo().GetID() == account.GetID()) {
                            accountSum += trans.GetAmount();
                        }
                        else {
                            alert("Transaction error");
                            return;
                        }
                    }
                }
                else {
                    alert("Error");
                    return;
                }

            }

            userSum += accountSum;
        }

        //Debug.WriteLine("User" + usr + " = " + userSum);

        totalSum += userSum;
    }
    ok(totalSum == 0, 'TotalSum is equal 0');
    /*if (totalSum != 0) {
    alert("TotalSum is not equal 0!");
    }*/

    ws.CloseWorkspace();
    bankProxy = null;
});