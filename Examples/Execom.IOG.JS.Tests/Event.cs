using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SignalR.Hubs;
using Execom.IOG.Events;

namespace Execom.IOG.JS.Tests
{
    [HubName("eventsHub")]
    public class Event : Hub, IDisconnect, IConnected
    {
        public void Send(String msg)
        {
            Clients[Context.ConnectionId].sendEvent(msg.ToJSON());
        }

        public System.Threading.Tasks.Task Connect()
        {
            return Clients.joined(Context.ConnectionId, DateTime.Now.ToString());
        }

        public System.Threading.Tasks.Task Reconnect(IEnumerable<string> groups)
        {
            return Clients.rejoined(Context.ConnectionId, DateTime.Now.ToString());
        }

        public System.Threading.Tasks.Task Disconnect()
        {
            MemberManager.RemoveSubscription(Context.ConnectionId);
            return Clients.leave(Context.ConnectionId, DateTime.Now.ToString());
        }
    }
}