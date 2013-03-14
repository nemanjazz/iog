<%@ Page Title="Webshop example" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="Execom.IOG.Webshop.Website._Default" %>
<%@ Register TagPrefix="execom" TagName="ListCategories" Src="~/UserControls/ListCategories.ascx" %>
<%@ Register TagPrefix="execom" TagName="ListProducts" Src="~/UserControls/ListProducts.ascx" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <div class="leftCol">
        <execom:ListCategories runat="server" ID="ListCategories"></execom:ListCategories>
    </div>
    <div class="main">
        <execom:ListProducts runat="server" ID="ListProducts"></execom:ListProducts>
    </div>
</asp:Content>
