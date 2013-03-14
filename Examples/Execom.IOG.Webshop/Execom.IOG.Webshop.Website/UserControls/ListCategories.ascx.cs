// -----------------------------------------------------------------------
// <copyright file="ListCategories.ascx.cs" company="Execom">
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
using Execom.IOG.Webshop.Service;
using Execom.IOG.Webshop.Data;

namespace Execom.IOG.Webshop.Website.UserControls
{
    /// <summary>
    /// User control for listing categories on page
    /// </summary>
    public partial class ListCategories : System.Web.UI.UserControl
    {
        private Workspace<SystemData> workspace;

        public delegate void PassSelectedCategory(string categoryID);

        /// <summary>
        /// Event that fires when category is selected
        /// </summary>
        public event PassSelectedCategory selectedCategory;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Open workspace on page load
            workspace = WebShopServicesSingleton.Instance.OpenWorkspace(IsolationLevel.ReadOnly);
            var categories = WebShopServicesSingleton.Instance.GetAllCategories(workspace);
            categoryList.DataSource = categories;
            categoryList.DataBind();
        }

        /// <summary>
        /// Command that fires event when some category is selected
        /// </summary>
        /// <param name="Sender">Contol on page</param>
        /// <param name="e">Event arguments</param>
        public void FilterProducts_ItemCommand(Object Sender, RepeaterCommandEventArgs e)
        {
            if (e.CommandName.Equals("FilterProducts_ItemCommand"))
            {
                String categoryID = e.CommandArgument.ToString();
                selectedCategory(categoryID);
            }
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            workspace.Dispose();
        }
    }
}