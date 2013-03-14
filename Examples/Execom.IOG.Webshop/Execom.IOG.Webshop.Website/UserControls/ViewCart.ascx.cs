// -----------------------------------------------------------------------
// <copyright file="ViewCart.ascx.cs" company="Execom">
// Copyright 2011 EXECOM d.o.o
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Execom.IOG.Webshop.Data;
using Execom.IOG.Webshop.Service;
using Execom.IOG.Types;

namespace Execom.IOG.Webshop.Website.UserControls
{
    /// <summary>
    /// User control for displaying shopping cart
    /// </summary>
    public partial class ViewCart : System.Web.UI.UserControl
    {
        private Workspace<SystemData> workspace;

        /// <summary>
        /// Shopping cart object
        /// </summary>
        protected Dictionary<Product, int> shoppingCart;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Open workspace on page load
            workspace = WebShopServicesSingleton.Instance.OpenWorkspace(IsolationLevel.ReadOnly);

            // See if there is a shopping cart object on session
            if (Session["shoppingCart"] != null)
            {
                Dictionary<String, int> tempShoppingCart = (Dictionary<String, int>)Session["shoppingCart"];
                if (tempShoppingCart != null)
                {
                    shoppingCart = new Dictionary<Product, int>();
                    Product product;
                    foreach (var cartItem in tempShoppingCart)
                    {
                        product = workspace.Data.Products.Single(p => p.ID.Equals(Guid.Parse(cartItem.Key)));
                        shoppingCart.Add(product, cartItem.Value);
                    }
                    CartRepeater.DataSource = shoppingCart;
                    CartRepeater.DataBind();
                }
            }
        }

        /// <summary>
        /// Creates new order based on current shopping cart
        /// </summary>
        /// <param name="Sender">Control on page</param>
        /// <param name="e">Event args</param>
        protected void CheckoutCommand(Object Sender, EventArgs e)
        {
            if (shoppingCart != null && shoppingCart.Count > 0)
            {
                using (var writeWorkspace = WebShopServicesSingleton.Instance.OpenWorkspace(IsolationLevel.Exclusive))
                {
                    // Creating new order
                    Order newOrder = writeWorkspace.New<Order>();
                    newOrder.ID = Guid.NewGuid();
                    newOrder.OrderDate = DateTime.Now;
                    newOrder.ShipDate = DateTime.Now;
                    newOrder.CustomerName = "Admin";
                    newOrder.CustomerAddress = "1st street";
                    // Creating collection of order items
                    newOrder.OrderItems = writeWorkspace.New<IIndexedCollection<OrderItem>>();
                    OrderItem newOrderItem = null;
                    foreach (var product in shoppingCart)
                    {
                        newOrderItem = writeWorkspace.New<OrderItem>();
                        newOrderItem.ID = Guid.NewGuid();
                        newOrderItem.Product = product.Key;
                        newOrderItem.Quantity = product.Value;
                        newOrder.OrderItems.Add(newOrderItem);
                    }

                    // Add newly created order to the collection of orders
                    writeWorkspace.Data.Orders.Add(newOrder);

                    // Commit changes in workspace
                    writeWorkspace.Commit();
                }


                // Forward the page workspace for reading new values
                workspace.Update();

                // Clear shopping cart from session
                Session["shoppingCart"] = null;
                shoppingCart = null;

                // Reload page
                CartRepeater.DataSource = null;
                CartRepeater.DataBind();
                cartPanel.Update(); 
            }
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            workspace.Dispose();
        }
    }
}