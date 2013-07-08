using System;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Route("/payment/paypal", "POST")]
    public class InitiatePayPalExpressCheckoutPaymentRequest : IReturn<PayPalExpressCheckoutPaymentResponse>
    {
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
    }

    public class PayPalExpressCheckoutPaymentResponse
    {
        public string CheckoutUrl { get; set; }
    }
}