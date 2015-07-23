using apcurium.MK.Common.Resources;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Authenticate]
    [Route("/account/orders/{OrderId}/ridelinqinfo", "GET")]
    public class RidelinqInfoRequest : IReturn<RideLinqInfoResponse>
    {
        public Guid OrderId { get; set; }
    }
}