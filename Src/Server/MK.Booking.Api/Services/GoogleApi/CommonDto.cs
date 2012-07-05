using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Api.Services.GoogleApi
{
    public enum ResultStatus
    {
        UNKNOWN,
        OK,
        ZERO_RESULTS,
        OVER_QUERY_LIMIT,
        REQUEST_DENIED,
        INVALID_REQUEST,

    }

    public class Location
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }



    public class Viewport
    {
        public Location Northeast { get; set; }
        public Location Southwest { get; set; }
    }





    public class Bounds
    {
        public Location Northeast { get; set; }
        public Location Southwest { get; set; }
    }

}
