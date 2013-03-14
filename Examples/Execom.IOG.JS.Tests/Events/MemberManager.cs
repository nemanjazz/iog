using System;
using System.Web.Routing;
using SignalR;
using SignalR.Hubs;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Execom.IOG.JS.Tests
{
    public class MemberManager
    {
        // key is subscriptionID, value is connectionID
        public static ConcurrentDictionary<String, String> users = new ConcurrentDictionary<String, String>(); 

        public static void AddSubscription(string connectionId, string subscriptionId)
        {
            if (!users.ContainsKey(subscriptionId))
            {
                users[subscriptionId] = connectionId;
            }
        }

        public static string FindConnection(string subscriptionId)
        {
            return users[subscriptionId];
        }

        public static void RemoveSubscription(string connectionId, string subscriptionId)
        {
            if (users.ContainsKey(subscriptionId))
            {
                String value = null;
                users.TryRemove(subscriptionId, out value);
            }
        }

        public static void RemoveSubscription(string connectionId)
        {
            String valueOut = null;
            var rez = from x in users
                      where x.Value == connectionId
                      select x;

            foreach (var ele in rez)
            {
                users.TryRemove(ele.Key, out valueOut);
            }

        }
    }
}