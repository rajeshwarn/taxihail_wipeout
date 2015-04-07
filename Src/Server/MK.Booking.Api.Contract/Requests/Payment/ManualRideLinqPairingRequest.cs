using apcurium.MK.Common.Entity;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Authenticate]
    [Route("/account/ridelinq", "POST")]
    public class ManualRideLinqPairingRequest : IReturn<OrderManualRideLinqDetail>
    {
        public string PairingCode { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public string ClientLanguageCode { get; set; }
    }
}
