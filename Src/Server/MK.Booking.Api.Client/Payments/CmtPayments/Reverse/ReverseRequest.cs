using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Client.Payments.CmtPayments.Reverse
{
    [Route("v2/fleet/{FleetToken}/device/{DeviceId}/reverse")]
    public class ReverseRequest : IReturn<ReverseResponse>
    {
        public long TransactionId { get; set; }

        public int DriverId { get; set; }

        public int TripId { get; set; }

        public string FleetToken { get; set; }

        public string DeviceId { get; set; }
    }
}