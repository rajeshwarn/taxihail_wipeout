using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/vehicles/eta", "POST")]
    public class EtaForPickupRequest : IReturn<EtaForPickupResponse>
    {
        public string VehicleRegistration { get; set; }

        public double? Longitude { get; set; }

        public double? Latitude { get; set; }
    }


    public class EtaForPickupResponse
    {
        public long? Eta { get; set; }

        public double? Longitude { get; set; }

        public double? Latitude { get; set; }
    }
}
