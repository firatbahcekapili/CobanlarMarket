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
    
    public partial class products_skus
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public products_skus()
        {
            this.cart_item = new HashSet<cart_item>();
            this.order_item = new HashSet<order_item>();
        }
    
        public int id { get; set; }
        public Nullable<int> product_id { get; set; }
        public Nullable<int> size_attribute_id { get; set; }
        public Nullable<int> color_attribute_id { get; set; }
        public string sku { get; set; }
        public Nullable<decimal> price { get; set; }
        public Nullable<decimal> old_price { get; set; }
        public Nullable<int> quantity { get; set; }
        public Nullable<System.DateTime> created_at { get; set; }
        public Nullable<System.DateTime> deleted_at { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<cart_item> cart_item { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<order_item> order_item { get; set; }
        public virtual products products { get; set; }
    }
}
