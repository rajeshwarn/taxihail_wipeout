using System;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Authenticate]
    [Route("/payments/pair", "POST")]
    public class PairingForPaymentRequest : IReturn<PairingResponse>
    {
        public Guid OrderId { get; set; }
        public string CardToken { get; set; }
        public int? AutoTipPercentage { get; set; }
        public double? AutoTipAmount { get; set; }
    }
}