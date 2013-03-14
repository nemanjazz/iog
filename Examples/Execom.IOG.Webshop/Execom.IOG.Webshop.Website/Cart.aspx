<%@ Page Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Cart.aspx.cs" Inherits="Execom.IOG.Webshop.Website.Cart" %>
<%@ Register TagPrefix="execom" TagName="ViewCart" Src="~/UserControls/ViewCart.ascx" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <div class="main">
        <execom:ViewCart runat="server" ID="ViewCart"></execom:ViewCart>
    </div>
</asp:Content>
