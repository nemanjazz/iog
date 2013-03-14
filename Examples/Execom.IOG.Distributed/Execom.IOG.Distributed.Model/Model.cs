using System;
using System.Collections.Generic;
using System.Text;

namespace Execom.IOG.Distributed.Model
{
    public interface IProduct
    {
        string Name { get; set; }
        int Price { get; set; }
    }

    public interface IOrder
    {
        DateTime Date { get; set; }
        IUser User { get; set; }
        IProduct Product { get; set; }
    }

    public interface IUser
    {
        string Username { get; set; }
        int Age { get; set; }
    }

    public interface IDataModel
    {
        ICollection<IUser> Users { get; set; }
        ICollection<IProduct> Products { get; set; }
        ICollection<IOrder> Orders { get; set; }
    }
}
