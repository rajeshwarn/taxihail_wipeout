using System;
using apcurium.MK.Common.Resources;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Authenticate]
    [Route("/payments/unpair", "POST")]
    public class UnpairingForPaymentRequest : IReturn<BasePaymentResponse>
    {
        public Guid OrderId { get; set; }
    }
}