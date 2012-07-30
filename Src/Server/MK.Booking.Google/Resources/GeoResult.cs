using System;
using System.Collections.Generic;
using System.Text;

namespace apcurium.MK.Booking.Google.Resources
{
    public class GeoResult
    {
        public ResultStatus Status { get; set; }
        public List<GeoObj> Results { get; set; }
    }
}
