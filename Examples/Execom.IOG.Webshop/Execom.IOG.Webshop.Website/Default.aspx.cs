// -----------------------------------------------------------------------
// <copyright file="_Default.ascx.cs" company="Execom">
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
using Execom.IOG.Webshop.Website.UserControls;

namespace Execom.IOG.Webshop.Website
{
    /// <summary>
    /// Default page
    /// </summary>
    public partial class _Default : System.Web.UI.Page
    {
        public String SelectedCategoryID { get; set; }

        protected Dictionary<String, int> shoppingCart;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["shoppingCart"] == null)
                {
                    // Create shopping cart object on session
                    Session["shoppingCart"] = new Dictionary<String, int>();
                }
            }
            // Register events for selecting category and adding to shopping cart
            // Needed for communication between user controls
            ListCategories.selectedCategory += new ListCategories.PassSelectedCategory(PassSelCategoryHandlerMethod);
            ListProducts.addToCart += new ListProducts.AddToCartSelectedProduct(AddToCartHandlerMethod);
        }

        /// <summary>
        /// Pases selected category from one user control to another
        /// </summary>
        /// <param name="categoryID">ID of the selected category</param>
        protected void PassSelCategoryHandlerMethod(string categoryID)
        {
            ListProducts.CategoryID = categoryID;
            ListProducts.UpdateProductList();
        }

        /// <summary>
        /// Adds product with given ID to shopping cart
        /// </summary>
        /// <param name="productID">ID of the product</param>
        protected void AddToCartHandlerMethod(string productID)
        {
            // Get shopping cart object from session
            Dictionary<String, int> shopingCart = (Dictionary<String, int>)Session["shoppingCart"];
            if (shopingCart.Keys.Contains(productID))
            {
                int productCount = shopingCart[productID];
                shopingCart[productID] = ++productCount;
            }
            else
            {
                shopingCart.Add(productID, 1);
            }

            Session["shoppingCart"] = shopingCart;
        }
    }
}
