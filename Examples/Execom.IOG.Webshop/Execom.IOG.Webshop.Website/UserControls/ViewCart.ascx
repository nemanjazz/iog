<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ViewCart.ascx.cs" Inherits="Execom.IOG.Webshop.Website.UserControls.ViewCart" %>
<h3>
    Products in cart
</h3>
<br />
<asp:UpdatePanel runat="server" ID="cartPanel" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:Repeater ID="CartRepeater" runat="server" EnableViewState="true">
            <HeaderTemplate>
                <table width="400" cellpadding="10" cellspacing="10">
                    <tr>
                        <th>
                            Product name
                        </th>
                        <th>
                            Quantity
                        </th>
                        <th>
                            Price per item
                        </th>
                    </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr align="center">
                    <td>
                        <%# DataBinder.Eval(Container.DataItem, "key.Name") %>
                    </td>
                    <td>
                        <%# DataBinder.Eval(Container.DataItem, "value") %>
                    </td>
                    <td>
                        <%# DataBinder.Eval(Container.DataItem, "key.Price") %>
                    </td>
                </tr>
            </ItemTemplate>
        </asp:Repeater>
        <div>
            <asp:Button ID="btnSCCheckout" runat="server" Text="Checkout" OnClick="CheckoutCommand" />
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
