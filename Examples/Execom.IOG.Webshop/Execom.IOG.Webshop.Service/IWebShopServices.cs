// -----------------------------------------------------------------------
// <copyright file="IWebShopServices.cs" company="Execom">
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

using System.Collections.Generic;
using System.Linq;
using Execom.IOG.Webshop.Data;
using System;

namespace Execom.IOG.Webshop.Service
{
    /// <summary>
    /// Web shop services interface
    /// </summary>
    public interface IWebShopServices
    {
        #region Product services

        IEnumerable<Product> GetAllProducts(Workspace<SystemData> workspace);
        Product GetProduct(Workspace<SystemData> workspace, Guid productId);
        Product UpdateProduct(Workspace<SystemData> workspace, Guid productId, Product updatedProduct);
        Product NewProduct(Workspace<SystemData> workspace);
        bool RemoveProduct(Workspace<SystemData> workspace, Guid productId);

        #endregion

        #region Category services

        IEnumerable<Category> GetAllCategories(Workspace<SystemData> workspace);
        IEnumerable<Product> GetCategoryProducts(Workspace<SystemData> workspace, Guid categoryId);
        Category GetCategory(Workspace<SystemData> workspace, Guid categoryId);
        Category UpdateCategory(Workspace<SystemData> workspace, Guid categoryId, Category updatedCategory);
        Category NewCategory(Workspace<SystemData> workspace);
        bool RemoveCategory(Workspace<SystemData> workspace, Guid categoryId);

        #endregion

        #region Order services

        IEnumerable<Order> GetAllOrders(Workspace<SystemData> workspace);
        Order GetOrder(Workspace<SystemData> workspace, Guid orderID);
        IEnumerable<OrderItem> GetOrderItems(Workspace<SystemData> workspace, Guid orderID);
        Order NewOrder(Workspace<SystemData> workspace);
        Order UpdateOrder(Workspace<SystemData> workspace, Guid orderID, Order updatedOrder);
        bool RemoveOrder(Workspace<SystemData> workspace, Guid orderID);
        bool AddOrderItem(Workspace<SystemData> workspace, Guid orderID, OrderItem orderItem);
        bool RemoveOrderItem(Workspace<SystemData> workspace, Guid orderID, Guid orderItemId);
        OrderItem UpdateOrderItem(Workspace<SystemData> workspace, Guid orderID, OrderItem updatedOrderItem);

        #endregion

        #region Order item services

        OrderItem NewOrderItem(Workspace<SystemData> workspace);
        OrderItem SetOrderItemProduct(Workspace<SystemData> workspace, Guid orderId, Guid orderItemId, Guid productId);

        #endregion
    }
}
