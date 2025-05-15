using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace campus_love_app.domain.entities
{
    public class Region
    {
        public int RegionID { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CountryID { get; set;}
    }
}