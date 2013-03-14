// -----------------------------------------------------------------------
// <copyright file="WebShopServices.cs" company="Execom">
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
using System.Text;
using Execom.IOG.Webshop.Data;
using Execom.IOG.Types;

namespace Execom.IOG.Webshop.Service
{
    /// <summary>
    /// Web shop services
    /// </summary>
    public class WebShopServices : IWebShopServices, IWorkspaceServices<SystemData>
    {
        #region IWebShopServices Members

        #region Categories services implementation

        /// <summary>
        /// Returns all categories in given workspace
        /// </summary>
        /// <param name="workspace">Workspace with data</param>
        /// <returns>Enumeration of all categories in given workspace</returns>
        public IEnumerable<Category> GetAllCategories(Workspace<SystemData> workspace)
        {
            return workspace.Data.Categories;
        }

        /// <summary>
        /// Returns all products of given category
        /// </summary>
        /// <param name="workspace">Workspace with data</param>
        /// <param name="categoryId">Category id</param>
        /// <returns>Enumeration of all products which belongs to given category, null otherwise</returns>
        public IEnumerable<Product> GetCategoryProducts(Workspace<SystemData> workspace, Guid categoryId)
        {
            Category category = GetCategory(workspace, categoryId);
            if (category != null)
            {
                return (from p in workspace.Data.Products where p.Category.ID.Equals(category.ID) select p);
            }
            return null;
        }

        /// <summary>
        /// Creates new category
        /// </summary>
        /// <param name="workspace">Workspace with data</param>
        /// <returns>Newly created category</returns>
        public Category NewCategory(Workspace<SystemData> workspace)
        {
            Category newCategory = workspace.New<Category>();
            newCategory.ID = new Guid();
            return newCategory;
        }

        /// <summary>
        /// Removes category with given id
        /// </summary>
        /// <param name="workspace">Workspace with data</param>
        /// <param name="categoryId">Category ID</param>
        /// <returns>True if categoru is removed, false otherwise</returns>
        public bool RemoveCategory(Workspace<SystemData> workspace, Guid categoryId)
        {
            try
            {
                Category categoryToRemove = GetCategory(workspace, categoryId);
                return workspace.Data.Categories.Remove(categoryToRemove);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Get category with given id
        /// </summary>
        /// <param name="workspace">Workspace with data</param>
        /// <param name="categoryId">Category ID</param>
        /// <returns>Category with given ID if exists, null otherwise</returns>
        public Category GetCategory(Workspace<SystemData> workspace, Guid categoryId)
        {
            try
            {
                return workspace.Data.Categories.Single(c => c.ID.Equals(categoryId));
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Updates category with given id
        /// </summary>
        /// <param name="workspace">Workspace with data</param>
        /// <param name="categoryId">Category ID</param>
        /// <param name="updatedCategory">Category with changed data</param>
        /// <returns>Updated category if update was successful, null otherwise</returns>
        public Category UpdateCategory(Workspace<SystemData> workspace, Guid categoryId, Category updatedCategory)
        {
            Category categoryToUpdate = GetCategory(workspace, categoryId);
            if (categoryId != null)
            {
                categoryToUpdate.Name = updatedCategory.Name;

                // Commit changes
                workspace.Commit();

                return categoryToUpdate;
            }
            return null;
        }

        #endregion

        #region Orders services implementation

        /// <summary>
        /// Returns all orders in given workspace
        /// </summary>
        /// <param name="workspace">Workspace with data</param>
        /// <returns>Enumeration of all orders in given workspace</returns>
        public IEnumerable<Order> GetAllOrders(Workspace<SystemData> workspace)
        {
            return workspace.Data.Orders;
        }

        /// <summary>
        /// Gets order with given id
        /// </summary>
        /// <param name="workspace">Workspace with data</param>
        /// <param name="orderID">Order ID</param>
        /// <returns>Order with given ID, if found, null otherwise</returns>
        public Order GetOrder(Workspace<SystemData> workspace, Guid orderID)
        {
            try
            {
                return workspace.Data.Orders.Single(o => o.ID.Equals(orderID));
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Returns all order items of given order
        /// </summary>
        /// <param name="workspace">Workspace with data</param>
        /// <param name="orderID">Order ID</param>
        /// <returns>Enumeration of all order items</returns>
        public IEnumerable<OrderItem> GetOrderItems(Workspace<SystemData> workspace, Guid orderID)
        {
            Order order = GetOrder(workspace, orderID);
            if (order != null)
            {
                return order.OrderItems;
            }
            return null;
        }

        /// <summary>
        /// Creates new order
        /// </summary>
        /// <param name="workspace">Workspace with data</param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public Order NewOrder(Workspace<SystemData> workspace)
        {
            Order newOrder = workspace.New<Order>();
            newOrder.ID = new Guid();
            newOrder.OrderItems = workspace.New<IIndexedCollection<OrderItem>>();
            return newOrder;
        }

        /// <summary>
        /// Updates order with given id
        /// </summary>
        /// <param name="workspace">Workspace with data</param>
        /// <param name="orderID">Order id</param>
        /// <param name="updatedOrder">Order with changed data</param>
        /// <returns>Updated order if update was successful, null otherwise</returns>
        public Order UpdateOrder(Workspace<SystemData> workspace, Guid orderID, Order updatedOrder)
        {
            Order orderToUpdate = GetOrder(workspace, orderID);
            if (orderToUpdate != null)
            {
                orderToUpdate.CustomerName = updatedOrder.CustomerName;
                orderToUpdate.OrderDate = updatedOrder.OrderDate;
                orderToUpdate.OrderItems = updatedOrder.OrderItems;
                orderToUpdate.ShipDate = updatedOrder.ShipDate;

                // Commit changes
                workspace.Commit();

                return orderToUpdate;
            }
            return null;
        }

        /// <summary>
        /// Removes order with given id
        /// </summary>
        /// <param name="workspace">Workspace with data</param>
        /// <param name="orderID">Order ID</param>
        /// <returns>True if order is removed, false otherwise</returns>
        public bool RemoveOrder(Workspace<SystemData> workspace, Guid orderID)
        {
            try
            {
                Order orderToRemove = GetOrder(workspace, orderID);
                return workspace.Data.Orders.Remove(orderToRemove);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Adds order item to order
        /// </summary>
        /// <param name="workspace">Workspace with data</param>
        /// <param name="orderID">Order ID</param>
        /// <param name="orderItem">Order item</param>
        /// <returns>True if adding was successful, false otherwise</returns>
        public bool AddOrderItem(Workspace<SystemData> workspace, Guid orderID, OrderItem orderItem)
        {
            Order order = GetOrder(workspace, orderID);
            if (order != null)
            {
                if (!order.OrderItems.Contains(orderItem))
                {
                    order.OrderItems.Add(orderItem);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes order item from given order
        /// </summary>
        /// <param name="workspace">Workspace with data</param>
        /// <param name="orderID">Order ID</param>
        /// <param name="orderItemId">Order item ID</param>
        /// <returns>True if removing was sucessful, false otherwise</returns>
        public bool RemoveOrderItem(Workspace<SystemData> workspace, Guid orderID, Guid orderItemId)
        {
            try
            {
                Order order = GetOrder(workspace, orderID);
                OrderItem orderItemToRemove = order.OrderItems.Single(oi => oi.ID.Equals(orderItemId));
                return order.OrderItems.Remove(orderItemToRemove);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Updates order item in order with given id
        /// </summary>
        /// <param name="workspace">Workspace with data</param>
        /// <param name="orderID">Order ID</param>
        /// <param name="updatedOrderItem">Order item with changed data</param>
        /// <returns>Updated order item if update was successful, null otherwise</returns>
        public OrderItem UpdateOrderItem(Workspace<SystemData> workspace, Guid orderID, OrderItem updatedOrderItem)
        {
            try
            {
                Order order = GetOrder(workspace, orderID);
                OrderItem orderItemToUpdate = order.OrderItems.Single(oi => oi.ID.Equals(updatedOrderItem.ID));
                orderItemToUpdate.Quantity = updatedOrderItem.Quantity;

                // Commit changes
                workspace.Commit();

                return orderItemToUpdate;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion

        #region Product services implementation

        /// <summary>
        /// Returns all products in given workspace
        /// </summary>
        /// <param name="workspace">Workspace with data</param>
        /// <returns>Enumeration of all products in given workspace</returns>
        public IEnumerable<Product> GetAllProducts(Workspace<SystemData> workspace)
        {
            return workspace.Data.Products;
        }

        /// <summary>
        /// Gets product with given id
        /// </summary>
        /// <param name="workspace">Workspace with data</param>
        /// <param name="productId">Product ID</param>
        /// <returns>Product with given ID if exists, null otherwise</returns>
        public Product GetProduct(Workspace<SystemData> workspace, Guid productId)
        {
            try
            {
                return workspace.Data.Products.Single(p => p.ID.Equals(productId));
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Updates product with given id
        /// </summary>
        /// <param name="workspace">Workspace with data</param>
        /// <param name="productId">Product ID</param>
        /// <param name="updatedProduct">Product with changed data</param>
        /// <returns>Updated product if update was successful, null otherwise</returns>
        public Product UpdateProduct(Workspace<SystemData> workspace, Guid productId, Product updatedProduct)
        {
            Product productToUpdate = GetProduct(workspace, productId);
            if (productToUpdate != null)
            {
                productToUpdate.Name = updatedProduct.Name;
                if (!updatedProduct.Photo.Equals(""))
                {
                    productToUpdate.Photo = updatedProduct.Photo;
                }
                productToUpdate.Price = updatedProduct.Price;
                productToUpdate.Description = updatedProduct.Description;
                productToUpdate.Category = updatedProduct.Category;

                // Commit changes
                workspace.Commit();

                return productToUpdate;
            }
            return null;
        }

        /// <summary>
        /// Creates new product
        /// </summary>
        /// <param name="workspace">Workspace with data</param>
        /// <returns>Newly created product</returns>
        public Product NewProduct(Workspace<SystemData> workspace)
        {
            Product newProduct = workspace.New<Product>();
            newProduct.ID = new Guid();
            newProduct.Category = workspace.New<Category>();
            return newProduct;
        }

        /// <summary>
        /// Removes product with given id from collection
        /// </summary>
        /// <param name="workspace">Workspace with data</param>
        /// <param name="productId">Product id</param>
        /// <returns>True if product is removed, false otherwise</returns>
        public bool RemoveProduct(Workspace<SystemData> workspace, Guid productId)
        {
            try
            {
                Product productToRemove = GetProduct(workspace, productId);
                return workspace.Data.Products.Remove(productToRemove);
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Order item services implementation

        /// <summary>
        /// Creates new order item
        /// </summary>
        /// <param name="workspace">Workspace with data</param>
        /// <returns>Newly created order item</returns>
        public OrderItem NewOrderItem(Workspace<SystemData> workspace)
        {
            OrderItem newOrderItem = workspace.New<OrderItem>();
            return newOrderItem;
        }

       /// <summary>
       /// Sets order item product
       /// </summary>
       /// <param name="workspace">Workspace with data</param>
       /// <param name="orderId">Order ID</param>
       /// <param name="orderItemId">Order item ID</param>
       /// <param name="productId">Product ID</param>
       /// <returns>Updated order item if updating was succesful, null otherwise</returns>
        public OrderItem SetOrderItemProduct(Workspace<SystemData> workspace, Guid orderId, Guid orderItemId, Guid productId)
        {
            try
            {
                Order order = GetOrder(workspace, orderId);
                OrderItem orderItemToUpdate = order.OrderItems.Single(oi => oi.ID.Equals(orderItemId));
                orderItemToUpdate.Product = workspace.Data.Products.Single(p => p.ID.Equals(productId));

                // Commit changes
                workspace.Commit();

                return orderItemToUpdate;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion

        #endregion

        #region IWorkspaceServices<ISystemData> Members

        /// <summary>
        /// Open workspace
        /// </summary>
        /// <param name="isolationLevel">Isolation level</param>
        /// <returns>Opened workspace</returns>
        public Workspace<SystemData> OpenWorkspace(IsolationLevel isolationLevel)
        {
            return StorageServicesSingleton.Instance.Context.OpenWorkspace<SystemData>(isolationLevel);
        }

        #endregion
    }
}
