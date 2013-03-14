// -----------------------------------------------------------------------
// <copyright file="ListProducts.ascx.cs" company="Execom">
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

namespace Execom.IOG.Webshop.Website.UserControls
{
    /// <summary>
    /// User control for listing products for given category
    /// </summary>
    public partial class ListProducts : System.Web.UI.UserControl
    {
        private Workspace<SystemData> workspace;

        public delegate void AddToCartSelectedProduct(string productID);

        /// <summary>
        /// Event that fires when product is added to shopping cart
        /// </summary>
        public event AddToCartSelectedProduct addToCart;

        /// <summary>
        /// ID of the selected category
        /// </summary>
        public String CategoryID { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Open workspace on page load
            workspace = WebShopServicesSingleton.Instance.OpenWorkspace(IsolationLevel.ReadOnly);
        }

        /// <summary>
        /// Command for adding item to shopping cart
        /// </summary>
        /// <param name="Sender">Control on page</param>
        /// <param name="e">Event arguments</param>
        public void AddToCart_ItemCommand(Object Sender, RepeaterCommandEventArgs e)
        {
            if (e.CommandName.Equals("AddToCart_ItemCommand"))
            {
                String productID = e.CommandArgument.ToString();
                addToCart(productID);
            }
        }

        /// <summary>
        /// Populate page with products in given category
        /// </summary>
        public void UpdateProductList()
        {
            var filteredProducts = WebShopServicesSingleton.Instance.GetCategoryProducts(workspace, Guid.Parse(CategoryID));
            productList.DataSource = filteredProducts;
            productList.DataBind();
            productsPanel.Update();
        }


        protected override void OnUnload(EventArgs e)
        {
            // Always close workspace
            base.OnUnload(e);
            workspace.Dispose();
        }
    }
}