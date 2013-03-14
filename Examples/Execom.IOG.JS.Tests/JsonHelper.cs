using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace Execom.IOG.JS.Tests
{
    /// <summary>
    /// Json helper that add to every object static method that translate any object to json object in string 
    /// form.
    /// </summary>
    /// <author>Ivan Vasiljevic</author>
    public static class JsonHelper
    {

        public static string ToJSON(this object obj)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(obj);
        }

        public static string ToJSON(this Dictionary<Object, Object> obj)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(obj);
        }
    }
}