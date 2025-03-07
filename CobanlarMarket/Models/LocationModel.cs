using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CobanlarMarket.Models
{
    public class LocationModel
    {
    }

    public class Quarter
    {
        public string Name { get; set; }
    }

    public class District
    {
        public string Name { get; set; }
        public List<Quarter> Quarters { get; set; }
    }

    public class Town
    {
        public string Name { get; set; }
        public List<District> Districts { get; set; }
    }

    public class City
    {

        public string Name { get; set; }
        public string Alpha_2_Code { get; set; }
        public List<Town> Towns { get; set; }
    }

    public class TurkeyData
    {
        public List<City> Iller { get; set; }
    }
}