using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Authenticate]
    [Route("/account/ridelinq/{OrderId}", "POST, DELETE")]
   public class ManualRideLinqRequest
    {
        public Guid OrderId { get; set; }
    }
}
