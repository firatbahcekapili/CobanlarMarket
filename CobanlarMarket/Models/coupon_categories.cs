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
    
    public partial class coupon_categories
    {
        public int id { get; set; }
        public int coupon_id { get; set; }
        public Nullable<int> category_id { get; set; }
        public Nullable<int> subcategory_id { get; set; }
        public string sub_subcategory_id { get; set; }
    
        public virtual categories categories { get; set; }
        public virtual coupons coupons { get; set; }
        public virtual sub_categories sub_categories { get; set; }
        public virtual sub_subcategories sub_subcategories { get; set; }
    }
}
