#region

using System;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment.Braintree
{
    [Authenticate]
    [Route("/payments/braintree/preAuthorizePayment", "POST")]
    public class PreAuthorizePaymentBraintreeRequest : IReturn<PreAuthorizePaymentResponse>
    {
        public decimal Amount { get; set; }

        public decimal Meter { get; set; }

        public decimal Tip { get; set; }

        public string CardToken { get; set; }
        public Guid OrderId { get; set; }
    }
}