<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="Execom.IOG.JS.Tests._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <script src="Scripts/integration.tests/change.notification.test.js" type="text/javascript"></script>
    <script src="Scripts/integration.tests/rollback.update.test.js" type="text/javascript"></script>
    <script src="Scripts/integration.tests/banking.test.js" type="text/javascript"></script>
    <script src="Scripts/integration.tests/type.test.js" type="text/javascript"></script>
    <script src="Scripts/integration.tests/collection.iog.test.js" type="text/javascript"></script>
    <script src="Scripts/core.tests/bplustree.test.js" type="text/javascript"></script>
    <script src="Scripts/core.tests/array.compare.test.js" type="text/javascript"></script>
    <script src="Scripts/core.tests/dictionary.test.js" type="text/javascript"></script>
    <script src="Scripts/core.tests/edge.data.test.js" type="text/javascript"></script>
    <script src="Scripts/core.tests/enumeration.test.js" type="text/javascript"></script>
    <script src="Scripts/core.tests/memory.storage.test.js" type="text/javascript"></script>
    <script src="Scripts/core.tests/node.test.js" type="text/javascript"></script>
    <script src="Scripts/core.tests/object.equals.test.js" type="text/javascript"></script>
    <script src="Scripts/core.tests/sorted.list.test.js" type="text/javascript"></script>
    <script src="Scripts/core.tests/table.element.test.js" type="text/javascript"></script>
    <script src="Scripts/core.tests/uuid.test.js" type="text/javascript"></script>

    <script src="/signalr/hubs" type="text/javascript"></script>
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h1 id="qunit-header">QUnit Test Suite</h1>
    <h2 id="qunit-banner"></h2>
    <div id="qunit-testrunner-toolbar"></div>
    <h2 id="qunit-userAgent"></h2>
    <ol id="qunit-tests"></ol>
    <div id="qunit-fixture">test markup</div>
</asp:Content>
