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
    
    public partial class sub_subcategories
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public sub_subcategories()
        {
            this.products = new HashSet<products>();
            this.coupon_categories = new HashSet<coupon_categories>();
        }
    
        public string id { get; set; }
        public string name { get; set; }
        public Nullable<int> parent_sub_category_id { get; set; }
        public string description { get; set; }
        public Nullable<System.DateTime> created_at { get; set; }
        public Nullable<System.DateTime> deleted_at { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<products> products { get; set; }
        public virtual sub_categories sub_categories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<coupon_categories> coupon_categories { get; set; }
    }
}
