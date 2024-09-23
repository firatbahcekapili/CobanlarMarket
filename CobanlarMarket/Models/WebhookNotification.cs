using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CobanlarMarket.Models
{
    public class WebhookNotification
    {
        public string Event { get; set; }
        public string PaymentId { get; set; }
        public string Status { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
    }
}