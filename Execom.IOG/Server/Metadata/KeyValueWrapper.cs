using System;
using System.Collections.Generic;
using System.Text;

namespace Execom.IOG.Server.Metadata
{
    /// <summary>
    /// This class is representing KeyValue pair, where Key is always EdgeDataWrapper and Value is always EdgeWrapper.
    /// This class is POCO class. It is used for sending data on js client.
    /// </summary>
    /// <author>Ivan Vasiljevic</author>
    public class KeyValueWrapper
    {

        public KeyValueWrapper()
        {

        }

        public KeyValueWrapper(EdgeDataWrapper key, EdgeWrapper value)
        {
            this.Key = key;
            this.Value = value;
        }

        public EdgeDataWrapper Key
        {
            get;
            set;
        }

        public EdgeWrapper Value
        {
            get;
            set;
        }
    }
}
