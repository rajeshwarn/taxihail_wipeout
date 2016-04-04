using System;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Route("/accounts/manualridelinq/{OrderId}/status", "GET")]
    [Route("/accounts/manualridelinq/{OrderId}/unpair", "DELETE")]
    public class ManualRideLinqRequest
    {
        public Guid OrderId { get; set; }
    }
}
