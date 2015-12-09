using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Authenticate]
	[NoCache]
    [Route("/account/manualridelinq/pair", "POST")]
    public class ManualRideLinqPairingRequest : IReturn<OrderManualRideLinqDetail>
    {
        public string PairingCode { get; set; }

        public Address PickupAddress { get; set; }
        
        public ServiceType ServiceType { get; set; }

        public string ClientLanguageCode { get; set; }
    }
}
