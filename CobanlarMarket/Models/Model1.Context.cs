﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class CobanlarMarketEntities : DbContext
    {
        public CobanlarMarketEntities()
            : base("name=CobanlarMarketEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<addresses> addresses { get; set; }
        public virtual DbSet<campaign_products> campaign_products { get; set; }
        public virtual DbSet<cart> cart { get; set; }
        public virtual DbSet<categories> categories { get; set; }
        public virtual DbSet<coupon_categories> coupon_categories { get; set; }
        public virtual DbSet<coupon_products> coupon_products { get; set; }
        public virtual DbSet<coupons> coupons { get; set; }
        public virtual DbSet<order_details> order_details { get; set; }
        public virtual DbSet<order_item> order_item { get; set; }
        public virtual DbSet<payment_details> payment_details { get; set; }
        public virtual DbSet<product_attributes> product_attributes { get; set; }
        public virtual DbSet<product_images> product_images { get; set; }
        public virtual DbSet<products> products { get; set; }
        public virtual DbSet<products_skus> products_skus { get; set; }
        public virtual DbSet<sub_categories> sub_categories { get; set; }
        public virtual DbSet<users> users { get; set; }
        public virtual DbSet<wishlist> wishlist { get; set; }
        public virtual DbSet<sub_subcategories> sub_subcategories { get; set; }
        public virtual DbSet<cart_item> cart_item { get; set; }
        public virtual DbSet<notification> notification { get; set; }
        public virtual DbSet<campaigns> campaigns { get; set; }
        public virtual DbSet<company_details> company_details { get; set; }
        public virtual DbSet<newsletter> newsletter { get; set; }
    }
}
