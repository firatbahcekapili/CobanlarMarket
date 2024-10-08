//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CobanlarMarket.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class payment_details
    {
        public int id { get; set; }
        public Nullable<int> order_id { get; set; }
        public Nullable<decimal> amount { get; set; }
        public string provider { get; set; }
        public string status { get; set; }
        public Nullable<System.DateTime> created_at { get; set; }
        public Nullable<System.DateTime> updated_at { get; set; }
        public string cardType { get; set; }
        public string cardFamily { get; set; }
        public Nullable<int> installment { get; set; }
        public Nullable<decimal> paidPrice { get; set; }
        public string paymentId { get; set; }
        public Nullable<decimal> cargoPrice { get; set; }
        public Nullable<decimal> couponDiscountValue { get; set; }
        public Nullable<int> couponId { get; set; }
    
        public virtual coupons coupons { get; set; }
        public virtual order_details order_details { get; set; }
    }
}
