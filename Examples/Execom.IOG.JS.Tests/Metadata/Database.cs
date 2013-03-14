using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Execom.IOG.Attributes;

namespace Execom.IOG.JS.Tests.Metadata
{
    [Concurrent]
    public interface IDatabase
    {
        IBankData Bank { get; set; }
        ITypeTest TypeTestData { get; set; }
        IDictionaryTest DictionaryTestData { get; set; }
        ICollection<IAppUser> Users { get; set; }
        IDriver Driver { get; set; }
    }

    public interface ICar
    {
        string Model {get; set;}
    }

    public interface IDriver
    {
        string Name { get; set; }
        ICar Car { get; set; }
    }

    public interface IAppUser
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string Email { get; set; }
        DateTime Birthday { get; set; }
        ESex Sex { get; set; }
        Double Salary { get; set; }
        Int32 NumberOfChildren { get; set; }
    }

    public enum ESex
    {
        FEMALE,
        MALE
    }

    public interface IPerson
    {
        [PrimaryKey]
        string Name { get; set; }
        int Age { get; set; }
    }

    public interface IDictionaryTest
    {
        ICollection<int> IntCollection { get; set; }
        ICollection<IPerson> PersonCollection { get; set; }
        IDictionary<int, IPerson> PersonDictionary { get; set; }
    }

    public interface IBaseEntity
    {
        Guid ID { get; set; }
    }

    [Concurrent]
    public interface IUser : IBaseEntity
    {
        String Name { get; set; }
        int Index { get; set; }
        ICollection<IAccount> Accounts { get; set; }
    }

    [Concurrent]
    public interface IAccount : IBaseEntity
    {
        int Index { get; set; }
        ICollection<ITransaction> Transactions { get; set; }
    }

    [Concurrent]
    public interface ITransaction : IBaseEntity
    {
        IAccount From { get; set; }
        IAccount To { get; set; }
        int Amount { get; set; }
    }

    [Concurrent]
    public interface IBankData
    {
        ICollection<IUser> Users { get; set; }
    }

    public interface ITypeTest
    {
        Int32 Int32Type { get; set; }
        Int64 Int64Type { get; set; }
        Boolean BooleanType { get; set; }
        String StringType { get; set; }
        Double DoubleType { get; set; }
        Byte ByteType { get; set; }
        //Char CharType { get; set; }
        Guid GuidType { get; set; }
        DateTime DateTimeType { get; set; }
        TimeSpan TimeSpanType { get; set; }
    }
}