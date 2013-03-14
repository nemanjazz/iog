<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ViewOrders.ascx.cs"
    Inherits="Execom.IOG.Webshop.Website.UserControls.ViewOrder" %>
<h3>
    Orders</h3>
<br />
<br />
<asp:UpdatePanel runat="server" ID="ordersPanel" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:Repeater ID="OrderListRepeater" runat="server" EnableViewState="true" OnItemDataBound="OrderListRepeater_ItemDataBound">
            <ItemTemplate>
                Order<br />
                <div>
                    <div>
                        <asp:Label ID="lblCustomerName" runat="server">Customer name: </asp:Label>
                        <%# DataBinder.Eval(Container.DataItem, "CustomerName")%>
                    </div>
                    <div>
                        <asp:Label ID="lblCustomerAddress" runat="server">Customer address: </asp:Label>
                        <%# DataBinder.Eval(Container.DataItem, "CustomerAddress")%>
                    </div>
                    <div>
                        <asp:Label ID="lblOrderDate" runat="server">Order date: </asp:Label>
                        <%# DataBinder.Eval(Container.DataItem, "OrderDate")%>
                    </div>
                    <div>
                        <asp:Label ID="lblShipDate" runat="server">Ship date: </asp:Label>
                        <%# DataBinder.Eval(Container.DataItem, "ShipDate")%>
                    </div>
                    <asp:Repeater ID="OrderItemsRepeater" runat="server" EnableViewState="true">
                        <ItemTemplate>
                            <br />
                            <div>
                                <asp:Label ID="lblProductName" runat="server">Product name: </asp:Label>
                                <%# DataBinder.Eval(Container.DataItem, "Product.Name")%><br>
                            </div>
                            <div>
                                <asp:Label ID="lblPrice" runat="server">Price per item: </asp:Label>
                                <%# DataBinder.Eval(Container.DataItem, "Product.Price")%><br>
                            </div>
                            <div>
                                <asp:Label ID="lblQunatity" runat="server">Qunatity: </asp:Label>
                                <%# DataBinder.Eval(Container.DataItem, "Quantity")%><br>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                <br />
                <br />
            </ItemTemplate>
        </asp:Repeater>
    </ContentTemplate>
</asp:UpdatePanel>
