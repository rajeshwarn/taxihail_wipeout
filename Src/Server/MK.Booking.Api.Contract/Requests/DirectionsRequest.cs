#region

using System;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/directions", "GET")]
    public class DirectionsRequest : BaseDto
    {
        public double? OriginLat { get; set; }
        public double? OriginLng { get; set; }

        public double? DestinationLat { get; set; }
        public double? DestinationLng { get; set; }

        public DateTime? Date { get; set; }
    }
}