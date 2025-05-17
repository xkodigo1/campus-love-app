using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace campus_love_app.domain.entities
{
    public class City
    {
        public int CityID { get; set; }
        public string CityName { get; set; } = string.Empty;
        public int RegionID { get; set; }
    }
}