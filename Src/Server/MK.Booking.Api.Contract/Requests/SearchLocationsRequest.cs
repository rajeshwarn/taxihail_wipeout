using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RestService("/searchlocation", "GET,OPTIONS")]
    public class SearchLocationsRequest : BaseDTO
    {
        public string Name { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
    }
}
