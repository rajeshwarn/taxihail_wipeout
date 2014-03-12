#region

using System;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment.Cmt
{
    [Authenticate]
    [Route("/payments/cmt/unpair", "POST")]
    public class UnpairingRidelinqCmtRequest : IReturn<BasePaymentResponse>
    {
        public Guid OrderId { get; set; }
    }
}