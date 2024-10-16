using Microsoft.AspNet;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CobanlarMarket;
using CobanlarMarket.Models;

namespace CobanlarMarket.Hubs
{
    public class NotificationHub : Hub
    {
        public void SendNotification(string message, List<order_item> orderItems, List<payment_details> pd, List<order_details> od, List<notification> notifications)
        {
            Clients.All.receiveNotification(message, orderItems, pd, od, notifications);
        }
    }
}
