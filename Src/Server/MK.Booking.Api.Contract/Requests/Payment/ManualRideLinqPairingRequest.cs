using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [RouteDescription("/accounts/manualridelinq/pair", "POST")]
    public class ManualRideLinqPairingRequest : IReturn<OrderManualRideLinqDetail>
    {
        public string PairingCode { get; set; }

        public Address PickupAddress { get; set; }

        public string ClientLanguageCode { get; set; }

        public string KountSessionId { get; set; }

        public string CustomerIpAddress { get; set; }
    }
}
