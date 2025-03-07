using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CobanlarMarket.Models
{

    public class SeoModel
    {
        public string Page { get; set; }

        public string Title { get; set; }
        public string Keywords { get; set; }
        public string Description { get; set; }
        public bool NoIndex { get; set; }
        public bool NoFollow { get; set; }
    }
}