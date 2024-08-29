using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CobanlarMarket.Models
{
    public class AllViewModel
    {

        public List<products> products;
        public List<products_skus> products_skus;

        public List<categories> categories;
        public List<sub_categories> sub_categories;
        public List<cart> carts;
        public List<users> users;
        public List<product_attributes> product_attributes;
        public List<order_details> order_details;
        public List<order_item> order_item;
        public List<coupons> coupons;
        public List<campaigns> campaigns;




    }
}