using System;
using apcurium.MK.Common.Resources;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Authenticate]
    [Route("/account/ridelinq/{OrderId}", "GET, DELETE")]
   public class ManualRideLinqRequest : IReturn<BasePaymentResponse>
    {
        public Guid OrderId { get; set; }
    }
}
