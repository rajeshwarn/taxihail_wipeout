using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    

    [Authenticate]
    [RestService("/geocode", "GET,OPTIONS")]
    public class GeocodingRequest : BaseDTO
    {
        public string Name{ get; set; }

        public double? Lat{ get; set; }
        
        public double? Lng { get; set; }
    }
}
