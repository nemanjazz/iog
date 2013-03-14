<%@ Page Title="Orders" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="OrderPage.aspx.cs" Inherits="Execom.IOG.Webshop.Website.OrderPage" %>
<%@ Register TagPrefix="execom" TagName="ViewOrders" Src="~/UserControls/ViewOrders.ascx" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <div class="main">
        <execom:ViewOrders runat="server" ID="ViewOrders"></execom:ViewOrders>
    </div>
</asp:Content>
