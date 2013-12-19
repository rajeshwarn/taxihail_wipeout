using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;

namespace apcurium.MK.Booking.Api.Contract.Requests.Braintree
{
    [Authenticate]
    [Route("/payments/braintree/preAuthorizeAndCommitPayment", "POST")]
    public class PreAuthorizeAndCommitPaymentBraintreeRequest : IReturn<CommitPreauthorizedPaymentResponse>
    {
        public decimal Amount { get; set; }
        public decimal MeterAmount { get; set; }
        public decimal TipAmount { get; set; }
        public string CardToken { get; set; }
        public Guid OrderId { get; set; }
    }
}