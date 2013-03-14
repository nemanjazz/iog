<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ListCategories.ascx.cs"
    Inherits="Execom.IOG.Webshop.Website.UserControls.ListCategories" %>
    <br />
<asp:UpdatePanel runat="server" ID="categoriesPanel" UpdateMode="Conditional">
    <ContentTemplate>
            <asp:Repeater ID="categoryList" OnItemCommand="FilterProducts_ItemCommand" runat="server" EnableViewState="true">
                <ItemTemplate>
                    <div>
                        <asp:LinkButton ID="btnFilterProducts" runat="server" Text="" CommandName="FilterProducts_ItemCommand"
                            CommandArgument='<%# DataBinder.Eval(Container.DataItem, "ID") %>'>
                                        <%# DataBinder.Eval(Container.DataItem, "Name") %>
                        </asp:LinkButton>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
    </ContentTemplate>
</asp:UpdatePanel>
