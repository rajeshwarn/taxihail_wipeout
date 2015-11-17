using System;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Authenticate]
    [Route("/account/manualridelinq/{OrderId}/status", "GET")]
    [Route("/account/manualridelinq/{OrderId}/unpair", "DELETE")]
    public class ManualRideLinqRequest
    {
        public Guid OrderId { get; set; }
    }
}
