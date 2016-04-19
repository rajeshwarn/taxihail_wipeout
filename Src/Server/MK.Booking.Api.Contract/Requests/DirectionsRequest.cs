#region

using System;
using apcurium.MK.Common.Http;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/directions", "GET")]
    public class DirectionsRequest : BaseDto
    {
        public double? OriginLat { get; set; }
        public double? OriginLng { get; set; }

        public double? DestinationLat { get; set; }
        public double? DestinationLng { get; set; }

        public DateTime? Date { get; set; }

        public int? VehicleTypeId { get; set; }
    }
}