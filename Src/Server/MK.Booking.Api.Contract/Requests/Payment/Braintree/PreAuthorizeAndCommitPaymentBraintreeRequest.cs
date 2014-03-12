#region

using System;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment.Braintree
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