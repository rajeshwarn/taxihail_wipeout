using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.MapDataProvider.Resources
{
    public class OSMAddress
    {
        public string house_number { get; set; }
        public string road { get; set; }
        public string suburb { get; set; }
        public string city { get; set; }
        public string state_district { get; set; }
        public string state { get; set; }
        public string postcode { get; set; }
        public string country { get; set; }
        public string country_code { get; set; }
    }
}
