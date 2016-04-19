using System;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/vehicles/eta", "POST")]
    public class EtaForPickupRequest : IReturn<EtaForPickupResponse>
    {
        public string VehicleRegistration { get; set; }

        public double? Longitude { get; set; }

        public double? Latitude { get; set; }

        public Guid OrderId { get; set; }
    }


    public class EtaForPickupResponse
    {
        public long? Eta { get; set; }

        public double? Longitude { get; set; }

        public double? Latitude { get; set; }

        public string Market { get; set; }
        
        public double? CompassCourse { get; set; }
    }
}
