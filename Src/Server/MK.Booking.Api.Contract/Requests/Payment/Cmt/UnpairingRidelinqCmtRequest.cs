using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;

namespace apcurium.MK.Booking.Api.Contract.Requests.Cmt
{
    [Authenticate]
    [Route("/payments/cmt/unpair", "POST")]
    public class UnpairingRidelinqCmtRequest : IReturn<BasePaymentResponse>
    {
        public Guid OrderId { get; set; }
    }
}