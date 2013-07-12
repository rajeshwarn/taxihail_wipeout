using System;
using ServiceStack.ServiceHost;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;

namespace apcurium.MK.Booking.Api.Contract.Requests.Braintree
{
    [Route("/payments/braintree/preAuthorizePayment", "POST")]
    public class PreAuthorizePaymentBraintreeRequest : IReturn<PreAuthorizePaymentResponse>
    {
        public decimal Amount { get; set; }
        public string CardToken { get; set; }
        public Guid OrderId { get; set; }
    }
}
