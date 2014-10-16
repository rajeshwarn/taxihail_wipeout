using System;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Authenticate]
    [Route("/payments/commitPayment", "POST")]
    public class CommitPaymentRequest : IReturn<CommitPreauthorizedPaymentResponse>
    {
        public decimal Amount { get; set; }
        public decimal MeterAmount { get; set; }
        public decimal TipAmount { get; set; }
        public string CardToken { get; set; }
        public Guid OrderId { get; set; }
        public bool IsNoShowFee { get; set; }
    }
}