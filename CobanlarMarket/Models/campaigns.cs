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
    
    public partial class campaigns
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public campaigns()
        {
            this.campaign_products = new HashSet<campaign_products>();
        }
    
        public int id { get; set; }
        public string campaign_title { get; set; }
        public System.DateTime campaign_start_date { get; set; }
        public System.DateTime campaign_end_date { get; set; }
        public string campaign_cover { get; set; }
        public Nullable<bool> is_active { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<campaign_products> campaign_products { get; set; }
    }
}
