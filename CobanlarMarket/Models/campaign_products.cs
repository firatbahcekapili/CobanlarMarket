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
    
    public partial class campaign_products
    {
        public int id { get; set; }
        public int product_id { get; set; }
        public int camapign_id { get; set; }
    
        public virtual products products { get; set; }
        public virtual campaigns campaigns { get; set; }
    }
}