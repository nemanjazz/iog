<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ListProducts.ascx.cs"
    Inherits="Execom.IOG.Webshop.Website.UserControls.ListProducts" %>
    <h3>Products:</h3>
    <br />
<asp:UpdatePanel runat="server" ID="productsPanel" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:Repeater ID="productList" runat="server"  OnItemCommand="AddToCart_ItemCommand" EnableViewState="true">
            <HeaderTemplate>
                <table width="400" cellpadding = "10" cellspacing="10">
                    <tr>
                        <th>
                            Picture
                        </th>
                        <th>
                            Name
                        </th>
                        <th>
                            Description
                        </th>
                        <th>
                            Price
                        </th>
                        <th>
                        </th>
                    </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr align="center">
                    <td>
                        <img class="profile_image" width="100" height="100" src="<%# DataBinder.Eval(Container.DataItem, "Photo") %>"
                            alt="" />
                    </td>
                    <td>
                        <%# DataBinder.Eval(Container.DataItem, "Name") %>
                    </td>
                    <td>
                        <%# DataBinder.Eval(Container.DataItem, "Description") %>
                    </td>
                    <td>
                        <%# DataBinder.Eval(Container.DataItem, "Price") %>
                    </td>
                    <td>
                        <asp:Button ID="btnAddToCart" runat="server" Text="Add to cart" CommandName="AddToCart_ItemCommand"
                            CommandArgument='<%# DataBinder.Eval(Container.DataItem, "ID") %>'>
                        </asp:Button>
                    </td>
                </tr>
                </table>
            </ItemTemplate>
        </asp:Repeater>
    </ContentTemplate>
</asp:UpdatePanel>
