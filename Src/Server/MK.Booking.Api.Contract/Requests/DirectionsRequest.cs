using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RestService("/directions", "GET")]
    public class DirectionsRequest : BaseDTO
    {
        public double? OriginLat { get; set; }
        public double? OriginLng { get; set; }

        public double? DestinationLat { get; set; }
        public double? DestinationLng { get; set; }

        public DateTime? Date { get; set; }
    }
}
