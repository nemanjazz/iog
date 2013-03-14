// -----------------------------------------------------------------------
// <copyright file="ViewOrder.ascx.cs" company="Execom">
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
using System.Data;

namespace Execom.IOG.Webshop.Website.UserControls
{
    /// <summary>
    /// User control for displaying orders
    /// </summary>
    public partial class ViewOrder : System.Web.UI.UserControl
    {
        private Workspace<SystemData> workspace;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Open workspace on page load
            workspace = WebShopServicesSingleton.Instance.OpenWorkspace(IsolationLevel.ReadOnly);
            var orders = WebShopServicesSingleton.Instance.GetAllOrders(workspace);
            OrderListRepeater.DataSource = orders;
            OrderListRepeater.DataBind();
        }

        /// <summary>
        /// Populates page with orders 
        /// </summary>
        /// <param name="sender">Control on page</param>
        /// <param name="e">Event arguments</param>
        protected void OrderListRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item ||
                      e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Repeater tempRpt =
                       (Repeater)e.Item.FindControl("OrderItemsRepeater");
                if (tempRpt != null)
                {
                    Order order = (Order)e.Item.DataItem;
                    tempRpt.DataSource = order.OrderItems;
                    tempRpt.DataBind();
                }
            }
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            workspace.Dispose();
        }
    }
}